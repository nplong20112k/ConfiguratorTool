using System.Net.Http.Headers;
using System.Text;

namespace ConfigGenerator
{
    class CBinNormalExceptionValueHandler : ABinSubHandler
    {
        private class CExceptionRecord
        {
            public string m_sRecordType;
            public int m_iMin;
            public int m_iMax;
            public int m_iValue;
            public CExceptionRecord()
            {
                m_sRecordType = "NONE";
            }
            public CExceptionRecord(int iValue)
            {
                m_sRecordType = "LIST";
                m_iValue = m_iMin = m_iMax = iValue;
            }
        }
        private const string BIN_REPORT_NORMAL_CONSTANT_BINARY_FORMAT_VALUE_FILE_NAME   = "ciro_nevf.bin"; 
        private const string MAGIC_NUMBER_NAME                                          = "NEVF";
        private string  m_sOutputFolder                                                 = null;
        private uint m_uiDataSize                                                       = 0;
        private ushort m_usDataRecordNumber                                             = 0;
        private List<CI_ENUM_INFO>  m_lsNormalEnumInfo                                  = null;
        private bool    m_bFileComplete                                                 = false;
        private IBinFileProcessor m_FileProcessor                                       = null;
        private List<CExceptionRecord> m_lsParser                                       = null;
        private CCrcCalculator m_CalcCRC16                                              = null;
        private byte[]    m_baHeader                                                    = null;
        private List<byte[]>    m_lsData                                                = null; 


        public CBinNormalExceptionValueHandler() : base()
        {
            m_FileProcessor = CFactoryBinFileProcessor.GetInstance().GetFileProcessor(BIN_FILE_PROCESSOR_TYPE.BIN_NORMAL_EXCEPTION_VALUE);
            m_CalcCRC16     = new CCrcCalculator();

            if (m_FileProcessor == null || m_CalcCRC16 == null)
            {
                return;
            }
        }

        public override bool Initialize(List<IShareObject> oDataList)
        {
            bool bRet = false;
            ReInitProperties();

            foreach (IShareObject oData in oDataList)
            {
                if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                {
                    m_sOutputFolder = (oData as ICommonProperties).GetOutputFolder();
                    m_lsNormalEnumInfo = (oData as CSourceInfoObject).GetCIEnumInfo()[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM];
                    m_lsParser = new List<CExceptionRecord>();
                    m_baHeader = new byte[CBinGeneralData.LEN_HEADER];
                    m_lsData = new List<byte[]>();

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + BIN_REPORT_NORMAL_CONSTANT_BINARY_FORMAT_VALUE_FILE_NAME);
                        bRet = AddHeader();
                    }
                    
                }
            }


