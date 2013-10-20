
namespace SV.ImageLoader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Defines a cache cleanup strategy which removes items by oldest access time. So, the more rarely items will be removed first.
    /// </summary>
    public class RarelyUsedItemsRemoveFirstCleanupStrategy : CacheCleanupStrategy
    {
        /// <summary>
        ///     Returns items that can be cleaned up using concrete strategy.
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
        protected override IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanupInternal(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemWeightEvaluator, long weightToFree)
        {
            var itemsToDelete = new List<CacheImageLoader.CacheItem>();
            long weight = 0;

            foreach (var cacheItem in items.OrderBy(i => i.LastAccessTime))
            {
                itemsToDelete.Add(cacheItem);
                weight += itemWeightEvaluator(cacheItem);

                if (weight > weightToFree)
                {
                    break;
                }
            }

            return itemsToDelete;
        }
    }
}
