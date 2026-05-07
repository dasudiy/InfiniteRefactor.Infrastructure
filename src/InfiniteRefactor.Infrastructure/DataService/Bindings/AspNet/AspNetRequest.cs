using System;
using System.Linq;
using System.Net;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.AspNet
{
    public class AspNetRequest : DataServiceRequest
    {
        private HttpRequest request;
        private HttpContext context;
        private Uri url;

        public AspNetRequest(AspNetDataServiceContext context, HttpContext context1) : base(context)
        {
            this.request = context1.Request;
            this.context = context1;
            url = new Uri(Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(request));
        }


        public override object this[string key]
        {
            get
            {                
                StringValues values;
                if (request.HasFormContentType && request.Form.ContainsKey(key))
                {
                    values = request.Form[key];                    
                }
                else if (request.Query.ContainsKey(key))
                {
                    values = request.Query[key];
                }
                else
                {
                    return null;
                }

                if (values.Count == 1)
                {
                    return values.Single();
                }
                else
                {
                    return values.ToArray();
                }
            }
        }

        public override Uri Url => url;

        public override object RawRequestObject => request;

        public override IPEndPoint Remote => new IPEndPoint(request.HttpContext.Connection.RemoteIpAddress, request.HttpContext.Connection.RemotePort);
    }
}
