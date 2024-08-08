using System.Text;

namespace ConfigGenerator
{
    class CBinSpecialDeltaConstantHandler : ABinSubHandler
    {
        private struct INTERFACE_VALID
        {
            public string m_sInterfaceName;
            public string m_sItfValue;
        }
        private const string BIN_REPORT_NORMAL_CONSTANT_BINARY_FORMAT_VALUE_FILE_NAME   = "ciro_sdcf.bin"; 
        private const string MAGIC_NUMBER_NAME                                          = "SDCF";
        private string  m_sOutputFolder                                                 = null;
        private uint m_uiDataSize                                                       = 0;
        private ushort m_usDataRecordNumber                                             = 0;
        private List<CI_ENUM_INFO>  m_lsNormalEnumInfo                                  = null;
        private List<INTERFACE_CLASS>   m_lsInterfaceClass                              = null;
        private List<INTERFACE_VALID> m_slsInterfaceValid                               = null;
        private bool    m_bFileComplete                                                 = false;
        private IBinFileProcessor m_FileProcessor                                       = null;
        private CCrcCalculator m_CalcCRC16                                              = null;
        private byte[]    m_baHeader                                                    = null;
        private List<byte[]>    m_lsData                                                = null; 


        public CBinSpecialDeltaConstantHandler() : base()
        {
            m_FileProcessor = CFactoryBinFileProcessor.GetInstance().GetFileProcessor(BIN_FILE_PROCESSOR_TYPE.BIN_SPECIAL_DELTA_CONSTANT);
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
                    m_lsInterfaceClass = (oData as CSourceInfoObject).GetInterfaceClass();
                    m_lsNormalEnumInfo = (oData as CSourceInfoObject).GetCIEnumInfo()[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM];
                    m_baHeader = new byte[CBinGeneralData.LEN_HEADER];
                    m_lsData = new List<byte[]>();

                    m_slsInterfaceValid = new List<INTERFACE_VALID>();
                    INTERFACE_VALID stItfValid;
                    stItfValid.m_sInterfaceName = string.Empty;
                    stItfValid.m_sItfValue = string.Empty;
                    foreach (INTERFACE_CLASS element in m_lsInterfaceClass)
                    {
                        foreach (INTERFACE_CLASS_MEMBER subElement in element.m_lsInterfaceMember)
                        {
                            string sItfValue = subElement.m_sCommand.Substring(2);
                            if (subElement.m_bInterfaceValid == true)
                            {
                                stItfValid.m_sInterfaceName = subElement.m_sInterfaceName;
                                stItfValid.m_sItfValue = sItfValue;

                                m_slsInterfaceValid.Add(stItfValid);
                            }
                        }
                    }
                    
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
                    CDeltaConstTable DeltaTableObj = (oDataIn as CIntegratedDataObject).GetDeltaTable();
                    if (DeltaTableObj != null)
                    {
                        string sTagName                             = DeltaTableObj.GetCIItemName();
                        string sSizeValue                           = DeltaTableObj.GetSizeValue();
                        List<DELTA_TABLE_ITEM> lsDeltaTableGroup    = DeltaTableObj.GetDeltaTableGroup();

                        foreach (DELTA_TABLE_ITEM element in lsDeltaTableGroup)
                        {
                            string sValue = null;
                            if (sSizeValue != "1")
                            {
                                sValue = element.m_sValue;
                                bRet = AddDataRecord(   element.m_sInterfaceName,
                                                        sTagName, 
                                                        sSizeValue,
                                                        sValue);
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
            m_slsInterfaceValid         = null;
            m_uiDataSize                = 0;
            m_usDataRecordNumber        = 0;
            m_baHeader                  = null;
            m_lsData                    = null;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ( (string.IsNullOrEmpty(m_sOutputFolder) == false) &&
                 (m_lsNormalEnumInfo != null) && (m_lsNormalEnumInfo.Count > 0) &&
                 (m_lsInterfaceClass != null))
            {
                bRet = true;
            }

            return bRet;
        }

        private bool AddDataRecord(string sInterfaceType, string sTagName, string sDefaultValueSize, string sDefaultValue)
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

                List<byte[]> lsByteArrayElement = new List<byte[]>();
                string sEnumValue;
                CHexIntConverter.DetectAndTrimHexPrefix(m_lsNormalEnumInfo[iIndex].m_sCIEnumValue, out sEnumValue);

                foreach (INTERFACE_VALID element in m_slsInterfaceValid)
                {
                    if(element.m_sInterfaceName == sInterfaceType)
                    {
                        lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(element.m_sItfValue));
                        lsByteArrayElement.Add(CStringByteConvertor.convertBEStringToLEByteArray(sEnumValue));
                        lsByteArrayElement.Add(CStringByteConvertor.convertBEByteArrayToLE(CStringByteConvertor.convertHexStringToByteArray(Convert.ToUInt16(sDefaultValueSize).ToString("X").PadLeft(4, '0'))));
                        lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(sDefaultValue.Replace("\\x", "")));
                        break;
                    }
                }

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