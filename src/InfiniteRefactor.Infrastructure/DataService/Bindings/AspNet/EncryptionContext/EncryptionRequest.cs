using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.Net;
using AirMaster.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNet.EncryptionContext
{
    public class EncryptionRequest : DataServiceRequest
    {
        private HttpRequest httpRequest;
        private string securityKey;
        private string provider;
        private Dictionary<string, string> formDataCache = null;
        private object formReadSync = new object();
        private EncryptionHttpContext context;

        public EncryptionRequest(HttpRequest httpRequest, DataServiceContext context)
            : base(context)
        {
            this.httpRequest = httpRequest;
            this.securityKey = (context as EncryptionHttpContext).SecurityKey;
            this.provider = EncryptionHttpContext.SymmetricAlgorithm;
            this.context = context as EncryptionHttpContext;
        }

        public override Uri Url
        {
            get { return new Uri(Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(this.httpRequest)); }
        }

        public override object RawRequestObject
        {
            get { return httpRequest; }
        }

        public override System.Net.IPEndPoint Remote
        {
            get { return new IPEndPoint(httpRequest.HttpContext.Connection.RemoteIpAddress, httpRequest.HttpContext.Connection.RemotePort); }
        }

        public override object this[string key]
        {
            get
            {
                if (this.httpRequest.Query.ContainsKey(key))
                {
                    //http://stackoverflow.com/questions/4674696/how-to-handle-url-encoding-in-httplistenerrequest
                    //url encoding can't change by code
                    //return this.httpListenerRequest.QueryString[key];
                    return HttpUtility.ParseQueryString(this.Url.Query)[key];
                }

                if (formDataCache == null)
                {
                    lock (formReadSync)
                    {
                        if (formDataCache == null)
                        {
                            formDataCache = new Dictionary<string, string>();

                            if (this.httpRequest.Body.CanRead)
                            {
                                var cs = Cryptography.CreateSymmetricCryptoStream(provider, securityKey, httpRequest.Body, CryptoStreamMode.Read);
                                {
                                    System.IO.Stream stream = cs;
                                    if (this.httpRequest.Headers["content-encoding"] == provider.ToLower() + "+gzip")
                                    {
                                        stream = new System.IO.Compression.GZipStream(cs, System.IO.Compression.CompressionMode.Decompress);
                                        context.GZip = true;
                                    }

                                    try
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            var data = HttpUtil.LoadForm(ms.ToArray(), Encoding.UTF8);
                                            foreach (var nKey in data.AllKeys)
                                            {
                                                if (nKey == null) { continue; }
                                                formDataCache[nKey] = data[nKey];
                                            }
                                        }

                                        //using (var reader = new StreamReader(stream))
                                        //{
                                        //    if (this.httpRequest.ContentType != null && this.httpRequest.ContentType.StartsWith("application/x-www-form-urlencoded"))
                                        //    {
                                        //        var formData = reader.ReadToEnd();
                                        //        var data = HttpUtility.ParseQueryString(formData, Encoding.UTF8);
                                        //        foreach (var nKey in data.AllKeys)
                                        //        {
                                        //            formDataCache[nKey] = data[nKey];
                                        //        }
                                        //    }
                                        //}
                                    }
                                    finally
                                    {
                                        stream.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }

                if (this.formDataCache.ContainsKey(key))
                {
                    return this.formDataCache[key];
                }
                else
                {
                    return null;
                }
            }
        }


        public string UserAgent
        {
            get
            {
                return httpRequest.Headers["User-Agent"];
            }
        }
    }

}
