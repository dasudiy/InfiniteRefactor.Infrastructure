//using AirMaster.Infrastructure.DataService.Attributes;
//using AirMaster.Infrastructure.DataService.Implement;
//using AirMaster.Infrastructure.Extensions;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.Remoting.Messaging;
//using System.Runtime.Remoting.Proxies;
//using System.Security.Permissions;

//namespace AirMaster.Infrastructure.DataService.Client
//{
//    //http://www.neovolve.com/post/2010/07/17/Creating-proxies-with-RealProxy.aspx
//    internal class DataServiceProxy<T> : RealProxy
//        where T : class
//    {
//        private DataServiceClientBase _client;
//        public string Formatter { get; set; }
//        //public string UrlAddress { get; set; }
//        //public DataServiceRouter Router { get; set; }
//        public DataServiceProxy(DataServiceClientBase client)
//            : base(typeof(T))
//        {
//            this._client = client;
//        }

//        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
//        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
//        {
//            IMethodCallMessage message = msg as IMethodCallMessage;
//            String methodName = (String)msg.Properties["__MethodName"];
//            Type[] parameterTypes = (Type[])msg.Properties["__MethodSignature"];
//            Object[] parameters = (Object[])msg.Properties["__Args"];
//            string typeName = (string)msg.Properties["__TypeName"];

//            if (methodName == "GetHashCode")
//            {
//                return new ReturnMessage(this.GetHashCode(), null, 0, null, message);
//            }
//            if (methodName == "Equals")
//            {
//                var ret = this.Equals(parameters[0]);
//                return new ReturnMessage(ret, null, 0, null, message);
//            }
//            if (methodName == "GetType")
//            {
//                return new ReturnMessage(this.GetType(), null, 0, null, message);
//            }

//            var serviceType = Type.GetType(typeName);
//            var dataServiceAttribute = serviceType.GetCustomAttribute<DataServiceAttribute>();
//            if (dataServiceAttribute == null) { throw new Exception("此类型未标记为DataService!"); }

//            MethodInfo method = typeof(T).GetMethod(methodName, parameterTypes, true);
//            var methodAttribute = method.GetCustomAttribute<DataServiceMethodAttribute>();
//            if (methodAttribute == null) { throw new Exception("此方法未标记为DataServiceMethod!"); }

//            var paramInfos = method.GetParameters();
//            var paremsDict = new Dictionary<string, object>();
//            for (int i = 0; i < parameters.Length; i++)
//            {
//                var item = paramInfos[i];
//                var parameterAttribute = item.GetCustomAttribute<DataServiceParamAttribute>();
//                var name = item.Name;
//                if (parameterAttribute != null)
//                {
//                    name = parameterAttribute.Name ?? name;
//                }

//                paremsDict.Add(name, parameters[i]);
//            }

//            if (!methodAttribute.Async)
//            {
//                var response = this._client.Invoke(
//    dataServiceAttribute.Name ?? serviceType.Name,
//    methodAttribute.Name ?? methodName,
//    Formatter,
//    paremsDict/*,
//                method.ReturnType*/);

//                object returnValue = null;
//                if (this._client.ResultExtractor != null)
//                {
//                    if (method.GetCustomAttribute<RawOutputAttribute>() != null)
//                    {
//                        returnValue = this._client.ResultExtractor.RawOutputExtract(response, method.ReturnType, Formatter);
//                    }
//                    else
//                    {
//                        dynamic result = this._client.ResultExtractor.Extract(response, method.ReturnType, Formatter);

//                        if (!result.success)
//                        {
//                            throw new Exception("调用DataService失败:" + string.Join("\r\n", (result.errors as Dictionary<string, string>).Select(a => a.Key + a.Value)));
//                        }

//                        if (result.output != null)
//                        {
//                            // processing output;
//                            foreach (KeyValuePair<string, object> item in result.output)
//                            {
//                                try
//                                {
//                                    paremsDict[item.Key] = item.Value.To(paramInfos.First(p => p.Name == item.Key).ParameterType.GetElementType());
//                                }
//                                catch
//                                {
//                                }
//                            }
//                        }

//                        returnValue = result.result;
//                    }
//                }
//                else
//                {
//                    returnValue = response;
//                }

