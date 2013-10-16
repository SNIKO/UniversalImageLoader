
namespace SV.ImageLoader
{
    using System;

    public class ImageLoaderException : Exception
    {
        public ImageLoaderException(string message)
            : base(message)
        {

        }

        public ImageLoaderException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
