using AirMaster.Infrastructure.DataService.Metadata;

namespace AirMaster.Infrastructure.DataService.Abstractions
{
    public abstract class DataServiceRouter //: MarshalByRefObject
    {
        public abstract RouteInfo ReadRouteInfo(DataServiceRequest request);
        public abstract string GetRouteUri(RouteInfo info);
    }
}