//                if (method.ReturnType == typeof(void))
//                {
//                    return new ReturnMessage(null, paremsDict.Values.ToArray(), paremsDict.Values.Count, null, message);
//                }
//                else
//                {
//                    return new ReturnMessage(Extensions.ObjectExtension.To(returnValue, method.ReturnType), paremsDict.Values.ToArray(), paremsDict.Values.Count, null, message);
//                }
//            }
//            else
//            {
//                this._client.InvokeAndForget(new DataService.RouteInfo { ServiceName = dataServiceAttribute.Name ?? serviceType.Name, ActionName = methodAttribute.Name ?? methodName, FormatName = Formatter }, paremsDict);
//                return new ReturnMessage(null, new object[] { }, 0, null, message);
//            }
//        }
//    }

//    internal class QueryProxy<T> : IQueryable<T>
//    {
//        public IEnumerator<T> GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        public Type ElementType
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public System.Linq.Expressions.Expression Expression
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public IQueryProvider Provider
//        {
//            get { throw new NotImplementedException(); }
//        }
//    }
//}


using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Annotations;
using AirMaster.Infrastructure.DataService.Common.Processor;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.Extensions;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AirMaster.Infrastructure.Serializer;

namespace AirMaster.Infrastructure.DataService.Internal
{
    public class DataServiceProxy : IInterceptor
    {
        private DataServiceClientBase _client;
        private string _formatter;

        private static MethodInfo _invokeAsyncMethodInfo =
            typeof(DataServiceProxy).GetMethod(nameof(DataServiceProxy.InvokeAsync),
                BindingFlags.NonPublic | BindingFlags.Instance);

        public DataServiceProxy(DataServiceClientBase client, string formatter)
        {
            _client = client;
            _formatter = formatter;
        }

        public void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            if (methodName == "GetHashCode")
            {
                invocation.ReturnValue = invocation.Proxy.GetHashCode();
                return;
            }

            if (methodName == "Equals")
            {
                invocation.ReturnValue = this.Equals(invocation.GetArgumentValue(0));
                return;
            }

            if (methodName == "GetType")
            {
                invocation.ReturnValue = invocation.Proxy.GetType();
                return;
            }

            var serviceType = invocation.GetConcreteMethod().DeclaringType;
            var dataServiceAttribute = serviceType.GetCustomAttribute<DataServiceAttribute>();
            if (dataServiceAttribute == null)
            {
                throw new Exception($"类型{serviceType.FullName}未标记为DataService!");
            }

            var methodInfo = invocation.GetConcreteMethod();
            var methodAttribute = methodInfo.GetCustomAttribute<DataServiceMethodAttribute>();
            if (methodAttribute == null)
            {
                throw new Exception($"方法{methodInfo.Name}未标记为DataServiceMethod!");
            }

            var paramInfos = methodInfo.GetParameters();
            var paramsDict = GetParameters(invocation, paramInfos);
            var route = new RouteInfo
            {
                ServiceName = dataServiceAttribute.Name ?? serviceType.Name,
                ActionName = methodAttribute.Name ?? methodName, FormatName = _formatter
            };

            var asyncMethod = typeof(Task).IsAssignableFrom(methodInfo.ReturnType); //new async method

            if (!asyncMethod)
            {
                if (methodAttribute.Async)
                {
                    this._client.InvokeAndForget(route, paramsDict);
                    return;
                } //old invoke and forget async mode, no return value;

                var response = this._client.Invoke(route, paramsDict);

                object returnValue = ExtractResult(response, methodInfo.ReturnType, methodInfo, paramInfos, paramsDict,
                    invocation);

                if (methodInfo.ReturnType == typeof(void))
                {
                    return;
                }
                else
                {
                    invocation.ReturnValue = ObjectExtension.To(returnValue, methodInfo.ReturnType);
                    return;
                }
            }
            else
            {
                if (methodInfo.ReturnType.IsGenericType)
                {
                    invocation.ReturnValue = _invokeAsyncMethodInfo
                        .MakeGenericMethod(methodInfo.ReturnType.GetGenericArguments()[0]).Invoke(this,
                            [route, methodInfo, paramInfos, paramsDict, invocation]);
                }
                else
                {
                    Func<Task> task = async () =>
                    {
                        var response = await this._client.InvokeAsync(route, paramsDict);
                        ExtractResult(response, typeof(void), methodInfo, paramInfos, paramsDict, invocation);
                    };
                    invocation.ReturnValue = task();
                }
            }
        }

