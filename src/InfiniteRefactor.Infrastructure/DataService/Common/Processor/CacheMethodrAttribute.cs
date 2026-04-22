using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    public class CacheMethod : ProcessorAttribute, IPreProcessor, IPostProcessor
    {
        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        public string[] ByParameters { get; set; }
        public bool ByUser { get; set; }
        int IHavePriority.Priority { get; set; }

        public double CacheSeconds { get; set; }

        public CacheMethod()
        {
            CacheSeconds = 60;
            ByParameters = new string[] { };
        }

        ProcessResult IPostProcessor.Process(DataServiceResponse response)
        {
            if (!response.Context.ServerParameters.ContainsKey("FROMCACHE") || (bool)response.Context.ServerParameters["FROMCACHE"] != true)
            {
                Task.Run(() =>
                {
                    if (response.Exception == null)
                    {
                        var key = GetKey(response.Context);
                        var result = response.RawResult;
                        cache.Set(key, result, DateTime.Now.AddSeconds(CacheSeconds));
                    }
                });
            }
            return ProcessResult.Default;
        }

        ProcessResult IPreProcessor.Process(DataServiceRequest request)
        {
            var key = GetKey(request.Context);
            object response;
            if (!cache.TryGetValue(key, out response))
            {
                return ProcessResult.Default;
            }
            else
            {
                request.Context.Response.RawResult = request.Context.Response.Result = response;
                request.Context.ServerParameters["FROMCACHE"] = true;

                return new ProcessResult { Last = true, CancelProcess = true, SourceName = this.GetType().FullName };
            }
        }

        private string GetKey(DataServiceContext context)
        {
            var param = string.Join(",", ByParameters.Select(p => context.Serializer.Serialize(context.Request.ReadParameter(p, typeof(object)))));
            var key = context.RouteInfo.ToString() + "$" + param;
            if (ByUser && context.Session != null && context.Session.User != null) { key = key + "$" + context.Session.User.Username; }
            return "DataServiceCacheMethod" + key;
        }

        public Task<ProcessResult> ProcessAsync(DataServiceRequest request)
        {
            return Task.FromResult(((IPreProcessor)this).Process(request));
        }

        public Task<ProcessResult> ProcessAsync(DataServiceResponse response)
        {
            return Task.FromResult(((IPostProcessor)this).Process(response));
        }
    }
}
