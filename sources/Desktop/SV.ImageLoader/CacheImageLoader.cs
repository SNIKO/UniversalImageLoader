
namespace SV.ImageLoader
{
    using SV.ImageLoader.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     The base class for all cache based image loaders.
    /// </summary>
    public abstract class CacheImageLoader : BaseImageLoader
    {
        #region Fields

        private readonly List<CacheImageLoader.CacheItem> index = new List<CacheItem>();

        #endregion

        #region Methods

        /// <summary>
        ///     Requests loading the image of <paramref name="size"/> located on <paramref name="uri"/>. 
        /// </summary>
        /// <param name="uri">
        ///     An uri of the image to load.
        /// </param>
        /// <param name="size">
        ///     The desired size of the image.
        /// </param>
        /// <returns>
        ///     Returns an observable sequence which contains an image(s) of <paramref name="size"/> loaded using <paramref name="uri"/>.
        /// </returns>
        /// <remarks>
        ///     Override this method to provide concrete logic of loading the image per each <see cref="BaseImageLoader"/>.
        /// </remarks>
        protected override sealed IObservable<ImageInfo> WhenLoadedInternal(Uri uri, Size size)
        {
            var result = Observable.Create<ImageInfo>(async (observer, token) =>
                {
                    CacheItem cacheItem = null;
                    byte[] imageData = null;

                    // Loading proper image from cache
                    var cachedImages = this.GetCachedImagesByAppropriateness(uri, size);
                    foreach (var item in cachedImages)
                    {
                        imageData = await this.GetCacheDataAsync(item);

                        if (imageData == null)
                        {
                            this.RemoveFromCache(item);
                        }
                        else
                        {
                            cacheItem = item;
                            break;
                        }

                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    if (cacheItem != null && token.IsCancellationRequested == false)
                    {
                        var resultImage = new ImageInfo { Uri = uri };

                        if (cacheItem.ImageSize.Width > size.Width || cacheItem.ImageSize.Height > size.Height)
                        {
                            Size imageSize;
                            var resizedImageData = imageData.Resize(size, true, out imageSize);

                            resultImage.Data = resizedImageData;
                            resultImage.Size = imageSize;

                            this.SaveToCache(resultImage);
                        }
                        else
                        {
                            resultImage.Data = imageData;
                            resultImage.Size = cacheItem.ImageSize;
                        }

                        resultImage.IsFinal = resultImage.Size.Width == size.Width || resultImage.Size.Height == size.Height;

                        observer.OnNext(resultImage);
                    }

                    observer.OnCompleted();

                    return () => { };
                });

            return result;
        }

        /// <summary>
        ///     Notifies that the image is loaded from fallback <see cref="BaseImageLoader"/>.
        /// </summary>
        /// <param name="image">
        ///     The loaded image.
        /// </param>
        /// <remarks>
        ///     Use this method to save/cache the loaded image. 
        /// </remarks>
        protected override sealed void OnFallbackImageLoaded(ImageInfo image)
        {
            this.SaveToCache(image);
        }

        /// <summary>
        ///     Generates the unique key for the image item.
        /// </summary>
        /// <param name="uri">
        ///     The URI of the image.
        /// </param>
        /// <returns>
        ///     A unique key for the image. 
        /// </returns>
        protected virtual string GenerateKey(Uri uri)
        {
            return uri.PathAndQuery;
        }

        /// <summary>
        ///     Adds the info about cached images to index.
        /// </summary>
        /// <param name="cacheItems">
        ///     The info about cached images to add.
        /// </param>
        protected void AddToIndex(IEnumerable<CacheItem> cacheItems)
        {
            lock (this.index)
            {
                var uniqueItems = cacheItems.Where(item => this.index.All(i => i.Key != item.Key || i.ImageSize != item.ImageSize)).ToList();

                this.index.AddRange(uniqueItems);
            }
        }

        /// <summary>
        ///     Removes the info about all cached images from index.
        /// </summary>
        protected void ClearIndex()
        {
            lock (this.index)
            {
                this.index.Clear();
            }
        }

        /// <summary>
        ///     Retrieves the binary data of the image from the cache.
        /// </summary>
        /// <param name="item">
        ///     The record that identifies the image in the cache.
        /// </param>
        /// <returns>
        ///     The binary data of the image.
        /// </returns>
        protected abstract Task<byte[]> GetCacheDataAsync(CacheItem item);

        /// <summary>
        ///     Saves the binary data of the image into the cache.
        /// </summary>
        /// <param name="item">
        ///     The record that identifies the image in the cache.
        /// </param>
        /// <param name="data">
        ///     The binary data of the image to save.
        /// </param>
        protected abstract Task SetCacheDataAsync(CacheItem item, byte[] data);

        /// <summary>
        ///     Deletes the binary data of the image from cache.
        /// </summary>
        /// <param name="item">
        ///     The record that identifies the image in the cache.
        /// </param>
        protected abstract void DeleteCacheData(CacheItem item);

        /// <summary>
        ///     Retrieves the list of cached images with different size associated with <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">
        ///     The uri of the image.
        /// </param>
        /// <param name="desiredSize">
        ///     The desire size of the image.
        /// </param>
        /// <returns>
        ///     The list of cached images with different size associated with <paramref name="uri"/>. The list is sorted according to appropriateness of the images' sizes
        ///     regarding to desired size. The order is following:
        ///     <list type="bullet">
        ///         <item>
        ///             Image with size equal <paramref name="desiredSize"/> (if found).
        ///         </item>
        ///         <item>
        ///             Images with size larger than <paramref name="desiredSize"/> (it is possible to resize them to <paramref name="desiredSize"/>).
        ///         </item>
        ///         <item>
        ///             Images with size smaller than <paramref name="desiredSize"/> (it is better to display smaller image than nothing).
        ///         </item>
        ///     </list>
        /// </returns>
        private IEnumerable<CacheItem> GetCachedImagesByAppropriateness(Uri uri, Size desiredSize)
        {
            var result = new List<CacheItem>();
            List<CacheItem> cachedImages;

            lock (this.index)
            {
                var key = GenerateKey(uri);
                cachedImages = this.index.Where(i => i.Key == key).ToList();
            }

            if (cachedImages.Any())
            {
                var item = cachedImages.First();
                var sizeComparer = item.ImageSize.Width > item.ImageSize.Height ? SizeComparer.ByWidth : SizeComparer.ByHeight;

                var largerItems = new List<CacheItem>();
                var smallerItems = new List<CacheItem>();
                CacheItem equalItem = null;

                foreach (var cacheItem in cachedImages)
                {
                    var comparisonResult = sizeComparer.Compare(cacheItem.ImageSize, desiredSize);
                    if (comparisonResult == 0)
                    {
                        equalItem = cacheItem;
                    }
                    else if (comparisonResult > 0)
                    {
                        largerItems.Add(cacheItem);
                    }
                    else
                    {
                        smallerItems.Add(cacheItem);
                    }
                }

                if (equalItem != null)
                {
                    result.Add(equalItem);
                }

                result.AddRange(largerItems.OrderBy(i => i.ImageSize, sizeComparer));
                result.AddRange(smallerItems.OrderByDescending(i => i.ImageSize, sizeComparer));
            }

            return result;
        }

        private void RemoveFromCache(CacheItem item)
        {
            lock (this.index)
            {
                this.index.Remove(item);
            }

            this.DeleteCacheData(item);
        }

        private void SaveToCache(ImageInfo image)
        {
            var cacheItem = new CacheItem();
            cacheItem.Key = this.GenerateKey(image.Uri);
            cacheItem.LastAccessTime = DateTime.UtcNow;
            cacheItem.ImageSize = image.Size;

            this.AddToIndex(new[] { cacheItem });
            this.SetCacheDataAsync(cacheItem, image.Data);
        }

        #endregion

        #region Types

        protected class CacheItem
        {
            /// <summary>
            ///     Gets or sets the unique key of the item in cache.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            ///     Gets or sets the size of the image in pixels.
            /// </summary>
            public Size ImageSize { get; set; }

            /// <summary>
            ///     Gets or sets the size of the image in bytes.
            /// </summary>
            public long Size { get; set; }

            /// <summary>
            ///     Gets or sets the time when the last time the image was acceessed.
            /// </summary>
            public DateTime LastAccessTime { get; set; }
        }

        protected abstract class SizeComparer : IComparer<Size>
        {
            #region Properties

            /// <summary>
            ///     Gets a <see cref="SizeComparer"/> object that performs comapring of the <see cref="Size"/> by <see cref="Size.Width"/>.
            /// </summary>
            public static SizeComparer ByWidth
            {
                get
                {
                    return new WidthComparer();
                }
            }

            /// <summary>
            ///     Gets a <see cref="SizeComparer"/> object that performs comapring of the <see cref="Size"/> by <see cref="Size.Height"/>.
            /// </summary>
            public static SizeComparer ByHeight
            {
                get
                {
                    return new HeightComparer();
                }
            }

            #endregion

            #region Methods

            public abstract int Compare(Size x, Size y);

            #endregion

            #region Concrete Comparers

            private class WidthComparer : SizeComparer
            {
                public override int Compare(Size x, Size y)
                {
                    return x.Width.CompareTo(y.Width);
                }
            }

            private class HeightComparer : SizeComparer
            {
                public override int Compare(Size x, Size y)
                {
                    return x.Height.CompareTo(y.Height);
                }
            }

            #endregion
        }

        #endregion
    }
}
