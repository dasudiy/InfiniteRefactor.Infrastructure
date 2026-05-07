using InfiniteRefactor.Infrastructure.DataService.Metadata;

namespace InfiniteRefactor.Infrastructure.DataService.Abstractions
{
    public abstract class DataServiceRouter //: MarshalByRefObject
    {
        public abstract RouteInfo ReadRouteInfo(DataServiceRequest request);
        public abstract string GetRouteUri(RouteInfo info);
    }
}
