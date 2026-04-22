using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    public class PagableAttribute : PostProcessorAttribute
    {
        public string TotalCountParameter { get; set; }
        public string StartParameter { get; set; }
        public string LimitParameter { get; set; }
        public string PageParameter { get; set; }

        private static MethodInfo _count, _skip, _take;
        private static MethodInfo _queryCount, _querySkip, _queryTake;

        static PagableAttribute()
        {
            var methods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var queryMethods = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public);

            _count = methods.FirstOrDefault(m => m.Name == "Count" && m.GetParameters().Length == 1);
            _skip = methods.FirstOrDefault(m => m.Name == "Skip" && m.GetParameters().Length == 2);
            _take = methods.FirstOrDefault(m => m.Name == "Take" && m.GetParameters().Length == 2);

            _queryCount = queryMethods.FirstOrDefault(m => m.Name == "Count" && m.GetParameters().Length == 1);
            _querySkip = queryMethods.FirstOrDefault(m => m.Name == "Skip" && m.GetParameters().Length == 2);
            _queryTake = queryMethods.FirstOrDefault(m => m.Name == "Take" && m.GetParameters().Length == 2);

        }

        public PagableAttribute()
        {
            Priority = 1;
            TotalCountParameter = "total";
            StartParameter = "start";
            LimitParameter = "limit";
            PageParameter = "page";
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            if (response.Result != null)
            {
                var interfaces = response.Result.GetType().GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (interfaces.Count() > 0)
                {
                    var entityType = interfaces.First().GetGenericArguments()[0];
                    var result = response.Result;
                    var isIQueryable = typeof(IQueryable<>).MakeGenericType(entityType).IsAssignableFrom(result.GetType());

                    var start = response.Context.Request.ReadParameter<int?>(StartParameter, null);
                    var limit = response.Context.Request.ReadParameter<int?>(LimitParameter, null);
                    if (start == null) { start = (response.Context.Request.ReadParameter<int?>(PageParameter, null) - 1) * limit; }

                    response.OutputParams[TotalCountParameter] = Count(isIQueryable, result, entityType);

                    if (start.HasValue)
                    {
                        result = Skip(isIQueryable, result, entityType, start.Value);
                    }
                    if (limit.HasValue)
                    {
                        result = Take(isIQueryable, result, entityType, limit.Value);
                    }

                    response.Result = result;
                }
            }
            return ProcessResult.Default;
        }

        public static int Count(bool isIQueryable, object source, Type entityType)
        {
            return (int)(isIQueryable ? _queryCount : _count).MakeGenericMethod(entityType).Invoke(null, new[] { source });
        }

        public static object Skip(bool isIQueryable, object source, Type entityType, int count)
        {
            return (isIQueryable ? _querySkip : _skip).MakeGenericMethod(entityType).Invoke(null, new[] { source, count });
        }

        public static object Take(bool isIQueryable, object source, Type entityType, int count)
        {
            return (isIQueryable ? _queryTake : _take).MakeGenericMethod(entityType).Invoke(null, new[] { source, count });
        }
    }
}
