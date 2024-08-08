using System;
using System.Numerics;
using System.Collections.Generic;

namespace ConfigGenerator
{
    public static class CHexIntConverter
    {
        public enum CONVERT_TYPE
        {
            CONVERT_HEX_TO_INT,
            CONVERT_INT_TO_HEX,
        }

        private static readonly List<char> KEYWORD_HEX_NUMBER = new List<char>() {'x', 'X'};

        public static string FormatToHexString(string sNumber)
        {
            string NumberRet = null;
            if (string.IsNullOrEmpty(sNumber) == false)
            {
                try
                {

                    if (DetectAndTrimHexPrefix(sNumber, out NumberRet))
                    {
                        // NumberRet = BigInteger.Parse(sNumber, System.Globalization.NumberStyles.HexNumber).ToString("X");
                    }
                    else
                    {
                        try
                        {
                            BigInteger biTemp = BigInteger.Parse(sNumber);
                            NumberRet = biTemp.ToString("X");
                            if (biTemp != 0)
                            {
                                NumberRet = NumberRet.TrimStart('0');
                            }
                        }
                        catch
                        {
                            NumberRet = BigInteger.Parse(sNumber, System.Globalization.NumberStyles.HexNumber).ToString("X");
                        }
                    }
                }
                catch (Exception e)
                {
                    NumberRet = null;
                }
            }
            return NumberRet;
        }

        public static string StringConvertHexToInt(string sData)
        {
            string sRet = null;
            BigInteger biTemp = 0;

            if (sData != null)
            {
                try
                {
                    sData = "0" + sData;
                    biTemp = BigInteger.Parse(sData, System.Globalization.NumberStyles.HexNumber);
                    sRet = biTemp.ToString();
                }
                catch { }
            }
            return sRet;
        }

        public static string StringConvertIntToHex(string sData)
        {
            string sRet = null;

            if (sData != null)
            {
                try
                {
                    BigInteger biTemp = BigInteger.Parse(sData);
                    sRet = biTemp.ToString("X");
                }
                catch { }
            }
            return sRet;
        }

        public static bool DetectAndTrimHexPrefix (string sNumber, out string sRetNumber)
        {
            bool bRet = false;
            string[] sTemp = null;

            sRetNumber = sNumber;
            foreach (char element in KEYWORD_HEX_NUMBER)
            {
                if (sNumber.Contains(element.ToString()))
                {
                    sTemp = sRetNumber.Split(element);

                    // Check only has 0x or 0...0x
                    if ((sTemp.Length == 2) && (sTemp[0].Replace("0", "").Length == 0))
                    {
                        sRetNumber = sTemp[1];
                        bRet = true;
                        break;
                    }
                }
            }

            return bRet;
        }
    }
}