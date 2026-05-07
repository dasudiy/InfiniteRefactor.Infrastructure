using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace InfiniteRefactor.Infrastructure.Net
{
    public class HttpClientExHandler : HttpClientHandler
    {
        public CustomContainer CustomCookieContainer { get; set; }
        public string CookiePersistantFile { get; set; }

        public HttpClientExHandler()
        {
            CustomCookieContainer = new CustomContainer();
            UseCookies = false;
            AutomaticDecompression = DecompressionMethods.All;   
        }

        public HttpClientExHandler(string persistanceCookieFile = null, bool ignoreCertificationError = false, string proxy = null)
        {
            if (File.Exists(persistanceCookieFile))
            {
                lock ("cookie-" + CookiePersistantFile)
                {
                    CustomCookieContainer = CookieHelper.LoadCustomContainer(persistanceCookieFile);
                }
            }
            else
            {
                CustomCookieContainer = new CustomContainer();
            }

            CookiePersistantFile = persistanceCookieFile;
            UseCookies = false;

            if (ignoreCertificationError)
            {
                ClientCertificateOptions = ClientCertificateOption.Manual;
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            }

            if (!string.IsNullOrWhiteSpace(proxy))
            {
                this.Proxy = new WebProxy(proxy);
            }
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Preprocess(request);
            var response = await base.SendAsync(request, cancellationToken);
            var domain = request.RequestUri.Host;
            Postprocess(response, domain);
            return response;
        }

        private void Preprocess(HttpRequestMessage request)
        {
            if (this.CustomCookieContainer != null)
            {
                var cookie = this.CustomCookieContainer.GetCookieHeader(request.RequestUri.ToString());
                if (!string.IsNullOrEmpty(cookie))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", cookie);
                }
            }
        }

        private void Postprocess(HttpResponseMessage response, string domain)
        {
            if (this.CustomCookieContainer != null && response.Headers.Contains("Set-Cookie"))
            {
                lock (CustomCookieContainer)
                {
                    var cookie = response.Headers.GetValues("Set-Cookie").First();
                    var cookies = System.Text.RegularExpressions.Regex.Split(cookie, @"(?<!expires=\w{3}),", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    foreach (var item in cookies)
                    {
                        this.CustomCookieContainer.Add(item, domain);
                    }


                    if (!string.IsNullOrWhiteSpace(CookiePersistantFile))
                    {
                        lock ("cookie-" + CookiePersistantFile)
                        {
                            File.Delete(CookiePersistantFile);
                            CustomCookieContainer.Save(CookiePersistantFile);
                        }
                    }
                }
            }
        }

        public void SetCookie(string key, string value, string domain, string path = "/")
        {
            CustomCookieContainer.Add(new Cookie(key, HttpUtility.UrlEncode(value), path, domain));
        }
    }
}
