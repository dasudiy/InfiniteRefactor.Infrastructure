using System.Collections.Generic;

namespace InfiniteRefactor.Infrastructure.DataService.Metadata
{


    //[Serializable]
    public class RouteInfo
    {
        public string ServiceName { get; set; }
        public string ActionName { get; set; }
        public string FormatName { get; set; }

        internal bool IsValid(Dictionary<string, ServiceInfo> services)
        {
            return services.ContainsKey(this.ServiceName) &&
                services[this.ServiceName].Actions.ContainsKey(this.ActionName);
        }

        internal ActionInfo GetActionInfo(Dictionary<string, ServiceInfo> services)
        {
            return services[this.ServiceName].Actions[this.ActionName];
        }

        public override string ToString()
        {
            return ServiceName + "." + ActionName;
        }
    }
}
