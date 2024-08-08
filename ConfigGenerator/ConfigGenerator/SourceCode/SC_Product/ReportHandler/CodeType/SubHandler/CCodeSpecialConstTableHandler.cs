using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeSpecialConstTableHandler : ACodeSubHandler
    {
        // This var should be considered to remove !! : its fixed number of elements within a item.
        private const int    KW_SPECIAL_ELEMENT_NUMBER           = 8;

        private const string CODE_REPORT_SPECIAL_TABLE_FILE_NAME = "config_item_special";

        private const string CODE_REPORT_SPECIAL_TABLE_HEADER    = "{0}struct {1}*  {3}()\n"                +
                                                                   "{0}{{\n"                                +
                                                                   "{0}    return (struct {1} *)&{2}[0];\n" +
                                                                   "{0}}}\n"                                +
                                                                   "\n"                                     +
                                                                   "{0}const struct {1} {2}[] =\n"          +
                                                                   "{0}{{\n";

        private const string CODE_REPORT_SPECIAL_TERMINATOR_NAME = "CI_TERMINATOR";

        private const string CODE_REPORT_SPECIAL_TERMINATOR_CODE = "FFFF";

        private const string CODE_REPORT_SPECIAL_TABLE_ITEM      = "{0}{{ {1,-54},{2} ,(unsigned char *){3,-50},(unsigned char *){4}\n" +
                                                                   "{0}    ,(unsigned char *){5}\n" +
                                                                   "{0}    ,{6,-7},(INTERFACE_MASK_TYPE){7}\n" +
                                                                   "{0}    ,{8,-15}}},\n\n";
                    
                                                                   // "{0}{{ {1,-54},{2} ,(unsigned char *){3,-50},(unsigned char *){4},\n" +
                                                                   // "{0}(unsigned char *){5,-119},{6,-7},(INTERFACE_MASK_TYPE){7,-50},{8,-15}}},\n\n";

        private const string CODE_REPORT_SPECIAL_STRUCTURE       = "&{0}.{1}";
        private const string CODE_REPORT_SPECIAL_DEFAULT_VALUE   = "\"{0}\" /*{1}*/";
        private const string CODE_REPORT_FOOTER                  = "{0}}};\n";

        private ICodeFileProcessor           m_FileProcessor            = null;
        private CSourceCodeTemplate          m_SourceTemplate           = null;
        private bool                         m_bFileComplete            = false;

        private string                       m_sOutputFolder            = null;
        private string                       m_sOutputFileName          = null;
        private string                       m_sSpecialTableType        = null;
        private string                       m_sSpecialTableName        = null;
        private string                       m_sSpecialTableGetFunc     = null;
        private string                       m_sCmVariableName          = null;
        private string                       m_sUsingNamespace          = null;
        private string                       m_sDecleareNamespace       = null;
        private List<TABLE_STRUCT_DATA_TYPE> m_SpecialTableElementOrder = null;

        private string                       m_sSpecialTableOutputType  = null;

        private uint                         m_uiIndentSpecialTableHeader  = 0;
        private uint                         m_uiIndentSpecialTableItem    = 1;
        private uint                         m_uiIndentSpecialTableFooter  = 0;

        public CCodeSpecialConstTableHandler()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_TABLE_FILE_PROCESSOR);
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
                    m_sDecleareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();
                    m_sUsingNamespace = (oData as CSourceInfoObject).GetUsingNamespace();
                    m_sSpecialTableType = (oData as ISpecialTableProperties).GetSpecialTableType();
                    m_sSpecialTableName = (oData as ISpecialTableProperties).GetSpecialTableName();
                    m_sSpecialTableGetFunc = (oData as ISpecialTableProperties).GetSpecialTableGetFunc();
                    m_sCmVariableName = (oData as ISpecialTableProperties).GetCmVariableName();
                    m_SpecialTableElementOrder = (oData as ISpecialTableProperties).GetSpecialTableElementOrder();

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_sSpecialTableOutputType = (oData as CSourceInfoObject).GetSpecialTableOutputType();
                        sOutputFileName = Path.DirectorySeparatorChar + CODE_REPORT_SPECIAL_TABLE_FILE_NAME + m_sSpecialTableOutputType;

                        m_FileProcessor.CreateNewFile(m_sOutputFolder + sOutputFileName);
                        if (m_sSpecialTableOutputType == m_SourceTemplate.m_sCPPtype)
                        {
                            if (m_sDecleareNamespace != null)
                            {
                                m_uiIndentSpecialTableHeader += 1;
                                m_uiIndentSpecialTableItem += 1;
                                m_uiIndentSpecialTableFooter += 1;
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

                            string sTagCode = CCodeFormater.FormatTagCode (SpecialConstItem.GetTagCode());

                            string sDefaultValue    = string.Format( CODE_REPORT_SPECIAL_DEFAULT_VALUE, 
                                                                     SpecialConstItem.GetDefaultValue(),
                                                                     SpecialConstItem.GetDefaultString() );

                            string sAltDefaultValue = string.Format( CODE_REPORT_SPECIAL_DEFAULT_VALUE,
                                                                     SpecialConstItem.GetAltDefaultValue(),
                                                                     SpecialConstItem.GetAltDefaultString() );
                            
                            string sStructure       = string.Format( CODE_REPORT_SPECIAL_STRUCTURE,
                                                                     m_sCmVariableName,
                                                                     SpecialConstItem.GetStructure() );

                            string sInterface = CCodeFormater.FormatInterface( SpecialConstItem.GetInterfaceMask());

                            string[] sElementsOrder = ArrangeElementsAsOrder( SpecialConstItem.GetTagName(),
                                                                              sTagCode,
                                                                              sStructure,
                                                                              sDefaultValue,
                                                                              sAltDefaultValue,
                                                                              SpecialConstItem.GetTagValueSize(),
                                                                              sInterface,
                                                                              SpecialConstItem.GetProductMask() );

                            if ((m_SpecialTableElementOrder != null) && (m_SpecialTableElementOrder.Count == KW_SPECIAL_ELEMENT_NUMBER))
                            {
                                string sNewItem = null;

                                sNewItem = string.Format(CODE_REPORT_SPECIAL_TABLE_ITEM,
                                                            m_SourceTemplate.GetIndent(m_uiIndentSpecialTableItem),
                                                            sElementsOrder[0],
                                                            sElementsOrder[1],
                                                            sElementsOrder[2],
                                                            sElementsOrder[3],
                                                            sElementsOrder[4],
                                                            sElementsOrder[5],
                                                            sElementsOrder[6],
                                                            sElementsOrder[7]);

                                bRet = m_FileProcessor.AddNewItem(sNewItem);
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            if ((m_bFileComplete == false) && (m_FileProcessor != null))
            {
                AddTerminator();
                m_FileProcessor.CloseFile();
                m_bFileComplete = true;
            }

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
                    sTemp += string.Format( m_SourceTemplate.m_sUsingNamespace,
                                            m_sUsingNamespace);
                }

                if (m_sDecleareNamespace != null)
                {
                    sTemp += string.Format( m_SourceTemplate.m_sDecleareNamespaceHeader,
                                            m_sDecleareNamespace);
                }
                
                sTemp += string.Format(CODE_REPORT_SPECIAL_TABLE_HEADER,
                                        m_SourceTemplate.GetIndent(m_uiIndentSpecialTableHeader),
                                        m_sSpecialTableType,
                                        m_sSpecialTableName,
                                        m_sSpecialTableGetFunc);
                
                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private void AddTerminator()
        {
            string sTagCode = CCodeFormater.FormatTagCode(CODE_REPORT_SPECIAL_TERMINATOR_CODE);
            string sInterface = CCodeFormater.FormatInterface("IF_ALL");

            string[] sElementsOrder = ArrangeElementsAsOrder( CODE_REPORT_SPECIAL_TERMINATOR_NAME,
                                                              sTagCode,
                                                              "0",
                                                              "0",
                                                              "0",
                                                              "0",
                                                              sInterface,
                                                              "ALL_PRODUCTS") ;

            if ((m_SpecialTableElementOrder != null) && (m_SpecialTableElementOrder.Count == KW_SPECIAL_ELEMENT_NUMBER))
            {
                string sNewItem = string.Format(   CODE_REPORT_SPECIAL_TABLE_ITEM,
                                            m_SourceTemplate.GetIndent(m_uiIndentSpecialTableItem),
                                            sElementsOrder[0],
                                            sElementsOrder[1],
                                            sElementsOrder[2],
                                            sElementsOrder[3],
                                            sElementsOrder[4],
                                            sElementsOrder[5],
                                            sElementsOrder[6],
                                            sElementsOrder[7]);

                if (m_sSpecialTableOutputType == m_SourceTemplate.m_sCPPtype)
                {
                    sNewItem += string.Format(  CODE_REPORT_FOOTER,
                                                m_SourceTemplate.GetIndent(m_uiIndentSpecialTableFooter));
                    if (m_sDecleareNamespace != null)
                    {
                        sNewItem += m_SourceTemplate.m_sDecleareNamespaceFooter;
                    }
                }

                m_FileProcessor.AddNewItem(sNewItem);
            }
        }

        private string[] ArrangeElementsAsOrder(string sTagName, string sTagCode, string sAddress, string sDefaultValue, string sDefaultValueAlt, string sSize, string sInfMask, string sProductMask)
        {
            string[] sArrayRet = null;

            if ((m_SpecialTableElementOrder != null) && (m_SpecialTableElementOrder.Count > 0))
            {
                sArrayRet = new string[m_SpecialTableElementOrder.Count];
                foreach (TABLE_STRUCT_DATA_TYPE element in m_SpecialTableElementOrder)
                {
                    switch (element.m_DatID)
                    {
                        case TABLE_STRUCT_MEMBER_ID.CI_NAME_ID:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sTagName;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.CI_CODE_ID:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sTagCode;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.CI_ADDRESS_ID:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sAddress;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.CI_DEFAULT_VALUE:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sDefaultValue;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.CI_DEFAULT_ALT_VALUE:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sDefaultValueAlt;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.CI_SIZE_ID:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sSize;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.INTERFACE_MASK_ID:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sInfMask;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.PRODUCT_MASK_ID:
                            sArrayRet[m_SpecialTableElementOrder.IndexOf(element)] = sProductMask;
                            break;

                        default:
                            break;
                    }
                }
            }

            return sArrayRet;
        }
        
        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ( (m_sOutputFolder               != null) && (m_sOutputFolder        != string.Empty) &&
                 (m_sOutputFileName             != null) && (m_sOutputFileName      != string.Empty) &&
                 (m_sSpecialTableType           != null) && (m_sSpecialTableType    != string.Empty) &&
                 (m_sSpecialTableName           != null) && (m_sSpecialTableName    != string.Empty) &&
                 (m_sSpecialTableGetFunc        != null) && (m_sSpecialTableGetFunc != string.Empty) &&
                 (m_sCmVariableName             != null) && (m_sCmVariableName      != string.Empty) &&
                 (m_SpecialTableElementOrder    != null) && (m_SpecialTableElementOrder.Count > 0) )
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bFileComplete                 = false;
        
            m_sOutputFolder                 = null;
            m_sOutputFileName               = null;
            m_sSpecialTableType             = null;
            m_sSpecialTableName             = null;
            m_sSpecialTableGetFunc          = null;
            m_sCmVariableName               = null;
            m_sUsingNamespace               = null;
            m_sDecleareNamespace            = null;
            m_SpecialTableElementOrder      = null;
            m_sSpecialTableOutputType       = null;
            
            m_uiIndentSpecialTableHeader    = 0;
            m_uiIndentSpecialTableItem      = 1;
            m_uiIndentSpecialTableFooter    = 0;
        }
    }
}