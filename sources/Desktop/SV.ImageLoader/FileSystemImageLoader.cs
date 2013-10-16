
namespace SV.ImageLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Loads the images from a specified folder.
    /// </summary>
    public class FileSystemImageLoader : CacheImageLoader
    {
        #region Constants

        private const string DefaultCacheDirectoryName = "McLarenChempion";

        #endregion

        #region Fields

        private readonly object indexSyncObject = new object();

        private string directory;

        private CancellationTokenSource updateIndexToken;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSystemImageLoader"/> class.
        /// </summary>
        public FileSystemImageLoader()
        {
            var defaultCacheDirectory = Path.Combine(Path.GetTempPath(), DefaultCacheDirectoryName);

            this.WithDirectory(defaultCacheDirectory);
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
        public FileSystemImageLoader WithDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentNullException("directory");
            }

            lock (this.indexSyncObject)
            {
                if (StringComparer.OrdinalIgnoreCase.Compare(this.directory, directory) != 0)
                {
                    EnsureDireactoryExist(directory);

                    if (this.updateIndexToken != null)
                    {
                        this.updateIndexToken.Cancel();
                    }

                    this.ClearIndex();

                    this.directory = directory;
                    this.updateIndexToken = new CancellationTokenSource();
                    this.UpdateIndexAsync(directory, this.updateIndexToken.Token);
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
            var filePath = GetCacheFilePath(item);

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(memoryStream);
                    }

                    if (memoryStream.Length > 0)
                    {
                        imageData = memoryStream.ToArray();
                    }
                    else
                    {
                        TryDeleteFile(filePath);
                    }
                }
            }
            catch (IOException)
            {
                // Doesn't metter
            }
            catch (SecurityException)
            {
                // Doesn't metter
            }
            catch (UnauthorizedAccessException)
            {
                // Doesn't metter
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
                using (var fileStream = new FileStream(GetCacheFilePath(item), FileMode.CreateNew))
                {
                    await fileStream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (IOException)
            {
                // Doesn't metter
            }
            catch (SecurityException)
            {
                // Doesn't metter
            }
            catch (UnauthorizedAccessException)
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
        protected override void DeleteCacheData(CacheItem item)
        {
            TryDeleteFile(GetCacheFilePath(item));
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
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.ASCII.GetBytes(uri.PathAndQuery));
                var sb = new StringBuilder();

                for (var i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("X2"));
                }

                var hash = sb.ToString();
                return hash;
            }
        }        

        private static void EnsureDireactoryExist(string directory)
        {
            try
            {
                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ImageLoaderException(string.Format("Can't use '{0}' as a cache directory", directory), ex);
            }
            catch (IOException ex)
            {
                throw new ImageLoaderException(string.Format("Can't use '{0}' as a cache directory", directory), ex);
            }
            catch (NotSupportedException ex)
            {
                throw new ImageLoaderException(string.Format("Can't use '{0}' as a cache directory", directory), ex);
            }
        }

        private static bool TryDeleteFile(string pathToFile)
        {
            try
            {
                File.Delete(pathToFile);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        private static CacheItem GetCacheItemFromFile(string filePath)
        {
            CacheItem result = null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name;
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
                                Size = fileInfo.Length
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

        private string GetCacheFilePath(CacheItem cacheItem)
        {
            var fileName = string.Format("{0}.{1}x{2}.jpg", cacheItem.Key, cacheItem.ImageSize.Width, cacheItem.ImageSize.Height);
            var filePath = Path.Combine(this.directory, fileName);

            return filePath;
        }

        private Task UpdateIndexAsync(string cacheDirectory, CancellationToken token)
        {
            return Task.Run(() =>
            {
                var index = new List<CacheItem>();
                var files = Directory.EnumerateFiles(cacheDirectory);

                foreach (var file in files)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var cacheItem = GetCacheItemFromFile(file);
                    if (cacheItem == null)
                    {
                        continue;
                    }

                    if (cacheItem.Size == 0)
                    {
                        TryDeleteFile(file);
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
            });
        }

        #endregion
    }
}
