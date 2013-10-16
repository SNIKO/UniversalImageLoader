
namespace SV.ImageLoader
{
    using System;

    public class ImageInfo
    {
        public byte[] Data { get; internal set; }

        public Size Size { get; internal set; }

        public Uri Uri { get; internal set; }

        internal bool IsFinal { get; set; }
    }
}
