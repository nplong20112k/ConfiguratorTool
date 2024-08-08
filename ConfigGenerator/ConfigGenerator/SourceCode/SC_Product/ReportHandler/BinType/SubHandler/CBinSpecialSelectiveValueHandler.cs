using System.Text;

namespace ConfigGenerator
{
    class CBinSpecialSelectiveValueHandler : ABinSubHandler
    {
        private const string BIN_REPORT_NORMAL_CONSTANT_BINARY_FORMAT_VALUE_FILE_NAME   = "ciro_ssvf.bin"; 
        private const string MAGIC_NUMBER_NAME                                          = "SSVF";
        private string  m_sOutputFolder                                                 = null;
        private uint m_uiDataSize                                                       = 0;
        private ushort m_usDataRecordNumber                                             = 0;
        private List<CI_ENUM_INFO>  m_lsNormalEnumInfo                                  = null;
        private bool    m_bFileComplete                                                 = false;
        private IBinFileProcessor m_FileProcessor                                       = null;
        private CCrcCalculator m_CalcCRC16                                              = null;
        private byte[]    m_baHeader                                                    = null;
        private List<byte[]>    m_lsData                                                = null; 


        public CBinSpecialSelectiveValueHandler() : base()
        {
            m_FileProcessor = CFactoryBinFileProcessor.GetInstance().GetFileProcessor(BIN_FILE_PROCESSOR_TYPE.BIN_SPECIAL_SELECTIVE_VALUE);
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
                        if (CommonConstTable.GetComConstTableType() == COMMON_CONST_TABLE_TYPE.SPECIAL_TABLE)
                        {
                            CSpecialConstItem SpecialConstTable = CommonConstTable as CSpecialConstItem;

                            string sTagName = CommonConstTable.GetTagName();
                            string sMinValue = SpecialConstTable.GetMinValue();
                            string sMaxValue = SpecialConstTable.GetMaxValue();

                            if ((string.IsNullOrEmpty(sMinValue) == false) &&
                                (string.IsNullOrEmpty(sMaxValue) == false))
                            {
                                byte[] baRecordValueRangeSize = new byte[2]{0x00, 0x04};
                                string sRangeType = "01";
                                byte[] byMinValue = CStringByteConvertor.convertHexStringToByteArray(sMinValue.PadLeft(4, '0'));
                                byte[] byMaxValue = CStringByteConvertor.convertHexStringToByteArray(sMaxValue.PadLeft(4, '0'));
                                byte[] baRecordValue = new byte[4] {byMinValue[0], 
                                                                    byMinValue[1], 
                                                                    byMaxValue[0], 
                                                                    byMaxValue[1]};

                                bRet = AddDataRecord(   sTagName,
                                                        sRangeType,
                                                        baRecordValueRangeSize,
                                                        baRecordValue);
                                if(bRet == false)
                                {
                                    return bRet;
                                }
                            }

                            List<string> lsValueList = SpecialConstTable.GetValueList();
                            VALUE_LIST_TYPE eValueListType = SpecialConstTable.GetValueListType();

                            if (lsValueList != null && eValueListType == VALUE_LIST_TYPE.ACCEPTED_TYPE)
                            {
                                string sListType = "02";
                                List<byte[]> lsRecordValue = new List<byte[]>();

                                foreach (string element in lsValueList)
                                {
                                    lsRecordValue.Add(CStringByteConvertor.convertHexStringToByteArray(element));
                                }
                                byte[] baCombineRecordValue = lsRecordValue
                                                                .SelectMany(a => a)
                                                                .ToArray();

                                string sDataLen = ((ushort)baCombineRecordValue.Length).ToString("X").PadLeft(4, '0');
                                byte[] baRecordValueListSize = CStringByteConvertor.convertHexStringToByteArray(sDataLen);

                                bRet = AddDataRecord(   sTagName,
                                                        sListType,
                                                        baRecordValueListSize,
                                                        baCombineRecordValue);
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
                lsByteArrayElement.Add(CStringByteConvertor.convertBEByteArrayToLE(baRecordValueSize));
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