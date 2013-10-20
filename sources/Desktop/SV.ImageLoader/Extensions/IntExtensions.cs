
namespace SV.ImageLoader.Extensions
{
    public static class IntExtensions
    {
        public static CacheSize Bytes(this int size)
        {
            return new CacheSize(size, CacheSize.SizeUnits.Bytes);
        }

        public static CacheSize Kilobytes(this int size)
        {
            return new CacheSize(size * 1000, CacheSize.SizeUnits.Bytes);
        }

        public static CacheSize Megabytes(this int size)
        {
            return new CacheSize(size * 1000 * 1000, CacheSize.SizeUnits.Bytes);
        }

        public static CacheSize Items(this int size)
        {
            return new CacheSize(size, CacheSize.SizeUnits.Items);
        }
    }
}