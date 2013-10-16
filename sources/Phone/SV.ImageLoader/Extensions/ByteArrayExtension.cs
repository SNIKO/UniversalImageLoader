
namespace SV.ImageLoader.Extensions
{
    using System.IO;
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
        /// <param name="resultSize">
        ///     The size of resized image. It equals to <paramref name="newSize"/> if <paramref name="keepAspectRatio"/> is <c>false</c>; otherwise, the height or width
        ///     will be different to keep original aspect ratio.
        /// </param>
        /// <returns>
        ///     The binary data of the resized image.
        /// </returns>
        public static byte[] Resize(this byte[] imageData, Size newSize, bool keepAspectRatio, out Size resultSize)
        {
            var image = imageData.ToBitmap();
            var percentWidth = (double)newSize.Width / (double)image.PixelWidth;
            var percentHeight = (double)newSize.Height / (double)image.PixelHeight;

            if (keepAspectRatio)
            {
                resultSize = percentWidth < percentHeight
                                    ? new Size(newSize.Width, (int)(image.PixelHeight * percentWidth))
                                    : new Size((int)(image.PixelWidth * percentHeight), newSize.Height);
            }
            else
            {
                resultSize = newSize;
            }

            var wb = new WriteableBitmap(image);
            using (var memoryStream = new MemoryStream())
            {
                wb.SaveJpeg(memoryStream, resultSize.Width, resultSize.Height, 0, 100);

                return memoryStream.ToArray();
            }
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
