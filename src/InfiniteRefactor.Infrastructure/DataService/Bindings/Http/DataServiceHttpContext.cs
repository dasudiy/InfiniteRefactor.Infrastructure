using AirMaster.Infrastructure.DataService;
using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.Session;
using System;

namespace AirMaster.Infrastructure.DataService.Bindings.Http
{
    public class DataServiceHttpContext : DataServiceContext
    {
        private System.Net.HttpListenerContext httpListenerContext;
        private DataServiceHttpRequest request;
        private DataServiceHttpResponse response;
        private ApplicationSession session;

        public DataServiceHttpContext(System.Net.HttpListenerContext httpListenerContext, bool allowCompress = true)
            : base(DataServiceHost.Instance.Router)
        {
            this.httpListenerContext = httpListenerContext;
            request = new DataServiceHttpRequest(this.httpListenerContext.Request, this);


            bool gzip = false;
            if (allowCompress)
            {
                var acceptEncoding = this.httpListenerContext.Request.Headers["Accept-Encoding"];
                if (!string.IsNullOrWhiteSpace(acceptEncoding) && acceptEncoding.Contains("gzip"))
                {
                    gzip = true;
                }
            }

            response = new DataServiceHttpResponse(this.httpListenerContext.Response, gzip, this);

            //string _sessionId = null;
            //if (httpListenerContext.Request.Cookies[DataServiceHttpSession.COOKIE_KEY] != null)
            //{
            //    _sessionId = httpListenerContext.Request.Cookies[DataServiceHttpSession.COOKIE_KEY].Value;
            //}
            //else
            //{
            //    _sessionId = Guid.NewGuid().ToString();
            //    httpListenerContext.Response.Cookies.Add(new Cookie(DataServiceHttpSession.COOKIE_KEY, _sessionId));
            //}

            //this.session = SessionManager.Instance.GetOrCreateSession(_sessionId, (id) => new DataServiceHttpSession(id));
        }

        public override DataServiceClientBase Client
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override DataServiceRequest Request
        {
            get { return request; }
        }

        public override DataServiceResponse Response
        {
            get { return response; }
        }

        public override ApplicationSession Session
        {
            get { return session; }
        }
    }
}
