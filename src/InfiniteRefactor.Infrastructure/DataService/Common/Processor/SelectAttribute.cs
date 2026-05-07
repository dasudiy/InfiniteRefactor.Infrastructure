using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Processor
{
    public class SelectAttribute : PostProcessorAttribute
    {
        private LambdaExpression _expression;
        private MethodInfo _select, _querySelect;


        public SelectAttribute(Selector selector)
        {
            _expression = selector.Expression;

            var enumerableMethods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var queryableMethods = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public);

            _select = enumerableMethods.FirstOrDefault(m => m.Name == "Select" && m.GetParameters().Length == 2);
            _querySelect = queryableMethods.FirstOrDefault(m => m.Name == "Select" && m.GetParameters().Length == 2);
        }

        public override ProcessResult Process(DataServiceResponse response)
        {
            if (response.Result != null)
            {
                var interfaces = response.Result.GetType().GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (interfaces.Count() > 0)
                {
                    var entityType = interfaces.First().GetGenericArguments()[0];
                    object parameter = _expression;
                    MethodInfo m;

                    if (typeof(IQueryable<>).MakeGenericType(entityType).IsAssignableFrom(response.Result.GetType())) //IQueryable
                    {
                        m = _querySelect;
                    }
                    else
                    {
                        m = _select;
                        parameter = _expression.Compile();
                    }

                    response.Result = m.MakeGenericMethod(entityType, typeof(object)).Invoke(null, new[] { response.Result, parameter });
                }
            }
            return ProcessResult.Default;
        }
    }

    public class Selector
    {
        public LambdaExpression Expression { get; set; }

        public Selector(LambdaExpression expression)
        {
            this.Expression = expression;
        }

        public static Selector<E, object> Create<E>(Expression<Func<E, object>> selector) { return new Selector<E, object>(selector); }
    }

    public class Selector<E, R> : Selector
    {
        public Selector(Expression<Func<E, R>> selector)
            : base(selector)
        {

        }
    }
}
