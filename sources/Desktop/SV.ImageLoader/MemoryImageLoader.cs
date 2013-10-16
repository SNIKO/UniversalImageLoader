
namespace SV.ImageLoader
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     Loads images from the memory.
    /// </summary>
    public class MemoryImageLoader : CacheImageLoader
    {
        #region Fields

        private readonly Dictionary<CacheItem, byte[]> cache = new Dictionary<CacheItem, byte[]>();

        #endregion

        #region Methods

        /// <summary>
        ///     Retrieves the binary data of the image from the cache.
        /// </summary>
        /// <param name="item">
        ///     The record that identifies the image in the cache.
        /// </param>
        /// <returns>
        ///     The binary data of the image.
        /// </returns>
        protected override Task<byte[]> GetCacheDataAsync(CacheItem item)
        {
            lock (this.cache)
            {
                return Task.FromResult(this.cache[item]);
            }
        }

        /// <summary>
        ///     Saves the binary data of the image into the cache.
        /// </summary>
        /// <param name="item">
        ///     The record that identifies the image in the cache.
        /// </param>
        /// <param name="data">
        ///     The binary data of the image to save.
        /// </param>
        protected override Task SetCacheDataAsync(CacheItem item, byte[] data)
        {
            lock (this.cache)
            {
                this.cache[item] = data;
            }

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Deletes the binary data of the image from cache.
        /// </summary>
        /// <param name="item">
        ///     The record that identifies the image in the cache.
        /// </param>
        protected override void DeleteCacheData(CacheItem item)
        {
            lock (this.cache)
            {
                this.cache.Remove(item);
            }
        }

        #endregion
    }
}
