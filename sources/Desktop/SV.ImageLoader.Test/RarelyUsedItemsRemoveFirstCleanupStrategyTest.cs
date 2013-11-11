
namespace SV.ImageLoader.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SV.ImageLoader.Test.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class RarelyUsedItemsRemoveFirstCleanupStrategyTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetItemsToCleanup_ItemsCollectionIsNull_ThrowException()
        {
            var strategy = new RarelyUsedItemsRemoveFirstCleanupStrategy();

            strategy.GetItemsToCleanup(null, i => i.Size, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetItemsToCleanup_SizeEvaluatorIsNull_ThrowException()
        {
            var strategy = new RarelyUsedItemsRemoveFirstCleanupStrategy();
            var cacheItems = new[] { new CacheImageLoader.CacheItem() };

            strategy.GetItemsToCleanup(cacheItems, null, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetItemsToCleanup_SizeToFreeIsLessThanOne_ThrowException()
        {
            var strategy = new RarelyUsedItemsRemoveFirstCleanupStrategy();
            var cacheItems = new[] { new CacheImageLoader.CacheItem() };

            strategy.GetItemsToCleanup(cacheItems, i => i.Size, 0);
        }

        [TestMethod]
        public void GetItemsToCleanup_ItemsCollectionIsEmpty_ReturnEmptyCollection()
        {
            var strategy = new RarelyUsedItemsRemoveFirstCleanupStrategy();
            var cacheItems = new List<CacheImageLoader.CacheItem>();

            var itemsToDelete = strategy.GetItemsToCleanup(cacheItems, i => i.Size, 100);
            Assert.AreEqual(0, itemsToDelete.Count(), "Items to delete");
        }

        [TestMethod]
        public void GetItemsToCleanup_SomeItemsAreSpecified_ReturnOldestItems()
        {
            var strategy = new RarelyUsedItemsRemoveFirstCleanupStrategy();
            var cacheItems = new[]
                {
                    new CacheImageLoader.CacheItem
                        {
                            Size = 100,
                            LastAccessTime = 5.DaysAgo()
                        },
                    new CacheImageLoader.CacheItem
                        {
                            Size = 50,
                            LastAccessTime = 4.DaysAgo()
                        },
                    new CacheImageLoader.CacheItem
                        {
                            Size = 50,
                            LastAccessTime = 3.DaysAgo()
                        },
                    new CacheImageLoader.CacheItem
                        {
                            Size = 10,
                            LastAccessTime = 2.DaysAgo()
                        },
                    new CacheImageLoader.CacheItem
                        {
                            Size = 10,
                            LastAccessTime = 2.DaysAgo()
                        }
                };

            var expectedItems = cacheItems.Take(3);
            var itemsToDelete = strategy.GetItemsToCleanup(cacheItems, i => i.Size, 200);

            itemsToDelete.CheckContainsSameItemsAs(expectedItems);
        }
    }
}
