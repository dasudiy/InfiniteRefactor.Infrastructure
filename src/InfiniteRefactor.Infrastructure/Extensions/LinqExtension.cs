using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirMaster.Infrastructure.Extensions
{
    public static class LinqExtension
    {
        public static List<Tuple<System.Linq.Expressions.Expression<Func<T, bool>>, System.Linq.Expressions.Expression<Func<T, bool>>>> AppendMutexPredicate<T>(this IQueryable<T> q, System.Linq.Expressions.Expression<Func<T, bool>> t, System.Linq.Expressions.Expression<Func<T, bool>> f)
        {
            var container = new List<System.Tuple<System.Linq.Expressions.Expression<System.Func<T, bool>>, System.Linq.Expressions.Expression<System.Func<T, bool>>>>();
            container.Add(Tuple.Create(t, f));
            return container;
        }
        public static List<Tuple<System.Linq.Expressions.Expression<Func<T, bool>>, System.Linq.Expressions.Expression<Func<T, bool>>>> AppendMutexPredicate<T>(this List<Tuple<System.Linq.Expressions.Expression<Func<T, bool>>, System.Linq.Expressions.Expression<Func<T, bool>>>> container, System.Linq.Expressions.Expression<Func<T, bool>> t, System.Linq.Expressions.Expression<Func<T, bool>> f)
        {
            container.Add(Tuple.Create(t, f));
            return container;
        }
        public static List<PredicateGroup<T>> BuildPredicateGroups<T>(this List<Tuple<System.Linq.Expressions.Expression<Func<T, bool>>, System.Linq.Expressions.Expression<Func<T, bool>>>> mutexPredicates)
        {
            var predicateGroups = new List<PredicateGroup<T>>();

            foreach (var f in mutexPredicates)
            {
                if (predicateGroups.Count == 0)
                {
                    var group1 = new PredicateGroup<T>(true);
                    group1.AddPredicate(true, f.Item1);
                    predicateGroups.Add(group1);
                    var group2 = new PredicateGroup<T>(true);
                    group2.AddPredicate(false, f.Item2);
                    predicateGroups.Add(group2);
                    continue;
                }
                foreach (var b in predicateGroups.ToArray())
                {
                    var cb = b.Clone();
                    b.AddPredicate(true, f.Item1);
                    cb.AddPredicate(false, f.Item2);
                    predicateGroups.Add(cb);
                }
            }
            return predicateGroups.OrderByDescending(c => c.Priority).ToList();
        }
        public static IQueryable<T> MatchBest<T>(this IQueryable<T> q, List<PredicateGroup<T>> predicateGroups)
        {
            IQueryable<T> x = null;
            foreach (var predicateGroup in predicateGroups)
            {
                x = q;
                foreach (var predicate in predicateGroup.Predicates)
                {
                    x = x.Where(predicate);
                }
                if (x.Any()) { break; }
                x = null;
            }
            return x ?? q.Where(c => 0 == 1);
        }
    }

    public struct PredicateGroup<T>
    {
        private List<System.Linq.Expressions.Expression<Func<T, bool>>> _predicates { get; set; }
        public List<System.Linq.Expressions.Expression<Func<T, bool>>> Predicates => _predicates;

        private List<bool> _value { get; set; }
        public int Priority
        {
            get
            {
                var s = string.Join(string.Empty, this._value.Select(c => c ? 1 : 0));
                return string.IsNullOrWhiteSpace(s) ? -1 : Convert.ToInt32(s);
            }
        }

        public PredicateGroup(bool initial)
        {
            this._predicates = new List<System.Linq.Expressions.Expression<System.Func<T, bool>>>();
            this._value = new List<bool>();
        }

        public void AddPredicate(bool t, System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            this._predicates.Add(predicate);
            this._value.Add(t);
        }

        public PredicateGroup<T> Clone()
        {
            var clone = new PredicateGroup<T>(true);
            clone._predicates.AddRange(this._predicates);
            clone._value.AddRange(this._value);
            return clone;
        }
    }
}
