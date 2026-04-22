using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Internal;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.DataService.Old.WebSocket;
using AirMaster.Infrastructure.Serializer;
using AirMaster.Infrastructure.Utilities;
using AirMaster.Infrastructure.Extensions;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    internal class StreamConnection : DataServiceClientBase, IDisposable
    {
        internal IConnectionController ConnectionController { get; set; }
        private object readThread;
        private CancellationTokenSource source = new CancellationTokenSource();
        private List<Packet> recvPacketCache = new List<Packet>();
        private ushort currentRecvMessageId;
        private ushort currentSendMessageId;
        private object syncroot = new object();
        private DataServiceHost host;
        private ConcurrentDictionary<string, TaskCompletionSource<string>> dsWaitHandlers = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        //private Dictionary<string, string> dsResults = new Dictionary<string, string>();
        private X509Certificate _remoteCertificate;

        public override X509Certificate RemoteCertificate
        {
            get
            {
                return _remoteCertificate;
            }
        }

        public StreamConnection(DataServiceHost host, IConnectionController conn, X509Certificate remoteCert, TimeSpan timeout, string proxy)
            : base(null, host: host, timeout: timeout, proxy: proxy)
        {
            this.host = host;
            ConnectionController = conn;
            this.readThread = Task.Run(() => ReadThread());
            this.ResultExtractor = new ResultExtractor();
            this._remoteCertificate = remoteCert;
        }

        private async void ReadThread()
        {
            //var buffer = new byte[512];
            //var mainOffset = 0;
            int packetLength = 0;
            var buffer = new byte[6];
            do
            {
                try
                {
                    await ConnectionController.Stream.ReadAsyncAtLeast(buffer, 0, 6, 6, source.Token).ConfigureAwait(false);
                    packetLength = (int)BitConverter.ToUInt32(buffer, 2);
                    var packet = new byte[packetLength];
                    Array.Copy(buffer, packet, buffer.Length);
                    await ConnectionController.Stream.ReadAsyncAtLeast(packet, buffer.Length, packetLength - buffer.Length, packetLength - buffer.Length, source.Token).ConfigureAwait(false);
                    try
                    {
                        ProcessPacket(packet);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    //var len = await ConnectionController.Stream.ReadAsyncAtLeast(buffer, mainOffset, buffer.Length - mainOffset, 6, source.Token);
                    //mainOffset = 0;
                    //int offset1 = 0;
                    //int packageLength = 0;

                    //while (len > offset1)
                    //{
                    //    if (offset1 + 6 >= buffer.Length)
                    //    {
                    //        Array.Copy(buffer, offset1, buffer, 0, buffer.Length - offset1);
                    //        mainOffset = buffer.Length - offset1;
                    //        break;
                    //    }               
                    //    packageLength = (int)BitConverter.ToUInt32(buffer.Skip(offset1 + 2).Take(4).ToArray(), 0);

                    //    var packet = new byte[packageLength];
                    //    Array.Copy(buffer, offset1, packet, 0, Math.Min(len - offset1, packageLength));

                    //    if (packageLength > (len - offset1)) //长度不够
                    //    {
                    //        int offset = len - offset1;
                    //        int remaining = packageLength - (len - offset1); //还差

                    //        do
                    //        {
                    //            len = await ConnectionController.Stream.ReadAsync(buffer, 0, buffer.Length, source.Token);
                    //            Array.Copy(buffer, 0, packet, offset, Math.Min(remaining, len));
                    //            offset += Math.Min(remaining, len);
                    //            offset1 = remaining;
                    //            remaining -= len;
                    //        } while (remaining > 0);



                    //        //len = await ConnectionController.Stream.ReadAsyncAtLeast(buffer, 0, buffer.Length, remaining, source.Token);
                    //        //Array.Copy(buffer, 0, packet, offset, remaining);
                    //    }
                    //    else
                    //    {
                    //        offset1 += packageLength;
                    //    }

                    //    //ProcessPacketAsync(packet).Forget();
                    //    try
                    //    {
                    //        ProcessPacketAsync(packet);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine(ex.ToString());
                    //    }
                    //}
                }
                catch
                {
                    Dispose();
                    break;
                }
            } while (ConnectionController.IsConnected && ConnectionController.Stream.CanRead && !source.Token.IsCancellationRequested);
        }

        private void ProcessPacket(byte[] bytes)
        {
            try
            {
                var packet = MarshalHelper.FromByteArray<Packet>(bytes);
                if (!packet.Header.Valid()) { Close(); }

                if (currentRecvMessageId != packet.Header.MessageId)
                {
                    currentRecvMessageId = packet.Header.MessageId;
                    recvPacketCache.Clear();
                }

                if (packet.Header.TotalPackets > 1)
                {
                    recvPacketCache.Add(packet);
                    if (recvPacketCache.Count == packet.Header.TotalPackets)
                    {
                        OnMessageReceived(recvPacketCache.ToArray());
                    }
                }
                else
                {
                    OnMessageReceived(packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnMessageReceived(params Packet[] packet)
        {
            var packetMsg = packet.Length == 1 ? packet[0].Content : packet.OrderBy(p => p.Header.SequenceIndex).SelectMany(p => p.Content).ToArray();
            var json = GetEncoding(packet[0].Header.Encoding).GetString(packetMsg);
            var obj = SerializerFactory.Deserialize<WebSocketMessage>("json", json);
            var messageType = obj.MessageType;
            switch (messageType)
            {
                case "DataServiceRequest":
                    {
                        var requestInfo = (obj.Content as JObject).ToObject<RequestInfo>();
                        //var requestInfo = 
                        //SerializerFactory.Deserialize<WebSocket.WebSocketMessage<WebSocket.Server.RequestInfo>>("json", json);
                        var context = new StreamContext(requestInfo, this);
                        //_ = host.ProcessContextAsync(context);
                        Task.Run(async () => await host.ProcessContextAsync(context));
                    }
                    break;
                case "DataServiceResponse":
                    {
                        var id = obj.MessageId;
                        if (dsWaitHandlers.TryRemove(id, out TaskCompletionSource<string> handler))
                        {
                            handler.TrySetResult(json);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private Encoding GetEncoding(EncodingEnum encoding)
        {
            switch (encoding)
            {
                case EncodingEnum.UTF32:
                    return Encoding.UTF32;
                case EncodingEnum.GBK:
                    return Encoding.GetEncoding("GBK");
                default:
                case EncodingEnum.UTF8:
                    return Encoding.UTF8;
            }
        }

        private void Close()
        {
            Dispose();
        }

        public override bool IsConnected
        {
            get
            {
                return ConnectionController.IsConnected;
            }
        }

        public void Dispose()
        {
            lock (source)
            {
                if (!source.IsCancellationRequested)
                {
                    source.Cancel();
                    ConnectionController.Close();
                }
            }
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            throw new NotImplementedException();
        }

        public override Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            var request = new RequestInfo
            {
                actionName = routeInfo.ActionName,
                requestId = Guid.NewGuid().ToString(),
                serviceName = routeInfo.ServiceName,
                arguments = new Dictionary<string, string>()
            };
            foreach (var item in paramsDict)
            {
                request.arguments[item.Key] = SerializerFactory.Serialize("json", item.Value);
            }

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (dsWaitHandlers.TryAdd(request.requestId, tcs))
            {
                Send("DataServiceRequest", request.requestId, request);
                var cancellationTokenSource = new CancellationTokenSource(Timeout);
                cancellationTokenSource.Token.Register(() =>
                {
                    cancellationTokenSource.Dispose(); //dispose multiple times will be fine.
                    dsWaitHandlers.TryRemove(request.requestId, out _);
                    tcs.TrySetCanceled();
                }, false);

                return tcs.Task.ContinueWith(t =>
                {
                    cancellationTokenSource.Dispose();//dispose multiple times will be fine.
                    var obj = SerializerFactory.Deserialize<WebSocketMessage>("json", t.Result);
                    if (obj.Content == null) { return null; }
                    return (obj.Content as JObject).ToString();
                });
            }
            else
            {
                throw new Exception("Duplicated requestid");
            }
        }

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return InvokeAsync(routeInfo, paramsDict).Result;
        }

        public override void InvokeAndForget(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            var request = new RequestInfo
            {
                actionName = routeInfo.ActionName,
                requestId = Guid.NewGuid().ToString(),
                serviceName = routeInfo.ServiceName,
                arguments = new Dictionary<string, string>()
            };
            foreach (var item in paramsDict)
            {
                request.arguments[item.Key] = SerializerFactory.Serialize("json", item.Value);
            }

            Send("DataServiceRequest", request.requestId, request);
        }

        public void Send<T>(string type, string messageId, T content)
        {
            Send(new WebSocketMessage<T>(content, messageId, type));
        }

        public Task SendAsync<T>(string type, string messageId, T content)
        {
            return SendAsync(new WebSocketMessage<T>(content, messageId, type));
        }

        public void Send(WebSocketMessage message)
        {
            var msg = SerializerFactory.Serialize("json", message);
            var bytes = Encoding.UTF8.GetBytes(msg);

            var packet = new Packet
            {
                Header = new PacketHeader
                {
                    AppAgent = "DataServiceClient/" + typeof(StreamConnection).AssemblyQualifiedName,
                    Encoding = EncodingEnum.UTF8,
                    MAC = new byte[64], //AirMaster.Infrastructure.Utilities.PerformanceInfo.GetPhysicalAddress(),
                    MessageId = GetNextMessageId(),
                    ProtocolVersion = 1,
                    SequenceIndex = 0,
                    TotalPackets = 1,
                    Length = (uint)Marshal.SizeOf(typeof(PacketHeader)) + (uint)bytes.Length
                },
                Content = bytes
            };

            lock (ConnectionController)
            {
                var buffer = MarshalHelper.ToByteArray(packet);
                ConnectionController.Stream.Write(buffer, 0, buffer.Length);
            }
        }

        public Task SendAsync(WebSocketMessage message)
        {
            var msg = SerializerFactory.Serialize("json", message);
            var bytes = Encoding.UTF8.GetBytes(msg);

            var packet = new Packet
            {
                Header = new PacketHeader
                {
                    AppAgent = "DataServiceClient/" + typeof(StreamConnection).AssemblyQualifiedName,
                    Encoding = EncodingEnum.UTF8,
                    MAC = new byte[64], //AirMaster.Infrastructure.Utilities.PerformanceInfo.GetPhysicalAddress(),
                    MessageId = GetNextMessageId(),
                    ProtocolVersion = 1,
                    SequenceIndex = 0,
                    TotalPackets = 1,
                    Length = (uint)Marshal.SizeOf(typeof(PacketHeader)) + (uint)bytes.Length
                },
                Content = bytes
            };

            //lock (ConnectionController)
            {
                var buffer = MarshalHelper.ToByteArray(packet);
                return ConnectionController.Stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        private ushort GetNextMessageId()
        {
            lock (syncroot)
            {
                currentSendMessageId++;
                if (currentSendMessageId == ushort.MaxValue)
                {
                    currentSendMessageId = 0;
                    //return ushort.MaxValue;
                }
                return currentSendMessageId;
            }
        }
    }
}