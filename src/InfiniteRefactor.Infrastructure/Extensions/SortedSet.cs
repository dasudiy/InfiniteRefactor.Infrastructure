using System.Collections.Generic;

namespace InfiniteRefactor.Infrastructure.Extensions
{
    public static class SortedSetExtension
    {
        public static void AddRange<T>(this SortedSet<T> source, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                source.Add(item);
            }
        }
    }
}
