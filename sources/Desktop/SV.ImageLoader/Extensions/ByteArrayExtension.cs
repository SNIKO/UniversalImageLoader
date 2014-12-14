
using System;

namespace SV.ImageLoader.Extensions
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    ///     Defiens extensions for <see cref="byte[]"/>.
    /// </summary>
    public static class ByteArrayExtension
    {
        /// <summary>
        ///     Resizes the image presented by the <paramref name="imageData"/> to a <paramref name="newSize"/>.
        /// </summary>
        /// <param name="imageData">
        ///     The binary data of the image to resize.
        /// </param>
        /// <param name="newSize">
        ///     The size to which to resize the image.
        /// </param>
        /// <param name="keepAspectRatio">
        ///     A flag indicating whether to save original aspect ratio.
        /// </param>
        /// <returns>
        ///     The structure which contains binary data of resized image and the actial size of resized image depending on <paramref name="keepAspectRatio"/> value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     An error occurred during resizing the image.
        /// </exception>
        public static Task<ImageInfo> ResizeAsync(this byte[] imageData, Size newSize, bool keepAspectRatio)
        {
            var result = new ImageInfo();
            var image = imageData.ToBitmap();
            var percentWidth = (double) newSize.Width/(double) image.PixelWidth;
            var percentHeight = (double) newSize.Height/(double) image.PixelHeight;

            ScaleTransform transform;
            if (keepAspectRatio)
            {
                transform = percentWidth < percentHeight
                                ? new ScaleTransform {ScaleX = percentWidth, ScaleY = percentWidth}
                                : new ScaleTransform {ScaleX = percentHeight, ScaleY = percentHeight};
            }
            else
            {
                transform = new ScaleTransform {ScaleX = percentWidth, ScaleY = percentHeight};
            }

            var resizedImage = new TransformedBitmap(image, transform);

            using (var memoryStream = new MemoryStream())
            {
                var jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.Frames.Add(BitmapFrame.Create(resizedImage));
                jpegEncoder.Save(memoryStream);

                result.Data = memoryStream.ToArray();
                result.Size = new Size(resizedImage.PixelWidth, resizedImage.PixelHeight);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Converts the byte array to a <see cref="BitmapImage"/> instance.
        /// </summary>
        /// <param name="imageData">
        ///     The byte array to convert.
        /// </param>
        /// <returns>
        ///     The <see cref="BitmapImage"/> instance if the image presented by <paramref name="imageData"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     An error occurred during converting the <paramref name="imageData"/> to <see cref="BitmapImage"/>.
        /// </exception>
        public static BitmapImage ToBitmap(this byte[] imageData)
        {
            var bm = new BitmapImage();
            bm.BeginInit();
            bm.StreamSource = new MemoryStream(imageData);
            bm.EndInit();

            return bm;
        }
    }
}
