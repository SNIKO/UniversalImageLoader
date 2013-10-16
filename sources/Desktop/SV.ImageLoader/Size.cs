
namespace SV.ImageLoader
{
    /// <summary>
    ///     Defines the size of the image.
    /// </summary>
    public struct Size
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Size"/> struct.
        /// </summary>
        /// <param name="width">
        ///     The width of the image.
        /// </param>
        /// <param name="height">
        ///     The height of the image.
        /// </param>
        public Size(int width, int height)
            : this()
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        ///     Gets the width of the image.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        ///     Gets the height of the image.
        /// </summary>
        public int Height { get; private set; }

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
            var size = (Size)obj;

            return size.Width == this.Width && size.Height == this.Height;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Size size1, Size size2)
        {
            return size1.Equals(size2);
        }

        public static bool operator !=(Size size1, Size size2)
        {
            return size1.Equals(size2) == false;
        }
    }
}
