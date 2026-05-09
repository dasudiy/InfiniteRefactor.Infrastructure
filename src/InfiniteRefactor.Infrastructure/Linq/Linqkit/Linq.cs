// LinqKit by Joseph Albahari and Tomas Petricek
// http://www.albahari.com/nutshell/linqkit.html
// Licensed under the MIT License
// https://github.com/scottksmith95/LINQKit

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace InfiniteRefactor.Infrastructure.Linq.Linqkit
{
    /// <summary>
    /// Another good idea by Tomas Petricek.
    /// See http://tomasp.net/blog/dynamic-linq-queries.aspx for information on how it's used.
    /// </summary>
    public static class Linq
    {
        // Returns the given anonymous method as a lambda expression
        public static Expression<Func<T, TResult>> Expr<T, TResult>(Expression<Func<T, TResult>> expr)
        {
            return expr;
        }

        // Returns the given anonymous function as a Func delegate
        public static Func<T, TResult> Func<T, TResult>(Func<T, TResult> expr)
        {
            return expr;
        }

        public static Expression<Func<TElement, bool>> BuildContainsExpression<TElement, TValue>(Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values)
        {

            if (null == valueSelector) { throw new ArgumentNullException("valueSelector"); }
            if (null == values) { throw new ArgumentNullException("values"); }
            ParameterExpression p = valueSelector.Parameters.Single();
            // p => valueSelector(p) == values[0] || valueSelector(p) == ...
            if (!values.Any())
            {
                return e => false;
            }

            var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));

            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

    }
}
