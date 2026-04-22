using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.Extensions
{
    public static class AsyncForeachExtension
    {
        //use EnumerableExtension.ForeachAsync instead
        //public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
        //{
        //    if (source == null)
        //    {
        //        return;
        //    }
        //    foreach (T item in source)
        //    {
        //        await action(item);
        //    }
        //}

        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action, int dop)
        {
            if (source == null)
            {
                return;
            }

            await Task.WhenAll(Partitioner.Create(source).GetPartitions(dop).Select(g => Task.Run(async () =>
            {
                using (g)
                {
                    while (g.MoveNext())
                    {
                        await action(g.Current);
                    }
                }
            })));
        }

        public static async Task ParallelForEachAsync<T>(this T[] source, Func<T, Task> action, int dop, bool balanced)
        {
            if (source == null)
            {
                return;
            }

            await Task.WhenAll(Partitioner.Create(source, balanced).GetPartitions(dop).Select(g => Task.Run(async () =>
            {
                using (g)
                {
                    while (g.MoveNext())
                    {
                        await action(g.Current);
                    }
                }
            })));
        }

        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, Task> action, int dop, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                return;
            }

            await Task.WhenAll(Partitioner.Create(source).GetPartitions(dop).Select(g => Task.Run(async () =>
            {
                using (g)
                {
                    while (g.MoveNext() && !cancellationToken.IsCancellationRequested)
                    {
                        await action(g.Current, cancellationToken);
                    }
                }
            })));
        }

        public static async Task ParallelForEachAsync<T>(this T[] source, Func<T, CancellationToken, Task> action, int dop, bool balanced, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                return;
            }

            await Task.WhenAll(Partitioner.Create(source, balanced).GetPartitions(dop).Select(g => Task.Run(async () =>
            {
                using (g)
                {
                    while (g.MoveNext() && !cancellationToken.IsCancellationRequested)
                    {
                        await action(g.Current, cancellationToken);
                    }
                }
            })));
        }

    }
}
