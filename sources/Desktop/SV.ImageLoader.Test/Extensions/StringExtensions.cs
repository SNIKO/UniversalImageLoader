
namespace SV.ImageLoader.Test.Extensions
{
    using System;

    public static class StringExtensions
    {
        public static Size Pixels(this string st)
        {
            var parts = st.ToUpper().Split('X');
            var size = new Size(int.Parse(parts[0]), int.Parse(parts[1]));

            return size;
        }

        public static Uri AsUri(this string st)
        {
            return new Uri(st);
        }
    }
}
