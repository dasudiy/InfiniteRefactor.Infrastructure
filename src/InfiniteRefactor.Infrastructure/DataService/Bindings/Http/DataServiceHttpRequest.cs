using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.Net;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Http
{
    public class DataServiceHttpRequest : DataServiceRequest
    {
        private System.Net.HttpListenerRequest httpListenerRequest;
        private object formReadSync = new object();
        private Dictionary<string, string> formDataCache = null;

        public DataServiceHttpRequest(System.Net.HttpListenerRequest httpListenerRequest, DataServiceContext context)
            : base(context)
        {
            this.httpListenerRequest = httpListenerRequest;
        }
        public override object RawRequestObject
        {
            get { return httpListenerRequest; }
        }

        public override System.Net.IPEndPoint Remote
        {
            get
            {
                return httpListenerRequest.RemoteEndPoint;
            }
        }

        public override Uri Url
        {
            get { return httpListenerRequest.Url; }
        }

        public override object this[string key]
        {
            get
            {
                if (httpListenerRequest.QueryString.AllKeys.Contains(key))
                {
                    //http://stackoverflow.com/questions/4674696/how-to-handle-url-encoding-in-httplistenerrequest
                    //url encoding can't change by code
                    //return this.httpListenerRequest.QueryString[key];

                    return HttpUtility.ParseQueryString(Url.Query)[key];
                }

                if (formDataCache == null)
                {
                    lock (formReadSync)
                    {
                        if (formDataCache == null)
                        {
                            formDataCache = new Dictionary<string, string>();

                            if (httpListenerRequest.InputStream.CanRead)
                            {
                                //using (var reader = new StreamReader(this.httpListenerRequest.InputStream))
                                {
                                    if (httpListenerRequest.ContentType != null && httpListenerRequest.ContentType.StartsWith("application/x-www-form-urlencoded"))
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            httpListenerRequest.InputStream.CopyTo(ms);
                                            var data = HttpUtil.LoadForm(ms.ToArray(), Encoding.UTF8);
                                            foreach (var nKey in data.AllKeys)
                                            {
                                                formDataCache[nKey] = data[nKey];
                                            }
                                        }


                                        //var formData = reader.ReadToEnd();
                                        //var data = HttpUtility.ParseQueryString(formData);
                                        //foreach (var nKey in data.AllKeys)
                                        //{
                                        //    formDataCache[nKey] = data[nKey];
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }

                if (formDataCache.ContainsKey(key))
                {
                    return formDataCache[key];
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