            return bRet;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            bool bRet = false;
            if (m_FileProcessor != null)
            {
                if (oDataIn != null)
                {
                    ACommonConstTable CommonConstTable = (oDataIn as CIntegratedDataObject).GetCommonConstTable();
                    if (CommonConstTable != null)
                    {
                        if (CommonConstTable.GetComConstTableType() == COMMON_CONST_TABLE_TYPE.NORMAL_TABLE)
                        {
                            CNormalConstItem NormalConstTable = CommonConstTable as CNormalConstItem;
                            List<string> sExceptionValueList = NormalConstTable.GetExceptionValueList();

                            if (sExceptionValueList != null && (sExceptionValueList.Count > 0))
                            {
                                string sTagName = CommonConstTable.GetTagName();
                                m_lsParser = SeparateToConsecutiveList(sExceptionValueList);

                                List<int> ListItem = new List<int>();
                                foreach (CExceptionRecord subList in m_lsParser)
                                {
                                    if(subList.m_sRecordType == "RANGE")
                                    {
                                        byte[] baRecordValueRangeSize = new byte[1]{0x02};
                                        string sRangeType = "01";
                                        byte[] baRecordValueRange = new byte[2]{Decimal.ToByte((byte)subList.m_iMin), Decimal.ToByte((byte)subList.m_iMax)};
                                        bRet = AddDataRecord(   sTagName,
                                                                sRangeType,
                                                                baRecordValueRangeSize,
                                                                baRecordValueRange);
                                        if(bRet == false)
                                        {
                                            return bRet;
                                        }
                                    }
                                    else if (subList.m_sRecordType == "LIST")
                                    {
                                        ListItem.Add(subList.m_iValue);
                                    }

                                }

                                if(ListItem.Count > 0)
                                {
                                    byte[] baRecordValueListSize = new byte[1] {(byte)ListItem.Count};
                                    string sListType = "02";
                                    byte[] baRecordValueList = new byte[ListItem.Count];
                                    int k = 0;
                                    foreach (int element in ListItem)
                                    {
                                        baRecordValueList[k++] = Decimal.ToByte((byte)element);
                                    }
                                    bRet = AddDataRecord(   sTagName,
                                                            sListType,
                                                            baRecordValueListSize,
                                                            baRecordValueList);
                                    if(bRet == false)
                                    {
                                        return bRet;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            byte[] baUpdateElement = null;

            baUpdateElement = CStringByteConvertor.convertBEStringToLEByteArray(((ushort)m_uiDataSize).ToString("X4"));
            Buffer.BlockCopy(baUpdateElement, 0, m_baHeader, CBinGeneralData.getOffsetDataSize(), baUpdateElement.Length);
            if(!m_FileProcessor.AddNewItem(baUpdateElement, CBinGeneralData.getOffsetDataSize(), baUpdateElement.Length))
            {
                return false;
            }
            
            baUpdateElement = CStringByteConvertor.convertBEStringToLEByteArray(m_usDataRecordNumber.ToString("X4"));
            Buffer.BlockCopy(baUpdateElement, 0, m_baHeader, CBinGeneralData.getOffsetDataRecordNumber(), baUpdateElement.Length);
            if(!m_FileProcessor.AddNewItem(baUpdateElement, CBinGeneralData.getOffsetDataRecordNumber(), baUpdateElement.Length))
            {
                return false;
            }

            baUpdateElement = m_lsData.SelectMany(s => s).ToArray();
            m_CalcCRC16.CalcConfigCRC(m_baHeader, m_baHeader.Length);
            m_CalcCRC16.CalcConfigCRC(baUpdateElement, baUpdateElement.Length);

            baUpdateElement = CStringByteConvertor.convertBEStringToLEByteArray(m_CalcCRC16.getConfigCRC().ToString("X4"));
            Buffer.BlockCopy(baUpdateElement, 0, m_baHeader, CBinGeneralData.getOffsetCrc16(), baUpdateElement.Length);
            if(!m_FileProcessor.AddNewItem(baUpdateElement, CBinGeneralData.getOffsetCrc16(), baUpdateElement.Length))
            {
                return false;
            }

            if ((m_bFileComplete == false) && (m_FileProcessor != null))
            {
                m_FileProcessor.CloseFile();
                m_bFileComplete = true;
            }

            return true;
        }

        private static List<CExceptionRecord> SeparateToConsecutiveList(List<string> sExceptionValueList)
        {
            List<CExceptionRecord> lsRangeOut = new List<CExceptionRecord>();
            List<CExceptionRecord> lsListOut = new List<CExceptionRecord>();
            CExceptionRecord element = new CExceptionRecord();
            foreach (string ExceptionValueElement in sExceptionValueList)
            {
                int iCurrentValue = int.Parse(ExceptionValueElement);
                if (element.m_sRecordType == "NONE")
                {
                    element.m_iValue = element.m_iMin = element.m_iMax = iCurrentValue;
                    element.m_sRecordType = "LIST";
                }
                else if (element.m_sRecordType == "LIST")
                {
                    if ((iCurrentValue - element.m_iValue) > 1)
                    {
                        // Non consecutive value => save current element and create new one
                        lsListOut.Add(element);
                        element = new CExceptionRecord(iCurrentValue);
                    }
                    else
                    {
                        // Consecutive value => change to RANGE and wait till max value
                        element.m_sRecordType = "RANGE";
                        element.m_iMax = iCurrentValue;
                    }
                }
                else if (element.m_sRecordType == "RANGE")
                {
                    if ((iCurrentValue - element.m_iMax) > 1)
                    {
                        // Non consecutive value
                        lsRangeOut.Add(element);
                        element = new CExceptionRecord(iCurrentValue);
                    }
                    else
                    {
                        element.m_iMax = iCurrentValue;
                    }
                }
            }

            if (element.m_sRecordType == "LIST")
            {
                lsListOut.Add(element);

            }
            else if (element.m_sRecordType == "RANGE")
            {
                lsRangeOut.Add(element);
            }
            lsRangeOut.AddRange(lsListOut);

            
            return lsRangeOut;
        }

        private bool AddHeader()
        {
            Buffer.BlockCopy(CStringByteConvertor.convertStringToByteArray(MAGIC_NUMBER_NAME), 0, m_baHeader, CBinGeneralData.getOffsetMagicNumber(), CBinGeneralData.LEN_MAGIC_NUMBER);  
            Buffer.BlockCopy(CBinGeneralData.getVersionNumber(), 0, m_baHeader, CBinGeneralData.getOffsetVersionNumber(), CBinGeneralData.LEN_VERSION_NUMBER);

            if(!m_FileProcessor.AddNewItem(m_baHeader, m_baHeader.Length))
            {
                return false;
            }

            return true;
        }

        private void ReInitProperties()
        {
            m_lsNormalEnumInfo          = null;
            m_bFileComplete             = false;
            m_uiDataSize                = 0;
            m_usDataRecordNumber        = 0;
            m_baHeader                  = null;
            m_lsData                    = null;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sOutputFolder) == false) &&
                (m_lsNormalEnumInfo != null) && (m_lsNormalEnumInfo.Count > 0))
            {
                bRet = true;
            }

            return bRet;
        }

        private bool AddDataRecord(string sTagName, string sRecordType, byte[] baRecordValueSize, byte[] baRecordValue)
        {
            bool bRet = false;
            if(m_lsNormalEnumInfo != null && m_lsNormalEnumInfo.Count > 0)
            {
                int iIndex = m_lsNormalEnumInfo.FindIndex(x => x.m_sCITagName.Equals(sTagName));
                if(iIndex == -1)
                {
                    // cannot file the item in enum file
                    return false;
                }

                string sEnumValue;
                CHexIntConverter.DetectAndTrimHexPrefix(m_lsNormalEnumInfo[iIndex].m_sCIEnumValue, out sEnumValue);

                List<byte[]> lsByteArrayElement = new List<byte[]>();

                lsByteArrayElement.Add(CStringByteConvertor.convertBEStringToLEByteArray(sEnumValue));
                lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(sRecordType));
                lsByteArrayElement.Add(baRecordValueSize);
                lsByteArrayElement.Add(baRecordValue);

                foreach (byte[] element in lsByteArrayElement)
                {
                    m_lsData.Add(element);
                    m_uiDataSize += (uint)element.Length;
                    if(!m_FileProcessor.AddNewItem(element, element.Length))
                    {
                        return false;
                    }
                }

                m_usDataRecordNumber += 1;
                bRet = true;
            }

            return bRet;
        }
    }
}