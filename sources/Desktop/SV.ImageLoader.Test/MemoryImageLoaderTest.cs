
namespace SV.ImageLoader.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MemoryImageLoaderTest : CacheImageLoaderTest
    {
        protected override BaseImageLoader GetImageLoaderInstance()
        {
            return new MemoryImageLoader();
        }
    }
}
