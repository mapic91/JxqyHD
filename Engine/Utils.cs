namespace Engine
{
    static public class Utils
    {
        //static public int GetBigEndianIntegerFromByteArray(byte[] data, ref int startIndex)
        //{
        //    var ret = (data[startIndex] << 24)
        //         | (data[startIndex + 1] << 16)
        //         | (data[startIndex + 2] << 8)
        //         | data[startIndex + 3];
        //    startIndex += 4;
        //    return ret;
        //}

        static public int GetLittleEndianIntegerFromByteArray(byte[] data, ref int startIndex)
        {
            var ret = (data[startIndex + 3] << 24)
                 | (data[startIndex + 2] << 16)
                 | (data[startIndex + 1] << 8)
                 | data[startIndex];
            startIndex += 4;
            return ret;
        }
    }
}
