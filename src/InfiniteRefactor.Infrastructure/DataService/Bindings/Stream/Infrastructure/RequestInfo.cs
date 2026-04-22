using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Metadata;
using System;
using System.Collections.Generic;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    public class RequestInfo : DataServiceRouter
    {
        public string serviceName { get; set; }
        public string actionName { get; set; }
        public Dictionary<string, string> arguments { get; set; }
        public string requestId { get; set; }
        public Dictionary<string, string> headers { get; internal set; }

        public override RouteInfo ReadRouteInfo(DataServiceRequest request)
        {
            return new RouteInfo
            {
                ActionName = actionName,
                FormatName = "json",
                ServiceName = serviceName
            };
        }

        public override string GetRouteUri(RouteInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
