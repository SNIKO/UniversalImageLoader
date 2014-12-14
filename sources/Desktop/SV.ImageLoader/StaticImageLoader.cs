
namespace SV.ImageLoader
{
    using System;
    using System.IO;
    using System.Reactive.Linq;
    using System.Security;

    /// <summary>
    ///     Loads static image from path specified.
    /// </summary>
    public class StaticImageLoader : BaseImageLoader
    {
        private byte[] imageData;

        /// <summary>
        ///     Specifies the static image to use.
        /// </summary>
        /// <param name="imagePath">
        ///     The path to the image.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="imagePath"/> is null <see cref="string.Empty"/>.
        /// </exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="IOException"></exception>
        public StaticImageLoader WithImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new ArgumentNullException("imagePath");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(imagePath, FileMode.Open))
                {
                    fileStream.CopyTo(memoryStream);
                }

                if (memoryStream.Length > 0)
                {
                    imageData = memoryStream.ToArray();
                }
                else
                {
                    throw new IOException(string.Format("The image file '{0}' is empty", imagePath));
                }
            }

            return this;
        }

        protected override IObservable<ImageInfo> WhenLoadedInternal(Uri uri, Size size)
        {
            return Observable.Create<ImageInfo>(observer =>
                {
                    if (imageData != null)
                    {
						var imageInfo = new ImageInfo(uri, size, imageData) { ForceFallback = true };
                        observer.OnNext(imageInfo);
                    }

                    observer.OnCompleted();

                    return () => { };
                });
        }
    }
}
