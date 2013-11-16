
namespace SV.ImageLoader
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     The base class for all strategies.
    /// </summary>
    public abstract class CacheCleanupStrategy : ICacheCleanupStrategy
    {
        /// <summary>
        ///     Gets a <see cref="RarelyUsedItemsRemoveFirstCleanupStrategy"/> instance.
        /// </summary>
        public static CacheCleanupStrategy RarelyUsedItemsRemoveFirst
        {
            get
            {
                return new RarelyUsedItemsRemoveFirstCleanupStrategy();
            }
        }

        /// <summary>
        ///     Gets a <see cref="SmallImagesRemoveFirstCleanupStrategy"/> instance.
        /// </summary>
        public static CacheCleanupStrategy SmallInstancesOfSameImageRemoveFirst
        {
            get
            {
                return new SmallImagesRemoveFirstCleanupStrategy();
            }
        }

        /// <summary>
        ///     Gets a <see cref="LargeImagesRemoveFirstCleanupStrategy"/> instance.
        /// </summary>
        public static CacheCleanupStrategy LargeInstancesOfSameImageRemoveFirst
        {
            get
            {
                return new LargeImagesRemoveFirstCleanupStrategy();
            }
        }

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
        public IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanup(IReadOnlyDictionary<string, List<CacheImageLoader.CacheItem>> items, Func<CacheImageLoader.CacheItem, long> itemSizeEvaluator, long sizeToFree)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (itemSizeEvaluator == null)
            {
                throw new ArgumentNullException("itemSizeEvaluator");
            }

            if (sizeToFree <= 1)
            {
                throw new ArgumentOutOfRangeException("sizeToFree", "The size should be greater than 0");
            }

            return this.GetItemsToCleanupInternal(items, itemSizeEvaluator, sizeToFree);
        }

        /// <summary>
        ///     Returns items that can be cleaned up using concrete strategy.
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
        protected abstract IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanupInternal(IReadOnlyDictionary<string, List<CacheImageLoader.CacheItem>> items, Func<CacheImageLoader.CacheItem, long> itemSizeEvaluator, long sizeToFree);
    }
}
