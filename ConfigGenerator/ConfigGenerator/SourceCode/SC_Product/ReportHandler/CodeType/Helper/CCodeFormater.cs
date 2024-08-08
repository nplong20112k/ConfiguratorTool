namespace ConfigGenerator
{
    public static class CCodeFormater
    {
        private static string CODE_REPORT_TAG_CODE = "{{'{0}','{1}','{2}','{3}'}} /*{4}*/";
        private static string CODE_REPORT_INTERFACE_MASK = "({0})";

        public static string FormatTagCode(string sTagCode)
        {
            string sRet = string.Format(CODE_REPORT_TAG_CODE,
                                        sTagCode[0],
                                        sTagCode[1],
                                        sTagCode[2],
                                        sTagCode[3],
                                        sTagCode);

            return sRet;
        }

        public static string FormatInterface (string sInterface)
        {
            string sRet = string.Format(CODE_REPORT_INTERFACE_MASK,
                                        sInterface);

            return sRet;
        }

        public static string PaddingValue(string sValue, int iValueSize, ATableRef TableRef)
        {
            string sRet;

            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII)
            {
                string sPadding = (TableRef as CTableRefAscii).GetTableRefPaddingValue();

                sRet = sValue;
                while (sRet.Length < iValueSize)
                {
                    sRet += sPadding;
                }
                sRet = sRet.Substring(0, iValueSize);
            }
            else
            {
                sRet = sValue.PadLeft(iValueSize, '0');
            }

            return sRet;
        }

        public static string FormatValue(string sValue)
        {
            string sSubString;
            string sRet = "";

            for (int i = 0; i < sValue.Length; i += 2)
            {
                sSubString = sValue.Substring(i, 2);
                sRet += "\\x" + sSubString;
            }

            return sRet;
        }
    }
}