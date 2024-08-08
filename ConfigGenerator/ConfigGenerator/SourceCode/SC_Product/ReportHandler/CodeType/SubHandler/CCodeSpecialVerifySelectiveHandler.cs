using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeSpecialVerifySelectiveHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_INCLUDE         =   "string.h";
                
        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_FILE_NAME       =   "config_item_verify_special_selective_value.cpp";

        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_HEADER          =   "{0}{{\n"                       +
                                                                                            "{0}    bool bRet = false;\n"   +
                                                                                            "\n"                            +
                                                                                            "{0}    switch ({1}->{2})\n"    +
                                                                                            "{0}    {{\n";

        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_CONTENT_HEADING =   "{0}        case {1}:\n";

        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_CONTENT_VALUE   =   "{0}            {{\n"                                                       +
                                                                                            "{0}                unsigned char pucRefValue[{1}][{2}/*{3}->{4}*/] = {{\n"        +
                                                                                            "{5}}};\n"                                                                  +
                                                                                            "{0}                for (int i = 0; i < {1}; i++)\n"                        +
                                                                                            "{0}                {{\n"                                                   +
                                                                                            "{0}                    if (memcmp ({6}, pucRefValue[i], {2}/*{3}->{4}*/) == 0)\n" +
                                                                                            "{0}                    {{\n"                                               +
                                                                                            "{0}                        bRet = true;\n"                                 +
                                                                                            "{0}                        break;\n"                                       +
                                                                                            "{0}                    }}\n"                                               +
                                                                                            "{0}                }}\n"                                                   +
                                                                                            "{0}            }}\n";

        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_ARRAY_VALUE           =   "{0}                                {1},\n";

        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_CONTENT_FOOTING =   "{0}            break;\n\n";

        private const string CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_FOOTER          =   "{0}        default:\n"         +
                                                                                            "{0}            bRet = true;\n" +
                                                                                            "{0}            break;\n"       +
                                                                                            "{0}    }}\n"                   +
                                                                                            "\n"                            +
                                                                                            "{0}    return bRet;\n"         +
                                                                                            "{0}}}\n";

        private ICodeFileProcessor m_FileProcessor                      = null;
        private CSourceCodeTemplate m_SourceTemplate                    = null;

        private string m_sOutputFolder                                  = null;
        private string m_sOutputFileName                                = null;
        private string m_sSpecialVerifySelectiveFuncType                = null;
        private string m_sSpecialVerifySelectiveFuncName                = null;
        private string m_sUsingNamespace                                = null;
        private string m_sDeclareNamespace                              = null;
        private string m_sDeclareNamespaceHeaderFormat                  = null;
        private string m_sDeclareNamespaceFooterFormat                  = null;
        private string m_sUsingNamespaceFormat                          = null;
        private string m_sItemNumVarName                                = null;
        private string m_sSizeIdVarName                                 = null;

        private List<string[]> m_lsSpecialVerifySelectiveFuncParams     = null;
        private bool m_bFileComplete                                    = false;
        private uint m_uiIndent                                         = 0;

        public CCodeSpecialVerifySelectiveHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_VERIFY_SELECTION_VALUE_FILE_PROCESSOR);
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
                    m_sOutputFolder = (oData as ICommonProperties).GetOutputFolder();
                    m_sOutputFileName = (oData as CSourceInfoObject).GetOutputFileName();
                    m_sSpecialVerifySelectiveFuncType = (oData as ISpecialVerifySelectiveFuncProperties).GetSpecialVerifySelectiveFuncType();
                    m_sSpecialVerifySelectiveFuncName = (oData as ISpecialVerifySelectiveFuncProperties).GetSpecialVerifySelectiveFuncName();
                    m_lsSpecialVerifySelectiveFuncParams = (oData as ISpecialVerifySelectiveFuncProperties).GetSpecialVerifySelectiveFuncParams();
                    m_sDeclareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();
                    m_sUsingNamespace = (oData as CSourceInfoObject).GetUsingNamespace();

                    List<TABLE_STRUCT_DATA_TYPE> SpecialTableElementOrder = (oData as ISpecialTableProperties).GetSpecialTableElementOrder();
                    if ((SpecialTableElementOrder != null) && (SpecialTableElementOrder.Count > 0))
                    {
                        foreach (TABLE_STRUCT_DATA_TYPE element in SpecialTableElementOrder)
                        {
                            if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.CI_NAME_ID)
                            {
                                m_sItemNumVarName = element.m_sDataName;
                            }
                            if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.CI_SIZE_ID)
                            {
                                m_sSizeIdVarName = element.m_sDataName;
                            }
                        }
                    }

                    string sOutputFilePath = null;
                    if (m_FileProcessor != null)
                    {
                        if (VerifyConfigInfo() == true)
                        {
                            if (string.IsNullOrEmpty(m_sOutputFolder) == false)
                            {
                                sOutputFilePath = m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_FILE_NAME;
                            }
                            else
                            {
                                sOutputFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_FILE_NAME;
                            }

                            if (m_FileProcessor.CreateNewFile(sOutputFilePath) == false)
                            {
                                return false;
                            }

                            if (m_sDeclareNamespace != null)
                            {
                                m_uiIndent += 1;
                                m_sDeclareNamespaceHeaderFormat = string.Format(m_SourceTemplate.m_sDecleareNamespaceHeader,
                                                                                    m_sDeclareNamespace);
                                m_sDeclareNamespaceFooterFormat = m_SourceTemplate.m_sDecleareNamespaceFooter;
                            }

                            if (m_sUsingNamespace != null)
                            {
                                m_sUsingNamespaceFormat = string.Format(m_SourceTemplate.m_sUsingNamespace,
                                                                        m_sUsingNamespace);
                            }

                            AddFunctionHeader();
                        }
                        else
                        {
                            m_FileProcessor = null;
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

            if ((m_FileProcessor != null) && (oDataIn != null))
            {
                ACommonConstTable CommonConstTable = (oDataIn as CIntegratedDataObject).GetCommonConstTable();
                if (CommonConstTable != null)
                {
                    if (CommonConstTable.GetComConstTableType() == COMMON_CONST_TABLE_TYPE.SPECIAL_TABLE)
                    {
                        CSpecialConstItem SpecialConstTable = CommonConstTable as CSpecialConstItem;

                        List<string> lsValueList = SpecialConstTable.GetValueList();
                        VALUE_LIST_TYPE eValueListType = SpecialConstTable.GetValueListType();
                        if (lsValueList != null && eValueListType == VALUE_LIST_TYPE.ACCEPTED_TYPE)
                        {
                            string sContent = null;
                            sContent = string.Format(CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_CONTENT_HEADING,
                                                            m_SourceTemplate.GetIndent(m_uiIndent),
                                                            SpecialConstTable.GetTagName());

                            string sValueAsArray = string.Empty;
                            foreach (string element in lsValueList)
                            {
                                string sTemp = ConvertValueToArray(element);
                                sValueAsArray += string.Format(CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_ARRAY_VALUE,
                                                            m_SourceTemplate.GetIndent(m_uiIndent),
                                                            sTemp);
                            }
                            sValueAsArray = sValueAsArray.TrimEnd(' ', ',', '\n');

                            sContent += string.Format(CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_CONTENT_VALUE,
                                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                                        lsValueList.Count.ToString(),
                                                        SpecialConstTable.GetTagValueSize(),
                                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        m_sSizeIdVarName,
                                                        sValueAsArray,
                                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);

                            sContent += string.Format(CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_CONTENT_FOOTING,
                                                            m_SourceTemplate.GetIndent(m_uiIndent));

                            m_FileProcessor.AddNewItem(sContent);

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
                AddFunctionFooter();
                m_FileProcessor.CloseFile();
                m_bFileComplete = true;
            }

            return true;
        }

        private void ReInitProperties()
        {
            m_bFileComplete = false;
            m_sOutputFolder = null;
            m_sOutputFileName = null;
            m_sSpecialVerifySelectiveFuncType = null;
            m_sSpecialVerifySelectiveFuncName = null;
            m_lsSpecialVerifySelectiveFuncParams = null;
            m_sUsingNamespace = null;
            m_sDeclareNamespace = null;
            m_sDeclareNamespaceHeaderFormat = null;
            m_sDeclareNamespaceFooterFormat = null;
            m_sUsingNamespaceFormat = null;

            m_uiIndent = 0;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sSpecialVerifySelectiveFuncType) == false) &&
                (string.IsNullOrEmpty(m_sSpecialVerifySelectiveFuncName) == false) &&
                (string.IsNullOrEmpty(m_sItemNumVarName) == false) &&
                (m_lsSpecialVerifySelectiveFuncParams != null) && (m_lsSpecialVerifySelectiveFuncParams.Count == 2/*NumberOfParam*/))
            {
                bRet = true;
            }

            return bRet;
        }

        private void AddFunctionHeader()
        {
            if (m_FileProcessor != null)
            {
                string sHeader = null;
                sHeader = string.Format(m_SourceTemplate.m_sIncludeHeader,
                                            CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_INCLUDE);
                sHeader += string.Format(m_SourceTemplate.m_sIncludeHeader,
                                            m_sOutputFileName);
                sHeader += m_sUsingNamespaceFormat;
                sHeader += m_sDeclareNamespaceHeaderFormat;

                string sParamsContent = string.Format(m_SourceTemplate.m_sVerifyRangeParamTemplate,
                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);

                sHeader += string.Format(m_SourceTemplate.m_sFuncTemplate,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_sSpecialVerifySelectiveFuncType,
                                        m_sSpecialVerifySelectiveFuncName,
                                        sParamsContent,
                                        null);

                sHeader += string.Format(CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_HEADER,
                                            m_SourceTemplate.GetIndent(m_uiIndent),
                                            m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                            m_sItemNumVarName);

                m_FileProcessor.AddNewItem(sHeader);
            }
        }

        private void AddFunctionFooter()
        {
            if (m_FileProcessor != null)
            {
                string sTemp;

                sTemp = string.Format(CODE_REPORT_SPECIAL_VERIFY_SELECTIVE_VALUE_FOOTER,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_lsSpecialVerifySelectiveFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                sTemp += m_sDeclareNamespaceFooterFormat;

                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private string ConvertValueToArray(string sValue)
        {
            string sResult = "{";
            for (int i = 0; i < sValue.Length; i += 2)
            {
                sResult += "0x" + sValue.Substring(i, 2) + ",";
            }
            sResult = sResult.Trim(',') + "}";
            return sResult;
        }
    }
}
