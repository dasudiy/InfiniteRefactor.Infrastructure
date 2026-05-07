using System;
using System.Collections.Generic;
using System.Threading;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using NLog;

namespace InfiniteRefactor.Infrastructure.DataService.Common
{
    public class EventLimitAttribute : PreProcessorAttribute
    {
        Queue<DateTime> requestTimes;
        int maxRequests;
        TimeSpan timeSpan;
        private int maxQueueSize = -1;
        int waitQueueSize = 0;
        int completeQueueSize = 0;

        public EventLimitAttribute(int maxRequests, int seconds, int maxQueueSize)
        {
            this.maxRequests = maxRequests;
            this.timeSpan = TimeSpan.FromSeconds(seconds);
            requestTimes = new Queue<DateTime>(maxRequests);
            this.maxQueueSize = maxQueueSize;
        }

        private void SynchronizeQueue()
        {
            while ((requestTimes.Count > 0) && (requestTimes.Peek().Add(timeSpan) < DateTime.UtcNow))
                requestTimes.Dequeue();
        }

        public bool CanRequestNow()
        {
            lock (this)
            {
                SynchronizeQueue();
                return requestTimes.Count < maxRequests;
            }
        }

        private int requestId = 0;
        private Logger log = LogManager.GetCurrentClassLogger();
        public void EnqueueRequest()
        {
            var id = Interlocked.Increment(ref requestId);
            try
            {
                Interlocked.Increment(ref waitQueueSize);
                if (maxQueueSize > 0 && (waitQueueSize > maxQueueSize))
                {
                    throw new Exception($"超过等待队列{maxQueueSize}");
                }
                lock (this)
                {
                    while (!CanRequestNow())
                    {
                        var time = requestTimes.Peek().Add(timeSpan).Subtract(DateTime.UtcNow);
                        if (time > TimeSpan.Zero)
                        {
                            Thread.Sleep(time);
                        }
                    }

                    // Was: System.Threading.Thread.Sleep(1000);

                    requestTimes.Enqueue(DateTime.UtcNow);
                }

            }
            finally
            {
                Interlocked.Decrement(ref waitQueueSize);
            }
        }

        public override ProcessResult Process(DataServiceRequest request)
        {
            EnqueueRequest();
            return ProcessResult.Default;
        }
    }
}
