
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
        public IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanup(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemSizeEvaluator, long sizeToFree)
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
        protected abstract IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanupInternal(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemSizeEvaluator, long sizeToFree);
    }
}
