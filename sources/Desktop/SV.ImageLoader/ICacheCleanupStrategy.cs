
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
        ///     Available items.
        /// </param>
        /// <param name="itemWeightEvaluator">
        ///     The evaluator to use for calculation a weight of the item in cache. The weight is characterized by a number of type <see cref="long"/>. The bigger this number, the heavier item in a cache, the less such items we
        ///     need to remove to free desired space.
        /// </param>
        /// <param name="weightToFree">
        ///     The total weight to free.
        /// </param>
        /// <returns>
        ///     The list of items that can be cleaned up.
        /// </returns>
        IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanup(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemWeightEvaluator, long weightToFree);
    }
}
