using AirMaster.Infrastructure.DataService.Abstractions;
using System;
using System.IO;
using System.Text;

namespace AirMaster.Infrastructure.DataService.Bindings.BatchQuery
{
    public class BatchQueryResponse : DataServiceResponse
    {
        private DataServiceResponse rawResponse;
        private MemoryStream ms = new MemoryStream();
        private Lazy<TextWriter> writer;
        public BatchQueryResponse(BatchQueryContext context, DataServiceResponse response)
            : base(context)
        {
            rawResponse = response;
            writer = new Lazy<TextWriter>(() => new StreamWriter(ms, Encoding.UTF8));
        }

        public override string ContentType { get; set; }

        public override bool IsClientConnected
        {
            get
            {
                return true;
            }
        }

        public override System.IO.Stream OutputStream
        {
            get
            {
                return ms;
            }
        }

        public override TextWriter OutputWriter
        {
            get
            {
                return writer.Value;
            }
        }

        public override object RawResponseObject
        {
            get
            {
                return rawResponse;
            }
        }

        internal string GetResult()
        {
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
