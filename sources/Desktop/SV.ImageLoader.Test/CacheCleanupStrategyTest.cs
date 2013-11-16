
namespace SV.ImageLoader.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public abstract class CacheCleanupStrategyTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetItemsToCleanup_ItemsCollectionIsNull_ThrowException()
        {
            var strategy = this.GetCacheCleanupStrategyInstance();

            strategy.GetItemsToCleanup(null, i => i.Size, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetItemsToCleanup_SizeEvaluatorIsNull_ThrowException()
        {
            var strategy = this.GetCacheCleanupStrategyInstance();
            var cacheItems = new Dictionary<string, List<CacheImageLoader.CacheItem>>();
            cacheItems["McLaren"] = new List<CacheImageLoader.CacheItem> { new CacheImageLoader.CacheItem() };

            strategy.GetItemsToCleanup(cacheItems, null, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetItemsToCleanup_SizeToFreeIsLessThanOne_ThrowException()
        {
            var strategy = this.GetCacheCleanupStrategyInstance();
            var cacheItems = new Dictionary<string, List<CacheImageLoader.CacheItem>>();
            cacheItems["McLaren"] = new List<CacheImageLoader.CacheItem> { new CacheImageLoader.CacheItem() };

            strategy.GetItemsToCleanup(cacheItems, i => i.Size, 0);
        }

        [TestMethod]
        public void GetItemsToCleanup_ItemsCollectionIsEmpty_ReturnEmptyCollection()
        {
            var strategy = this.GetCacheCleanupStrategyInstance();
            var cacheItems = new Dictionary<string, List<CacheImageLoader.CacheItem>>();

            var itemsToDelete = strategy.GetItemsToCleanup(cacheItems, i => i.Size, 100);
            Assert.AreEqual(0, itemsToDelete.Count(), "Items to delete");
        }

        protected abstract CacheCleanupStrategy GetCacheCleanupStrategyInstance();
    }
}
