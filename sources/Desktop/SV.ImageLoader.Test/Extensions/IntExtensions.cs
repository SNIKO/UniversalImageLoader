
namespace SV.ImageLoader.Test.Extensions
{
    using System;

    public static class IntExtensions
    {
        public static DateTime DaysAgo(this int days)
        {
            return DateTime.Now.AddDays(-days);
        }
    }
}
