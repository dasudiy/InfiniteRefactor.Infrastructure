using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using AirMaster.Infrastructure.Serializer;
using AirMaster.Infrastructure.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    public static class FilterExtension
    {
        public static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable, params FilterableAttribute.FilterInfo[] filters)
        {
            return FilterableAttribute.ProcessFilter(enumerable, filters);
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> enumerable, params FilterableAttribute.FilterInfo[] filters)
        {
            return FilterableAttribute.ProcessFilter(enumerable, filters);
        }
    }

    public class FilterableAttribute : PostProcessorAttribute
    {
        private static MethodInfo _where, _queryableWhere, _contains, _queryableContains;
        private static Type _predicateType;

        static FilterableAttribute()
        {
            var enumerableMethods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var queryableMethods = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            _where = enumerableMethods.FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2);
            _contains = enumerableMethods.FirstOrDefault(m => m.Name == "Contains" && m.GetParameters().Length == 2);
            _queryableWhere = queryableMethods.FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2);
            _queryableContains = queryableMethods.FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2);
            _predicateType = typeof(Func<,>);
        }

        public FilterableAttribute()
        {
            Priority = 3;
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            if (response.Result != null)
            {
                var filters = response.Context.Request.ReadParameter<FilterInfo[]>("filter", null);
                if (filters != null && filters.Length > 0)
                {
                    var interfaces = response.Result.GetType().GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    if (interfaces.Count() > 0)
                    {
                        var entityType = interfaces.First().GetGenericArguments()[0];
                        //var result = response.Result;
                        object result;


                        result = ProcessFilter(response.Result as IEnumerable, filters, entityType, response.Context.Serializer);

                        response.Result = result;
                    }
                }
            }

            return ProcessResult.Default;
        }

        public static IEnumerable<T> ProcessFilter<T>(IEnumerable<T> source, params FilterInfo[] filters)
        {
            return ProcessFilter(source, filters, typeof(T), SerializerFactory.GetDefault()) as IEnumerable<T>;
        }

        public static IQueryable<T> ProcessFilter<T>(IQueryable<T> source, params FilterInfo[] filters)
        {
            return ProcessFilter(source, filters, typeof(T), SerializerFactory.GetDefault()) as IQueryable<T>;
        }

        public static object ProcessFilter(IEnumerable source, FilterInfo[] filters, Type entityType, ISerializer serializer)
        {
            if (filters == null || filters.Length == 0) { return source; }
            object result;
            var preicateType = _predicateType.MakeGenericType(entityType, typeof(bool));
            var entityTypeParam = ParameterExpression.Parameter(entityType, "EntityType");
            Expression body = Expression.Constant(true, typeof(bool));

            foreach (var filter in filters.Where(t => t != null))
            {
                var property = entityType.GetProperty(filter.field);
                if (property == null) { throw new ArgumentException("Field '" + filter.field + "' was not found."); }

                var e = MakePredicateExpression(property, filter, entityTypeParam, serializer);
                body = Expression.And(body, e);
            }

            var lambda = Expression.Lambda(preicateType, body, entityTypeParam);
            result = Filter(source, entityType, lambda);
            return result;
        }

        private static object Filter(object source, Type entityType, LambdaExpression expression)
        {
            MethodInfo m;
            object parameter = expression;
            if (typeof(IQueryable<>).MakeGenericType(entityType).IsAssignableFrom(source.GetType())) //IQueryable
            {
                m = _queryableWhere;
            }
            else
            {
                m = _where;
                parameter = expression.Compile();
            }
            m = m.MakeGenericMethod(entityType);
            return m.Invoke(null, new[] { source, parameter });
        }

        private static Expression MakePredicateExpression(PropertyInfo property, FilterInfo filter, ParameterExpression entityTypeParam, ISerializer serializer)
        {
            Expression expression = null;
            var v = filter.value.To(property.PropertyType);

            if (filter.type == "string")
            {
                expression = Expression.Call(
                        Expression.Property(entityTypeParam, property.GetGetMethod()),
                        "Contains",
                        null,
                        Expression.Constant(v, property.PropertyType));
            }
            else if (filter.type == "list" || filter.type == "boolean")
            {
                expression = Expression.Equal(
                    Expression.Property(entityTypeParam, property.GetGetMethod()),
                    Expression.Constant(v, property.PropertyType)
                );
            }
            else if (filter.type == "in")
            {
                expression = Expression.Call(
                    _contains.MakeGenericMethod(property.PropertyType),
                        Expression.Constant(serializer.Deserialize(v.ToString(), property.PropertyType.MakeArrayType()), property.PropertyType.MakeArrayType()),
                        Expression.Property(entityTypeParam, property.GetGetMethod()));
            }
            else
            {
                switch (filter.comparison)
                {
                    case Comparison.ne:
                        expression = Expression.NotEqual(
                                Expression.Property(entityTypeParam, property.GetGetMethod()),
                                Expression.Constant(v, property.PropertyType)
                            );
                        break;
                    case Comparison.eq:
                        expression = Expression.Equal(
                                Expression.Property(entityTypeParam, property.GetGetMethod()),
                                Expression.Constant(v, property.PropertyType)
                            );
                        break;
                    case Comparison.gt:
                        expression = Expression.GreaterThan(
                                Expression.Property(entityTypeParam, property.GetGetMethod()),
                                Expression.Constant(v, property.PropertyType)
                            );
                        break;
                    case Comparison.lt:
                        expression = Expression.LessThan(
                                Expression.Property(entityTypeParam, property.GetGetMethod()),
                                Expression.Constant(v, property.PropertyType)
                            );
                        break;
                    case Comparison.ge:
                        expression = Expression.GreaterThanOrEqual(
                                Expression.Property(entityTypeParam, property.GetGetMethod()),
                                Expression.Constant(v, property.PropertyType)
                            );
                        break;
                    case Comparison.le:
                        expression = Expression.LessThanOrEqual(
                                Expression.Property(entityTypeParam, property.GetGetMethod()),
                                Expression.Constant(v, property.PropertyType)
                            );
                        break;
                    default:
                        break;
                }
            }

            return expression;
        }

        public class FilterInfo
        {
            public string type { get; set; }
            public object value { get; set; }
            public string field { get; set; }
            public Comparison? comparison { get; set; }
        }

        public enum Comparison
        {
            ne,
            eq,
            gt,
            lt,
            ge,
            le
        }
    }
}
