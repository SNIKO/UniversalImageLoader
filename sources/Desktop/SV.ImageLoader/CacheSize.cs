
namespace SV.ImageLoader
{
    public class CacheSize
    {
        public CacheSize(int size, SizeUnits units)
        {
            this.Size = size;
            this.Units = units;
        }

        public int Size { get; private set; }

        public SizeUnits Units { get; private set; }        

        public enum SizeUnits
        {
            Bytes,

            Items
        }
    }
}
