using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using AirMaster.Infrastructure.Serializer;
using AirMaster.Infrastructure.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    public class ResultWrapperAttribute : PostProcessorAttribute
    {
        public bool DirectOutputParameterToResult { get; set; }
        private int MaxResult { get; set; }
        private static readonly MethodInfo ToList;

        static ResultWrapperAttribute()
        {
            var enumerableMethods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            ToList = enumerableMethods.FirstOrDefault(m => m.Name == "ToList" && m.GetParameters().Length == 1);
        }

        public ResultWrapperAttribute()
        {
            DirectOutputParameterToResult = true;
            Priority = 0;
            MaxResult = 100;
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            if (response.Context.Serializer is SimpleSerializer) { return ProcessResult.Default; }
            var result = new Dictionary<string, object>();
            var context = response.Context;
            if (response.Result != null)
            {
                try
                {
                    var applyLimit = true;
                    if (context.ServerParameters.ContainsKey("ApplyLimit"))
                    {
                        applyLimit = context.ServerParameters["ApplyLimit"].To<bool>();
                    }
                    result["result"] = ExecuteQuery(response.Result, MaxResult, applyLimit);
                }
                catch (Exception ex)
                {
                    ex.Source = this.GetType().Name;
                    response.Exception = ex;
                }
            }

            result["success"] = response.Exception == null;

            if (response.Exception != null)
            {
                if (response.Exception is DataServiceException exception)
                {
                    result["errors"] = ProcessDataServiceException(exception, response.Context.DataServiceHost.Name);
                }
                else
                {
                    result["errors"] = ProcessException(response.Exception);    
                }
                result["message"] = response.Exception.Message;
                response.Exception = null;
            }

            if (response.OutputParams != null)
            {
                if (DirectOutputParameterToResult)
                {
                    foreach (var item in response.OutputParams)
                    {
                        result[item.Key] = item.Value;
                    }
                }
                else
                {
                    result["output"] = response.OutputParams;
                }
            }

            if (response.Context.IsDebug)
            {
                try
                {
                    WriteDebugInfo(result, response.Context);
                }
                catch { }
            }

            response.Result = result;
            return ProcessResult.Default;
        }



        internal static void WriteDebugInfo(Dictionary<string, object> result, DataServiceContext context)
        {
            if (context.IsDebug)
            {
                result["debug"] = new { elapsed = context.GetElpsedTime() };

                //if (context.Response.Result is ObjectQuery)
                //{
                //    result["debug"] = new { sql = ((ObjectQuery)context.Response.Result).ToTraceString(), parameters = ((ObjectQuery)context.Response.Result).Parameters, elapsed = context.GetElpsedTime() };
                //}
            }
        }

        internal static object ExecuteQuery(object result, int maxResult, bool applyLimit = true)
        {
            var interfaces = result.GetType().GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (interfaces.Count() > 0)
            {
                var entityType = interfaces.First().GetGenericArguments()[0];
                var isIQueryable = typeof(IQueryable<>).MakeGenericType(entityType).IsAssignableFrom(result.GetType());

                if (applyLimit && isIQueryable)
                {
                    result = PagableAttribute.Take(isIQueryable, result, entityType, maxResult);
                }

                if (isIQueryable)
                {
                    result = ToList.MakeGenericMethod(entityType).Invoke(null, new[] { result });
                }
            }
            return result;
        }

        internal static Dictionary<string, string> ProcessException(Exception error, Dictionary<string, string> dict = null)
        {
            if (dict == null) { dict = new Dictionary<string, string>(); }
            if (error == null) { return dict; }
            dict[error.Source ?? error.GetType().Name] = error.Message;
            return ProcessException(error.InnerException, dict);
        }
        
        internal static Dictionary<string, object> ProcessDataServiceException(DataServiceException exception, string source)
        {
            var dict = new Dictionary<string, object>
            {
                ["dataServiceException"] = true,
                ["type"] = exception.GetType().AssemblyQualifiedName
            };
            exception.Source = source;
            var ex = JObject.FromObject(exception);
            ex.Remove("InnerException");
            ex.Remove("StackTrace");
            dict["ex"] = ex;
            return dict;
        }
    }
}
