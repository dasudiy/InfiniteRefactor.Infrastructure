using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Common;
using AirMaster.Infrastructure.DataService.Internal;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.Net;
using AirMaster.Infrastructure.Serializer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Bindings.Http
{
    public class DataServiceHttpClient : DataServiceClientBase
    {
        public bool AllowCompress { get; set; }
        public DataServiceRouter DataServiceRouter { get; set; }
        public static DataServiceHttpClient Instance { get; set; }

        public Net.Http HttpClient { get; private set; }

        public static void Init(string dataserviceAddress, DataServiceRouter router = null, ResultExtractor extractor = null)
        {
            var client = new DataServiceHttpClient(dataserviceAddress);
            client.DataServiceRouter = router;
            client.ResultExtractor = extractor;
            Instance = client;
        }

        public DataServiceHttpClient(string url, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null, TimeSpan? timeout = null, string proxy = null)
            : base(url, certificate, validateCertificate, host, timeout, proxy)
        {
            this.DataServiceRouter = new DefaultServiceRouter();
            this.ResultExtractor = new ResultExtractor();
            this.AllowCompress = true;
            this.HttpClient = new Net.Http(null, proxy: proxy);
            this.HttpClient.UserAgent = "DataServiceClient/" + typeof(DataServiceHttpClient).GetTypeInfo().Assembly.GetName().Version.ToString();
            this.HttpClient.Timeout = timeout ?? TimeSpan.FromMinutes(1);
        }

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            var dict = GetParameters(paramsDict, routeInfo.FormatName);
            var url = GetUrl(routeInfo);

            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(HttpUtil.ToQueryString(dict, HttpClient.Encoding)),
                RequestUri = new Uri(url),
            };
            if (DefaultHeaders.Any())
            {
                foreach (var kvp in DefaultHeaders)
                {
                    msg.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            return HttpClient.Send(msg).ReadAsString();
        }

        public override Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            var dict = GetParameters(paramsDict, routeInfo.FormatName);
            var url = GetUrl(routeInfo);

            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(HttpUtil.ToQueryString(dict, HttpClient.Encoding), HttpClient.Encoding, "application/x-www-form-urlencoded"),
                RequestUri = new Uri(url),
            };
            if (DefaultHeaders.Any())
            {
                foreach (var kvp in DefaultHeaders)
                {
                    msg.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            return HttpClient.SendAsync(msg).ReadAsStringAsync();
        }

        protected string GetUrl(RouteInfo routeInfo)
        {
            var url = string.Format("{0}{1}", Url, DataServiceRouter.GetRouteUri(routeInfo));
            url += ((url.IndexOf('?') > 0) ? '&' : '?') + "timestamp=" + DateTime.Now.Ticks;
            return url;
        }

        protected Dictionary<string, object> GetParameters(Dictionary<string, object> paramsDict, string formatter = "json")
        {
            Dictionary<string, object> dict = null;
            if (paramsDict != null)
            {
                dict = paramsDict.ToDictionary(i => i.Key, i =>
                {
                    if (i.Value != null && i.Value.GetType() != typeof(string) && !i.Value.GetType().GetTypeInfo().IsValueType) { return SerializerFactory.Create(formatter).Serialize(i.Value); }
                    return i.Value;
                });
            }
            return dict;
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            var dict = GetParameters(paramsDict, routeInfo.FormatName);
            var url = GetUrl(routeInfo);

            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(HttpUtil.ToQueryString(dict, HttpClient.Encoding)),
                RequestUri = new Uri(url),
            };
            if (DefaultHeaders.Any())
            {
                foreach (var kvp in DefaultHeaders)
                {
                    msg.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            return HttpClient.Send(msg).ReadAsBytes();
        }
    }
}
