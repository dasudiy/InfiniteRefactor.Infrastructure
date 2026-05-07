using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using InfiniteRefactor.Infrastructure.Serializer;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public class JsonWithPaddingProcessor : PostProcessorAttribute
    {
        public override ProcessResult Process(DataServiceResponse response)
        {
            var request = DataServiceContext.Current.Request;
            var callback = request.ReadParameter<string>("callback", "callback");
            if (response.Context.RouteInfo.FormatName == "jsonp")
            {
                var result = string.Format("{0}({1});", callback, NewtonJsonSerializerAdapter.Instance.Serialize(response.Result));
                response.ContentType = "text/javascript";
                response.OutputWriter.Write(result);
                response.End();
                return new ProcessResult { Last = true, CancelProcess = true, SourceName = this.GetType().Name };
            }

            return ProcessResult.Default;
        }
    }
}
