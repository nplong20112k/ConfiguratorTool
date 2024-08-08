using System;

namespace ConfigGenerator
{
    public static class CBitUltilizer
    {
        public enum CONVERT_TYPE
        {
            CONVERT_HEX_TO_INT,
            CONVERT_INT_TO_HEX,
        }

        public static int GetMostSignificantSetBit(uint uiNumber)
        {
            int uiRet;

            if (uiNumber == 0)
            {
                uiRet = 0;
            }
            else
            {
                uint uiTemp = 1;
                for (uiRet = 1; uiRet < 32; uiRet++)
                {
                    uiTemp = (uint)1 << uiRet;
                    if ((uiNumber / uiTemp) == 0)
                    {
                        break;
                    }
                }
            }

            return uiRet;
        }

        public static uint ConvertSizeByteToSizeBit(string sSizeByte)
        {
            uint uiRet = 0;

            if ((sSizeByte != null) && (sSizeByte != string.Empty))
            {
                try
                {
                    uiRet = Convert.ToUInt32(sSizeByte) * 8;
                }
                catch { }
            }

            return uiRet;
        }

        public static uint ConvertSizeBitToSizeAppSupport(string sSizeBit, uint uiSizeAppSupport)
        {
            uint uiRet = 0;

            if ((sSizeBit != null) && (sSizeBit != string.Empty) && (uiSizeAppSupport != 0))
            {
                try
                {
                    uiRet = Convert.ToUInt32(sSizeBit) / uiSizeAppSupport;
                    if ((Convert.ToUInt32(sSizeBit) % uiSizeAppSupport) > 0)
                    {
                        uiRet += 1;
                    }
                }
                catch { }
            }

            return uiRet;
        }
    }
}