
namespace SV.ImageLoader.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SV.ImageLoader.Test.Extensions;
    using System.Collections.Generic;

    [TestClass]
    public class LargeImagesRemoveFirstCleanupStrategyTest : CacheCleanupStrategyTest
    {
        [TestMethod]
        public void GetItemsToCleanup_SomeItemsAreSpecified_ReturnItemsAccordingToStrategy()
        {
            var strategy = new LargeImagesRemoveFirstCleanupStrategy();
            var cacheItems = new Dictionary<string, List<CacheImageLoader.CacheItem>>();

            cacheItems["McLaren"] = new List<CacheImageLoader.CacheItem>
                {
                    new CacheImageLoader.CacheItem { Key = "McLaren", ImageSize = "10x10".Pixels(), Size = 10 },
                    new CacheImageLoader.CacheItem { Key = "McLaren", ImageSize = "50x50".Pixels(), Size = 50 },
                    new CacheImageLoader.CacheItem { Key = "McLaren", ImageSize = "100x100".Pixels(), Size = 100 },
                };

            cacheItems["MU"] = new List<CacheImageLoader.CacheItem> {
                    new CacheImageLoader.CacheItem { Key = "MU", ImageSize = "10x10".Pixels(), Size = 10 },
                    new CacheImageLoader.CacheItem { Key = "MU", ImageSize = "50x50".Pixels(), Size = 50 },
                };

            cacheItems["Rooney"] = new List<CacheImageLoader.CacheItem> {
                    new CacheImageLoader.CacheItem { Key = "Rooney", ImageSize = "70x70".Pixels(), Size = 70 },
                };

            cacheItems["Magnussen"] = new List<CacheImageLoader.CacheItem> {
                    new CacheImageLoader.CacheItem { Key = "Magnussen", ImageSize = "20x20".Pixels(), Size = 20 },
                    new CacheImageLoader.CacheItem { Key = "Magnussen", ImageSize = "80x80".Pixels(), Size = 80 }
                };

            var expectedItems = new[] { cacheItems["McLaren"][1], cacheItems["McLaren"][2], cacheItems["MU"][1], cacheItems["Magnussen"][1] };
            var itemsToDelete = strategy.GetItemsToCleanup(cacheItems, i => i.Size, 280);

            itemsToDelete.CheckContainsSameItemsAs(expectedItems);
        }

        #region CacheCleanupStrategyTest

        protected override CacheCleanupStrategy GetCacheCleanupStrategyInstance()
        {
            return new LargeImagesRemoveFirstCleanupStrategy();
        }

        #endregion
    }
}
