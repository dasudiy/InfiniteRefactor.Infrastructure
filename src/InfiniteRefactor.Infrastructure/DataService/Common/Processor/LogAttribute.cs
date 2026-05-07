using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using InfiniteRefactor.Infrastructure.Serializer;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public class LogAttribute : PostProcessorAttribute, IPreProcessor
    {
        public string[] LogParameters { get; set; }
        public LogType LogType { get; set; }
        public bool LogResult { get; set; }
        public string Template { get; set; }
        public string ExceptionTemplate { get; set; }


        public LogAttribute(params string[] parameters)
        //: this()
        {
            LogType = LogType.All;
            LogResult = false;
            this.Priority = 2;
            LogParameters = parameters;
        }

        public ProcessResult Process(DataServiceRequest request)
        {
            if (!LogResult && LogType == LogType.All && ExceptionTemplate == null)
            {
                var context = request.Context;
                return Log(context);
            }
            return ProcessResult.Default;
        }

        public Task<ProcessResult> ProcessAsync(DataServiceRequest request)
        {
            return Task.FromResult(Process(request));
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            if (LogResult || LogType != LogType.All || ExceptionTemplate != null)
            {
                if (response.Exception != null && this.LogType == LogType.SuccessOnly) { return ProcessResult.Default; }
                if (response.Exception == null && this.LogType == LogType.FailOnly) { return ProcessResult.Default; }

                var context = response.Context;
                return Log(response, context);
            }
            return ProcessResult.Default;
        }

        private ProcessResult Log(DataServiceContext context)
        {
            //var logger = LogManager.GetLogger(context.ServiceInstance.GetType().FullName);
            var parameter = LogParameters.Select(p => new { Key = p, Value = context.Request.ReadParameter<string>(p, null) })
                .ToDictionary(k => k.Key, v => v.Value);
            var paramstr = SerializerFactory.Serialize("json", parameter);

            if (Template == null)
            {
                string log = "{0}.{1} \r\n\tparameters: \r\n\t{2}";
                context.Log.Info(string.Format(log, context.RouteInfo.ServiceName, context.RouteInfo.ActionName, paramstr));
            }
            else
            {
                context.Log.Info(BindItem(Template, context.Request, context.Response));
            }

            return ProcessResult.Default;

        }


        private ProcessResult Log(DataServiceResponse response, DataServiceContext context)
        {
            //var logger = LogManager.GetLogger(context.ServiceInstance.GetType().FullName);
            var parameter = LogParameters.Select(p => new { Key = p, Value = context.Request.ReadParameter<string>(p, null) })
                .ToDictionary(k => k.Key, v => v.Value);
            var paramstr = SerializerFactory.Serialize("json", parameter);
            var result = LogResult ? SerializerFactory.Serialize("json", (response.Result ?? "null")) : "NOT LOG";

            if (Template == null)
            {
                string log = "{0}.{1} \r\n\tparameters: \r\n\t{2} \r\n\tresult:\r\n\t{3}";
                if (response.Exception != null)
                {
                    context.Log.Warn(response.Exception, string.Format(log, context.RouteInfo.ServiceName, context.RouteInfo.ActionName, paramstr, result));
                }
                else
                {
                    context.Log.Info(response.Exception, string.Format(log, context.RouteInfo.ServiceName, context.RouteInfo.ActionName, paramstr, result));
                }
            }
            else
            {
                if (response.Exception == null)
                {
                    context.Log.Info(BindItem(Template, context.Request, context.Response));
                }
                else if (ExceptionTemplate != null)
                {
                    context.Log.Warn(response.Exception, BindItem(ExceptionTemplate, context.Request, context.Response));
                }
            }

            return ProcessResult.Default;
        }

        private string BindItem(string template, DataServiceRequest dataServiceRequest, DataServiceResponse dataServiceResponse)
        {
            var newItem = template;

            newItem = bindItemRegex.Replace(newItem, new MatchEvaluator(m =>
            {
                switch (m.Groups[1].Value)
                {
                    case "Result":
                        return SerializerFactory.Serialize("json", dataServiceResponse.Result, new Dictionary<String, object> { { "indent", true } });
                    case "Exception":
                        return SerializerFactory.Serialize("json", dataServiceResponse.Exception.Message, new Dictionary<String, object> { { "indent", true } });
                    default:
                        string value;
                        if (m.Groups[1].Value.StartsWith("$"))
                        {
                            value = dataServiceRequest.Context.ServerParameters[m.Groups[1].Value.Substring(1)].ToString();
                        }
                        else
                        {
                            value = dataServiceRequest.ReadParameter(m.Groups[1].Value, string.Empty);
                        }

                        try
                        {
                            value = JsonHelper.FormatJson(value);
                        }
                        catch
                        {
                        }
                        return string.Format("{0" + m.Groups[2].Value + "}", value);
                }
            }));
            return newItem;
        }


        private Regex bindItemRegex = new Regex(@"{([$\w]+)([^}]+)?}", RegexOptions.Compiled);
    }

    static class JsonHelper
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }

        private const string INDENT_STRING = "    ";
        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();

                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }


    public enum LogType
    {
        All,
        SuccessOnly,
        FailOnly
    }


}
