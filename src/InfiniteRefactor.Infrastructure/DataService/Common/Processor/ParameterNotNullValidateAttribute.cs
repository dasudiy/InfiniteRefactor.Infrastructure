using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public class ParameterNotNullValidateAttribute : PreProcessorAttribute
    {
        public string[] Parameters { get; set; }

        public ParameterNotNullValidateAttribute(params string[] parameters)
        {
            this.Parameters = parameters;
        }

        public override ProcessResult Process(DataServiceRequest request)
        {
            foreach (var item in Parameters)
            {
                if (request.ReadParameter<object>(item, null) == null)
                {
                    return new ProcessResult { CancelProcess = true, Last = true, Message = string.Format("必须提供{0}参数", item) };
                }
            }

            return ProcessResult.Default;
        }
    }
}
