using System.Collections.Generic;

namespace ZLinq
{
    public static class ZLinqExtensions
    {
        public static IEnumerable<T> ToEnumerable<TEnumerator, T>(this ValueEnumerable<TEnumerator, T> valueEnumerable)
            where TEnumerator : struct, IValueEnumerator<T>
        {
            using var e = valueEnumerable.Enumerator;
            while (e.TryGetNext(out var current))
            {
                yield return current;
            }
        }
    }
}
