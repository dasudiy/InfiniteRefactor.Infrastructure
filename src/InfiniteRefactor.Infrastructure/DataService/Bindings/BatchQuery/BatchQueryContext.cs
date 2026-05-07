using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.Session;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.BatchQuery
{
    public class BatchQueryContext : DataServiceContext
    {
        public DataServiceContext Parent { get; private set; }
        private BatchQueryRequest request;
        private BatchQueryResponse response;

        public BatchQueryContext(DataServiceContext context, string requestArgument)
            : base(DataServiceHost.Instance.Router)
        {
            Parent = context;
            request = new BatchQueryRequest(this, context.Request, requestArgument);
            response = new BatchQueryResponse(this, context.Response);
        }

        public override bool IsBatchContext
        {
            get
            {
                return false;
            }
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
                return Parent.Session;
            }
        }

        public override DataServiceClientBase Client
        {
            get
            {
                return Parent.Client;
            }
        }

        public string GetResult()
        {
            response.WriteResult();
            return response.GetResult();
        }
    }
}
