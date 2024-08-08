using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeSearchNormalItemIndex : ACodeSubHandler
    {
        private const string CODE_SEARCH_NORMAL_ITEM_INDEX_FILE_NAME = "config_item_search_normal_index.cpp";

        private const string CODE_BINARY_SEARCH_NORMAL_ITEM_INDEX_BODY = "{0}{{\n"                                        +
                                                                         "{0}    int iLeft = 0;\n"                        +
                                                                         "{0}    int iRight = {1}() -1;\n"                +
                                                                         "{0}    int iMid = 0;\n"                         +
                                                                         "{0}    while(iLeft <= iRight)\n"                +
                                                                         "{0}    {{\n"                                    +
                                                                         "{0}        iMid = (iLeft + iRight) / 2;\n"      +
                                                                         "{0}        if ({2} == {3}[iMid].{4})\n" +
                                                                         "{0}        {{\n"                                +
                                                                         "{0}            return iMid;\n"                  +
                                                                         "{0}        }}\n\n"                              +
                                                                         "{0}        if({2} < {3}[iMid].{4})\n"   +
                                                                         "{0}        {{\n"                                +
                                                                         "{0}            iRight = iMid - 1;\n"            +
                                                                         "{0}        }}\n"                                +
                                                                         "{0}        else\n"                              +
                                                                         "{0}        {{\n"                                +
                                                                         "{0}            iLeft = iMid + 1;\n"             +
                                                                         "{0}        }}\n"                                +
                                                                         "{0}    }}\n"                                    +
                                                                         "{0}    return -1;\n"                            +
                                                                         "{0}}}\n";

        private const string CODE_LINEAR_SEARCH_NORMAL_ITEM_INDEX_BODY = "{0}{{\n"                                                                          +
                                                                         "{0}    int iReIndex = -1;\n"                                                      +
                                                                         "{0}    for(int iIndex = 0; CI_TERMINATOR != {2}[iIndex].{3}; iIndex++)\n" +
                                                                         "{0}    {{\n"                                                                      +
                                                                         "{0}        if({1} == {2}[iIndex].{3})\n"                                  +
                                                                         "{0}        {{\n"                                                                  +
                                                                         "{0}            iReIndex = iIndex;\n"                                              +
                                                                         "{0}            break;\n"                                                          +
                                                                         "{0}        }}\n"                                                                  +
                                                                         "{0}    }}\n"                                                                      +
                                                                         "{0}    return iReIndex;\n"                                                        +
                                                                         "{0}}}\n";

        private ICodeFileProcessor  m_FileProcessor   = null;
        private CSourceCodeTemplate m_SourceTemplate = null;
        private bool                m_bFileComplete                 = false;

        private string              m_sOutputFolder     = null;
        private string              m_sOutputFileName   = null;
        private string              m_sUsingNamespace   = null;
        private string              m_sDeclareNamespace = null;

        private string              m_sSearchItemFunctionType = null;
        private string              m_sSearchItemFuntionName  = null;
        private string              m_sItemNumVarName = null;
        private List<string[]>      m_lsSearchNormalItemIndexFuncParams = null;
        private string              m_sNormalTableName = null;
        private uint                m_uiIndentHeader   = 0;

        private string              m_sGetNormalTableSizeFunc = null;


        public CCodeSearchNormalItemIndex()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_SEARCH_ITEM_INDEX_FILE_PROCESSOR);
            m_SourceTemplate = CSourceCodeTemplate.GetInstance();

            if ((m_FileProcessor == null) || (m_SourceTemplate == null))
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
                    m_sOutputFolder     = (oData as ICommonProperties).GetOutputFolder();
                    m_sOutputFileName   = (oData as CSourceInfoObject).GetOutputFileName();
                    m_sUsingNamespace   = (oData as CSourceInfoObject).GetUsingNamespace();
                    m_sDeclareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();

                    m_sSearchItemFuntionName  = (oData as CSourceInfoObject).GetSearchNormalItemIndexFuncName();
                    m_sSearchItemFunctionType = (oData as CSourceInfoObject).GetSearchNormalItemIndexFuncType();
                    m_lsSearchNormalItemIndexFuncParams = (oData as CSourceInfoObject).GetSearchNormalItemIndexParamList();

                    m_sGetNormalTableSizeFunc = (oData as INormalTableProperties).GetNormalTableSizeFuncName();
                    m_sNormalTableName = (oData as INormalTableProperties).GetNormalTableName();

                    List<TABLE_STRUCT_DATA_TYPE> NormalTableElementOrder = (oData as INormalTableProperties).GetNormalTableElementOrder();
                    if ((NormalTableElementOrder != null) && (NormalTableElementOrder.Count > 0))
                    {
                        foreach (TABLE_STRUCT_DATA_TYPE element in NormalTableElementOrder)
                        {
                            if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.CI_NAME_ID)
                            {
                                m_sItemNumVarName = element.m_sDataName;
                            }
                        }
                    }

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_SEARCH_NORMAL_ITEM_INDEX_FILE_NAME);
                        AddFunctionHeader();
                        bRet = true;
                    }
                }
            }

            return bRet;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            bool bRet = true;
            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            if ((m_bFileComplete == false) && (m_FileProcessor != null))
            {
                AddFunctionFooter();
                m_FileProcessor.CloseFile();
                m_bFileComplete = true;
            }

            return true;
        }

        private void AddFunctionHeader()
        {
            if (m_FileProcessor != null)
            {
                string sTemp;

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

                    m_uiIndentHeader += 1;
                }

                string sParamsContent = string.Format(m_SourceTemplate.m_sSearchNormalItemParamTemplate,
                                                                    m_lsSearchNormalItemIndexFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                                                    m_lsSearchNormalItemIndexFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);

                sTemp += string.Format(m_SourceTemplate.m_sFuncTemplate,
                                       m_SourceTemplate.GetIndent(m_uiIndentHeader),
                                       m_sSearchItemFunctionType,
                                       m_sSearchItemFuntionName,
                                       sParamsContent,
                                       null);

                sTemp += string.Format(CODE_BINARY_SEARCH_NORMAL_ITEM_INDEX_BODY,
                                        m_SourceTemplate.GetIndent(m_uiIndentHeader),
                                        m_sGetNormalTableSizeFunc,
                                        m_lsSearchNormalItemIndexFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_sNormalTableName,
                                        m_sItemNumVarName);

                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private void AddFunctionFooter()
        {
            if (m_FileProcessor != null)
            {
                if (m_sDeclareNamespace != null)
                {
                    m_FileProcessor.AddNewItem(m_SourceTemplate.m_sDecleareNamespaceFooter);
                }
            }
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sOutputFolder) == false) &&
               (string.IsNullOrEmpty(m_sOutputFileName) == false) &&
               (string.IsNullOrEmpty(m_sSearchItemFunctionType) == false) &&
               (string.IsNullOrEmpty(m_sSearchItemFuntionName) == false) &&
               (string.IsNullOrEmpty(m_sGetNormalTableSizeFunc) == false) &&
               (string.IsNullOrEmpty(m_sNormalTableName) == false) &&
               (string.IsNullOrEmpty(m_sItemNumVarName) == false) &&
               (m_lsSearchNormalItemIndexFuncParams != null))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bFileComplete = false;

            m_sOutputFolder = null;
            m_sOutputFileName = null;
            m_sSearchItemFunctionType = null;
            m_sSearchItemFuntionName = null;
            m_lsSearchNormalItemIndexFuncParams = null;
            m_sNormalTableName = null;
            m_sUsingNamespace = null;
            m_sDeclareNamespace = null;
            m_sGetNormalTableSizeFunc = null;
            m_sItemNumVarName = null;
            m_uiIndentHeader   = 0;
        }
    }
}