using AirMaster.Infrastructure.DataService.Abstractions;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNet
{
    internal class AspNetResponse : DataServiceResponse
    {
        private HttpResponse response;
        private HttpResponseTextWriter writer;

        public AspNetResponse(AspNetDataServiceContext aspNetDataServiceContext, HttpContext context)
            : base(aspNetDataServiceContext)
        {
            this.response = context.Response;
            writer = new HttpResponseTextWriter(this.response);
        }


        public override object RawResponseObject => response;

        public override System.IO.Stream OutputStream => response.Body;

        public override TextWriter OutputWriter => writer;

        public override bool IsClientConnected => response.HttpContext.RequestAborted.IsCancellationRequested;

        public override string ContentType
        {
            get
            {
                return response.ContentType;
            }
            set
            {
                response.ContentType = value;
            }
        }
    }

    public class HttpResponseTextWriter : TextWriter
    {
        private HttpResponse HttpResponse { get; set; }

        public HttpResponseTextWriter(HttpResponse httpResponse) { this.HttpResponse = httpResponse; }

        public override Encoding Encoding => this.HttpResponse.ContentType.Split(';').Select(c => { var a = c.Split('='); return new { Name = a.First(), Value = a.Last() }; }).Where(c => c.Name == "charset").Select(c => Encoding.GetEncoding(c.Value)).FirstOrDefault();

        public override void Write(string value)
        {
            HttpResponse.WriteAsync(value);
        }
    }
}