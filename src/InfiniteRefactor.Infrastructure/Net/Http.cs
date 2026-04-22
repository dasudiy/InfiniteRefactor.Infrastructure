using AirMaster.Infrastructure.Serializer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace AirMaster.Infrastructure.Net
{
    public class Http : System.Net.Http.HttpClient
    {
        public HttpClientExHandler HttpClientHandler { get; set; }

        public Http() : this(new HttpClientExHandler())
        {

        }

        public Http(HttpClientExHandler handler) : base(handler)
        {
            this.HttpClientHandler = handler;
        }

        public Http(string cookieFile, bool ignoreCertificationError = false, string proxy = null) : this(new HttpClientExHandler(cookieFile, ignoreCertificationError, proxy))
        {

        }

        public readonly static Http Instance = new Http();


        public CustomContainer CookieContainer => this.HttpClientHandler.CustomCookieContainer;
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public string UserAgent
        {
            set
            {
                this.DefaultRequestHeaders.UserAgent.Clear();
                this.DefaultRequestHeaders.UserAgent.TryParseAdd(value);
            }
            get
            {
                return this.DefaultRequestHeaders.UserAgent.ToString();
            }
        }

        public void SetProxy(string serverAddress, string username, string password)
        {
            if (!HttpClientHandler.SupportsProxy)
            {
                throw new ArgumentException("Http Handler does not support proxy!");
            }
            NetworkCredential networkCredential = null;
            if (username != null)
            {
                networkCredential = new NetworkCredential(username, password);
            }
            HttpClientHandler.Proxy = new WebProxy(serverAddress, true, null, networkCredential);
            HttpClientHandler.UseProxy = true;
        }

        public async Task<HttpResponseMessage> PostAsync(string uri, string content, string contentType = "application/json", System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.PostAsync(uri, new StringContent(content, Encoding, contentType), cancellationToken);
        }

        public async Task<HttpResponseMessage> PostFormAsync(string uri, NameValueCollection form, System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {            
            return await this.PostAsync(uri, HttpUtil.ToQueryString(form, Encoding), "application/x-www-form-urlencoded", cancellationToken);
        }

        public async Task<HttpResponseMessage> PostFormAsync(string uri, IDictionary<string, object> form, System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.PostAsync(uri, HttpUtil.ToQueryString(form, Encoding), "application/x-www-form-urlencoded", cancellationToken);
        }


        public HttpResponseMessage Post(string uri, string content, string contentType = "application/json", System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(content, Encoding, contentType)
            };

            return this.Send(request, cancellationToken);
        }

        public HttpResponseMessage PostAsJson<TValue>(string uri, TValue value, System.Threading.CancellationToken cancellationToken = default(CancellationToken))
            => Post(uri, SF.JSON.Serialize(value), "application/json", cancellationToken);

        public HttpResponseMessage PostForm(string uri, NameValueCollection form, System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.Post(uri, HttpUtil.ToQueryString(form, Encoding), "application/x-www-form-urlencoded", cancellationToken);
        }

        public HttpResponseMessage PostForm(string uri, IDictionary<string, object> form, System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.Post(uri, HttpUtil.ToQueryString(form, Encoding), "application/x-www-form-urlencoded", cancellationToken);
        }

        public HttpResponseMessage Get(string uri, System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return this.Send(request, cancellationToken);
        }
    }

    public static class HttpExtensions
    {
        private static async Task<HttpResponseMessage> EnsureStatusCodeAndGetResponseMessage(Task<HttpResponseMessage> httpSendTask)
        {
            var message = await httpSendTask;
            message.EnsureSuccessStatusCode();
            return message;
        }

        public static async Task<string> ReadAsStringAsync(this Task<HttpResponseMessage> httpSendTask)
        {
            var message = await EnsureStatusCodeAndGetResponseMessage(httpSendTask);
            return await message.Content.ReadAsStringAsync();
        }

        public static async Task<Stream> ReadAsStreamAsync(this Task<HttpResponseMessage> httpSendTask)
        {
            var message = await EnsureStatusCodeAndGetResponseMessage(httpSendTask);
            return await message.Content.ReadAsStreamAsync();
        }

        public static async Task<byte[]> ReadAsBytesAsync(this Task<HttpResponseMessage> httpSendTask)
        {
            var message = await EnsureStatusCodeAndGetResponseMessage(httpSendTask);
            return await message.Content.ReadAsByteArrayAsync();
        }

        public static async Task<T> ReadFromJson<T>(this Task<HttpResponseMessage> httpSendTask)
        {
            var message = await EnsureStatusCodeAndGetResponseMessage(httpSendTask);
            return await message.Content.ReadFromJsonAsync<T>();
        }

        /// <summary>
        /// 适配Newtonsoft.Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpSendTask"></param>
        /// <returns></returns>
        public static async Task<T> ReadFromJson2<T>(this Task<HttpResponseMessage> httpSendTask)
        {
            var message = await EnsureStatusCodeAndGetResponseMessage(httpSendTask);
            return SF.JSON.Deserialize<T>(await message.Content.ReadAsStringAsync());
        }


        public static async Task<string> ReadAsStringAsync(this HttpResponseMessage httpSendTask, Encoding encoding = null)
        {
            httpSendTask.EnsureSuccessStatusCode();
            if (encoding == null)
            {
                return await httpSendTask.Content.ReadAsStringAsync();
            }

            using (var sr = new StreamReader(await httpSendTask.Content.ReadAsStreamAsync(), encoding))
            {
                return sr.ReadToEnd();
            }
        }

        public static async Task<T> ReadFromJson2Async<T>(this HttpResponseMessage httpSendTask)
        {
            using (var stream = await httpSendTask.Content.ReadAsStreamAsync())
            {
                return SF.JSON.Deserialize<T>(stream);
            }
        }

        public static string ReadAsString(this HttpResponseMessage httpSendTask, Encoding encoding = null)
        {
            httpSendTask.EnsureSuccessStatusCode();
            using (var sr = new StreamReader(httpSendTask.Content.ReadAsStream(), encoding ?? Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        public static Stream ReadAsStream(this HttpResponseMessage httpSendTask)
        {
            httpSendTask.EnsureSuccessStatusCode();
            return httpSendTask.Content.ReadAsStream();
        }

        public static byte[] ReadAsBytes(this HttpResponseMessage httpSendTask)
        {
            httpSendTask.EnsureSuccessStatusCode();
            using (var ms = new MemoryStream())
            {
                httpSendTask.Content.ReadAsStream().CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static T ReadFromJson<T>(this HttpResponseMessage httpSendTask)
        {
            using (var stream = httpSendTask.ReadAsStream())
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(stream);
            }
        }

        public static T ReadFromJson2<T>(this HttpResponseMessage httpSendTask)
        {
            using (var stream = httpSendTask.ReadAsStream())
            {
                return SF.JSON.Deserialize<T>(stream);
            }
        }
    }
}