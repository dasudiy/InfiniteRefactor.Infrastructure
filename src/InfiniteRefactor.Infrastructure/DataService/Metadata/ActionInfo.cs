using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using InfiniteRefactor.Infrastructure.DataService.Annotations;
using InfiniteRefactor.Infrastructure.DataService.Internal;
using InfiniteRefactor.Infrastructure.Serializer;

namespace InfiniteRefactor.Infrastructure.DataService.Metadata
{
    public class ActionInfo
    {
        public string Name { get; set; }
        public string RawName { get; set; }
        public List<ParamInfo> Parameters { get; private set; }
        public bool Async { get; set; }
        internal MethodInfo MethodInfo { get; private set; }

        public string Summary { get; internal set; }
        public string Description { get; internal set; }
        public List<IPreProcessor> PreProcessors { get; private set; }
        public List<IPostProcessor> PostProcessors { get; private set; }

        public ActionInfo(MethodInfo methodInfo, IEnumerable<IPreProcessor> preprocessor, IEnumerable<IPostProcessor> postprocessor)
        {
            MethodInfo = methodInfo;
            RawName = Name = MethodInfo.Name;
            var attr = methodInfo.GetCustomAttribute<DataServiceMethodAttribute>();
            if (attr != null)
            {
                Name = attr.Name ?? Name;
            }

            Parameters = new List<ParamInfo>();
            foreach (var item in methodInfo.GetParameters())
            {
                var param = new ParamInfo(attr, item);
                Parameters.Add(param);
            }

            Summary = attr.Summary;
            Description = attr.Description;

            var processors = methodInfo.GetCustomAttributes<ProcessorAttribute>();
            var tmpPreProcessors = new List<IPreProcessor>();
            tmpPreProcessors.AddRange(preprocessor);
            tmpPreProcessors.AddRange(processors.Where(a => a is IPreProcessor).Cast<IPreProcessor>());
            PreProcessors = tmpPreProcessors.OrderByDescending(p => p.Priority).ToList();

            var tmpPostProcessors = new List<IPostProcessor>();
            tmpPostProcessors.AddRange(postprocessor);
            tmpPostProcessors.AddRange(processors.Where(a => a is IPostProcessor).Cast<IPostProcessor>());
            PostProcessors = tmpPostProcessors.OrderByDescending(p => p.Priority).ToList();
        }

        public void Invoke(DataServiceContext context)
        {
            if (DataServiceHost.PreProcessContext(context, PreProcessors))
            {
                try
                {
                    var outputParameters = new Dictionary<int, string>();
                    object[] param = new object[Parameters.Count];
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        var parameter = Parameters[i];
                        if (parameter.IsOut)
                        {
                            outputParameters.Add(i, parameter.Name);
                            continue;
                        }

                        param[i] = parameter.ValueReader.ReadValue(parameter, context.Request);
                        //context.Request.ReadParameter(parameter);
                    }

                    try
                    {
                        var result = Invoke(context.ServiceInstance, param);
                        var outputParameter = outputParameters.Select(p => new KeyValuePair<string, object>(p.Value, param[p.Key])).ToDictionary(f => f.Key, f => f.Value);
                        context.Response.Result = context.Response.RawResult = result;

                        foreach (var item in outputParameter)
                        {
                            context.Response.OutputParams[item.Key] = item.Value;
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        DataServiceHost.Log.Error(ex.InnerException, $"调用{LogSanitizer.Sanitize(context.RouteInfo.ToString())}失败{LogSanitizer.Sanitize(ex.InnerException.Message)},参数:{LogSanitizer.Sanitize(SerializerFactory.Serialize("json", param))}");
                        context.Response.Exception = ex.InnerException;
                    }
                }
                catch (Exception ex)
                {
                    DataServiceHost.Log.Error(ex, $"调用{LogSanitizer.Sanitize(context.RouteInfo.ToString())}失败{LogSanitizer.Sanitize(ex.Message)}");
                    context.Response.Exception = ex;
                }
            }
            DataServiceHost.PostProcessContext(context, PostProcessors);
        }

        public async Task InvokeAsync(DataServiceContext context)
        {
            if (await DataServiceHost.PreProcessContextAsync(context, PreProcessors))
            {
                try
                {
                    var outputParameters = new Dictionary<int, string>();
                    object[] param = new object[Parameters.Count];
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        var parameter = Parameters[i];
                        if (parameter.IsOut)
                        {
                            outputParameters.Add(i, parameter.Name);
                            continue;
                        }

                        param[i] = await parameter.ValueReader.ReadValueAsync(parameter, context.Request);
                        //context.Request.ReadParameter(parameter);
                    }

                    try
                    {
                        var result = await InvokeAsync(context.ServiceInstance, param);
                        var outputParameter = outputParameters.Select(p => new KeyValuePair<string, object>(p.Value, param[p.Key])).ToDictionary(f => f.Key, f => f.Value);
                        context.Response.Result = context.Response.RawResult = result;

                        foreach (var item in outputParameter)
                        {
                            context.Response.OutputParams[item.Key] = item.Value;
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        DataServiceHost.Log.Error(ex.InnerException, $"调用{LogSanitizer.Sanitize(context.RouteInfo.ToString())}失败{LogSanitizer.Sanitize(ex.InnerException.Message)},参数:{LogSanitizer.Sanitize(SerializerFactory.Serialize("json", param))}");
                        context.Response.Exception = ex.InnerException;
                    }
                }
                catch (Exception ex)
                {
                    DataServiceHost.Log.Error(ex, $"调用{LogSanitizer.Sanitize(context.RouteInfo.ToString())}失败{LogSanitizer.Sanitize(ex.Message)}");
                    context.Response.Exception = ex;
                }
            }
            await DataServiceHost.PostProcessContextAsync(context, PostProcessors);

        }

        public object Invoke(object instance, params object[] parameters)
        {
            //return AirMaster.Infrastructure.Utilities.EmitHelper.Run(MethodInfo, instance, parameters);
            return MethodInfo.Invoke(instance, parameters);
        }

        public async Task<object> InvokeAsync(object instance, params object[] parameters)
        {
            var ret = MethodInfo.Invoke(instance, parameters);
            if (typeof(Task).IsAssignableFrom(MethodInfo.ReturnType))
            {
                var task = (ret as Task);
                await task;
                if (MethodInfo.ReturnType.IsGenericType)
                {
                    var resultProperty = typeof(Task<>).MakeGenericType(MethodInfo.ReturnType.GenericTypeArguments[0]).GetProperty("Result");
                    return resultProperty.GetValue(task);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return ret;
            }
        }
    }
}
