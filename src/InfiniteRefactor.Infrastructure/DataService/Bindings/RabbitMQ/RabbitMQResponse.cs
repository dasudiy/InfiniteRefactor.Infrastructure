using System;
using System.IO;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.RabbitMQ
{
    class RabbitMQResponse : DataServiceResponse
    {
        public RabbitMQResponse(DataServiceContext context) : base(context)
        {
        }

        public override object RawResponseObject => throw new NotImplementedException();

        public override System.IO.Stream OutputStream => throw new NotImplementedException();

        public override TextWriter OutputWriter => throw new NotImplementedException();

        public override bool IsClientConnected => throw new NotImplementedException();

        public override string ContentType { get; set; }

        public override void WriteResult()
        {

        }

        public override Task WriteResultAsync()
        {
            return Task.CompletedTask;
        }
    }
}
