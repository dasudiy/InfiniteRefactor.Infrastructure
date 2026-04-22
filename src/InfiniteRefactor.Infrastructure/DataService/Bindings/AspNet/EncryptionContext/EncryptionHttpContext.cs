using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.Session;
using Microsoft.AspNetCore.Http;
using System;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNet.EncryptionContext
{
    public class EncryptionHttpContext : DataServiceContext
    {
        private HttpContext _rawContext;
        private EncryptionRequest _request;
        private EncryptionResponse _response;
        private ApplicationSession _session;

        public string SecurityKey { get; private set; }

        static EncryptionHttpContext()
        {
            SymmetricAlgorithm = "AES";
        }

        public EncryptionHttpContext(HttpContext context)
            : base(DataServiceHost.Instance.Router)
        {
            if (GetSerucityKey == null) { throw new Exception("EncryptionHttpContext未初始化，GetSerucityKey必须赋值才可使用。"); }

            SecurityKey = GetSerucityKey(context);

            this._rawContext = context;
            this._request = new EncryptionRequest(context.Request, this);
            this._response = new EncryptionResponse(context.Response, this);
            if (context.Session != null) { this._session = new AspNetSession(context); }

            foreach (var key in context.Items.Keys)
            {
                this.ServerParameters[key.ToString()] = context.Items[key];
            }
        }

        public override DataServiceRequest Request { get { return _request; } }

        public override DataServiceResponse Response { get { return _response; } }

        public static AspNetDataServiceContext Handle(HttpContext context)
        {
            var cnx = new AspNetDataServiceContext(context);
            DataServiceHost.Instance.ProcessContext(cnx);
            return cnx;
        }

        public override ApplicationSession Session
        {
            get { return _session; }
        }

        public static Func<HttpContext, string> GetSerucityKey { get; set; }
        public static string SymmetricAlgorithm { get; set; }

        public bool GZip { get; set; }

        public override DataServiceClientBase Client
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

}
