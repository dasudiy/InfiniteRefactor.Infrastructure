using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public static class SortExtension
    {
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> enumerable, params SortableAttribute.SortInfo[] sorts)
        {
            object result = enumerable;
            bool isOrdered = false;
            foreach (var sortInfo in sorts)
            {
                var property = typeof(T).GetProperty(sortInfo.property);
                if (property == null) { throw new ArgumentException("Field '" + sortInfo.property + "' was not found."); }

                result = SortableAttribute.Sort(result, typeof(T), property, sortInfo, isOrdered);
                isOrdered = true;
            }

            return (IEnumerable<T>)result;
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> enumerable, string sort, string order)
        {
            var property = typeof(T).GetProperty(sort);
            if (property == null) { throw new ArgumentException("Field '" + sort + "' was not found."); }

            return (IEnumerable<T>)SortableAttribute.Sort(enumerable, typeof(T), property,
                new SortableAttribute.SortInfo
                {
                    property = sort,
                    direction = (SortableAttribute.SortDirection)Enum.Parse(typeof(SortableAttribute.SortDirection), order, true)
                }, false);
        }
    }

    public class SortableAttribute : PostProcessorAttribute
    {
        private static MethodInfo _orderBy, _orderByDescending, _queryableOrderBy, _queryableOrderByDescending, _thenBy, _thenByDescending, _queryThenBy, _queryThenByDescending;
        private static Type _selectorType;
        public bool MultiSorting { get; set; }

        static SortableAttribute()
        {
            var enumerableMethods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var queryableMethods = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            _orderBy = enumerableMethods.FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            _orderByDescending = enumerableMethods.FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            _thenBy = enumerableMethods.FirstOrDefault(m => m.Name == "ThenBy" && m.GetParameters().Length == 2);
            _thenByDescending = enumerableMethods.FirstOrDefault(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2);

            _queryableOrderBy = queryableMethods.FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            _queryableOrderByDescending = queryableMethods.FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            _queryThenBy = queryableMethods.FirstOrDefault(m => m.Name == "ThenBy" && m.GetParameters().Length == 2);
            _queryThenByDescending = queryableMethods.FirstOrDefault(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2);


            _selectorType = typeof(Func<,>);
        }

        public SortableAttribute()
        {
            Priority = 2;
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            if (response.Result != null)
            {
                bool isOrdered = false;
                var interfaces = response.Result.GetType().GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (interfaces.Count() > 0)
                {
                    var entityType = interfaces.First().GetGenericArguments()[0];

                    if (MultiSorting)
                    {
                        var sorts = response.Context.Request.ReadParameter<List<SortInfo>>("sort", null);
                        if (sorts != null && sorts.Count > 0)
                        {

                            foreach (var sortInfo in sorts)
                            {
                                var property = entityType.GetProperty(sortInfo.property);
                                if (property == null) { throw new ArgumentException("Field '" + sortInfo.property + "' was not found."); }

                                response.Result = Sort(response.Result, entityType, property, sortInfo, isOrdered);
                                isOrdered = true;
                            }

                        }
                    }
                    else
                    {
                        var sort = response.Context.Request.ReadParameter<string>("sort", null);
                        var direction = response.Context.Request.ReadParameter<string>("order", "ASC");

                        if (!string.IsNullOrEmpty(sort))
                        {
                            var property = entityType.GetProperty(sort);
                            if (property == null) { throw new ArgumentException("Field '" + sort + "' was not found."); }

                            response.Result = Sort(response.Result, entityType, property, new SortInfo { direction = (SortDirection)Enum.Parse(typeof(SortDirection), direction, true), property = sort }, isOrdered);
                            isOrdered = true;
                        }
                    }
                }
            }
            return ProcessResult.Default;
        }

        internal static object Sort(object source, Type entityType, PropertyInfo property, SortInfo sortInfo, bool isOrdered)
        {
            var selectorExpression = MakeKeySelectorExpression(entityType, property);
            object parameter = selectorExpression;
            MethodInfo m;
            if (isOrdered)
            {
                if (typeof(IQueryable<>).MakeGenericType(entityType).IsAssignableFrom(source.GetType())) //IQueryable
                {
                    m = sortInfo.direction == SortDirection.ASC ? _queryThenBy : _queryThenByDescending;
                }
                else
                {
                    m = sortInfo.direction == SortDirection.ASC ? _thenBy : _thenByDescending;
                    parameter = selectorExpression.Compile();
                }
            }
            else
            {
                if (typeof(IQueryable<>).MakeGenericType(entityType).IsAssignableFrom(source.GetType())) //IQueryable
                {
                    m = sortInfo.direction == SortDirection.ASC ? _queryableOrderBy : _queryableOrderByDescending;
                }
                else
                {
                    m = sortInfo.direction == SortDirection.ASC ? _orderBy : _orderByDescending;
                    parameter = selectorExpression.Compile();
                }
            }
            m = m.MakeGenericMethod(entityType, property.PropertyType);
            return m.Invoke(null, new[] { source, parameter });
        }

        private static LambdaExpression MakeKeySelectorExpression(Type entityType, PropertyInfo property)
        {
            var selectorType = _selectorType.MakeGenericType(entityType, property.PropertyType);
            var param = ParameterExpression.Parameter(entityType, "EntityType");

            return Expression.Lambda(selectorType,
                Expression.Property(param, property.GetGetMethod()), param);
        }

        public class SortInfo
        {
            public string property;
            public SortDirection direction;
        }

        public enum SortDirection
        {
            ASC,
            DESC
        }
    }
}
