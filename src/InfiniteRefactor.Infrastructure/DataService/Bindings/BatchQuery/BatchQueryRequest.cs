using System;
using System.Collections.Specialized;
using System.Net;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;

//using System.Web;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.BatchQuery
{
    public class BatchQueryRequest : DataServiceRequest
    {
        private NameValueCollection col;
        private DataServiceRequest rawRequest;

        public BatchQueryRequest(BatchQueryContext context, DataServiceRequest request, string requestArgument)
            : base(context)
        {
            throw new NotImplementedException();
            //col = HttpUtility.ParseQueryString(requestArgument);
            //rawRequest = request;
        }

        public override object this[string key]
        {
            get
            {
                return col[key];
            }
        }

        public override object RawRequestObject
        {
            get
            {
                return rawRequest;
            }
        }

        public override IPEndPoint Remote
        {
            get
            {
                return rawRequest.Remote;
            }
        }

        public override Uri Url
        {
            get
            {
                return rawRequest.Url;
            }
        }
    }
}
