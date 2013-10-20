
namespace SV.ImageLoader.Extensions
{
    using System;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Windows.Graphics.Imaging;
    using Windows.Storage.Streams;

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
        public static async Task<ImageInfo> ResizeAsync(this byte[] imageData, Size newSize, bool keepAspectRatio)
        {
            var result = new ImageInfo();

            var decoder = await BitmapDecoder.CreateAsync(await imageData.AsRandomAccessStreamAsync());
            var percentWidth = (double)newSize.Width / (double)decoder.PixelWidth;
            var percentHeight = (double)newSize.Height / (double)decoder.PixelHeight;

            if (keepAspectRatio)
            {
                result.Size = percentWidth < percentHeight
                                    ? new Size(newSize.Width, (int)(decoder.PixelHeight * percentWidth))
                                    : new Size((int)(decoder.PixelWidth * percentHeight), newSize.Height);
            }
            else
            {
                result.Size = newSize;
            }

            var transform = new BitmapTransform { ScaledWidth = (uint)result.Size.Width, ScaledHeight = (uint)result.Size.Height };
            var pixelData = await decoder.GetPixelDataAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);

            using (var destinationStream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, destinationStream);
                encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied, (uint)result.Size.Width, (uint)result.Size.Height, 96, 96, pixelData.DetachPixelData());
                await encoder.FlushAsync();

                var data = new byte[destinationStream.Size];
                destinationStream.Seek(0);
                await destinationStream.ReadAsync(data.AsBuffer(), (uint)destinationStream.Size, InputStreamOptions.None);

                result.Data = data;
            }

            return result;
        }

        public static async Task<IRandomAccessStream> AsRandomAccessStreamAsync(this byte[] array)
        {
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(array.AsBuffer());
            stream.Seek(0);

            return stream;
        }
    }
}
