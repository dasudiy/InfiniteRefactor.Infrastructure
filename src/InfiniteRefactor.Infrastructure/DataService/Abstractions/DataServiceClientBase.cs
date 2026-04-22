using AirMaster.Infrastructure.DataService.Bindings.AspNetCore;
using AirMaster.Infrastructure.DataService.Bindings.Http;
using AirMaster.Infrastructure.DataService.Bindings.RabbitMQ;
using AirMaster.Infrastructure.DataService.Bindings.Stream.NP;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Tcp;
using AirMaster.Infrastructure.DataService.Internal;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.DataService.Old.WebSocket;
using AirMaster.Infrastructure.Net;
using AirMaster.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AirMaster.Infrastructure.Extensions;
using AirMaster.Infrastructure.Serializer;

namespace AirMaster.Infrastructure.DataService.Abstractions
{
    public abstract class DataServiceClientBase
    {
        public DataServiceClientBase(string url, X509Certificate2 certificate = null, bool validateCertificate = true,
            DataServiceHost host = null, TimeSpan? timeout = null, string proxy = null)
        {
            this.Url = url;
            LocalCertificate = certificate;
            this.ValidateCertificate = validateCertificate;
            this.DataServiceHost = host ?? DataServiceHost.Instance;
            this.UseSSL = certificate != null;
            this.Timeout = timeout ?? TimeSpan.FromMinutes(1);
            Proxy = proxy;
        }

        public bool UseSSL { get; set; }
        public TimeSpan Timeout { get; }
        public string Proxy { get; }
        public ResultExtractor ResultExtractor { get; set; }

        public virtual bool IsConnected
        {
            get { return true; }
        }

        public virtual X509Certificate LocalCertificate { get; set; }

        public virtual X509Certificate RemoteCertificate
        {
            get { return null; }
        }

        public string Url { get; private set; }
        public bool ValidateCertificate { get; private set; }
        public DataServiceHost DataServiceHost { get; private set; }

        public Dictionary<string, string> DefaultHeaders { get; private set; } = new Dictionary<string, string>();

        public T Create<T>(string formatter = "json") where T : class
        {
            return DataServiceProxy.Create<T>(this, formatter);
        }

        public object DynamicCreate(Type type, string formatter = "json")
        {
            return DataServiceProxy.Create(this, type, formatter);
        }

        public abstract byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict);

        public abstract string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict);

        public abstract Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict);

        public virtual void InvokeAndForget(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            Task.Run(() => Invoke(routeInfo, paramsDict));
        }

        public string Invoke(string serviceName, string actionName, Dictionary<string, object> paramsDict)
        {
            return Invoke(new RouteInfo { ServiceName = serviceName, ActionName = actionName, FormatName = "json" },
                paramsDict);
        }

        public string Invoke(string serviceName, string actionName, string formatter,
            Dictionary<string, object> paramsDict)
        {
            return Invoke(new RouteInfo { ServiceName = serviceName, ActionName = actionName, FormatName = formatter },
                paramsDict);
        }

        // public T InvokeAndExtract<T>(string serviceName, string actionName,
        //     Dictionary<string, object> paramsDict = null)
        // {
        //     var result = Invoke(serviceName, actionName, paramsDict);
        //     var dataServiceResult = (DataServiceResult<T>)ResultExtractor.Extract(result, typeof(T));
        //     if (dataServiceResult.success)
        //     {
        //         return dataServiceResult.result;
        //     }
        //     else
        //     {
        //         if (dataServiceResult.errors["dataServiceException"].To<bool>(false) == true)
        //         {
        //             var typeName = dataServiceResult.errors["type"].ToString();
        //             var type = Type.GetType(typeName) ?? typeof(DataServiceException);
        //             if (NewtonJsonSerializerAdapter.Instance.Deserialize(dataServiceResult.errors["ex"].ToString(),
        //                     type) is DataServiceException ex)
        //                 throw ex;
        //         }
        //
        //         throw new Exception(string.Join(",", dataServiceResult.errors.Select(i => i.Key + ":" + i.Value)));
        //     }
        // }
        

        public T[] BatchInvoke<T>(string serviceName, string actionName, Dictionary<string, object>[] paramsDicts,
            string formatter = "json")
        {
            var dict = new Dictionary<string, object>();
            dict["DSBatch"] = true;
            dict["DSBatchCount"] = paramsDicts.Length;
            for (int i = 0; i < paramsDicts.Length; i++)
            {
                dict["DSArgument" + i] = HttpUtil.ToQueryString(paramsDicts[i]);
            }

            if (paramsDicts.Length == 0)
            {
                return new T[] { };
            }

            var response = this.Invoke(serviceName, actionName, formatter, dict);

            T[] returnValue = null;
            if (this.ResultExtractor != null)
            {
                string[] jsonResult =
                    (string[])this.ResultExtractor.RawOutputExtract(response, typeof(string[]), formatter);

                if (jsonResult == null && response.IndexOf("Exception") > 0)
                {
                    var ex = (Exception)this.ResultExtractor.RawOutputExtract(response, typeof(Exception), formatter);
                    throw ex;
                }

                returnValue = jsonResult.Select(i =>
                {
                    try
                    {
                        dynamic itemResult = this.ResultExtractor.Extract(i, typeof(T), formatter);

                        if (!itemResult.success)
                        {
                            //throw new Exception("调用DataService失败:" + string.Join("\r\n", (itemResult.errors as Dictionary<string, string>).Select(a => a.Key + a.Value)));
                            return default(T);
                        }

                        return (T)itemResult.result;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        throw;
                    }
                }).ToArray();

                return returnValue;
            }
            else
            {
                throw new Exception();
            }
        }

        public static DataServiceClientBase CreateClient(string url, X509Certificate2 certificate = null,
            bool validateCertificate = true, DataServiceHost host = null, TimeSpan? timeout = null, string proxy = null)
        {
            var uri = new Uri(url);
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    return new DataServiceHttpClient(url, certificate, validateCertificate, host, timeout, proxy);
                case "np":
                case "nps":
                    return new NamedPipeClient(url, certificate, validateCertificate, host, timeout);
                case "tcp":
                case "tcps":
                    return new DataServiceTcpClient(url, certificate, validateCertificate, host, timeout, proxy);
                case "amqp":
                    return new RabbitMQClient(url, host, timeout);
                case "ws":
                case "wss":
                    return new WebSocketConnection(url, certificate, validateCertificate, host, timeout, proxy);
                default:
                    throw new NotImplementedException();
            }
        }

        private static X509Certificate2 GetClientCertificate()
        {
            return null;
        }
    }
}