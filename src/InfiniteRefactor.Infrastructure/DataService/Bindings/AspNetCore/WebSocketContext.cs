using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using InfiniteRefactor.Infrastructure.DataService.Old.WebSocket;
using InfiniteRefactor.Infrastructure.Session;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.AspNetCore
{
    internal class WebSocketContext : DataServiceContext
    {
        public WebSocketContext(WebSocketConnection connection, WebSocketMessage webSocketMessage,
            RequestInfo requestInfo)
            : base(requestInfo)
        {
            requestInfo.headers = webSocketMessage.Headers;
            Connection = connection;
            request = new WebSocketRequest(this, requestInfo);
            response = new WebSocketResponse(this, requestInfo);
        }

        public override DataServiceRequest Request => request;

        public override DataServiceResponse Response => response;

        public override ApplicationSession Session => throw new NotImplementedException();

        public override DataServiceClientBase Client => Connection;

        public WebSocketConnection Connection { get; }

        private readonly WebSocketRequest request;
        private readonly WebSocketResponse response;
    }

    internal class WebSocketRequest(WebSocketContext context, RequestInfo requestInfo) : DataServiceRequest(context)
    {
        // always use serializer for websocket
        public override object ReadParameter(string name, Type targetType)
        {
            if (!ParameterCache.TryGetValue(name, out var rawValue))
            {
                rawValue = ParameterCache[name] = this[name];
            }
            if (rawValue != null)
            {
                return ParameterCache[name] = Context.Serializer.Deserialize(rawValue.ToString(), targetType);
            }
            return null;
        }

        public override object this[string key] => requestInfo.arguments?.TryGetValue(key, out var value) == true ? value : null;

        public override Uri Url => new Uri(this.Context.RouteInfo.ToString());

        public override object RawRequestObject => requestInfo;

        public override IPEndPoint Remote => context.Connection.Remote as IPEndPoint;
    }

    internal class WebSocketResponse(WebSocketContext context, RequestInfo requestInfo) : DataServiceResponse(context)
    {
        public override object RawResponseObject => (this.Context as WebSocketContext).Connection;

        public override System.IO.Stream OutputStream => throw new NotImplementedException();

        public override TextWriter OutputWriter => throw new NotImplementedException();

        public override bool IsClientConnected => (this.Context as WebSocketContext).Connection.IsConnected;

        public override string ContentType { get; set; }

        public override void WriteResult()
        {
            WriteResultAsync().Wait();
        }

        public override Task WriteResultAsync()
        {
            return (Context as WebSocketContext).Connection
                .Send("DataServiceResponse", requestInfo.requestId, this.Result);
        }
    }
}