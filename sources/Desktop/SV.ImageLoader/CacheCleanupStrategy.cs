
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
        public IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanup(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemWeightEvaluator, long weightToFree)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (itemWeightEvaluator == null)
            {
                throw new ArgumentNullException("itemWeightEvaluator");
            }

            return this.GetItemsToCleanupInternal(items, itemWeightEvaluator, weightToFree);
        }

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
        protected abstract IEnumerable<CacheImageLoader.CacheItem> GetItemsToCleanupInternal(IEnumerable<CacheImageLoader.CacheItem> items, Func<CacheImageLoader.CacheItem, long> itemWeightEvaluator, long weightToFree);
    }
}
