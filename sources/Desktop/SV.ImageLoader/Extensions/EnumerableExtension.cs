
namespace SV.ImageLoader.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtension
    {
        public static ulong Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector)
        {
            return source.Aggregate<TSource, ulong>(0, (current, item) => current + selector(item));
        }
    }
}
