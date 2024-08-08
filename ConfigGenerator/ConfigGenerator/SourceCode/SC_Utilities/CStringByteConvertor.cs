using System.Text;

namespace ConfigGenerator
{
    public static class CStringByteConvertor
    {
        public static byte[] convertStringToByteArray(string sData)
        {
            return Encoding.UTF8.GetBytes(sData);
        }

        public static byte[] convertHexStringToByteArray(string sHexData)
        {
            return Enumerable.Range(0, sHexData.Length - 1)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(sHexData.Substring(x, 2), 16))
                     .ToArray();
        }

        public static byte[] convertBEStringToLEByteArray(string sHexData)
        {
            byte[] buffer = Enumerable.Range(0, sHexData.Length - 1)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(sHexData.Substring(x, 2), 16))
                     .ToArray();

            byte[] bufferOut = new byte[buffer.Length];
            int j = 0;

            for(int i = buffer.Length-1; i >= 0; i--)
            {
                bufferOut[j++] = buffer[i];
            }

            return bufferOut;  
        }
        public static byte[] convertBEByteArrayToLE(byte[] buffer)
        {
            byte[] bufferOut = new byte[buffer.Length];
            int j = 0;

            for(int i = buffer.Length-1; i >= 0; i--)
            {
                bufferOut[j++] = buffer[i];
            }

            return bufferOut;  
        }
    }
}