using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeDeltaSpecialTableHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_DELTA_TABLE_FILE_NAME  = "config_item_delta_special_table";

        private const string DELTA_TABLE_FORMAT                 = "{0}const {1,-30} {2}[] = \n" +
                                                                  "{0}{{\n" +
                                                                  "{3}" +
                                                                  "{0}}};\n" +
                                                                  "\n";

        private const string DELTA_SPECIAL_ITEM_VALUE           = "(unsigned char *) \"{0}\"";

        private ICodeFileProcessor              m_FileProcessor                 = null;
        private CSourceCodeTemplate             m_SourceTemplate                = null;

        private string[]                        m_sContentSpecialDeltaTable     = null;
    
        private List<DELTA_TABLE_GROUP_NAME>    m_lsDeltaTableGroupName         = null;
        private string                          m_sOutputFolder                 = null;
        private string                          m_sOutputFileName               = null;

        private string                          m_sSpecialDeltaTableName        = null;
        private string                          m_sSpecialDeltaTableOutputType  = null;
        private string                          m_sUsingNamespace               = null;
        private string                          m_sDecleareNamespace            = null;

        private uint                            m_uiIndentDeltaFormat           = 0;
        private uint                            m_uiIndentDeltaItem             = 1;
        
        public CCodeDeltaSpecialTableHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_DELTA_TABLE_FILE_PROCESSOR);
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
                    m_sSpecialDeltaTableName = (oData as CSourceInfoObject).GetSpecialDeltaTableName();
                    m_sSpecialDeltaTableOutputType = (oData as CSourceInfoObject).GetSpecialDeltaTableOutputType();
                    m_lsDeltaTableGroupName = (oData as CSourceInfoObject).GetDeltaGroupName();
                    m_sDecleareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();
                    m_sUsingNamespace = (oData as CSourceInfoObject).GetUsingNamespace();

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        sOutputFileName = Path.DirectorySeparatorChar + CODE_REPORT_DELTA_TABLE_FILE_NAME + m_sSpecialDeltaTableOutputType;
                        m_FileProcessor.CreateNewFile(m_sOutputFolder + sOutputFileName);
                        m_sContentSpecialDeltaTable = new string[m_lsDeltaTableGroupName.Count];

                        if (m_sSpecialDeltaTableOutputType == m_SourceTemplate.m_sCPPtype)
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
                        string sNameCIItem = DeltaTableObj.GetCIItemName();
                        string sSizeValue = DeltaTableObj.GetSizeValue();
                        List<DELTA_TABLE_ITEM> lsDeltaTableGroup = DeltaTableObj.GetDeltaTableGroup();

                        foreach (DELTA_TABLE_ITEM element in lsDeltaTableGroup)
                        {
                            for (int i = 0; i < m_lsDeltaTableGroupName.Count; i++)
                            {
                                if (element.m_sInterfaceName == m_lsDeltaTableGroupName[i].m_sInterfaceName)
                                {
                                    string sValue = null;
                                    if (sSizeValue != "1")
                                    {
                                        sValue = string.Format(DELTA_SPECIAL_ITEM_VALUE, element.m_sValue);
                                        m_sContentSpecialDeltaTable[i] += string.Format(m_SourceTemplate.m_sDeltaTableItem,
                                                                                        m_SourceTemplate.GetIndent(m_uiIndentDeltaItem),
                                                                                        sNameCIItem,
                                                                                        sValue);                                        
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
            string sTempSpecialItem     = null;
            string sSpecialTerminator   = null;
       
            sSpecialTerminator = string.Format(m_SourceTemplate.m_sDeltaTableItem,
                                                m_SourceTemplate.GetIndent(m_uiIndentDeltaItem),
                                                "0",
                                                "NULL");

            for (int i = 0; i < m_lsDeltaTableGroupName.Count; i++)
            {
                m_sContentSpecialDeltaTable[i] += sSpecialTerminator;

                sTempSpecialItem += string.Format(DELTA_TABLE_FORMAT,
                                                    m_SourceTemplate.GetIndent(m_uiIndentDeltaFormat),
                                                    m_sSpecialDeltaTableName,
                                                    m_lsDeltaTableGroupName[i].m_sEugeneSpecialName,
                                                    m_sContentSpecialDeltaTable[i]);
            }

            if (m_sSpecialDeltaTableOutputType == m_SourceTemplate.m_sCPPtype)
            {
                if (m_sDecleareNamespace != null)
                {
                    sTempSpecialItem += m_SourceTemplate.m_sDecleareNamespaceFooter;
                }
            }          
            
            m_FileProcessor.AddNewItem(sTempSpecialItem);
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

            if ((m_sOutputFolder != null)                   && (m_sOutputFolder != string.Empty)                &&
                (m_sOutputFileName != null)                 && (m_sOutputFileName != string.Empty)              &&
                (m_sSpecialDeltaTableName != null)          && (m_sSpecialDeltaTableName != string.Empty)       &&
                (m_sSpecialDeltaTableOutputType != null)    && (m_sSpecialDeltaTableOutputType != string.Empty) &&
                (m_lsDeltaTableGroupName != null)           && (m_lsDeltaTableGroupName.Count > 0))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_sContentSpecialDeltaTable     = null;
            m_lsDeltaTableGroupName         = null;
            m_sOutputFolder                 = null;
            m_sOutputFileName               = null;
            m_sSpecialDeltaTableName        = null;
            m_sSpecialDeltaTableOutputType  = null;
            m_sUsingNamespace               = null;
            m_sDecleareNamespace            = null;
            m_uiIndentDeltaFormat           = 0;
            m_uiIndentDeltaItem             = 1;
        }
    }
}