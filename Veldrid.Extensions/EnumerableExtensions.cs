using System;
using System.Collections.Generic;
using System.Text;

namespace Veldrid.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
        {
            foreach (var first in enumerable)
                yield return first;
            foreach (var second in other)
                yield return second;
        }
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T other)
        {
            foreach (var first in enumerable)
                yield return first;
            yield return other;
        }
    }
}
