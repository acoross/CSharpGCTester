namespace GCTester
{
    class NetBuffer
    {
        public readonly byte[] data;
        public static int allocSize = 0;
        public static int releaseSize = 0;

        public static int allocCount = 0;
        public static int releaseCount = 0;

        public static int activeSize
        {
            get
            {
                return allocSize - releaseSize;
            }
        }

        public static int activeCount
        {
            get
            {
                return allocCount - releaseCount;
            }
        }

        public NetBuffer(int size)
        {
            allocCount++;
            allocSize += size;
            data = new byte[size];
        }

        public NetBuffer(byte[] data)
        {
            allocCount++;
            allocSize += data.Length;
            this.data = data;
        }

        ~NetBuffer()
        {
            releaseCount++;
            releaseSize += data.Length;
        }

        public static string AllocStateString()
        {
            return $"cnt:({allocCount}/{releaseCount}/{activeCount}), " +
                $"size:({allocSize/1000000.0:n2}mb/{releaseSize/1000000.0:n2}mb/{activeSize/1000.0:n2}kb)";
        }
    }
}
