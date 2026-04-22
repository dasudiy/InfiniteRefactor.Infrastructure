using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Metadata;
using System;
using System.Linq;

namespace AirMaster.Infrastructure.DataService.Common
{
    public class DefaultServiceRouter : DataServiceRouter
    {
        public override RouteInfo ReadRouteInfo(DataServiceRequest request)
        {
            var seg = request.Url.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (seg.Length < 2) { return null; }

            var action = seg.Last();
            var format = string.Empty;
            if (action.IndexOf(".") > 0)
            {
                var args = action.Split('.');
                action = args[0];
                format = args[1];
            }

            //return new RouteInfo { ServiceName = string.Join(".", seg.Take(seg.Length - 1).ToArray()), ActionName = action, FormatName = format };
            return new RouteInfo { ServiceName = seg[seg.Length - 2], ActionName = action, FormatName = format };
        }

        public override string GetRouteUri(RouteInfo info)
        {
            var url = "/" + info.ServiceName + "/" + info.ActionName;
            if (!string.IsNullOrWhiteSpace(info.FormatName))
            {
                url += "." + info.FormatName;
            }

            return url;
        }
    }

    public class QueryStringServiceRouter : DataServiceRouter
    {
        public string ServiceParamName { get; set; }
        public string ActionParamName { get; set; }
        public string FormatParamName { get; set; }

        public QueryStringServiceRouter(string serviceParamName = "service", string actionParamName = "action", string formatParamName = "format")
        {
            this.ServiceParamName = serviceParamName;
            this.ActionParamName = actionParamName;
            this.FormatParamName = formatParamName;
        }

        public override RouteInfo ReadRouteInfo(DataServiceRequest request)
        {
            return new RouteInfo
            {
                ServiceName = request.ReadParameter<string>(ServiceParamName, string.Empty),
                ActionName = request.ReadParameter<string>(ActionParamName, string.Empty),
                FormatName = request.ReadParameter<string>(FormatParamName, string.Empty)
            };
        }

        public override string GetRouteUri(RouteInfo info)
        {
            var url = string.Format("?{0}={1}&{2}={3}", ServiceParamName, info.ServiceName, ActionParamName, info.ActionName);
            if (!string.IsNullOrWhiteSpace(info.FormatName))
            {
                url += string.Format("&{0}={1}", FormatParamName, info.FormatName);
            }
            return url;
        }
    }
}
