using System.Text;

namespace ConfigGenerator
{
    class CBinSpecialConstantHandler : ABinSubHandler
    {
        private const string BIN_REPORT_NORMAL_CONSTANT_BINARY_FORMAT_VALUE_FILE_NAME   = "ciro_scbf.bin";
        private const string MAGIC_NUMBER_NAME                                          = "SCBF";
        private string  m_sOutputFolder                                                 = null;
        private uint m_uiDataSize                                                       = 0;
        private ushort m_usDataRecordNumber                                             = 0;
        private List<CI_ENUM_INFO>  m_lsNormalEnumInfo                                  = null;
        private bool    m_bFileComplete                                                 = false;
        private IBinFileProcessor m_FileProcessor                                       = null;
        private List<SUPPORT_INTERFACE> m_sSupportItf                                   = null;
        private CCrcCalculator m_CalcCRC16                                              = null;
        private byte[]    m_baHeader                                                    = null;
        private List<byte[]>    m_lsData                                                = null; 

        public CBinSpecialConstantHandler() : base()
        {
            m_FileProcessor = CFactoryBinFileProcessor.GetInstance().GetFileProcessor(BIN_FILE_PROCESSOR_TYPE.BIN_SPECIAL_CONSTANT);
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
                    m_sSupportItf = (oData as CSourceInfoObject).GetSupportInterfaceInfo();
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

            return bRet ;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            bool bRet = false;
            if (m_FileProcessor != null)
            {
                if (oDataIn != null)
                {
                    ACommonConstTable CommonConstTable = null;
                    CommonConstTable = (oDataIn as CIntegratedDataObject).GetCommonConstTable();

                    if (CommonConstTable != null)
                    {
                        if (CommonConstTable.GetComConstTableType() == COMMON_CONST_TABLE_TYPE.SPECIAL_TABLE)
                        {
                            CSpecialConstItem SpecialConstItem = CommonConstTable as CSpecialConstItem;
                            string sTagCode = SpecialConstItem.GetTagCode();
                            string sDefaultValue    = SpecialConstItem.GetDefaultValue();
                            string sAltDefaultValue = SpecialConstItem.GetAltDefaultValue();
                            string sInterface = string.Format("({0})", SpecialConstItem.GetInterfaceMask());

                            bRet = AddDataRecord(   SpecialConstItem.GetTagName(),
                                                    sTagCode,
                                                    sInterface,
                                                    SpecialConstItem.GetTagValueSize(),
                                                    sDefaultValue,
                                                    sAltDefaultValue);

                        }

                    }
                }
            }


            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            byte[] baUpdateElement = null;

            AddTerminator();

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
            m_sSupportItf               = null;
            m_uiDataSize                = 0;
            m_usDataRecordNumber        = 0;
            m_baHeader                  = null;
            m_lsData                    = null;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sOutputFolder) == false) &&
                (m_lsNormalEnumInfo != null) && (m_lsNormalEnumInfo.Count > 0) &&
                (m_sSupportItf != null) && (m_sSupportItf.Count > 0))
            {
                bRet = true;
            }

            return bRet;
        }

        private bool AddDataRecord(string sTagName, string sTagCode, string sInfMask, string sSize, string sEugeneValue, string sBolognaValue)
        {
            bool bRet = false;

            if(m_lsNormalEnumInfo != null && m_lsNormalEnumInfo.Count > 0)
            {
                int iIndex = m_lsNormalEnumInfo.FindIndex(x => x.m_sCITagName.Equals(sTagName));
                if(iIndex == -1)
                {
                    // cannot file the item in enum file
                    Console.WriteLine("cannot file the item in enum file");
                    return false;
                }

                string sContent = string.Empty;

                string sEnumValue;
                CHexIntConverter.DetectAndTrimHexPrefix(m_lsNormalEnumInfo[iIndex].m_sCIEnumValue, out sEnumValue);

                List<byte[]> lsByteArrayElement = new List<byte[]>();

                lsByteArrayElement.Add(CStringByteConvertor.convertBEStringToLEByteArray(sEnumValue));
                lsByteArrayElement.Add(CStringByteConvertor.convertStringToByteArray(m_lsNormalEnumInfo[iIndex].m_sCITagCode));
                lsByteArrayElement.Add(CStringByteConvertor.convertBEStringToLEByteArray(GetInterfaceMaskHexValue(sInfMask)));
                lsByteArrayElement.Add(CStringByteConvertor.convertBEByteArrayToLE(CStringByteConvertor.convertHexStringToByteArray(Convert.ToUInt16(sSize).ToString("X").PadLeft(4, '0'))));  
                lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(sEugeneValue.Replace("\\x", "")));
                lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(sBolognaValue.Replace("\\x", "")));

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

        private void AddTerminator()
        {
            string sTagCode     = "FFFF";
            string sInterface   = "(IF_ALL)";
            string sTagName     = "CI_TERMINATOR";

            AddDataRecord(  sTagName,
                            sTagCode,
                            sInterface,
                            "0",
                            "0",
                            "0");
        }

        private string GetInterfaceMaskHexValue(string ItfMask)
        {
            List<string> lsItfMask = new List<string>();
            if(ItfMask.Contains("(") && ItfMask.Contains(")"))
            {
                ulong itfMaskValueAll = 0;
                ItfMask = (ItfMask.Split("(")[1]).Split(")")[0];
                if(ItfMask.Contains("|"))
                {
                    for(uint i = 0; i < ItfMask.Split("|").Length; i++)
                    {
                        lsItfMask.Add((ItfMask.Split("|")[i]).Split("IF_")[1].Trim());
                    }

                    foreach (SUPPORT_INTERFACE element in m_sSupportItf)
                    {
                        foreach (string element1 in lsItfMask)
                        {
                            if(element1 == element.m_sName)
                            {
                                itfMaskValueAll |= Convert.ToUInt64(element.m_sMask, 16);
                            }
                        }
                    }

                    return itfMaskValueAll.ToString("X16");
                }
                else
                {
                    foreach (SUPPORT_INTERFACE element in m_sSupportItf)
                    {
                        if(ItfMask.Contains(element.m_sName))
                        {
                            return element.m_sMask;
                        }
                    }
                }
            }

            return null;
        }
    }
}