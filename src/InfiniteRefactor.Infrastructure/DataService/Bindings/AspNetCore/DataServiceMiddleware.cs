using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using AirMaster.Infrastructure.DataService.Bindings.AspNet;
using AirMaster.Infrastructure.DataService.Bindings.AspNet.EncryptionContext;
using AirMaster.Infrastructure.DataService.Common.Processor;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.Extensions;
using AirMaster.Infrastructure.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NLog;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNetCore
{
    public static class DataServiceMiddlewareExtensions
    {
        public static IApplicationBuilder UseDataService(
            this IApplicationBuilder builder, DataServiceOption options, DataServiceHost host = null)
        {
            return builder.UseMiddleware<DataServiceMiddleware>(Options.Create(options),
                host ?? DataServiceHost.Instance);
        }
    }

    public class DataServiceMiddleware
    {
        private readonly RequestDelegate next;
        private readonly DataServiceOption option;
        private readonly DataServiceHost host;

        public DataServiceMiddleware(RequestDelegate next, IOptions<DataServiceOption> options, DataServiceHost host)
        {
            this.next = next;
            option = options.Value;
            this.host = host;

            //暂时注释了，初始化放到initfn里面比较灵活点
            if (option.UseRestApi)
            {
                var apiprocessor = new RestApiProcessor();
                host.PreProcessors.Add(apiprocessor);
                host.PostProcessors.Add(apiprocessor);
                host.DefaultSerializer = SerializerFactory.Create("json2");
            }

            option.InitFn(host);

            if (option.EnableMetadataService)
            {
                this.host.AddService(typeof(MetadataService));
            }
            //if (!host.PostProcessors.Any(t => t is ResultWrapperAttribute))
            //{
            //    host.PostProcessors.Add(new ResultWrapperAttribute() { Priority = -1, DirectOutputParameterToResult = false });
            //}
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(option.WebSocketPath.HasValue
                    ? option.WebSocketPath
                    : option.RequestPath))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                    var addr = context.Connection.RemoteIpAddress;
                    var port = context.Connection.RemotePort;

                    var tcs = new TaskCompletionSource<object>();
                    var connection =
                        new WebSocketConnection(host, ws, new IPEndPoint(addr, port), option.WebsocketTimeout);
                    this.host.NewConnection(connection);
                    connection.ConnectionClosed += (sender, _) =>
                    {
                        tcs.SetResult(true);
                        connection.Dispose();
                        this.host.ConnectionClosed(connection);
                    };
                    await tcs.Task;
                }
                else if (context.Request.Path.StartsWithSegments(option.RequestPath))
                {
                    DataServiceContext dsContext = context.Request.Headers.ContainsKey("APPID")
                        ? new EncryptionHttpContext(context)
                        : new AspNetDataServiceContext(context);
                    await host.ProcessContextAsync(dsContext);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                // Call the next delegate/middleware in the pipeline
                await next(context);
            }
        }
    }

    public class RestApiProcessor : IPreProcessor, IPostProcessor
    {
        public int Priority { get; set; }

        private readonly MethodInfo toList;

        public int MaxResult { get; set; }

        public RestApiProcessor()
        {
            var enumerableMethods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            toList = enumerableMethods.FirstOrDefault(m => m.Name == "ToList" && m.GetParameters().Length == 1);
            //DirectOutputParameterToResult = true;
            MaxResult = 100;
            Priority = 1;
        }

        #region IPreProcessor

        ProcessResult IPreProcessor.Process(DataServiceRequest request)
        {
            if (request.RawRequestObject is HttpRequest req)
            {
                if (req.ContentType != null && req.ContentType.Contains("application/json"))
                {
                    JsonDocument payload = (JsonDocument)SerializerFactory.Create("json2")
                        .Deserialize(req.Body, typeof(JsonDocument));
                    request.ParameterCache["payload"] = payload;
                    if (payload != null)
                    {
                        foreach (var item in payload.RootElement.EnumerateObject())
                        {
                            request.ParameterCache.Add(item.Name, item.Value);
                        }
                    }
                }
            }

            return ProcessResult.Default;
        }

        public async Task<ProcessResult> ProcessAsync(DataServiceRequest request)
        {
            if (request.RawRequestObject is HttpRequest req)
            {
                if (req.ContentType != null && req.ContentType.Contains("application/json"))
                {
                    JsonDocument payload = (JsonDocument)await SerializerFactory.Create("json2")
                        .DeserializeAsync(req.Body, typeof(JsonDocument));
                    request.ParameterCache["payload"] = payload;
                    if (payload != null)
                    {
                        foreach (var item in payload.RootElement.EnumerateObject())
                        {
                            request.ParameterCache.Add(item.Name, item.Value);
                        }
                    }
                }
            }

            return ProcessResult.Default;
        }

        #endregion

        #region IPostProcessor

        ProcessResult IPostProcessor.Process(DataServiceResponse response)
        {
            if (response.Context.Serializer is SimpleSerializer)
            {
                return ProcessResult.Default;
            }

            var result = new Dictionary<string, object> { { "timestamp", DateTime.Now.GetUnixTime().To<int>() } };
            var context = response.Context;
            if (response.Result != null)
            {
                try
                {
                    var applyLimit = true;
                    if (context.ServerParameters.TryGetValue("ApplyLimit", out object value))
                    {
                        applyLimit = value.To<bool>();
                    }

                    result["result"] = ResultWrapperAttribute.ExecuteQuery(response.Result, MaxResult, applyLimit);
                }
                catch (Exception ex)
                {
                    ex.Source = this.GetType().Name;
                    response.Exception = ex;
                }
            }
            else
            {
                result["result"] = null;
            }

            result["code"] = response.Exception == null ? 0 : -1;
            result["success"] = response.Exception == null;

            if (response.Exception != null)
            {
                var guid = Guid.NewGuid().ToString("n");
                var serviceTypeName = (response.Context.ServiceInstance ?? this).GetType().FullName;
                if (serviceTypeName != null)
                    LogManager.GetLogger(serviceTypeName)
                        .Error(response.Exception, "DataService error: " + guid);

                if (response.Exception is DataServiceException ex)
                {
                    result["message"] = response.Exception.Message;
                    result["errors"] = ResultWrapperAttribute.ProcessDataServiceException(ex, response.Context.DataServiceHost.Name);
                    if (response.Context.Response.RawResponseObject is HttpResponse rawResponse)
                    {
                        rawResponse.StatusCode = (int)ex.StatusCode;
                    }
                }
                else if (context.IsDebug)
                {
                    result["errors"] = ResultWrapperAttribute.ProcessException(response.Exception);
                    result["message"] = response.Exception.Message;
                    if (response.Context.Response.RawResponseObject is HttpResponse rawResponse)
                    {
                        rawResponse.StatusCode = 500;
                    }
                }
                else
                {
                    result["message"] = $"Server error, reference code: {guid}";
                    if (response.Context.Response.RawResponseObject is HttpResponse rawResponse)
                    {
                        rawResponse.StatusCode = 500;
                    }
                }

                response.Exception = null;
            }

            if (response.OutputParams != null)
            {
                // if (false)
                // {
                //     foreach (var item in response.OutputParams)
                //     {
                //         result[item.Key] = item.Value;
                //     }
                // }
                // else
                {
                    result["output"] = response.OutputParams;
                }
            }

            if (response.Context.IsDebug)
            {
                try
                {
                    ResultWrapperAttribute.WriteDebugInfo(result, response.Context);
                }
                catch
                {
                    // ignored
                }
            }

            response.Result = result;
            return ProcessResult.Default;
        }

        Task<ProcessResult> IPostProcessor.ProcessAsync(DataServiceResponse response)
        {
            return Task.FromResult(((IPostProcessor)this).Process(response));
        }

        #endregion
    }

    public class DataServiceOption
    {
        public PathString RequestPath { get; set; }
        public PathString WebSocketPath { get; set; }
        public Action<DataServiceHost> InitFn { get; set; }
        public bool EnableMetadataService { get; set; }
        public bool UseRestApi { get; set; } = true;
        public TimeSpan WebsocketTimeout { get; set; } = TimeSpan.FromMinutes(1);
    }
}