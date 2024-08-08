using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeDeltaNormalTableHandler : ACodeSubHandler 
    {        
        private const string CODE_REPORT_DELTA_TABLE_FILE_NAME  = "config_item_delta_normal_table";
        private const string DELTA_TABLE_FORMAT                 = "{0}const {1,-30} {2}[] = \n" +
                                                                  "{0}{{\n" +
                                                                  "{3}" +
                                                                  "{0}}};\n" +
                                                                  "\n";
        private const string DELTA_NORMAL_ITEM_VALUE            = "0x{0}";

        ICodeFileProcessor                      m_FileProcessor                 = null;
        private CSourceCodeTemplate             m_SourceTemplate                = null;

        private string[]                        m_sContentNormalDeltaTable      = null;
        private List<DELTA_TABLE_GROUP_NAME>    m_lsDeltaTableGroupName         = null;
        private string                          m_sOutputFolder                 = null;
        private string                          m_sOutputFileName               = null;
   
        private string                          m_sNormalDeltaTableName         = null;
        private string                          m_sNormalDeltaTableOutputType   = null;
        private string                          m_sUsingNamespace               = null;
        private string                          m_sDecleareNamespace            = null;
       
        private uint                            m_uiIndentDeltaFormat           = 0;
        private uint                            m_uiIndentDeltaItem             = 1;

         

        public CCodeDeltaNormalTableHandler()
            :base()
        {
            m_FileProcessor  = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_DELTA_TABLE_FILE_PROCESSOR);
            m_SourceTemplate = CSourceCodeTemplate.GetInstance();

            if ((m_FileProcessor == null) || (m_SourceTemplate == null))
            {
                return;
            }
        }

        public override bool Initialize(List<IShareObject> oDataList)
        {
            bool bRet = false;
            string sOutputFileName = null;
            ReInitProperties();

            foreach (IShareObject oData in oDataList)
            {
                if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                {
                    m_sOutputFolder = (oData as ICommonProperties).GetOutputFolder();
                    m_sOutputFileName = (oData as CSourceInfoObject).GetOutputFileName();
                    m_sNormalDeltaTableName = (oData as CSourceInfoObject).GetNormalDeltaTableName();
                    m_sNormalDeltaTableOutputType = (oData as CSourceInfoObject).GetNormalDeltaTableOutputType();
                    m_lsDeltaTableGroupName = (oData as CSourceInfoObject).GetDeltaGroupName();
                    m_sDecleareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();
                    m_sUsingNamespace = (oData as CSourceInfoObject).GetUsingNamespace();

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {

                        sOutputFileName = Path.DirectorySeparatorChar + CODE_REPORT_DELTA_TABLE_FILE_NAME + m_sNormalDeltaTableOutputType;

                        m_FileProcessor.CreateNewFile(m_sOutputFolder + sOutputFileName);
                        m_sContentNormalDeltaTable = new string[m_lsDeltaTableGroupName.Count];

                        if (m_sNormalDeltaTableOutputType == m_SourceTemplate.m_sCPPtype)
                        {
                            if (m_sDecleareNamespace != null)
                            {
                                m_uiIndentDeltaFormat += 1;
                                m_uiIndentDeltaItem += 1;
                            }

                            AddFuncionHeader();
                        }

                        bRet = true;
                    }
                }
            }

            return bRet;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            if (m_FileProcessor != null)
            {
                if (oDataIn != null)
                {                    
                    CDeltaConstTable DeltaTableObj = (oDataIn as CIntegratedDataObject).GetDeltaTable();
                    if (DeltaTableObj != null)
                    {
                        string sNameCIItem                          = DeltaTableObj.GetCIItemName();
                        string sSizeValue                           = DeltaTableObj.GetSizeValue();
                        List<DELTA_TABLE_ITEM> lsDeltaTableGroup    = DeltaTableObj.GetDeltaTableGroup();

                        foreach (DELTA_TABLE_ITEM element in lsDeltaTableGroup)
                        {
                            for (int i = 0; i < m_lsDeltaTableGroupName.Count; i++)
                            {
                                if (element.m_sInterfaceName == m_lsDeltaTableGroupName[i].m_sInterfaceName)
                                {
                                    string sValue = null;
                                    if (sSizeValue == "1")
                                    {
                                        sValue = string.Format(DELTA_NORMAL_ITEM_VALUE, element.m_sValue.PadLeft(2, '0'));                                        
                                        m_sContentNormalDeltaTable[i] += string.Format(m_SourceTemplate.m_sDeltaTableItem,
                                                                                        m_SourceTemplate.GetIndent(m_uiIndentDeltaItem),
                                                                                        sNameCIItem,
                                                                                        sValue);                                                                   
                                        // fail syntax
                                        //m_lsInterfaceMap[i].m_sContentNormalDeltaTable += string.Format(CODE_REPORT_DELTA_TABLE_NORMAL_ITEM,
                                        //                                                            sNameCIItem,
                                        //                                                            element.m_sValue);
                                    }
                                }
                            }
                        }

                    }
                }
            }

            return true;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            string sNormalTerminator    = null;
            string sTempNormalItem      = null;

            sNormalTerminator = string.Format(  m_SourceTemplate.m_sDeltaTableItem,
                                                m_SourceTemplate.GetIndent(m_uiIndentDeltaItem),
                                                "0",
                                                "0");

            for (int i = 0; i < m_lsDeltaTableGroupName.Count; i++)
            {
                m_sContentNormalDeltaTable[i] += sNormalTerminator;

                sTempNormalItem += string.Format(DELTA_TABLE_FORMAT,
                                                    m_SourceTemplate.GetIndent(m_uiIndentDeltaFormat),
                                                    m_sNormalDeltaTableName,
                                                    m_lsDeltaTableGroupName[i].m_sEugeneName,
                                                    m_sContentNormalDeltaTable[i]);
            }

            if (m_sNormalDeltaTableOutputType == m_SourceTemplate.m_sCPPtype)
            {
                if (m_sDecleareNamespace != null)
                {
                    sTempNormalItem += m_SourceTemplate.m_sDecleareNamespaceFooter; 
                }
            }

            m_FileProcessor.AddNewItem(sTempNormalItem);
            return true;
        }
                    
        private void AddFuncionHeader()
        {
            string sTemp;

            if (m_FileProcessor != null)
            {                
                sTemp = string.Format(  m_SourceTemplate.m_sIncludeHeader,
                                        m_sOutputFileName);

                if (m_sUsingNamespace != null)
                {
                    sTemp += string.Format(m_SourceTemplate.m_sUsingNamespace,
                                            m_sUsingNamespace);
                }

                if (m_sDecleareNamespace != null)
                {
                    sTemp += string.Format(m_SourceTemplate.m_sDecleareNamespaceHeader,
                                            m_sDecleareNamespace);
                }

                m_FileProcessor.AddNewItem(sTemp);             
            }
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((m_sOutputFolder != null)               && (m_sOutputFolder != string.Empty)            &&                
                (m_sOutputFileName != null)             && (m_sOutputFileName != string.Empty)          &&
                (m_sNormalDeltaTableName != null)       && (m_sNormalDeltaTableName != string.Empty)    &&
                (m_sNormalDeltaTableOutputType != null) && (m_sNormalDeltaTableOutputType != string.Empty) &&
                (m_lsDeltaTableGroupName != null)       && (m_lsDeltaTableGroupName.Count > 0))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_sContentNormalDeltaTable      = null;
            m_lsDeltaTableGroupName         = null;
            m_sOutputFolder                 = null;
            m_sOutputFileName               = null;
            m_sNormalDeltaTableName         = null;
            m_sNormalDeltaTableOutputType   = null;
            m_sUsingNamespace               = null;
            m_sDecleareNamespace            = null;
            m_uiIndentDeltaFormat           = 0;
            m_uiIndentDeltaItem             = 1;
        }
    }
}