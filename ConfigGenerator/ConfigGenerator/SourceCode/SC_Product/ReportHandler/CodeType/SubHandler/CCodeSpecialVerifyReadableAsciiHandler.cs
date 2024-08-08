using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeSpecialVerifyReadableAsciiHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_VERIFY_READABLE_ASCII_VALUE_FILE_NAME = "config_item_verify_special_readable_ascii_value.cpp";

        private const string CODE_REPORT_VERIFY_READABLE_ACSCII_VALUE_HEADER = "{0}{{\n"                                    +
                                                                               "{0}    bool bRet = true;\n"                 +
                                                                               "\n"                                         +
                                                                               "{0}    switch ({1}->{2})\n"                 +
                                                                               "{0}    {{\n";

        private const string CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_VALUE_CONTENT_HEADING = "{0}        case {1}:\n";

        private const string CODE_REPORT_VERIFY_READABLE_ASCII_NO_VALUE_CONTENT = "{0}            {{\n"                                                         +
                                                                                  "{0}                unsigned char ucMinValue = 0x20;\n"                       +
                                                                                  "{0}                unsigned char ucMaxValue = 0x7E;\n"                       +
                                                                                  "{0}                for (int i = 0; i < {1}->{2}; i++)\n"                     +
                                                                                  "{0}                {{\n"                                                     +
                                                                                  "{0}                    if ({3}[i] == 0x00 || {3}[i] == 0xFF)\n"              +
                                                                                  "{0}                    {{\n"                                                 +
                                                                                  "{0}                        break;\n"                                         +
                                                                                  "{0}                    }}\n"                                                 +
                                                                                  "{0}                    if (({3}[i] < ucMinValue) || ({3}[i] > ucMaxValue))\n"+
                                                                                  "{0}                    {{\n"                                                 +
                                                                                  "{0}                        bRet = false;\n"                                  +
                                                                                  "{0}                        break;\n"                                         +
                                                                                  "{0}                    }}\n"                                                 +
                                                                                  "{0}                }}\n"                                                     +
                                                                                  "{0}            }}\n"                                                         +
                                                                                  "{0}            break;\n";

        private const string CODE_REPORT_VERIFY_READABLE_ASCII_VALUE_CONTENT = "{0}            {{\n"                                                                                                   +
                                                                               "{0}                unsigned char pucExceptValue[{5}] = {{\n"                                                           +
                                                                               "{0}                                          {4}}};\n"                                                                 +
                                                                               "{0}                for (int i = 0; i < {1}->{2}; i++)\n"                                                               +
                                                                               "{0}                {{\n"                                                                                               +
                                                                               "{0}                    for (int j = 0; j < sizeof(pucExceptValue); j++)\n"                                             +
                                                                               "{0}                    {{\n"                                                                                           +
                                                                               "{0}                        if (({3}[i] == pucExceptValue[j]) || ({3}[i] < MinValue) || ({3}[i] > MaxValue))\n"         +
                                                                               "{0}                        {{\n"                                                                                       +
                                                                               "{0}                            bRet = false;\n"                                                                        +
                                                                               "{0}                            break;\n"                                                                               +
                                                                               "{0}                        }}\n"                                                                                       +
                                                                               "{0}                    }}\n"                                                                                           +
                                                                               "{0}                    if (false == bRet)\n"                                                                           +
                                                                               "{0}                    {{\n"                                                                                           +
                                                                               "{0}                        break;\n"                                                                                   +
                                                                               "{0}                    }}\n"                                                                                           +
                                                                               "{0}                }}\n"                                                                                               +
                                                                               "{0}            }}\n"                                                                                                   +
                                                                               "{0}            break;\n";

        private const string CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_ARRAY_VALUE_ = "{0}                                          {1},\n";
                                                                                                                                            
        private const string CODE_REPORT_VERIFY_READABLE_ASCII_VALUE_FOOTER = "{0}\n"                                       +
                                                                              "{0}        default:\n"                       +
                                                                              "{0}           break;\n"                      +
                                                                              "{0}    }}\n"                                 +
                                                                              "\n"                                          +
                                                                              "{0}    return bRet;\n"                       +
                                                                              "{0}}}\n";


        private ICodeFileProcessor m_FileProcessor = null;
        private CSourceCodeTemplate m_SourceTemplate = null;

        private string m_sOutputFolder = null;
        private string m_sOutputFileName = null;
        private string m_sSpecialVerifyReadableAsciiFuncType = null;
        private string m_sSpecialVerifyReadableAsciiFuncName = null;
        private string m_sUsingNamespace = null;
        private string m_sDeclareNamespace = null;
        private string m_sDeclareNamespaceHeaderFormat = null;
        private string m_sDeclareNamespaceFooterFormat = null;
        private string m_sUsingNamespaceFormat = null;
        private string m_sItemNumVarName = null;
        private string m_sSizeIdVarName = null;

        private List<string[]> m_lsSpecialVerifyReadableAsciiFuncParams = null;
        private bool m_bFileComplete = false;
        private uint m_uiIndent = 0;
        private string m_sContentTextNonExceptedType = null;

        public CCodeSpecialVerifyReadableAsciiHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_VERIFY_READABLE_ASCII_FILE_PROCESSOR);
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
                    m_sSpecialVerifyReadableAsciiFuncType = (oData as ISpecialVerifyReadableAsciiFuncProperties).GetSpecialVerifyReadableAsciiFuncType();
                    m_sSpecialVerifyReadableAsciiFuncName = (oData as ISpecialVerifyReadableAsciiFuncProperties).GetSpecialVerifyReadableAsciiFuncName();
                    m_lsSpecialVerifyReadableAsciiFuncParams = (oData as ISpecialVerifyReadableAsciiFuncProperties).GetSpecialVerifyReadableAsciiFuncParams();
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
                            else if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.CI_SIZE_ID)
                            {
                                m_sSizeIdVarName = element.m_sDataName;
                            }
                        }
                    }

                    if (m_FileProcessor != null)
                    {
                        if (VerifyConfigInfo() == true)
                        {
                            m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_VERIFY_READABLE_ASCII_VALUE_FILE_NAME);

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

        public override bool DataHandling(IShareObject oDatain)
        {
            bool bRet = false;

            if ((m_FileProcessor != null) && (oDatain != null))
            {
                ACommonConstTable CommonConstTable = (oDatain as CIntegratedDataObject).GetCommonConstTable();
                if (CommonConstTable != null)
                {
                    if (CommonConstTable.GetComConstTableType() == COMMON_CONST_TABLE_TYPE.SPECIAL_TABLE)
                    {
                        CSpecialConstItem SpecialConstTable = CommonConstTable as CSpecialConstItem;
                        List<string> lsValueList = SpecialConstTable.GetValueList();
                        VALUE_LIST_TYPE eValueListType = SpecialConstTable.GetValueListType();
                        string sContent = null;

                        if (lsValueList != null && eValueListType == VALUE_LIST_TYPE.TEXT_EXCEPTED_TYPE)
                        {
                            sContent = string.Format(CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_VALUE_CONTENT_HEADING,
                                m_SourceTemplate.GetIndent(m_uiIndent),
                                SpecialConstTable.GetTagName());

                            string sValueAsArray = null;
                            foreach (string element in lsValueList)
                            {
                                string sTemp = ConvertValue(element);
                                sValueAsArray += string.Format(CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_ARRAY_VALUE_,
                                    m_SourceTemplate.GetIndent(m_uiIndent),
                                    sTemp);
                            }
                            sValueAsArray = sValueAsArray.Trim(' ', ',', '\n');

                            sContent += string.Format(CODE_REPORT_VERIFY_READABLE_ASCII_VALUE_CONTENT,
                                m_SourceTemplate.GetIndent(m_uiIndent),
                                m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                m_sSizeIdVarName,
                                m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                sValueAsArray,
                                lsValueList.Count.ToString());

                            m_FileProcessor.AddNewItem(sContent);
                        }
                        else if (lsValueList == null && eValueListType == VALUE_LIST_TYPE.TEXT_NON_EXCEPTED_TYPE)
                        {
                            m_sContentTextNonExceptedType += string.Format(CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_VALUE_CONTENT_HEADING,
                                m_SourceTemplate.GetIndent(m_uiIndent),
                                SpecialConstTable.GetTagName());
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
            m_sSpecialVerifyReadableAsciiFuncType = null;
            m_sSpecialVerifyReadableAsciiFuncName = null;
            m_sUsingNamespace = null;
            m_sDeclareNamespace = null;
            m_sDeclareNamespaceFooterFormat = null;
            m_sDeclareNamespaceFooterFormat = null;
            m_sUsingNamespaceFormat = null;

            m_uiIndent = 0;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sSpecialVerifyReadableAsciiFuncType) == false) &&
                (string.IsNullOrEmpty(m_sSpecialVerifyReadableAsciiFuncName) == false) &&
                (string.IsNullOrEmpty(m_sItemNumVarName) == false) &&
                (m_lsSpecialVerifyReadableAsciiFuncParams != null) && (m_lsSpecialVerifyReadableAsciiFuncParams.Count == 2/*NumberOfParam*/))
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
                sHeader += string.Format(m_SourceTemplate.m_sIncludeHeader,
                                            m_sOutputFileName);
                sHeader += m_sUsingNamespaceFormat;
                sHeader += m_sDeclareNamespaceHeaderFormat;

                string sParamsContent = string.Format(m_SourceTemplate.m_sVerifyReadableAsciiParamTemplate,
                                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);

                sHeader += string.Format(m_SourceTemplate.m_sFuncTemplate,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_sSpecialVerifyReadableAsciiFuncType,
                                        m_sSpecialVerifyReadableAsciiFuncName,
                                        sParamsContent,
                                        null);

                sHeader += string.Format(CODE_REPORT_VERIFY_READABLE_ACSCII_VALUE_HEADER,
                                            m_SourceTemplate.GetIndent(m_uiIndent),
                                            m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                            m_sItemNumVarName);

                m_FileProcessor.AddNewItem(sHeader);
            }
        }

        private void AddFunctionFooter()
        {
            if (m_FileProcessor != null)
            {
                string sTemp;

                if (!string.IsNullOrEmpty(m_sContentTextNonExceptedType))
                {
                    m_sContentTextNonExceptedType += string.Format(CODE_REPORT_VERIFY_READABLE_ASCII_NO_VALUE_CONTENT,
                        m_SourceTemplate.GetIndent(m_uiIndent),
                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                        m_sSizeIdVarName,
                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);

                    m_FileProcessor.AddNewItem(m_sContentTextNonExceptedType);
                }

                sTemp = string.Format(CODE_REPORT_VERIFY_READABLE_ASCII_VALUE_FOOTER,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_lsSpecialVerifyReadableAsciiFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                sTemp += m_sDeclareNamespaceFooterFormat;

                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private string ConvertValue(string sValue)
        {
            string sResult = "";
            sResult += "0x" + sValue + ",";
            sResult = sResult.Trim(',');
            return sResult;
        }
    }
}
