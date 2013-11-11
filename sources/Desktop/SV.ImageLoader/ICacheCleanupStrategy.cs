
namespace SV.ImageLoader
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines methods for finding the less significant items that can be removed from cache.
    /// </summary>
    public interface ICacheCleanupStrategy
    {
        /// <summary>
        ///     Returns items that can be cleaned up.
        /// </summary>
        /// <param name="items">
        ///     All available items.
        /// </param>
        /// <param name="itemSizeEvaluator">
        ///     The evaluator to use for calculation a size of the item in cache. The bigger this number, the heavier item in a cache, the less such items we
        ///     need to remove to free desired space.
        /// </param>
        /// <param name="sizeToFree">
        ///     The total size to free.
        /// </param>
        /// <returns>
        ///     The list of items that can be cleaned up.
        /// </returns>
        IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanup(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemSizeEvaluator, long sizeToFree);
    }
}
