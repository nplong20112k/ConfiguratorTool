using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeNormalConstTableHandler : ACodeSubHandler
    {
        private struct TABLE_ELEMENT_INFO
        {
            public string m_sWriteToFile;
            public int m_iEnumValue;
        }

        // This var should be considered to remove !! : its fixed number of elements within a item.
        private const int    KW_NORMAL_ELEMENT_NUMBER               = 7;

        private const string CODE_REPORT_NORMAL_TABLE_FILE_NAME     = "config_item_normal";

        private const string CODE_REPORT_NORMAL_TABLE_HEADER        = "{0}struct {1}*  {3}()\n"                +
                                                                      "{0}{{\n"                                +
                                                                      "{0}    return (struct {1} *)&{2}[0];\n"     +
                                                                      "{0}}}\n"                                +
                                                                      "\n"                                     +
                                                                      "{0}const struct {1} {2}[] =\n"          +
                                                                      "{0}{{\n";

        private const string CODE_REPORT_NORMAL_TABLE_SIZE          = "{0}{1} {2}()\n"                                +
                                                                      "{0}{{\n"                                       +
                                                                      "{0}    return sizeof({3})/sizeof({4});\n"      +
                                                                      "{0}}}\n";

        private const string CODE_REPORT_NORMAL_TABLE_ITEM          = "{0}{{ {1,-54},{2} ,0x{3,-5},0x{4,-5},(INTERFACE_MASK_TYPE){5}\n" +
                                                                      "{0}    ,{6,-15},0x{7,-4} }},\n\n";
                                                                        // "{0}{{ {1,-54},{2} ,0x{3,-5},0x{4,-5},(INTERFACE_MASK_TYPE){5,-54},{6,-15},0x{7,-4} }},\n";

        private const string CODE_REPORT_NORMAL_TABLE_ITEM_OPTIMIZE = "{0}{{ {1,-54},{2} ,0x{3,-5},0x{4,-5},(INTERFACE_MASK_TYPE){5}\n" +
                                                                      "{0}    ,{6,-15},0x{7,-5},{8,-5},{9,-5},{10,-5} }},\n\n";
                                                                        // "{0}{{ {1,-54},{2} ,0x{3,-5},0x{4,-5},(INTERFACE_MASK_TYPE){5,-54},{6,-15},0x{7,-5},{8,-5},{9,-5},{10,-5} }},\n";

        private const string CODE_REPORT_NORMAL_TERMINATOR_NAME     = "CI_TERMINATOR";

        private const string CODE_REPORT_NORMAL_TERMINATOR_CODE     = "FFFF";

        private const string CODE_REPORT_TABLE_FOOTER               =   "{0}}};\n" +
                                                                        "\n";

        private ICodeFileProcessor              m_FileProcessor             = null;
        private CSourceCodeTemplate             m_SourceTemplate            = null;
        private bool                            m_bFileComplete             = false;

        private string                          m_sOutputFolder             = null;
        private string                          m_sOutputFileName           = null;
        private string                          m_sNormalTableType          = null;
        private string                          m_sNormalTableName          = null;
        private string                          m_sNormalTableGetFunc       = null;
        private string                          m_sNormalTableOutputType    = null;
        private string                          m_sUsingNamespace           = null;
        private string                          m_sDeclareNamespace         = null;
        private string                          m_sNormalTableGetSizeFuncName = null;
        private string                          m_sNormalTableGetSizeFuncType = null;
        private List<CI_ENUM_INFO>              m_lsNormalEnumInfo          = null;
        private List<TABLE_ELEMENT_INFO>        m_lsTableInfo               = null;
        private List<TABLE_STRUCT_DATA_TYPE>    m_NormalTableElementOrder   = null;

        private uint                            m_uiIndentNormalTableHeader   = 0;
        private uint                            m_uiIndentNormalTableItem     = 1;
        private uint                            m_uiIndentNormalTableFooter   = 0;

        public CCodeNormalConstTableHandler ()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_TABLE_FILE_PROCESSOR);
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
                    m_sDeclareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();
                    m_sUsingNamespace = (oData as CSourceInfoObject).GetUsingNamespace();
                    m_sNormalTableType = (oData as INormalTableProperties).GetNormalTableType();
                    m_sNormalTableName = (oData as INormalTableProperties).GetNormalTableName();
                    m_sNormalTableGetFunc = (oData as INormalTableProperties).GetNormalTableGetFunc();
                    m_NormalTableElementOrder = (oData as INormalTableProperties).GetNormalTableElementOrder();
                    m_sNormalTableGetSizeFuncName = (oData as INormalTableProperties).GetNormalTableSizeFuncName();
                    m_sNormalTableGetSizeFuncType = (oData as INormalTableProperties).GetNormalTableSizeFuncType();

                    m_lsNormalEnumInfo = (oData as CSourceInfoObject).GetCIEnumInfo()[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM];
                    m_lsTableInfo = new List<TABLE_ELEMENT_INFO>();

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_sNormalTableOutputType = (oData as CSourceInfoObject).GetNormalTableOutputType();
                        sOutputFileName = Path.DirectorySeparatorChar + CODE_REPORT_NORMAL_TABLE_FILE_NAME + m_sNormalTableOutputType;

                        m_FileProcessor.CreateNewFile(m_sOutputFolder + sOutputFileName);
                        if (m_sNormalTableOutputType == m_SourceTemplate.m_sCPPtype)
                        {
                            if (m_sDeclareNamespace != null)
                            {
                                m_uiIndentNormalTableHeader += 1;
                                m_uiIndentNormalTableItem += 1;
                                m_uiIndentNormalTableFooter += 1;
                            }

                            AddFunctionHeader();
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
                        if (CommonConstTable.GetComConstTableType() == COMMON_CONST_TABLE_TYPE.NORMAL_TABLE)
                        {
                            CNormalConstItem NormalConstItem = CommonConstTable as CNormalConstItem;
                            string sTagCode = CCodeFormater.FormatTagCode(NormalConstItem.GetTagCode());
                            string sInterface = CCodeFormater.FormatInterface(NormalConstItem.GetInterfaceMask());
                            string sNewItem = null;

                            string[] sElementsOrder = ArrangeElementsAsOrder( NormalConstItem.GetTagName(),
                                                                              sTagCode,
                                                                              NormalConstItem.GetMinValue(),
                                                                              NormalConstItem.GetMaxValue(),
                                                                              sInterface,
                                                                              NormalConstItem.GetProductMask(),
                                                                              NormalConstItem.GetDefaultValue());

                            if ((sElementsOrder != null) && (sElementsOrder.Length == KW_NORMAL_ELEMENT_NUMBER))
                            {
                                CStructure StructureObj = (oDataIn as CIntegratedDataObject).GetStructure();
                                if (StructureObj != null)
                                { 
                                    uint    uiSizeInBit = uint.Parse(StructureObj.GetTagSizeInBit());                                     
                                    sNewItem = string.Format(CODE_REPORT_NORMAL_TABLE_ITEM_OPTIMIZE,
                                                                    m_SourceTemplate.GetIndent(m_uiIndentNormalTableItem),
                                                                    sElementsOrder[0],
                                                                    sElementsOrder[1],
                                                                    sElementsOrder[2],
                                                                    sElementsOrder[3],
                                                                    sElementsOrder[4],
                                                                    sElementsOrder[5],
                                                                    sElementsOrder[6],
                                                                    StructureObj.GetIndex(uiSizeInBit),
                                                                    StructureObj.GetLocation(uiSizeInBit),
                                                                    StructureObj.GetBitLength(uiSizeInBit));
                                }
                                bRet = AddNewItemToTableInfo(sNewItem, NormalConstItem.GetTagName());
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            // Sort the list table info base on enum value (order: ascending)
            m_lsTableInfo.Sort((x, y) => x.m_iEnumValue.CompareTo(y.m_iEnumValue));

            // Wire the table to output file
            foreach (TABLE_ELEMENT_INFO element in m_lsTableInfo)
            {
                if (!m_FileProcessor.AddNewItem(element.m_sWriteToFile))
                {
                    return false;
                }
            }

            if ((m_bFileComplete == false) && (m_FileProcessor != null))
            {
                AddTerminator();
                m_FileProcessor.CloseFile();
                m_bFileComplete = true;
            }

            return true;
        }

        private void AddFunctionHeader()
        {
            string sTemp;

            if (m_FileProcessor != null)
            {             
                sTemp = string.Format(m_SourceTemplate.m_sIncludeHeader,
                                        m_sOutputFileName);

                if (m_sUsingNamespace != null)
                {
                    sTemp += string.Format(m_SourceTemplate.m_sUsingNamespace,
                                            m_sUsingNamespace);
                }
                    
                if (m_sDeclareNamespace != null)
                {
                    sTemp += string.Format(m_SourceTemplate.m_sDecleareNamespaceHeader,
                                            m_sDeclareNamespace);
                }

                sTemp += string.Format(CODE_REPORT_NORMAL_TABLE_HEADER,
                                        m_SourceTemplate.GetIndent(m_uiIndentNormalTableHeader),
                                        m_sNormalTableType,
                                        m_sNormalTableName,
                                        m_sNormalTableGetFunc);
                                            
                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private void AddTerminator()
        {
            string sTagCode = CCodeFormater.FormatTagCode(CODE_REPORT_NORMAL_TERMINATOR_CODE);

            string sInterface = CCodeFormater.FormatInterface("IF_ALL");

            string[] sElementsOrder = ArrangeElementsAsOrder( CODE_REPORT_NORMAL_TERMINATOR_NAME,
                                            sTagCode,
                                            "0",
                                            "0",
                                            sInterface,
                                            "ALL_PRODUCTS",
                                            "0");
            string sNewItem = null;

            if ((sElementsOrder != null) && (sElementsOrder.Length == KW_NORMAL_ELEMENT_NUMBER))
            {                                 
                sNewItem = string.Format(CODE_REPORT_NORMAL_TABLE_ITEM_OPTIMIZE,
                                        m_SourceTemplate.GetIndent(m_uiIndentNormalTableItem),
                                        sElementsOrder[0],
                                        sElementsOrder[1],
                                        sElementsOrder[2],
                                        sElementsOrder[3],
                                        sElementsOrder[4],
                                        sElementsOrder[5],
                                        sElementsOrder[6],
                                        "0",
                                        "0",
                                        "0");                 

                sNewItem += string.Format(CODE_REPORT_TABLE_FOOTER,
                                          m_SourceTemplate.GetIndent(m_uiIndentNormalTableFooter));

                sNewItem += string.Format(CODE_REPORT_NORMAL_TABLE_SIZE,
                                          m_SourceTemplate.GetIndent(m_uiIndentNormalTableFooter),
                                          m_sNormalTableGetSizeFuncType,
                                          m_sNormalTableGetSizeFuncName,
                                          m_sNormalTableName,
                                          m_sNormalTableType);


                if (m_sNormalTableOutputType == m_SourceTemplate.m_sCPPtype)
                {
                    if (m_sDeclareNamespace != null)
                    {
                        sNewItem += m_SourceTemplate.m_sDecleareNamespaceFooter;
                    }
                }

                m_FileProcessor.AddNewItem(sNewItem);              
            }
        }

        private string[] ArrangeElementsAsOrder(string sTagName, string sTagCode, string sMinValue, string sMaxValue, string sInfMask, string sProductMask, string sDefaultValue)
        {
            string[] sArrayRet = null;

            if ((m_NormalTableElementOrder != null) && (m_NormalTableElementOrder.Count > 0))
            {
                sArrayRet = new string[m_NormalTableElementOrder.Count];
                foreach (TABLE_STRUCT_DATA_TYPE element in m_NormalTableElementOrder)
                {
                    switch (element.m_DatID)
                    {
                        case TABLE_STRUCT_MEMBER_ID.CI_NAME_ID:
                            sArrayRet[m_NormalTableElementOrder.IndexOf(element)] = sTagName;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.CI_CODE_ID:
                            sArrayRet[m_NormalTableElementOrder.IndexOf(element)] = sTagCode;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.MIN_VALUE_ID:
                            sArrayRet[m_NormalTableElementOrder.IndexOf(element)] = sMinValue.PadLeft(2, '0');
                            break;

                        case TABLE_STRUCT_MEMBER_ID.MAX_VALUE_ID:
                            sArrayRet[m_NormalTableElementOrder.IndexOf(element)] = sMaxValue.PadLeft(2, '0');
                            break;

                        case TABLE_STRUCT_MEMBER_ID.INTERFACE_MASK_ID:
                            sArrayRet[m_NormalTableElementOrder.IndexOf(element)] = sInfMask;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.PRODUCT_MASK_ID:
                            sArrayRet[m_NormalTableElementOrder.IndexOf(element)] = sProductMask;
                            break;

                        case TABLE_STRUCT_MEMBER_ID.DEFAULT_VALUE:
                            sArrayRet[m_NormalTableElementOrder.IndexOf(element)] = sDefaultValue.PadLeft(2, '0');
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

            if ( (m_sOutputFolder           != null) && (m_sOutputFolder        != string.Empty) &&
                 (m_sOutputFileName         != null) && (m_sOutputFileName      != string.Empty) &&              
                 (m_sNormalTableType        != null) && (m_sNormalTableType     != string.Empty) &&
                 (m_sNormalTableName        != null) && (m_sNormalTableName     != string.Empty) &&
                 (m_sNormalTableGetFunc     != null) && (m_sNormalTableGetFunc  != string.Empty) &&
                 (m_sNormalTableGetSizeFuncName != null) && (m_sNormalTableGetSizeFuncName != string.Empty) &&
                 (m_sNormalTableGetSizeFuncType != null) && (m_sNormalTableGetSizeFuncType != string.Empty) &&
                 (m_NormalTableElementOrder != null) && (m_NormalTableElementOrder.Count > 0))
            {
                bRet = true;
            }

            if((m_lsNormalEnumInfo == null || m_lsNormalEnumInfo.Count == 0) || 
                m_lsTableInfo      == null )
            {
                bRet = false;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bFileComplete             = false;
            
            m_sOutputFolder             = null;
            m_sOutputFileName           = null;
            m_sNormalTableType          = null;
            m_sNormalTableName          = null;
            m_sNormalTableGetFunc       = null;
            m_sNormalTableOutputType    = null;
            m_sUsingNamespace           = null;
            m_sDeclareNamespace         = null;
            m_NormalTableElementOrder   = null;
            m_lsTableInfo               = null;
            m_lsNormalEnumInfo          = null;
            m_sNormalTableGetSizeFuncName = null;
            m_sNormalTableGetSizeFuncType = null;

            m_uiIndentNormalTableHeader = 0;
            m_uiIndentNormalTableItem   = 1;
            m_uiIndentNormalTableFooter = 0;
        }

        private bool AddNewItemToTableInfo(string sItem, string sTagName)
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

                TABLE_ELEMENT_INFO stNewItem;
                stNewItem.m_sWriteToFile = sItem;

                string sEnumValue;
                CHexIntConverter.DetectAndTrimHexPrefix(m_lsNormalEnumInfo[iIndex].m_sCIEnumValue, out sEnumValue);
                sEnumValue = CHexIntConverter.StringConvertHexToInt(sEnumValue);
                stNewItem.m_iEnumValue = int.Parse(sEnumValue);

                m_lsTableInfo.Add(stNewItem);
                bRet = true;
            }

            return bRet;
        }
    }
}