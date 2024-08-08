using System;

namespace ConfigGenerator
{
    public static class CBinGeneralData
    {
        public const int LEN_MAGIC_NUMBER           = 4;
        public const int LEN_VERSION_NUMBER         = 2;
        public const int LEN_DATA_SIZE              = 2;
        public const int LEN_DATA_RECORD_NUMBER     = 2;
        public const int LEN_CRC16                  = 2;
        public const int LEN_PADDING                = 4;
        public const int LEN_HEADER                 = LEN_MAGIC_NUMBER + LEN_VERSION_NUMBER + LEN_DATA_SIZE + LEN_DATA_RECORD_NUMBER + LEN_CRC16 + LEN_PADDING;
        public static byte[] m_baVersionNumber      = new byte[LEN_VERSION_NUMBER] {0x01, 0x00};

        public static void setRevisonNumber(string sRevNum)
        {
            if(sRevNum.Contains('.'))
            {
                sRevNum = sRevNum.Split('.')[0];
            }
            
            m_baVersionNumber[1] = Decimal.ToByte((byte)Int32.Parse(sRevNum));
        }

        public static byte[] getVersionNumber()
        {
            return m_baVersionNumber;
        }

        public static int getOffsetMagicNumber()
        {
            return 0;
        }
        
        public static int getOffsetVersionNumber()
        {
            return getOffsetMagicNumber() + LEN_MAGIC_NUMBER;
        }

        public static int getOffsetDataSize()
        {
            return getOffsetVersionNumber() + LEN_VERSION_NUMBER;
        }
        
        public static int getOffsetDataRecordNumber()
        {
            return getOffsetDataSize() + LEN_DATA_SIZE;
        }

        public static int getOffsetCrc16()
        {
            return getOffsetDataRecordNumber() + LEN_DATA_RECORD_NUMBER;
        }
    }
}