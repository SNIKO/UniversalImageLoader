
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
        ///     Returns items that can be cleaned up.
        /// </summary>
        /// <param name="items">
        ///     Available items for cleanup. Each dictionary item represents an image with specific key (uri). The value of the dictionary item is a list of concrete instances 
        ///     of the image ordered by image size, from smallest to largest.
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
        protected override IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanupInternal(IReadOnlyDictionary<string, List<CacheImageLoader.CacheItem>> items, Func<CacheImageLoader.CacheItem, ulong> itemSizeEvaluator, ulong sizeToFree)
        {
            var itemsToDelete = new List<CacheImageLoader.CacheItem>();
            ulong weight = 0;

            var allItems = new List<CacheImageLoader.CacheItem>();
            foreach (var sameKeyItems in items.Values)
            {
                allItems.AddRange(sameKeyItems);
            }

            foreach (var cacheItem in allItems.OrderBy(i => i.LastAccessTime))
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
