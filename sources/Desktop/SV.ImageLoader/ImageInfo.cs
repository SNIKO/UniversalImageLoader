
namespace SV.ImageLoader
{
    using System;

    public class ImageInfo
    {
        public ImageInfo()
        {
        }

        public ImageInfo(Uri uri, Size size, byte[] data, bool isFinal)
        {
            this.Uri = uri;
            this.Size = size;
            this.Data = data;
            this.IsFinal = isFinal;
        }

        public byte[] Data { get; internal set; }

        public Size Size { get; internal set; }

        public Uri Uri { get; internal set; }

        internal bool IsFinal { get; set; }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Size.GetHashCode() * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
            }
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Uri={0}, Size={1}", this.Uri, this.Size.ToString());
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="System.Object" /> to compare with this instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ImageInfo)obj);
        }

        internal ImageInfo WithUri(string uri)
        {
            this.Uri = new Uri(uri);

            return this;
        }

        internal ImageInfo WithSize(Size size)
        {
            this.Size = size;

            return this;
        }

        protected bool Equals(ImageInfo other)
        {
            return Size.Equals(other.Size) && Equals(Uri, other.Uri);
        }
    }
}
