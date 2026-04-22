using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using AirMaster.Infrastructure.DataService.Common.Processor;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.DataService.Old.WebSocket;
using AirMaster.Infrastructure.Extensions;
using AirMaster.Infrastructure.Serializer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNetCore
{
    internal class WebSocketConnection : DataServiceClientBase, IDisposable
    {
        private readonly Task readThread;
        private readonly Task autoReconnectTask;
        private int isAutoReconnecting; // Use int for Interlocked operations (0 = false, 1 = true)
        private readonly ConcurrentDictionary<string, TaskCompletionSource<WebSocketMessage>> dsWaitHandlers = new();
        private readonly CancellationTokenSource source = new();
        private readonly SemaphoreSlim initSem = new(1, 1);
        private readonly SemaphoreSlim sendSem = new(1, 1);
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public bool IsServerWebSocket { get; }

        public WebSocketConnection(DataServiceHost host, WebSocket ws, EndPoint remote, TimeSpan timeout)
            : base(remote.ToString(), host: host, timeout: timeout)
        {
            WebSocket = ws;
            this.Remote = remote;
            this.readThread = Task.Run(ReadThread);
            IsServerWebSocket = true;
            this.ResultExtractor = new Internal.ResultExtractor();
        }

        public WebSocketConnection(string url, X509Certificate2 certificate = null, bool validateCertificate = true,
            DataServiceHost host = null, TimeSpan? timeout = null, string proxy = null)
            : base(url, certificate, validateCertificate, host, timeout, proxy)
        {
            IsServerWebSocket = false;
            if (!this.DataServiceHost.PostProcessors.Any(p => p is ResultWrapperAttribute))
            {
                this.DataServiceHost.PostProcessors.Add(new ResultWrapperAttribute()
                { Priority = -1, DirectOutputParameterToResult = false });
            }

            this.ResultExtractor = new Internal.ResultExtractor();

            // Start read thread and auto-reconnect task for client connections
            this.readThread = Task.Run(ReadThread);
            this.autoReconnectTask = Task.Run(AutoReconnectLoop);

            // Trigger initial connection
            TriggerAutoReconnect();
        }

        public EndPoint Remote { get; private set; }

        public override bool IsConnected => WebSocket?.State == WebSocketState.Open;

        public WebSocket WebSocket { get; private set; }

        public event EventHandler ConnectionClosed;

        public async Task Close()
        {
            // this.DataServiceHost.ConnectionClosed(this);
            // Cancel the read thread first before disposing
            await source.CancelAsync();

            try
            {
                if (WebSocket != null && WebSocket.State == WebSocketState.Open)
                {
                    await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                }
                ConnectionClosed?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                // ignored
            }
            finally
            {
                WebSocket?.Dispose();
            }
        }

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return InvokeAsync(routeInfo, paramsDict).GetAwaiter().GetResult();
        }

        public override async Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
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
                if (item.Value != null)
                {
                    request.arguments[item.Key] = this.DataServiceHost.DefaultSerializer.Serialize(item.Value);
                }
            }

            var tcs = new TaskCompletionSource<WebSocketMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (dsWaitHandlers.TryAdd(request.requestId, tcs))
            {
                await Send("DataServiceRequest", request.requestId, request);
                var cancellationTokenSource = new CancellationTokenSource(Timeout);
                cancellationTokenSource.Token.Register(() =>
                {
                    cancellationTokenSource.Dispose(); //dispose multiple times will be fine.
                    dsWaitHandlers.TryRemove(request.requestId, out _);
                    tcs.TrySetCanceled();
                }, false);

                return await tcs.Task.ContinueWith(t =>
                {
                    cancellationTokenSource.Dispose(); //dispose multiple times will be fine.
                    //var obj = SerializerFactory.Deserialize<WebSocketMessage>("json", t.Result);
                    var obj = t.Result;
                    if (obj.Content == null)
                    {
                        return null;
                    }

                    return ((obj.Content as JObject) ?? obj.Content).ToString();
                });
            }
            else
            {
                throw new Exception("Duplicated requestId");
            }
        }

        internal async Task Send<T>(string dataType, string requestId, T content)
        {
            // Wait up to 10 seconds for connection to be ready
            if (this.WebSocket == null || this.WebSocket.State != WebSocketState.Open)
            {
                var waitStartTime = DateTime.UtcNow;
                var timeout = TimeSpan.FromSeconds(10);

                while ((this.WebSocket == null || this.WebSocket.State != WebSocketState.Open) &&
                       (DateTime.UtcNow - waitStartTime) < timeout)
                {
                    await Task.Delay(100);
                }

                // Check again after waiting
                if (this.WebSocket == null || this.WebSocket.State != WebSocketState.Open)
                {
                    throw new Exception(
                        $"WebSocket connection is not ready after waiting 10 seconds. Current state: {this.WebSocket?.State}");
                }
            }

            var mesg = new WebSocketMessage(content, requestId, dataType);
            mesg.Headers = this.DefaultHeaders;

            this.DataServiceHost.OnBeforeSendMessage(mesg);

            //var msg = SerializerFactory.Serialize("json", mesg);
            var msg = this.DataServiceHost.DefaultSerializer.Serialize(mesg);
            var buffer = Encoding.UTF8.GetBytes(msg);

            await sendSem.WaitAsync();
            try
            {
                await this.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send message, triggering auto-reconnect.");
                // Trigger auto-reconnect if not already running
                if (Interlocked.CompareExchange(ref isAutoReconnecting, 0, 0) == 0 && !IsServerWebSocket)
                {
                    TriggerAutoReconnect();
                }

                throw;
            }
            finally
            {
                sendSem.Release();
            }
        }

        private void TriggerAutoReconnect()
        {
            // Use CompareExchange: if current value is 0 (false), set to 1 (true) and return 0
            // If current value is 1 (true), don't change and return 1
            if (Interlocked.CompareExchange(ref isAutoReconnecting, 1, 0) == 0)
            {
                Log.Info("Auto-reconnect triggered.");
            }
        }

        private async Task AutoReconnectLoop()
        {
            const int retryIntervalMs = 5000; // Retry every 5 seconds

            try
            {
                while (!source.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Check if connection is healthy
                        if (this.WebSocket != null && this.WebSocket.State == WebSocketState.Open)
                        {
                            Interlocked.Exchange(ref isAutoReconnecting, 0);
                            // Connection is OK, wait before checking again
                            await Task.Delay(retryIntervalMs, source.Token);
                            continue;
                        }

                        // Connection is broken, try to reconnect
                        if (Interlocked.CompareExchange(ref isAutoReconnecting, 0, 0) == 1)
                        {
                            Log.Info("Attempting to auto-reconnect WebSocket...");
                            try
                            {
                                await InitClientWebSocket();
                                Interlocked.Exchange(ref isAutoReconnecting, 0);
                                Log.Info("Auto-reconnect succeeded.");
                            }
                            catch (Exception ex)
                            {
                                Log.Warn(ex, "Auto-reconnect failed, will retry in {0}ms.", retryIntervalMs);
                                await Task.Delay(retryIntervalMs, source.Token);
                            }
                        }
                        else
                        {
                            // Wait a bit before checking again
                            await Task.Delay(1000, source.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Unexpected error in AutoReconnectLoop.");
                        await Task.Delay(retryIntervalMs, source.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AutoReconnectLoop terminated unexpectedly.");
            }
        }

        private async Task InitClientWebSocket()
        {
            await initSem.WaitAsync();
            try
            {
                var uri = new Uri(this.Url);
                if (!IPAddress.TryParse(uri.Host, out var ipAddress))
                {
                    ipAddress = (await Dns.GetHostAddressesAsync(uri.Host)).First();
                }

                var port = uri.Port;

                this.Remote = new IPEndPoint(ipAddress, port);
                Try.Todo(() => this.WebSocket?.Dispose());

                var webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(uri, CancellationToken.None);
                this.WebSocket = webSocket;

                Log.Info("WebSocket reconnected successfully.");
            }
            finally
            {
                initSem.Release();
            }
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            throw new NotImplementedException();
        }

        private async Task ReadThread()
        {
            var buffer = new byte[1024 * 1024];

            try
            {
                while (!source.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Wait for connection to be ready
                        while ((WebSocket?.State != WebSocketState.Open) && !source.Token.IsCancellationRequested)
                        {
                            await Task.Delay(100, source.Token);
                        }

                        if (source.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        int length = 0;
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await WebSocket.ReceiveAsync(
                                new ArraySegment<byte>(buffer, length, buffer.Length - length), source.Token);
                            length += result.Count;

                            // Resize buffer exponentially if we run out of space for partial messages
                            if (length >= buffer.Length && !result.EndOfMessage)
                            {
                                var newBuffer = new byte[buffer.Length * 2];
                                Buffer.BlockCopy(buffer, 0, newBuffer, 0, length);
                                buffer = newBuffer;
                            }
                            if (result.MessageType == WebSocketMessageType.Close || result.CloseStatus != null)
                            {
                                Log.Info("WebSocket close message received from {0}.",
                                    IsServerWebSocket ? "client" : "server");

                                // Complete the close handshake
                                try
                                {
                                    if (WebSocket.State == WebSocketState.Open ||
                                        WebSocket.State == WebSocketState.CloseReceived)
                                    {
                                        await WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,
                                            "Acknowledged", CancellationToken.None);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Warn(ex, "Failed to send close response.");
                                }

                                if (!IsServerWebSocket)
                                {
                                    // Client: trigger reconnect and continue loop
                                    TriggerAutoReconnect();
                                    break;
                                }
                                else
                                {
                                    // Server: connection closed by client, exit ReadThread completely
                                    Log.Info("Server WebSocket closed by client, exiting ReadThread.");
                                    return;
                                }
                            }
                        } while (!result.EndOfMessage);

                        if (length > 0 && result.MessageType != WebSocketMessageType.Close)
                        {
                            var json = Encoding.UTF8.GetString(buffer, 0, length);
                            _ = Task.Run(() => ProcessPacket(json));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation token was signaled, exit gracefully
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error in WebSocket ReadThread.");
                        Try.Todo(() => ConnectionClosed?.Invoke(this, EventArgs.Empty));

                        if (!IsServerWebSocket)
                        {
                            TriggerAutoReconnect();
                            // Wait a bit before trying to read again
                            try
                            {
                                await Task.Delay(1000, source.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                        }
                        else
                        {
                            // Server WebSocket error, exit
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error in WebSocket ReadThread.");
            }
        }

        private async Task ProcessPacket(string json)
        {
            try
            {
                var obj = this.DataServiceHost.DefaultSerializer.Deserialize<WebSocketMessage>(json);

                if (obj == null || string.IsNullOrEmpty(obj.MessageType))
                {
                    return;
                }

                this.DataServiceHost.OnBeforeReceiveMessage(obj);

                var messageType = obj.MessageType;
                switch (messageType)
                {
                    case "DataServiceRequest":
                        {
                            using (var context = new WebSocketContext(this, obj, (obj.Content.To<JObject>()).ToObject<RequestInfo>()))
                            {
                                await this.DataServiceHost.ProcessContextAsync(context);
                            }
                        }
                        break;
                    case "DataServiceResponse":
                        {
                            var id = obj.MessageId;
                            if (dsWaitHandlers.TryRemove(id, out TaskCompletionSource<WebSocketMessage> handler))
                            {
                                handler.TrySetResult(obj);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing WebSocket packet.");
            }
        }


        public void Dispose()
        {
            try
            {
                // Cancel the read thread and auto-reconnect loop
                Try.Todo(async () => await source.CancelAsync());

                // Wait for threads to finish (with timeout to prevent hanging)
                var tasks = new[] { readThread, autoReconnectTask }.Where(t => t != null).ToArray();
                if (tasks.Length > 0)
                {
                    Task.WaitAll(tasks, TimeSpan.FromSeconds(5));
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                Try.Todo(() => readThread?.Dispose());
                Try.Todo(() => autoReconnectTask?.Dispose());
                Try.Todo(() => source?.Dispose());
                Try.Todo(() => initSem?.Dispose());
                Try.Todo(() => sendSem?.Dispose());
                Try.Todo(() => WebSocket?.Dispose());
            }
        }
    }
}