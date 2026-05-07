using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Http;
using InfiniteRefactor.Infrastructure.DataService.Common;
using InfiniteRefactor.Infrastructure.DataService.Internal;
using InfiniteRefactor.Infrastructure.DataService.Metadata;
using InfiniteRefactor.Infrastructure.Net;
using InfiniteRefactor.Infrastructure.Security;
using InfiniteRefactor.Infrastructure.Serializer;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.HttpWithEncryption
{
    public class EncryptionDataServiceClient : DataServiceClientBase
    {
        public bool AllowCompress { get; set; }
        //public string DataServiceAddress { get; set; }
        public DataServiceRouter DataServiceRouter { get; set; }
        public static DataServiceHttpClient Instance { get; set; }

        public string AlgName { get; set; }
        public string SecurityKey { get; set; }
        public bool GZip { get; set; }
        public Net.Http HttpClient { get; private set; }

        public Action<HttpRequestMessage> PrepareRequest { get; set; }

        public EncryptionDataServiceClient(string securityKey, string url, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null, string provider = "AES", TimeSpan? timeout = null)
            : this(securityKey, url, certificate, validateCertificate, host, provider, timeout, null) //兼容需要
        {
        }

        public EncryptionDataServiceClient(string securityKey, string url, X509Certificate2 certificate, bool validateCertificate, DataServiceHost host, string provider, TimeSpan? timeout, string proxy)
            : base(url, certificate, validateCertificate, host, timeout)
        {
            this.DataServiceRouter = new DefaultServiceRouter();
            this.ResultExtractor = new ResultExtractor();
            this.AllowCompress = true;
            this.HttpClient = new Net.Http(null, proxy: proxy);
            this.HttpClient.UserAgent = "DataServiceClient/" + typeof(DataServiceHttpClient).GetTypeInfo().Assembly.GetName().Version.ToString();
            this.HttpClient.Timeout = timeout ?? TimeSpan.FromMinutes(1);

            this.SecurityKey = securityKey;
            this.AlgName = provider;
            //this.HttpClient.AllowCompress = false;
        }

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            var dict = GetParameters(paramsDict, routeInfo.FormatName);
            var url = GetUrl(routeInfo);
            var requestContentEncoding = AlgName.ToLower();

            try
            {
                using (var ms = new MemoryStream())
                {
                    if (paramsDict != null)
                    {
                        System.IO.Stream outputStream = Cryptography.CreateSymmetricCryptoStream(AlgName, SecurityKey, ms, CryptoStreamMode.Write);

                        if (GZip)
                        {
                            outputStream = new GZipStream(outputStream, CompressionMode.Compress);
                            requestContentEncoding = AlgName.ToLower() + "+gzip";
                        }

                        try
                        {
                            using (var writer = new StreamWriter(outputStream, Encoding.UTF8))
                            {
                                var content = HttpUtil.ToQueryString(dict);
                                writer.Write(content);
                                writer.Flush();
                            }
                        }
                        finally
                        {
                            outputStream.Dispose();
                        }
                    }

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(url),
                        Content = new ByteArrayContent(ms.ToArray())
                        {
                            Headers = {
                                {  "content-type", "application/x-www-form-urlencoded" },
                                {  "content-encoding", requestContentEncoding },
                            }
                        }
                    };
                    PrepareRequest?.Invoke(request);

                    var response = HttpClient.Send(request);
                    using (var stream = response.ReadAsStream())
                    {
                        System.IO.Stream inputStream = null;
                        var cs = Cryptography.CreateSymmetricCryptoStream(AlgName, SecurityKey, stream, CryptoStreamMode.Read);
                        inputStream = cs;

                        if (response.Content.Headers.TryGetValues("Content-Encoding", out IEnumerable<string> contentEncoding))
                        {
                            if (contentEncoding.Any(t => t.IndexOf("gzip") > 0))
                            {
                                inputStream = new GZipStream(cs, CompressionMode.Decompress);
                            }
                        }

                        try
                        {
                            using (var reader = new StreamReader(inputStream, Encoding.UTF8))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                        finally
                        {
                            inputStream.Dispose();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                string errorDetail = string.Empty;
                if (ex.Response != null && ex.Response.GetResponseStream() != null)
                {
                    using (var reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        errorDetail = reader.ReadToEnd();
                    }
                }

                if (errorDetail != string.Empty)
                {
                    throw new WebException("调用失败:" + errorDetail, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            throw new NotImplementedException();
        }

        public override async Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            var dict = GetParameters(paramsDict, routeInfo.FormatName);
            var url = GetUrl(routeInfo);
            var requestContentEncoding = AlgName.ToLower();

            try
            {
                using (var ms = new MemoryStream())
                {
                    if (paramsDict != null)
                    {
                        System.IO.Stream outputStream = Cryptography.CreateSymmetricCryptoStream(AlgName, SecurityKey, ms, CryptoStreamMode.Write);

                        if (GZip)
                        {
                            outputStream = new GZipStream(outputStream, CompressionMode.Compress);
                            requestContentEncoding = AlgName.ToLower() + "+gzip";
                        }

                        try
                        {
                            using (var writer = new StreamWriter(outputStream, Encoding.UTF8))
                            {
                                var content = HttpUtil.ToQueryString(dict);
                                writer.Write(content);
                                writer.Flush();
                            }
                        }
                        finally
                        {
                            outputStream.Dispose();
                        }
                    }

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(url),
                        Content = new ByteArrayContent(ms.ToArray())
                        {
                            Headers = {
                                {  "content-type", "application/x-www-form-urlencoded" },
                                {  "content-encoding", requestContentEncoding },
                            }
                        }
                    };
                    PrepareRequest?.Invoke(request);

                    var response = await HttpClient.SendAsync(request);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        System.IO.Stream inputStream = null;
                        var cs = Cryptography.CreateSymmetricCryptoStream(AlgName, SecurityKey, stream, CryptoStreamMode.Read);
                        inputStream = cs;

                        if (response.Content.Headers.TryGetValues("Content-Encoding", out IEnumerable<string> contentEncoding))
                        {
                            if (contentEncoding.Any(t => t.IndexOf("gzip") > 0))
                            {
                                inputStream = new GZipStream(cs, CompressionMode.Decompress);
                            }
                        }

                        try
                        {
                            using (var reader = new StreamReader(inputStream, Encoding.UTF8))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                        finally
                        {
                            inputStream.Dispose();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                string errorDetail = string.Empty;
                if (ex.Response != null && ex.Response.GetResponseStream() != null)
                {
                    using (var reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        errorDetail = reader.ReadToEnd();
                    }
                }

                if (errorDetail != string.Empty)
                {
                    throw new WebException("调用失败:" + errorDetail, ex);
                }
                else
                {
                    throw;
                }
            }
        }


        protected string GetUrl(RouteInfo routeInfo)
        {
            var url = string.Format("{0}{1}", Url, DataServiceRouter.GetRouteUri(routeInfo));
            url += ((url.IndexOf('?') > 0) ? '&' : '?') + "timestamp=" + DateTime.Now.Ticks;
            return url;
        }

        protected Dictionary<string, object> GetParameters(Dictionary<string, object> paramsDict, string formatter = "json")
        {
            Dictionary<string, object> dict = null;
            if (paramsDict != null)
            {
                dict = paramsDict.ToDictionary(i => i.Key, i =>
                {
                    if (i.Value != null && i.Value.GetType() != typeof(string) && !i.Value.GetType().GetTypeInfo().IsValueType) { return SerializerFactory.Create(formatter).Serialize(i.Value); }
                    return i.Value;
                });
            }
            return dict;
        }

    }
}
