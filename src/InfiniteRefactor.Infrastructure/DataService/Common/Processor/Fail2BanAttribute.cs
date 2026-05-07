using System;
using System.Net;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using Microsoft.Extensions.Caching.Memory;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public class Fail2BanAttribute : PostProcessorAttribute, IPreProcessor
    {
        public int MaxRetry { get; set; } = 5;
        public int BanTime { get; set; } = 3600;
        public string ErrorMessage { get; set; } = "Access denied";

        public static MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

        public ProcessResult Process(DataServiceRequest request)
        {
            if (CheckIsBanned(request.Remote.Address))
            {
                return new ProcessResult { CancelProcess = true, Last = true, Message = ErrorMessage };
            }
            return ProcessResult.Default;
        }

        public Task<ProcessResult> ProcessAsync(DataServiceRequest request)
        {
            return Task.FromResult(Process(request));
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            var request = response.Context.Request;
            if (response.Exception != null)
            {
                RecordFailRequest(request.Remote.Address); ;
            }
            return ProcessResult.Default;
        }

        private class Record
        {
            public int Retried { get; set; }
            public bool Banned { get; set; }
            public DateTime BanTime { get; set; }
        }

        public void RecordFailRequest(IPAddress ip)
        {
            var key = ip.ToString();

            var record = memoryCache.GetOrCreate<Record>(key, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromSeconds(BanTime));
                return new Record
                {
                    Banned = false,
                    Retried = 0
                };
            });

            if (!record.Banned)
            {
                record.Retried++;
                if (record.Retried >= MaxRetry)
                {
                    record.Banned = true;
                    record.BanTime = DateTime.Now;
                }
            }
        }

        public bool CheckIsBanned(IPAddress ip)
        {
            var key = ip.ToString();
            if (memoryCache.TryGetValue(key, out Record result))
            {
                if (result.Banned)
                {
                    if (result.BanTime.Add(TimeSpan.FromSeconds(BanTime)) > DateTime.Now)
                    {
                        return true;
                    }
                    else
                    {
                        memoryCache.Remove(key);
                    }
                }
            }
            return false;
        }
    }
}
