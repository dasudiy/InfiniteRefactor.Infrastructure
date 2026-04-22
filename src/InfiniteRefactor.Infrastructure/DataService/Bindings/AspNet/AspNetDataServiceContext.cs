using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Common;
using AirMaster.Infrastructure.Session;
using Microsoft.AspNetCore.Http;
using System;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNet
{
    public class AspNetDataServiceContext : DataServiceContext
    {
        private HttpContext context;
        private AspNetRequest request;
        private AspNetResponse response;
        private Lazy<ApplicationSession> session;

        public AspNetDataServiceContext(HttpContext context)
            : base(new DefaultServiceRouter())
        {
            this.context = context;
            this.request = new AspNetRequest(this, context);
            this.response = new AspNetResponse(this, context);

            this.session = new Lazy<ApplicationSession>(() =>
            {
                if (context.Session != null)
                {
                    return new AspNetSession(context);
                }
                else
                {
                    return null;
                }
            });
        }

        public override DataServiceRequest Request => request;

        public override DataServiceResponse Response => response;

        public override ApplicationSession Session => session.Value;

        public override DataServiceClientBase Client => throw new NotImplementedException();
    }
}
