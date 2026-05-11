using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using InfiniteRefactor.Infrastructure.DataService.Annotations;
using InfiniteRefactor.Infrastructure.DataService.Common;
using InfiniteRefactor.Infrastructure.DataService.Common.Activator;
using InfiniteRefactor.Infrastructure.DataService.Internal;
using InfiniteRefactor.Infrastructure.DataService.Metadata;
using InfiniteRefactor.Infrastructure.Serializer;
using InfiniteRefactor.Infrastructure.Utilities;
using NLog;

namespace InfiniteRefactor.Infrastructure.DataService
{
    public sealed class DataServiceHost
    {
        private readonly ConcurrentDictionary<int, DataServiceClientBase> activeClients = new();
        internal Dictionary<string, ServiceInfo> Services { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 服务名称，用于文档生成
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 服务版本，用于文档生成
        /// </summary>
        public string Version { get; set; }

        public static readonly DataServiceHost Instance = new();
        public DataServiceRouter Router { get; } = new DefaultServiceRouter();
        public List<IPreProcessor> PreProcessors { get; } = new();
        public List<IPostProcessor> PostProcessors { get; } = new();
        public IEnumerable<string> RawServiceNames => Services.Select(s => s.Value.RawName);
        public IEnumerable<string> ServiceNames => Services.Select(s => s.Value.Name);
        public IReadOnlyList<DataServiceClientBase> ActiveClients => activeClients.Values.ToList().AsReadOnly();
        public ISerializer DefaultSerializer { get; set; } = SerializerFactory.GetDefault();
        public string Name { get; set; } = Assembly.GetEntryAssembly()!.GetName().Name;

        public event EventHandler<SimpleArgument<object>> BeforeSendMessage;
        public event EventHandler<SimpleArgument<object>> BeforeReceiveMessage;
        public event EventHandler<SimpleArgument<DataServiceClientBase>> ClientConnected;
        public event EventHandler<SimpleArgument<DataServiceClientBase>> ClientDisconnected;


        public Dictionary<string, Type> FindDataservicesFromAssembly(Assembly assembly)
        {
            var services = new Dictionary<string, Type>();

            foreach (var type in assembly.GetTypes())
            {
                try
                {
                    var serviceInterface = type.GetInterfaces()
                        .FirstOrDefault(x => x.GetCustomAttribute<DataServiceAttribute>() != null);

                    var ret = serviceInterface == null ? AddService(type) : AddService(serviceInterface, type);
                    if (ret.HasValue)
                    {
                        services.Add(ret.Value.Key, ret.Value.Value);
                    }
                    //AddSerializer(type);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "加载Service失败");
                }
            }

            return services;
        }

        public KeyValuePair<string, Type>? AddService(Type serviceType)
        {
            if (serviceType.GetCustomAttribute<DataServiceAttribute>() != null)
            {
                var service = new ServiceInfo(serviceType, DefaultServiceCreator.Instance, PreProcessors,
                    PostProcessors);

                Services[service.Name] = service;
                return new KeyValuePair<string, Type>(service.Name, serviceType);
            }

            return null;
        }

        public KeyValuePair<string, Type>? AddService(Type contractType, Type serviceType)
        {
            if (contractType.GetCustomAttribute<DataServiceAttribute>() != null)
            {
                var service = new ServiceInfo(contractType,
                    new InterfaceServiceInstanceCreator(contractType, serviceType) { Singleton = true }, PreProcessors,
                    PostProcessors);
                var processors = serviceType.GetCustomAttributes<ProcessorAttribute>();
                IEnumerable<ProcessorAttribute> processorAttributes =
                    processors as ProcessorAttribute[] ?? processors.ToArray();
                service.PreProcessors.AddRange(processorAttributes.Where(a => a is IPreProcessor)
                    .Cast<IPreProcessor>());
                service.PostProcessors.AddRange(processorAttributes.Where(a => a is IPostProcessor)
                    .Cast<IPostProcessor>());


                //var service = new ServiceInfo(contractType, new SingletonServiceInstanceRetriver());
                Services[service.Name] = service;
                return new KeyValuePair<string, Type>(service.Name, contractType);
            }

            return null;
        }

        public KeyValuePair<string, Type>? AddService(Type contractType, object serviceInstance)
        {
            var serviceType = serviceInstance.GetType();
            if (contractType.GetCustomAttribute<DataServiceAttribute>() != null)
            {
                var service = new ServiceInfo(contractType,
                    new InterfaceServiceInstanceCreator(contractType, serviceInstance) { Singleton = true },
                    PreProcessors, PostProcessors);

                var processors = serviceType.GetCustomAttributes<ProcessorAttribute>();
                IEnumerable<ProcessorAttribute> processorAttributes =
                    processors as ProcessorAttribute[] ?? processors.ToArray();
                service.PreProcessors.AddRange(processorAttributes.Where(a => a is IPreProcessor)
                    .Cast<IPreProcessor>());
                service.PostProcessors.AddRange(processorAttributes.Where(a => a is IPostProcessor)
                    .Cast<IPostProcessor>());

                if (contractType.BaseType != null && contractType.BaseType != typeof(object))
                {
                    AddService(contractType.BaseType, serviceInstance);
                }

                foreach (var ifType in contractType.GetInterfaces())
                {
                    AddService(ifType, serviceInstance);
                }

                //var service = new ServiceInfo(contractType, new SingletonServiceInstanceRetriver());
                Services[service.Name] = service;
                return new KeyValuePair<string, Type>(service.Name, contractType);
            }

            return null;
        }

        public void RemoveService(string name)
        {
            if (Services.ContainsKey(name))
            {
                Services.Remove(name);
            }
        }

        internal ActionInfo GetAction(RouteInfo routeInfo)
        {
            if (!routeInfo.IsValid(Services))
            {
                return null;
            }

            return Services[routeInfo.ServiceName].Actions[routeInfo.ActionName];
        }

        internal ServiceInfo GetService(RouteInfo routeInfo)
        {
            if (!routeInfo.IsValid(Services))
            {
                return null;
            }

            return Services[routeInfo.ServiceName];
        }

        public void ProcessContext(DataServiceContext context)
        {
            try
            {
                context.DataServiceHost = this;
                context.MoveToThread();

                context.Serializer = DefaultSerializer;
                var info = context.RouteInfo;
                if (info != null)
                {
                    if (!string.IsNullOrWhiteSpace(info.FormatName))
                    {
                        context.Serializer = SerializerFactory.Create(info.FormatName);
                    }

                    context.Response.ContentType = context.Serializer.ContentType;
                }

                if (info == null || !info.IsValid(Services))
                {
                    PreProcessContext(context, this.PreProcessors);
                    context.Response.Exception = new DataServiceException("无此方法！")
                        { Source = this.GetType().Name, StatusCode = System.Net.HttpStatusCode.NotFound };
                    PostProcessContext(context, this.PostProcessors);
                }
                else
                {
                    var act = GetAction(info);
                    if (context.IsBatchContext)
                    {
                        var result = context.GetBatchContexts().AsParallel().AsOrdered().Select(cnx =>
                        {
                            cnx.MoveToThread();
                            try
                            {
                                cnx.ServiceInstance = Services[info.ServiceName].GetInstance();
                                cnx.ServiceInstanceRequireDispose = Services[info.ServiceName].Creator.RequireDispose;
                                act.Invoke(cnx);

                                return cnx.GetResult();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"调用{LogSanitizer.Sanitize(info.ToString())}失败");
                                return ex.Message;
                            }
                            finally
                            {
                                cnx.Dispose();
                            }
                        }).ToArray();

                        context.Response.Result = result;
                    }
                    else
                    {
                        context.ServiceInstance = Services[info.ServiceName].GetInstance();
                        context.ServiceInstanceRequireDispose = Services[info.ServiceName].Creator.RequireDispose;

                        act.Invoke(context);
                        if (act.Async)
                        {
                            //ignore response;
                            return;
                        }
                    }
                }

                context.Response.WriteResult();
            }
            finally
            {
                context.Dispose();
            }
        }