        private async Task<T> InvokeAsync<T>(RouteInfo route, MethodInfo methodInfo, ParameterInfo[] paramInfos,
            Dictionary<string, object> paramsDict, IInvocation invocation)
        {
            var returnType = methodInfo.ReturnType.GetGenericArguments().First();

            var response = await this._client.InvokeAsync(route, paramsDict);
            object returnValue = ExtractResult(response, returnType, methodInfo, paramInfos, paramsDict, invocation);
            if (returnType == typeof(void))
            {
                return default(T);
            }
            else
            {
                return (T)ObjectExtension.To(returnValue, returnType);
            }
        }

        private object ExtractResult(string response, Type returnType, MethodInfo method, ParameterInfo[] paramInfos,
            Dictionary<string, object> paremsDict, IInvocation invocation)
        {
            if (this._client.ResultExtractor != null)
            {
                if (method.GetCustomAttribute<RawOutputAttribute>() != null)
                {
                    return this._client.ResultExtractor.RawOutputExtract(response, returnType, _formatter);
                }
                else
                {
                    dynamic result = this._client.ResultExtractor.Extract(response, returnType, _formatter);

                    if (!result.success)
                    {
                        if (result.errors is Dictionary<string, object> errorsDict)
                        {
                            if (errorsDict.ContainsKey("dataServiceException") && errorsDict["dataServiceException"].To<bool>(false) == true)
                            {
                                var typeName = errorsDict["type"].ToString();
                                var type = Type.GetType(typeName) ?? typeof(DataServiceException);
                                if (NewtonJsonSerializerAdapter.Instance.Deserialize(errorsDict["ex"].ToString(),
                                        type) is DataServiceException ex)
                                    throw ex;
                            }

                            throw new Exception("调用DataService失败:" + string.Join("\r\n",
                                errorsDict.Select(a => a.Key + a.Value)));
                        }
                        else
                        {
                            throw new DataServiceException("调用DataService失败:" + result.message);
                        }
                    }

                    if (result.output != null)
                    {
                        // processing output;
                        foreach (KeyValuePair<string, object> item in result.output)
                        {
                            try
                            {
                                paremsDict[item.Key] = item.Value.To(paramInfos.First(p => p.Name == item.Key)
                                    .ParameterType.GetElementType());
                                var i = Array.FindIndex(method.GetParameters(), p => p.Name == item.Key);
                                invocation.SetArgumentValue(i,
                                    item.Value.To(method.GetParameters()[i].ParameterType.GetElementType()));
                            }
                            catch
                            {
                            }
                        }
                    }

                    return result.result;
                }
            }
            else
            {
                return response;
            }
        }

        private static Dictionary<string, object> GetParameters(IInvocation invocation, ParameterInfo[] paramInfos)
        {
            var paremsDict = new Dictionary<string, object>();
            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                var item = paramInfos[i];
                var parameterAttribute = item.GetCustomAttribute<DataServiceParamAttribute>();
                var name = item.Name;
                if (parameterAttribute != null)
                {
                    name = parameterAttribute.Name ?? name;
                }

                paremsDict.Add(name, invocation.Arguments[i]);
            }

            return paremsDict;
        }


        private static ProxyGenerator gen = new ProxyGenerator();

        public static T Create<T>(DataServiceClientBase client, string formatter = "json") where T : class
        {
            var typeInfo = typeof(T).GetTypeInfo();
            if (typeInfo.IsInterface)
            {
                return gen.CreateInterfaceProxyWithoutTarget<T>(new DataServiceProxy(client, formatter));
            }
            else if (typeInfo.IsClass)
            {
                return gen.CreateClassProxy<T>(new DataServiceProxy(client, formatter));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal static object Create(DataServiceClientBase client, Type type, string formatter)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsInterface)
            {
                return gen.CreateInterfaceProxyWithoutTarget(type, new DataServiceProxy(client, formatter));
            }
            else if (typeInfo.IsClass)
            {
                return gen.CreateClassProxy(type, new DataServiceProxy(client, formatter));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}