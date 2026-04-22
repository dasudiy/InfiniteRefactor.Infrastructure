using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    public class SessionAuthorizationAttribute : PreProcessorAttribute
    {
        public SessionAuthorizationAttribute()
        {
            base.Priority = 3;
        }

        public override ProcessResult Process(DataServiceRequest request)
        {
            var user = request.Context.Session?.User;
            if (user == null)
            {
                return new ProcessResult { CancelProcess = true, Message = "请登录后再进行此操作！", SourceName = "AccessDeniedException" };
            }
            else
            {
                return new ProcessResult { SourceName = this.GetType().Name };
            }
        }
    }
}
