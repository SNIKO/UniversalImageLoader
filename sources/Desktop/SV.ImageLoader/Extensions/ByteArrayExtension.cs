
using System.Windows.Media;

namespace SV.ImageLoader.Extensions
{
    using System.IO;
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
            
            ScaleTransform transform;
            if (keepAspectRatio)
            {
                transform = percentWidth < percentHeight
                                ? new ScaleTransform { ScaleX = percentWidth, ScaleY = percentWidth }
                                : new ScaleTransform { ScaleX = percentHeight, ScaleY = percentHeight };
            }
            else
            {
                transform = new ScaleTransform { ScaleX = percentWidth, ScaleY = percentHeight };
            }

            var resizedImage = new TransformedBitmap(image, transform);
            resultSize = new Size(resizedImage.PixelWidth, resizedImage.PixelHeight);
            
            using (var memoryStream = new MemoryStream())
            {
                var jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.Frames.Add(BitmapFrame.Create(resizedImage));
                jpegEncoder.Save(memoryStream);

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
            var bm = new BitmapImage();
            bm.BeginInit();
            bm.StreamSource = new MemoryStream(imageData);
            bm.EndInit();

            return bm;
        }
    }
}
