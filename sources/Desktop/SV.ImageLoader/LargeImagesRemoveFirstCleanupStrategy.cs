
namespace SV.ImageLoader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Defines a cache cleanup strategy which removes large copies of the images first.
    /// </summary>
    public class LargeImagesRemoveFirstCleanupStrategy : CacheCleanupStrategy
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
        protected override IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanupInternal(IReadOnlyDictionary<string, List<CacheImageLoader.CacheItem>> items, Func<CacheImageLoader.CacheItem, long> itemSizeEvaluator, long sizeToFree)
        {
            var itemsToDelete = new List<CacheImageLoader.CacheItem>();

            if (items.Any())
            {
                var itemsGroupedByKeys = items.Values;
                var releasedSize = (long)0;

                var maxGroupSize = itemsGroupedByKeys.Max(v => v.Count);
                var currentSize = maxGroupSize;

                while (currentSize > 0 && releasedSize < sizeToFree)
                {
                    foreach (var item in from groupedItems in itemsGroupedByKeys
                                         where groupedItems.Count >= currentSize
                                         select groupedItems[currentSize - 1])
                    {
                        itemsToDelete.Add(item);
                        releasedSize += itemSizeEvaluator(item);

                        if (releasedSize >= sizeToFree)
                        {
                            break;
                        }
                    }

                    currentSize--;
                }
            }

            return itemsToDelete;
        }
    }
}