        /// <summary>
        /// 仅当dataservice方法为异步方法时才进行异步执行
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessContextAsync(DataServiceContext context)
        {
            try
            {
                context.DataServiceHost = this;
                context.MoveToThread();

                context.Serializer = DefaultSerializer;
                var info = context.RouteInfo;
                if (info != null)
                {
                    if (!string.IsNullOrWhiteSpace(info.FormatName))
                    {
                        context.Serializer = SerializerFactory.Create(info.FormatName);
                    }

                    context.Response.ContentType = context.Serializer.ContentType;
                }

                if (info == null || !info.IsValid(Services))
                {
                    await PreProcessContextAsync(context, this.PreProcessors);
                    context.Response.Exception = new DataServiceException("无此方法！")
                        { Source = this.GetType().Name, StatusCode = System.Net.HttpStatusCode.NotFound };
                    await PostProcessContextAsync(context, this.PostProcessors);
                }
                else
                {
                    var act = GetAction(info);
                    if (context.IsBatchContext)
                    {
                        var result = context.GetBatchContexts().AsParallel().AsOrdered().Select(async cnx =>
                        {
                            cnx.MoveToThread();
                            try
                            {
                                cnx.ServiceInstance = Services[info.ServiceName].GetInstance();
                                cnx.ServiceInstanceRequireDispose = Services[info.ServiceName].Creator.RequireDispose;
                                await act.InvokeAsync(cnx);

                                return cnx.GetResult();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"调用{LogSanitizer.Sanitize(info.ToString())}失败");
                                return ex.Message;
                            }
                            finally
                            {
                                cnx.Dispose();
                            }
                        });
                        await Task.WhenAll(result);
                        context.Response.Result = result.Select(t => t.Result).ToArray();
                    }
                    else
                    {
                        context.ServiceInstance = Services[info.ServiceName].GetInstance();
                        context.ServiceInstanceRequireDispose = Services[info.ServiceName].Creator.RequireDispose;

                        await act.InvokeAsync(context);
                        if (act.Async)
                        {
                            //ignore response;
                            return;
                        }
                    }
                }

                await context.Response.WriteResultAsync();
            }
            finally
            {
                context.Dispose();
            }
        }

