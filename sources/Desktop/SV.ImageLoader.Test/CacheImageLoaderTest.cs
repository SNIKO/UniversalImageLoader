
namespace SV.ImageLoader.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SV.ImageLoader.Test.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    [TestClass]
    public abstract class CacheImageLoaderTest : BaseImageLoaderTest
    {
        [TestMethod]
        public void WhenLoadedIntegrationTest()
        {
            var fallbackLoaderMock = new ImageLoaderMock();
            var loaderToTest = GetImageLoaderInstance().AsFallbackUse(fallbackLoaderMock);
            IEnumerable<ImageInfo> images;

            // Cache:           []
            // FallbackLoader:  [10x20]
            // Request:         [20x40] 
            // Expect:          10x10 from fallback loader
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "20x40".Pixels(), "10x20".Pixels(), 1);
            images = loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "20x40".Pixels()).ToEnumerable().ToList();
            images.CheckContainsSameItemsAs(new[]
                {
                    new ImageInfo().WithUri("http://mclaren.com/1.jpg").WithSize("10x20".Pixels())            
                });
            fallbackLoaderMock.Verify();

            // Cache:           [10x20]
            // FallbackLoader:  [10x20]
            // Request:         [10x20] 
            // Expect:          10x20 from cache
            fallbackLoaderMock.Reset();
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "10x20".Pixels(), "10x20".Pixels(), 0);
            images = loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "10x20".Pixels()).ToEnumerable().ToList();
            images.CheckContainsSameItemsAs(new[]
                {
                    new ImageInfo().WithUri("http://mclaren.com/1.jpg").WithSize("10x20".Pixels())            
                });
            fallbackLoaderMock.Verify();

            // Cache:           [10x20]
            // FallbackLoader:  [20x40]
            // Request:         [20x40] 
            // Expect:          10x20 from cache and 20x40 from fallback loader
            fallbackLoaderMock.Reset();
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "20x40".Pixels(), "20x40".Pixels(), 1);
            images = loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "20x40".Pixels()).ToEnumerable().ToList();
            images.CheckContainsSameItemsAs(new[]
                {
                    new ImageInfo().WithUri("http://mclaren.com/1.jpg").WithSize("10x20".Pixels()),
                    new ImageInfo().WithUri("http://mclaren.com/1.jpg").WithSize("20x40".Pixels())
                });
            fallbackLoaderMock.Verify();

            // Cache:           [10x20, 20x40]
            // FallbackLoader:  [5x10]
            // Request:         [5x10] 
            // Expect:          5x10 resized from 10x20 from cache
            fallbackLoaderMock.Reset();
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "5x10".Pixels(), "5x10".Pixels(), 0);
            images = loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "5x10".Pixels()).ToEnumerable().ToList();
            images.CheckContainsSameItemsAs(new[]
                {
                    new ImageInfo().WithUri("http://mclaren.com/1.jpg").WithSize("5x10".Pixels())                    
                });
            fallbackLoaderMock.Verify();

            // Cache:           [5x10, 10x20, 20x40]
            // FallbackLoader:  [15x30]
            // Request:         [15x30] 
            // Expect:          15x30 resized from 20x40 from cache
            fallbackLoaderMock.Reset();
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "15x30".Pixels(), "15x30".Pixels(), 0);
            images = loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "15x30".Pixels()).ToEnumerable().ToList();
            images.CheckContainsSameItemsAs(new[]
                {
                    new ImageInfo().WithUri("http://mclaren.com/1.jpg").WithSize("15x30".Pixels())                    
                });
            fallbackLoaderMock.Verify();

            // Cache:           [5x10, 10x20, 15x30, 20x40]
            // FallbackLoader:  [25x30]
            // Request:         [25x30] 
            // Expect:          18x35 resized from 20x40 from cache with keeping aspect ratio
            fallbackLoaderMock.Reset();
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "25x35".Pixels(), "25x35".Pixels(), 0);
            images = loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "25x35".Pixels()).ToEnumerable().ToList();
            images.CheckContainsSameItemsAs(new[]
                {
                    new ImageInfo().WithUri("http://mclaren.com/1.jpg").WithSize("18x35".Pixels())                    
                });
            fallbackLoaderMock.Verify();
        }

        [TestMethod]
        public void WhenLoadedWithCleanupStrategyIntegrationTest()
        {
            var cleanupStrategyMock = new Moq.Mock<ICacheCleanupStrategy>();
            cleanupStrategyMock
                .Setup(f => f.GetItemsToCleanup(It.IsAny<IReadOnlyDictionary<string, List<CacheImageLoader.CacheItem>>>(), It.IsAny<Func<CacheImageLoader.CacheItem, long>>(), It.IsAny<long>()))
                .Returns((IReadOnlyDictionary<string, List<CacheImageLoader.CacheItem>> items, Func<CacheImageLoader.CacheItem, long> sizeEvaluator, long sizeToFree) =>
                {
                    var sortedItems = items.SelectMany(s => s.Value).OrderByDescending(c => c.ImageSize.Width);

                    return sortedItems.Take((int)sizeToFree).ToList();
                });

            var fallbackLoaderMock = new ImageLoaderMock();
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "10x10".Pixels(), "10x10".Pixels(), 1);
            fallbackLoaderMock.Setup("http://mclaren.com/1.jpg", "20x20".Pixels(), "20x20".Pixels(), 2);
            fallbackLoaderMock.Setup("http://mclaren.com/2.jpg", "30x30".Pixels(), "30x30".Pixels(), 1);

            var loaderToTest = ((CacheImageLoader)GetImageLoaderInstance())
                .WithCacheCleanupStrategy(cleanupStrategyMock.Object)
                .WithCacheSize(new CacheSize(2, CacheSize.SizeUnits.Items))
                .AsFallbackUse(fallbackLoaderMock);

            loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "10x10".Pixels()).ToEnumerable().ToList();  // From fallback loader
            loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "10x10".Pixels()).ToEnumerable().ToList();  // From cache
            loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "20x20".Pixels()).ToEnumerable().ToList();  // From fallback loader
            loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "20x20".Pixels()).ToEnumerable().ToList();  // From cache
            loaderToTest.WhenLoaded("http://mclaren.com/2.jpg".AsUri(), "30x30".Pixels()).ToEnumerable().ToList();  // Cahche overwlow here, it should push out 20x20 image from cache
            loaderToTest.WhenLoaded("http://mclaren.com/1.jpg".AsUri(), "20x20".Pixels()).ToEnumerable().ToList();  // Since 20x20 is not in cache anymore, it should be request from fallback loader once again

            fallbackLoaderMock.Verify();
        }
    }
}
