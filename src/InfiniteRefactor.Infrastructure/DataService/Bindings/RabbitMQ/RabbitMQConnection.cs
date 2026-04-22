using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using AirMaster.Infrastructure.DataService.Internal;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.DataService.Old.WebSocket;
using AirMaster.Infrastructure.Serializer;
using AirMaster.Infrastructure.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace AirMaster.Infrastructure.DataService.Bindings.RabbitMQ
{
    class RabbitMQConnection : DataServiceClientBase, IDisposable
    {
        public string ConsumeQueueName { get; }
        public string ProduceQueueName { get; }

        private ConnectionFactory factory;
        private IConnection connection;
        private IModel channel;

        private ConcurrentDictionary<string, TaskCompletionSource<string>> dsWaitHandlers =
            new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        private MemoryCache mc;

        //private volatile SemaphoreSlim sem;        
        public ushort PrefetchCount { get; set; } = 10;

        public RabbitMQConnection(string url, TimeSpan? timeout, DataServiceHost host = null) : base(url, null, false,
            host, timeout)
        {
            var uri = new Uri(url);

            var qs = HttpUtility.ParseQueryString(uri.Query);
            if (string.IsNullOrWhiteSpace(qs["consume"]))
            {
                throw new ArgumentException("必须指定consume");
            }

            if (string.IsNullOrWhiteSpace(qs["produce"]))
            {
                throw new ArgumentException("必须指定produce");
            }

            this.ConsumeQueueName = qs["consume"];
            this.ProduceQueueName = qs["produce"];

            this.factory = new ConnectionFactory
                { Uri = uri, AutomaticRecoveryEnabled = true, DispatchConsumersAsync = false };
            this.ResultExtractor = new ResultExtractor();

            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            var args = new Dictionary<string, object>();
            args.Add("x-message-ttl", TimeSpan.FromSeconds(10).TotalMilliseconds.To<int>());
            channel.QueueDeclare(queue: ConsumeQueueName, durable: false, exclusive: false,
                autoDelete: ConsumeQueueName.Contains("REPLY"), arguments: args);

            this.mc = new MemoryCache(new MemoryCacheOptions());

            //this.client = new RabbitMQConnection( url, factory, connection, channel, timeout, host);
        }

        public void Start()
        {
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: ConsumeQueueName,
                autoAck: false, consumer: consumer);

            consumer.Received += Consumer_Received;
            //consumer.Received += Consumer_ReceivedAsync;
        }

        //private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs e)
        //{
        //    var body = e.Body;
        //    var props = e.BasicProperties;
        //    try
        //    {
        //        var message = Encoding.UTF8.GetString(body);
        //        var messageType = Encoding.UTF8.GetString(e.BasicProperties.Headers["DataServiceMessageType"] as byte[]);
        //        var messageId = e.BasicProperties.CorrelationId;

        //        switch (messageType)
        //        {
        //            case "DataServiceRequest":
        //                {
        //                    //sem?.Wait();
        //                    var replyProps = channel.CreateBasicProperties();
        //                    replyProps.CorrelationId = props.CorrelationId;
        //                    //replyProps.Expiration = this.Timeout.TotalMilliseconds.To<int>().ToString();
        //                    replyProps.Headers = new Dictionary<string, object>();
        //                    replyProps.Headers.Add("DataServiceMessageType", "DataServiceResponse");

        //                    _ = Task.Run(async () =>
        //                     {
        //                         try
        //                         {
        //                             var requestInfo = SerializerFactory.Deserialize<WebSocketMessage<RequestInfo>>("json", message);
        //                             var context = new RabbitMQContext(requestInfo.Content);
        //                             await DataServiceHost.ProcessContextAsync(context);

        //                             var response = new WebSocketMessage(context.Response.Result, messageId, "DataServiceResponse");
        //                             var responseJson = context.Serializer.Serialize(response);
        //                             var responseBytes = Encoding.UTF8.GetBytes(responseJson);

        //                             channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
        //                               basicProperties: replyProps, body: responseBytes);
        //                         }
        //                         finally
        //                         {
        //                             channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
        //                             //sem?.Release();
        //                         }
        //                     });

        //                }
        //                break;
        //            case "DataServiceResponse":
        //                {
        //                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);

        //                    TaskCompletionSource<string> task;
        //                    if (dsWaitHandlers.TryRemove(messageId, out task))
        //                    {
        //                        task.TrySetResult(message);
        //                    }
        //                    else
        //                    {
        //                        //channel.BasicNack(e.DeliveryTag, false, false);
        //                    }
        //                }
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    finally
        //    {
        //    }
        //}

        internal void SetConcurrentLimit(int qty)
        {
            //this.sem = new System.Threading.SemaphoreSlim(qty, qty);
            channel.BasicQos(0, qty.To<ushort>(), false);
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var props = e.BasicProperties;
            try
            {
                var message = Encoding.UTF8.GetString(body.ToArray());
                var messageType =
                    Encoding.UTF8.GetString(e.BasicProperties.Headers["DataServiceMessageType"] as byte[]);
                var messageId = e.BasicProperties.CorrelationId;

                switch (messageType)
                {
                    case "DataServiceRequest":
                    {
                        //sem?.Wait();
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;
                        //replyProps.Expiration = this.Timeout.TotalMilliseconds.To<int>().ToString();
                        replyProps.Headers = new Dictionary<string, object>();
                        replyProps.Headers.Add("DataServiceMessageType", "DataServiceResponse");

                        Task.Run(async () =>
                        {
                            try
                            {
                                var clientId = props.ReplyTo.Split('-').LastOrDefault();

                                var client = mc.GetOrCreate(clientId, entry =>
                                {
                                    entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));
                                    return new RabbitMQServerClient(this, clientId);
                                });

                                var requestInfo =
                                    SerializerFactory.Deserialize<WebSocketMessage<RequestInfo>>("json", message);
                                var context = new RabbitMQContext(requestInfo.Content, client);
                                await DataServiceHost.ProcessContextAsync(context);

                                var response = new WebSocketMessage(context.Response.Result, messageId,
                                    "DataServiceResponse");
                                var responseJson = context.Serializer.Serialize(response);
                                var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                                channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                    basicProperties: replyProps, body: responseBytes);
                            }
                            catch (Exception ex)
                            {
                                NLog.LogManager.GetCurrentClassLogger().Error(ex);
                            }
                            finally
                            {
                                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                            }
                        }); //.ContinueWith(t =>
                        //{
                        //    sem?.Release();
                        //});
                    }
                        break;
                    case "DataServiceResponse":
                    {
                        channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                        TaskCompletionSource<string> task;
                        if (dsWaitHandlers.TryRemove(messageId, out task))
                        {
                            task.TrySetResult(message);
                        }
                        else
                        {
                            //channel.BasicNack(e.DeliveryTag, false, false);
                        }
                    }
                        break;
                    default:
                        break;
                }
            }
            finally
            {
            }
        }

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return InvokeAsync(routeInfo, paramsDict).Result;
        }

        internal Task<string> InternalSendAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict,
            string clientId)
        {
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
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

            dsWaitHandlers[request.requestId] = tcs;
            Send("DataServiceRequest", request.requestId, request, clientId);
            Task.Delay(Timeout).ContinueWith(t =>
            {
                if (dsWaitHandlers.ContainsKey(request.requestId))
                {
                    dsWaitHandlers.TryRemove(request.requestId, out _);
                    tcs.SetException(new TimeoutException());
                }
            });
            return tcs.Task.ContinueWith(t =>
            {
                var obj = SerializerFactory.Deserialize<WebSocketMessage>("json", t.Result);
                if (obj.Content == null)
                {
                    return null;
                }

                return obj.Content.ToString();
            });
        }

        internal void InternalSendIgnoreResponse(RouteInfo routeInfo, Dictionary<string, object> paramsDict,
            string clientId)
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

            Send("DataServiceRequest", request.requestId, request, clientId);
        }

        public override Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return InternalSendAsync(routeInfo, paramsDict, null);
        }

        private void Send<T>(string type, string messageId, T content, string clientId = null)
        {
            Send(new WebSocketMessage<T>(content, messageId, type), clientId);
        }

        public void Send(WebSocketMessage message, string clientId = null)
        {
            var props = channel.CreateBasicProperties();
            props.CorrelationId = message.MessageId;
            props.ReplyTo = ConsumeQueueName;
            props.ContentType = "application/json";
            props.Headers = new Dictionary<string, object>();
            props.Headers.Add("DataServiceMessageType", message.MessageType);

            var msg = SerializerFactory.Serialize("json", message);
            var bytes = Encoding.UTF8.GetBytes(msg);

            var produceQueue = this.ProduceQueueName;
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                produceQueue = produceQueue + "-" + clientId;
            }

            channel.BasicPublish(
                exchange: "",
                routingKey: produceQueue,
                basicProperties: props,
                body: bytes);
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Try.Todo(() => dsWaitHandlers.ForEach(wait => wait.Value.SetCanceled()));

            channel.Dispose();
            connection.Dispose();
        }
    }
}