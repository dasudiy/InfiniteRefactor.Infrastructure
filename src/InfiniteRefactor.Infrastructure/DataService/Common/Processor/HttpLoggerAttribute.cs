using System.IO;
using System.Text;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using Microsoft.AspNetCore.Http;
using NLog;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public class HttpLoggerAttribute : PostProcessorAttribute
    {
        public bool Trace { get; set; } = false;

        public override ProcessResult Process(DataServiceResponse response)
        {
            var log = GetLogger(response.Context);
            var request = response.Context.Request;

            var template = @"
Request Info:
    Request Ip: {0}
    Request Url: {1}
    Request Method: {2}
    Request Headers:
{3}
    Request Body: 
{4}
Response Info:
    Response Code: {5}
    Response Headers:
{6}
    Response Body: 
{7}
";
            var result = string.Format(template,
                request.Remote.Address.ToString(),
                request.Url.ToString(),
                (request.RawRequestObject as HttpRequest).Method,
                ExtractHeaders((request.RawRequestObject as HttpRequest).Headers),
                ReadBody(request.RawRequestObject as HttpRequest),
                (response.RawResponseObject as HttpResponse).StatusCode,
                ExtractHeaders((response.RawResponseObject as HttpResponse).Headers),
                (response.Result != null ? request.Context.Serializer.Serialize(response.Result) : string.Empty));
            if (Trace)
            {
                log.Trace(result);
            }
            else
            {
                log.Info(result);
            }
            return ProcessResult.Default;
        }

        private object ReadBody(HttpRequest httpRequest)
        {
            //httpRequest.EnableRewind();
            //必须用这个，虽然表面F12进去也是调EnableRewind，但是实际运行就不会报错。不知道为什么。
            httpRequest.EnableBuffering();
            //httpRequest.Body.Position = 0;
            using (var reader = new StreamReader(httpRequest.Body, Encoding.UTF8))
            {
                var text = reader.ReadToEnd();
                return text;
            }
        }

        private object ExtractHeaders(IHeaderDictionary headers)
        {
            var sb = new StringBuilder();
            foreach (var item in headers)
            {
                sb.AppendFormat("\t{0}:{1}\r\n", item.Key, string.Join(",", item.Value));
            }

            return sb.ToString();
        }

        private Logger GetLogger(DataServiceContext context)
        {
            return LogManager.GetLogger(context.ServiceInstance.GetType().FullName);
        }
    }
}
