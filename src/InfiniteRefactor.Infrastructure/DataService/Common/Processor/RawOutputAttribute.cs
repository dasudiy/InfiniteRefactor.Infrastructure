using System.IO;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using InfiniteRefactor.Infrastructure.Serializer;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public class RawOutputAttribute : PostProcessorAttribute, IPreProcessor
    {
        public bool UseSerializer { get; set; }
        public string ContentType { get; set; }
        public bool IsLast { get; set; }
        public string Format { get; set; }

        public RawOutputAttribute()
        {
            Priority = 1;
            UseSerializer = true;
            IsLast = true;
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            if (response.Exception == null)
            {
                response.Result = response.RawResult;
            }
            else
            {
                response.Result = response.Exception.ToString();
                response.Exception = null;
            }
            if (!UseSerializer)
            {
                if (!string.IsNullOrWhiteSpace(ContentType)) { response.ContentType = ContentType; }
                if (response.Result.GetType() == typeof(string))
                {
                    response.OutputWriter.Write(response.Result);
                }
                else if (response.Result.GetType() == typeof(byte[]))
                {
                    byte[] bytes = response.Result as byte[];
                    response.OutputStream.Write(bytes, 0, bytes.Length);
                }
                else if (response.Result.GetType() == typeof(Stream))
                {
                    Stream stream = response.Result as Stream;
                    if (stream.CanRead)
                    {
                        stream.CopyTo(response.OutputStream);
                    }
                }
            }
            else
            {
                response.WriteResult();
            }
            response.End();
            return new ProcessResult { Last = this.IsLast };
        }

        ProcessResult IPreProcessor.Process(DataServiceRequest request)
        {
            if (!string.IsNullOrWhiteSpace(Format))
            {
                request.Context.Serializer = SerializerFactory.Create(Format);
            }
            return ProcessResult.Default;
        }

        public Task<ProcessResult> ProcessAsync(DataServiceRequest request)
        {
            return Task.FromResult(((IPreProcessor)this).Process(request));
        }
    }

}
