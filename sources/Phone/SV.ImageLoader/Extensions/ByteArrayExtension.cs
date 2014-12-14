
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
		public static Task<ImageInfo> ResizeAsync(this byte[] imageData, SV.ImageLoader.Size newSize, bool keepAspectRatio)
		{
			var tcs = new TaskCompletionSource<ImageInfo>();

			DispatcherHelper.InvokeAsync(() =>
			{
			   var result = new ImageInfo();
			   var bitmap = imageData.ToBitmap(newSize);

				using (var resizedStream = new MemoryStream())
				{
					var writableBitmap = new WriteableBitmap(bitmap);
					writableBitmap.SaveJpeg(resizedStream, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
					
					resizedStream.Seek(0, SeekOrigin.Begin);
					result.Data = resizedStream.GetBuffer();
					result.Size = new SV.ImageLoader.Size(bitmap.PixelWidth, bitmap.PixelHeight);
				}

				tcs.SetResult(result);
			});

			return tcs.Task;
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
        public static BitmapImage ToBitmap(this byte[] imageData, SV.ImageLoader.Size size)
        {
			var bitmap = new BitmapImage();

			if (size.Width >= size.Height)
			{
				bitmap.DecodePixelWidth = size.Width;
			}
			else
			{
				bitmap.DecodePixelHeight = size.Height;
			}

			using (var stream = new MemoryStream(imageData))
			{
				bitmap.SetSource(stream);
			}

			return bitmap;
        }
    }
}
