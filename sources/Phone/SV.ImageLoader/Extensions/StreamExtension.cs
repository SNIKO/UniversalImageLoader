
namespace SV.ImageLoader.Extensions
{
    using System.IO;
    using System.Threading.Tasks;

    public static class StreamExtension
    {
        /// <summary>
        ///     Writes the stream contents to a byte array.
        /// </summary>
        /// <param name="stream">
        ///     The stream to convert to byte array.
        /// </param>
        /// <returns>
        ///     A byte array of the <paramref name="stream"/>'s content.
        /// </returns>
        public static byte[] ToArray(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        ///     Asynchronously writes the stream contents to a byte array.
        /// </summary>
        /// <param name="stream">
        ///     The stream to convert to byte array.
        /// </param>
        /// <returns>
        ///     A byte array of the <paramref name="stream"/>'s content.
        /// </returns>
        public async static Task<byte[]> ToArrayAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }
    }
}
