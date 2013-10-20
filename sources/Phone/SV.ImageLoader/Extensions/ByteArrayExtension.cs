
namespace SV.ImageLoader.Extensions
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    /// <summary>
    ///     Defiens extensions for byte array.
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
        public static Task<ImageInfo> ResizeAsync(this byte[] imageData, Size newSize, bool keepAspectRatio)
        {
            var result = new ImageInfo();

            var image = imageData.ToBitmap();
            var percentWidth = (double)newSize.Width / (double)image.PixelWidth;
            var percentHeight = (double)newSize.Height / (double)image.PixelHeight;

            if (keepAspectRatio)
            {
                result.Size = percentWidth < percentHeight
                                    ? new Size(newSize.Width, (int)(image.PixelHeight * percentWidth))
                                    : new Size((int)(image.PixelWidth * percentHeight), newSize.Height);
            }
            else
            {
                result.Size = newSize;
            }

            var wb = new WriteableBitmap(image);
            using (var memoryStream = new MemoryStream())
            {
                wb.SaveJpeg(memoryStream, result.Size.Width, result.Size.Height, 0, 100);

                result.Data = memoryStream.ToArray();
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
        public static BitmapImage ToBitmap(this byte[] imageData)
        {
            var image = new BitmapImage();
            image.SetSource(new MemoryStream(imageData));

            return image;
        }
    }
}
