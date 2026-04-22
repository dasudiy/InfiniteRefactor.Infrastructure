using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using System;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class TimePeroidLimitAttribute : PreProcessorAttribute
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public Mode LimitMode { get; set; }
        public string DeniedInfo { get; set; }

        public enum Mode
        {
            WhiteList, BlackList
        }

        public override ProcessResult Process(DataServiceRequest request)
        {
            bool inList = true;
            if (!string.IsNullOrWhiteSpace(StartTime))
            {
                inList &= DateTime.Now >= DateTime.Now.Date.Add(TimeSpan.Parse(StartTime));
            }
            if (!string.IsNullOrWhiteSpace(EndTime))
            {
                inList &= DateTime.Now <= DateTime.Now.Date.Add(TimeSpan.Parse(EndTime));
            }

            if (inList == (LimitMode == TimePeroidLimitAttribute.Mode.BlackList))
            {
                var result = new ProcessResult();
                result.SourceName = this.GetType().Name;
                result.CancelProcess = true;
                result.Last = true;
                result.Message = DeniedInfo ?? "当前时间不允许执行此方法，如有疑问请与管理员联系！";
                return result;
            }
            else
            {
                return ProcessResult.Default;
            }
        }
    }
}
