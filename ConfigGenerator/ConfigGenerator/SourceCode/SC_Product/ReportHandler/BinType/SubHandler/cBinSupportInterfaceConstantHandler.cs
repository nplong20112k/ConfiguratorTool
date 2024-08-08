namespace ConfigGenerator
{
    class CBinSupportInterfaceConstantHandler : ABinSubHandler
    {
        private const string BIN_REPORT_NORMAL_CONSTANT_BINARY_FORMAT_VALUE_FILE_NAME   = "ciro_sicf.bin"; 
        private const string MAGIC_NUMBER_NAME                                          = "SICF";
        private string  m_sOutputFolder                                                 = null;
        private uint m_uiDataSize                                                       = 0;
        private ushort m_usDataRecordNumber                                             = 0;
        private List<INTERFACE_CLASS>   m_lsInterfaceClass                              = null;
        private bool    m_bFileComplete                                                 = false;
        private IBinFileProcessor m_FileProcessor                                       = null;
        private List<SUPPORT_INTERFACE> m_sSupportItf                                   = null;
        private CCrcCalculator m_CalcCRC16                                              = null;
        private byte[]    m_baHeader                                                    = null;
        private List<byte[]>    m_lsData                                                = null; 


        public CBinSupportInterfaceConstantHandler() : base()
        {
            m_FileProcessor = CFactoryBinFileProcessor.GetInstance().GetFileProcessor(BIN_FILE_PROCESSOR_TYPE.BIN_SUPPORT_INTERFACE_CONSTANT);
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
                    m_sSupportItf = (oData as CSourceInfoObject).GetSupportInterfaceInfo();
                    m_baHeader = new byte[CBinGeneralData.LEN_HEADER];
                    m_lsData = new List<byte[]>();
                    
                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + BIN_REPORT_NORMAL_CONSTANT_BINARY_FORMAT_VALUE_FILE_NAME);
                        bRet = AddHeader();
                        if(bRet == false)
                        {
                            return bRet;
                        }

                        foreach (INTERFACE_CLASS element in m_lsInterfaceClass)
                        {
                            foreach (INTERFACE_CLASS_MEMBER subElement in element.m_lsInterfaceMember)
                            {
                                string sItfValue = subElement.m_sCommand.Substring(2);
                                if (subElement.m_bInterfaceValid == true)
                                {
                                    foreach (SUPPORT_INTERFACE subList in m_sSupportItf)
                                    {
                                        if (subList.m_sValue == sItfValue)
                                        {
                                            AddDataRecord(  subList.m_sValue,
                                                            subList.m_sUserBlock,
                                                            subList.m_sConfigBlock,
                                                            subList.m_sMask         );
                                        }
                                    }
                                }
                            }
                        }

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

                        bRet = true;
                    }  
                }
            }


            return bRet;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            return true;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
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

            if ((m_lsInterfaceClass != null) && 
                (string.IsNullOrEmpty(m_sOutputFolder) == false) &&
                (m_sSupportItf != null) && (m_sSupportItf.Count > 0))
            {
                bRet = true;
            }

            return bRet;
        }

        private bool AddDataRecord(string sInterfaceType, string sUserBlockID, string sConfigBlockID, string sInterfaceMask)
        {
            bool bRet = false;
            List<byte[]> lsByteArrayElement = new List<byte[]>();

            lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(sInterfaceType));
            lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(sUserBlockID));
            lsByteArrayElement.Add(CStringByteConvertor.convertHexStringToByteArray(sConfigBlockID));
            lsByteArrayElement.Add(CStringByteConvertor.convertBEStringToLEByteArray(sInterfaceMask));

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

            return bRet;
        }
    }
}