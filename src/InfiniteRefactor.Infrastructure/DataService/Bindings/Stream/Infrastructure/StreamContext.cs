using System.Net;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.Session;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    internal class StreamContext : DataServiceContext
    {
        internal RequestInfo requestInfo;
        private StreamRequest request;
        private StreamResponse response;
        private ApplicationSession session = null;

        internal StreamConnection StreamConnection { get; set; }

        public StreamContext(RequestInfo requestInfo, StreamConnection streamConnection)
            : base(requestInfo)
        {
            this.requestInfo = requestInfo;
            this.StreamConnection = streamConnection;
            this.request = new StreamRequest(this, requestInfo);
            this.request.InternalRemote = this.StreamConnection.ConnectionController.Remote as IPEndPoint;
            this.response = new StreamResponse(this);
            //this.session = SessionManager.Instance.GetOrCreateSession(streamConnection.GetHashCode().ToString(), id => new CommonSession(id));
        }

        public override DataServiceRequest Request
        {
            get
            {
                return request;
            }
        }

        public override DataServiceResponse Response
        {
            get
            {
                return response;
            }
        }

        public override ApplicationSession Session
        {
            get
            {
                return session;
            }
        }

        public override DataServiceClientBase Client
        {
            get
            {
                return StreamConnection;
            }
        }

        public override string Identity
        {
            get
            {
                return StreamConnection.RemoteCertificate?.Subject;
            }
        }
    }
}