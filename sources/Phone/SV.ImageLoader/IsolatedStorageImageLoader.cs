
namespace SV.ImageLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Storage;

    /// <summary>
    ///     Loads the images from a specified folder.
    /// </summary>
    public class IsolatedStorageImageLoader : CacheImageLoader
    {
        #region Constants

        private const string DefaultCacheDirectoryName = "ImagesCache";

        #endregion

        #region Fields

        private readonly object indexSyncObject = new object();

        private StorageFolder cacheFolder;

        private CancellationTokenSource updateIndexToken;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IsolatedStorageImageLoader"/> class.
        /// </summary>
        public IsolatedStorageImageLoader()
        {
            this.WithDirectory(DefaultCacheDirectoryName);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Specifies a path to directory where to save/load images.
        /// </summary>
        /// <param name="directory">
        ///     The path to directory with images.
        /// </param>
        /// <returns>
        ///     The instance of the loader.
        /// </returns>
        /// <remarks>
        ///     If the directory is not specified, i.e. <see cref="WithDirectory"/> is not called, then the default temp directory will be used.
        /// </remarks>
        public IsolatedStorageImageLoader WithDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentNullException("directory");
            }

            lock (this.indexSyncObject)
            {
                if (this.cacheFolder == null || this.cacheFolder.Path.EndsWith(directory, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    if (this.updateIndexToken != null)
                    {
                        this.updateIndexToken.Cancel();
                    }

                    this.ClearIndex();

                    this.cacheFolder = ApplicationData.Current.LocalFolder.CreateFolderAsync(directory, CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
                    this.updateIndexToken = new CancellationTokenSource();
                    this.UpdateIndexAsync(this.updateIndexToken.Token);
                }
            }

            return this;
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
        protected override async Task<byte[]> GetCacheDataAsync(CacheItem item)
        {
            byte[] imageData = null;
            var fileName = GetCacheFileName(item);
            var file = await this.cacheFolder.GetFileAsync(fileName);

            if (file != null)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var fileStream = await file.OpenReadAsync())
                        {
                            await fileStream.AsStreamForRead().CopyToAsync(memoryStream);
                        }

                        if (memoryStream.Length > 0)
                        {
                            imageData = memoryStream.ToArray();
                        }
                        else
                        {
                            TryDeleteFileAsync(file);
                        }
                    }
                }
                catch (IsolatedStorageException)
                {
                    // Doesn't metter
                }
            }
            else
            {
                // TODO: Log it
            }

            return imageData;
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
        protected override async Task SetCacheDataAsync(CacheItem item, byte[] data)
        {
            try
            {
                var fileName = GetCacheFileName(item);
                var file = await this.cacheFolder.CreateFileAsync(fileName);

                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await fileStream.AsStreamForWrite().WriteAsync(data, 0, data.Length);
                }
            }
            catch (IsolatedStorageException)
            {
                // Doesn't metter
            }
        }

        /// <summary>
        ///     Deletes the binary data of the image from cache.
        /// </summary>
        /// <param name="item">
        ///     The record that identifies the image in the cache.
        /// </param>
        protected override async Task DeleteCacheDataAsync(CacheItem item)
        {
            var fileName = GetCacheFileName(item);
            var file = await this.cacheFolder.GetFileAsync(fileName);

            if (file != null)
            {
                await this.TryDeleteFileAsync(file);
            }
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
        protected override string GenerateKey(Uri uri)
        {
            var sha = new SHA1Managed();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(uri.PathAndQuery));
            var sb = new StringBuilder();

            for (var i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }

            var hash = sb.ToString();
            return hash;
        }


        private static Size GetSizeFromString(string st)
        {
            var parts = st.Split('x');

            int width;
            if (Int32.TryParse(parts[0], out width))
            {
                int heigth;
                if (Int32.TryParse(parts[1], out heigth))
                {
                    return new Size(width, heigth);
                }
            }

            return default(Size);
        }

        private static async Task<CacheItem> GetCacheItemFromFileAsync(StorageFile file)
        {
            CacheItem result = null;

            try
            {
                var fileName = file.Name;
                var basicProperties = await file.GetBasicPropertiesAsync();
                var parts = fileName.Split('.');

                if (parts.Length == 3)
                {
                    var size = GetSizeFromString(parts[1]);

                    if (size != default(Size))
                    {
                        result = new CacheItem
                            {
                                Key = parts[0],
                                ImageSize = size,
                                Size = basicProperties.Size,
                                LastAccessTime = basicProperties.DateModified.UtcDateTime
                            };
                    }
                }
            }
            catch (SecurityException)
            {
                // Doesn't metter
            }
            catch (UnauthorizedAccessException)
            {
                // Doesn't metter
            }

            return result;
        }

        private async Task<bool> TryDeleteFileAsync(StorageFile file)
        {
            try
            {
                await file.DeleteAsync();

                return true;
            }
            catch (IsolatedStorageException)
            {
                return false;
            }
        }

        private static string GetCacheFileName(CacheItem cacheItem)
        {
            var fileName = string.Format("{0}.{1}x{2}.jpg", cacheItem.Key, cacheItem.ImageSize.Width, cacheItem.ImageSize.Height);

            return fileName;
        }

        private async Task UpdateIndexAsync(CancellationToken token)
        {
            var index = new List<CacheItem>();
            var files = await this.cacheFolder.GetFilesAsync().AsTask();

            foreach (var file in files)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                var cacheItem = await GetCacheItemFromFileAsync(file);
                if (cacheItem == null)
                {
                    continue;
                }

                if (cacheItem.Size == 0)
                {
                    TryDeleteFileAsync(file);
                }
                else
                {
                    index.Add(cacheItem);
                }
            }

            lock (this.indexSyncObject)
            {
                if (token.IsCancellationRequested == false)
                {
                    this.AddToIndex(index);
                }
            }
        }

        #endregion
    }
}
