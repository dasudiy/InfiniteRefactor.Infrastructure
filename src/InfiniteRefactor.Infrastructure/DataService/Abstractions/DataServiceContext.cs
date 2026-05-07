using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using InfiniteRefactor.Infrastructure.DataService.Bindings.BatchQuery;
using InfiniteRefactor.Infrastructure.DataService.Metadata;
using InfiniteRefactor.Infrastructure.Serializer;
using InfiniteRefactor.Infrastructure.Session;
using NLog;

namespace InfiniteRefactor.Infrastructure.DataService.Abstractions
{
    public abstract class DataServiceContext : IDisposable
    {
        #region Static Members

        private static AsyncLocal<DataServiceContext> _asyncContext = new AsyncLocal<DataServiceContext>();

        public static DataServiceContext Current
        {
            get { return _asyncContext.Value; }
        }

        #endregion

        protected DataServiceContext()
        {
        }

        private readonly Stopwatch stopwatch = new();

        private readonly Lazy<Logger> log = new(LogManager.GetCurrentClassLogger);

        public virtual bool IsBatchContext
        {
            get
            {
                return Request.ReadParameter<bool>("DSBatch", false) &&
                       Request.ReadParameter<int>("DSBatchCount", 0) > 0;
            }
        }

        public IEnumerable<BatchQueryContext> GetBatchContexts()
        {
            for (int i = 0; i < Request.ReadParameter<int>("DSBatchCount", 0); i++)
            {
                yield return new BatchQueryContext(this, Request.ReadParameter<string>("DSArgument" + i, string.Empty))
                    { Serializer = this.Serializer };
            }
        }

        public abstract DataServiceRequest Request { get; }
        public abstract DataServiceResponse Response { get; }
        public abstract ApplicationSession Session { get; }
        public abstract DataServiceClientBase Client { get; }
        public Logger Log => log.Value;

        public virtual string Identity
        {
            get { return this?.Session?.User?.Username; }
        }

        public Dictionary<string, object> ServerParameters { get; private set; }
        public ISerializer Serializer { get; internal set; }
        public DataServiceHost DataServiceHost { get; internal set; }
        public object ServiceInstance { get; internal set; }

        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public RouteInfo RouteInfo => router.ReadRouteInfo(this.Request);

        private readonly DataServiceRouter router;

        protected DataServiceContext(DataServiceRouter router)
        {
            this.ServerParameters = new Dictionary<string, object>();
            stopwatch.Start();
            this.router = router;
        }

        public void Dispose()
        {
            if (ServiceInstance != null && ServiceInstance is IDisposable && ServiceInstanceRequireDispose)
            {
                ((IDisposable)ServiceInstance).Dispose();
            }

            stopwatch.Stop();
            Request.Dispose();
            Response.Dispose();
        }

        public long GetElpsedTime()
        {
            return stopwatch.ElapsedMilliseconds;
        }

        internal void MoveToThread()
        {
            _asyncContext.Value = this;
        }

        public bool ServiceInstanceRequireDispose { get; set; }

        private static readonly ConcurrentDictionary<int, DataServiceClientBase> Dict = new();

        public static IReadOnlyList<DataServiceClientBase> ActiveClients => Dict.Values.ToList().AsReadOnly();

        internal static void ConnectionClosed(DataServiceClientBase client)
        {
            Dict.TryRemove(client.GetHashCode(), out _);
        }

        internal static void NewConnection(DataServiceClientBase client)
        {
            Dict.TryAdd(client.GetHashCode(), client);
        }
    }
}