        internal static bool PreProcessContext(DataServiceContext context, List<IPreProcessor> preProcessor)
        {
            foreach (var item in preProcessor)
            {
                try
                {
                    var result = item.Process(context.Request);
                    if (result.CancelProcess)
                    {
                        if (!string.IsNullOrWhiteSpace(result.Message))
                        {
                            context.Response.Exception = new DataServiceException(result.Message)
                                { Source = result.SourceName };
                        }

                        return false;
                    }

                    if (result.Last)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    ex.Source = item.GetType().Name;
                    context.Response.Exception = ex;
                    return false;
                }
            }

            return true;
        }

        internal static void PostProcessContext(DataServiceContext context, List<IPostProcessor> postProcessor)
        {
            foreach (var item in postProcessor)
            {
                try
                {
                    var result = item.Process(context.Response);
                    if (result.Last)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    ex.Source = item.GetType().Name;
                    context.Response.Exception = ex;
                }
            }
            //return true;
        }

        internal static async Task<bool> PreProcessContextAsync(DataServiceContext context,
            List<IPreProcessor> preProcessor)
        {
            foreach (var item in preProcessor)
            {
                try
                {
                    var result = await item.ProcessAsync(context.Request);
                    if (result.CancelProcess)
                    {
                        if (!string.IsNullOrWhiteSpace(result.Message))
                        {
                            context.Response.Exception = new DataServiceException(result.Message)
                                { Source = result.SourceName };
                        }

                        return false;
                    }

                    if (result.Last)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    ex.Source = item.GetType().Name;
                    context.Response.Exception = ex;
                    return false;
                }
            }

            return true;
        }

        internal static async Task PostProcessContextAsync(DataServiceContext context,
            List<IPostProcessor> postProcessor)
        {
            foreach (var item in postProcessor)
            {
                try
                {
                    var result = await item.ProcessAsync(context.Response);
                    if (result.Last)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    ex.Source = item.GetType().Name;
                    context.Response.Exception = ex;
                }
            }
            //return true;
        }

        internal void ConnectionClosed(DataServiceClientBase client)
        {
            if (!activeClients.TryRemove(client.GetHashCode(), out _)) return;

            ClientDisconnected?.Invoke(this, SimpleArgument.Create(client));
            Log.Info($"client {client.Url} disconnected...");
        }

        internal void NewConnection(DataServiceClientBase client)
        {
            if (!activeClients.TryAdd(client.GetHashCode(), client)) return;
            ClientConnected?.Invoke(this, SimpleArgument.Create(client));

            Log.Info($"client {client.Url} connected...");
        }

        internal void OnBeforeSendMessage(object message)
        {
            BeforeSendMessage?.Invoke(this, new SimpleArgument<object>(message));
        }

        internal void OnBeforeReceiveMessage(object message)
        {
            BeforeReceiveMessage?.Invoke(this, new SimpleArgument<object>(message));
        }
    }
}