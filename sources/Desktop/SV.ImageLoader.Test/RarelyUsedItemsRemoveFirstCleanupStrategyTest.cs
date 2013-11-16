
namespace SV.ImageLoader.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SV.ImageLoader.Test.Extensions;
    using System.Collections.Generic;

    [TestClass]
    public class RarelyUsedItemsRemoveFirstCleanupStrategyTest : CacheCleanupStrategyTest
    {
        [TestMethod]
        public void GetItemsToCleanup_SomeItemsAreSpecified_ReturnItemsAccordingToStrategy()
        {
            var strategy = new RarelyUsedItemsRemoveFirstCleanupStrategy();
            var cacheItems = new Dictionary<string, List<CacheImageLoader.CacheItem>>();

            cacheItems["McLaren"] = new List<CacheImageLoader.CacheItem>
                {
                    new CacheImageLoader.CacheItem { Size = 100, LastAccessTime = 5.DaysAgo() },
                    new CacheImageLoader.CacheItem { Size = 50, LastAccessTime = 3.DaysAgo() },
                    new CacheImageLoader.CacheItem { Size = 10, LastAccessTime = 2.DaysAgo() },
                };

            cacheItems["MU"] = new List<CacheImageLoader.CacheItem> {
                    new CacheImageLoader.CacheItem { Size = 50, LastAccessTime = 4.DaysAgo() },
                    new CacheImageLoader.CacheItem { Size = 10, LastAccessTime = 2.DaysAgo() },
                    new CacheImageLoader.CacheItem { Size = 10, LastAccessTime = 2.DaysAgo() }
                };

            var expectedItems = new[] { cacheItems["McLaren"][0], cacheItems["McLaren"][1], cacheItems["MU"][0] };
            var itemsToDelete = strategy.GetItemsToCleanup(cacheItems, i => i.Size, 200);

            itemsToDelete.CheckContainsSameItemsAs(expectedItems);
        }

        protected override CacheCleanupStrategy GetCacheCleanupStrategyInstance()
        {
            return new RarelyUsedItemsRemoveFirstCleanupStrategy();
        }
    }
}
