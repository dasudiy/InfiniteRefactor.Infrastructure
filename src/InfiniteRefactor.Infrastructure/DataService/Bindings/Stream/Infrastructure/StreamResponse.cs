using AirMaster.Infrastructure.DataService.Abstractions;
using System;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    public class StreamResponse : DataServiceResponse
    {
        public StreamResponse(DataServiceContext context) : base(context)
        {
        }

        public override string ContentType { get; set; }

        public override bool IsClientConnected
        {
            get
            {
                return (Context as StreamContext).StreamConnection.ConnectionController.IsConnected;
            }
        }

        public override System.IO.Stream OutputStream
        {
            get
            {
                throw new NotImplementedException("流输出未实现！");
            }
        }

        public override System.IO.TextWriter OutputWriter
        {
            get
            {
                throw new NotImplementedException("OutputWriter未实现！");
            }
        }

        public override object RawResponseObject
        {
            get
            {
                return (Context as StreamContext).StreamConnection;
            }
        }

        public override void WriteResult()
        {
            (Context as StreamContext).StreamConnection.Send("DataServiceResponse", (Context as StreamContext).requestInfo.requestId, this.Result);
        }

        public override Task WriteResultAsync()
        {
            return (Context as StreamContext).StreamConnection.SendAsync("DataServiceResponse", (Context as StreamContext).requestInfo.requestId, this.Result);
        }
    }
}
