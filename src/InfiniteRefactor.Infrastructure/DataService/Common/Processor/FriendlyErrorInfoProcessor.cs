using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    public class FriendlyErrorInfoProcessor : IPostProcessor
    {
        public Dictionary<Func<Exception, bool>, string> ExceptionProcessor { get; set; }

        public FriendlyErrorInfoProcessor()
        {
            ExceptionProcessor = new Dictionary<Func<Exception, bool>, string>();
        }


        public ProcessResult Process(DataServiceResponse response)
        {
            if (response.Exception != null)
            {
                foreach (var item in ExceptionProcessor)
                {
                    if (CheckType(response.Exception, item.Key))
                    {
                        response.Exception = new FriendlyErrorInfo(item.Value, response.Exception);
                        break;
                    }
                }
            }
            return ProcessResult.Default;
        }

        private bool CheckType(Exception exception, Func<Exception, bool> exceptionPredicate)
        {
            if (exception != null)
            {
                return exceptionPredicate(exception) || CheckType(exception.InnerException, exceptionPredicate);
            }
            return false;
        }

        public Task<ProcessResult> ProcessAsync(DataServiceResponse response)
        {
            return Task.FromResult(Process(response));
        }

        public int Priority
        {
            get;
            set;
        }
    }

    public class FriendlyErrorInfo : Exception
    {
        public FriendlyErrorInfo(string message, Exception internalException) : base(message, internalException)
        {
            this.Source = "错误信息";
        }
    }
}
