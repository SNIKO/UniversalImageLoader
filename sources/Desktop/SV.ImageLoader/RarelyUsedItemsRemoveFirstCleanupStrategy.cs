
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
        protected override IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanupInternal(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemSizeEvaluator, long sizeToFree)
        {
            var itemsToDelete = new List<CacheImageLoader.CacheItem>();
            long weight = 0;

            foreach (var cacheItem in items.OrderBy(i => i.LastAccessTime))
            {
                itemsToDelete.Add(cacheItem);
                weight += itemSizeEvaluator(cacheItem);

                if (weight >= sizeToFree)
                {
                    break;
                }
            }

            return itemsToDelete;
        }
    }
}
