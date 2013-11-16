
namespace SV.ImageLoader
{
    using System;
    using System.IO;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Media.Imaging;

    /// <summary>
    ///     Loads static image from path specified.
    /// </summary>
    public class StaticImageLoader : BaseImageLoader
    {
        private byte[] imageData;

        /// <summary>
        ///     Specifies the static image to use.
        /// </summary>
        /// <param name="image">
        ///     The <see cref="BitmapImage"/> instance which represents the static image.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="image"/> is null.
        /// </exception>
        public StaticImageLoader WithImage(BitmapImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            using (var memoryStream = new MemoryStream())
            {
                var wb = new WriteableBitmap(image);
                wb.SaveJpeg(memoryStream, wb.PixelWidth, wb.PixelHeight, 0, 100);

                imageData = memoryStream.ToArray();
            }

            return this;
        }

        /// <summary>
        ///     Specifies the static image to use.
        /// </summary>
        /// <param name="imageUri">
        ///     The uri to static image.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="imageUri"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The image specified in <paramref name="imageUri"/> not found.
        /// </exception>
        public StaticImageLoader WithImage(Uri imageUri)
        {
            if (imageUri == null)
            {
                throw new ArgumentNullException("imageUri");
            }

            var bitmap = new BitmapImage();
            var resourceInfo = Application.GetResourceStream(imageUri);

            if (resourceInfo != null)
            {
                bitmap.SetSource(resourceInfo.Stream);
            }
            else
            {
                throw new ArgumentException(string.Format("The resource '{0}' not found", imageUri));
            }

            return this.WithImage(bitmap);
        }

        protected override IObservable<ImageInfo> WhenLoadedInternal(Uri uri, Size size)
        {
            return Observable.Create<ImageInfo>(observer =>
                {
                    if (imageData != null)
                    {
                        var imageInfo = new ImageInfo(uri, new Size(0, 0), imageData, false);
                        observer.OnNext(imageInfo);
                    }

                    observer.OnCompleted();

                    return () => { };
                });
        }
    }
}
