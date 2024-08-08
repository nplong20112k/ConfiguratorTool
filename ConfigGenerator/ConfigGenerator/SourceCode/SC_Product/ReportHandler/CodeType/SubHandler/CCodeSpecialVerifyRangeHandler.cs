using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeSpecialVerifyRangeHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_VERIFY_VALUE_SPECIAL_FILE_NAME = "config_item_verify_special_range_value.cpp";

        private const string CODE_REPORT_VERIFY_VALUE_SPECIAL_HEADER = "{0}{{\n" +
                                                                        "{0}    bool bRet = true;\n" +
                                                                        "{0}    bool bCheck = true;\n" +
                                                                        "{0}    unsigned long long Value64bits = 0;\n" +
                                                                        "{0}    unsigned long long MinValue64bits = 0;\n" +
                                                                        "{0}    unsigned long long MaxValue64bits = 0;\n\n" +
                                                                        "{0}    switch ({1}->{2})\n" +
                                                                        "{0}    {{\n";

        private const string CODE_REPORT_VERIFY_VALUE_SPECIAL_FOOTER =  "{0}        default:\n" +
                                                                        "{0}            bCheck = false;\n" +
                                                                        "{0}            break;\n" +
                                                                        "{0}    }}\n\n" +
                                                                        "{0}    if (true == bCheck)\n" +
                                                                        "{0}    {{\n" +
                                                                        "{0}        Value64bits = *((unsigned char *){3});\n" +
                                                                        "{0}        for (int i = 1; i < ({1}->{2}); i++)\n" +
                                                                        "{0}        {{\n" +
                                                                        "{0}            Value64bits = Value64bits << 8;\n" +
                                                                        "{0}            Value64bits |= *((unsigned char *)({3} + i));\n" +
                                                                        "{0}        }}\n\n" +
                                                                        "{0}        if ((Value64bits < MinValue64bits) || (Value64bits > MaxValue64bits))\n" +
                                                                        "{0}        {{\n" +
                                                                        "{0}            bRet = false;\n" +
                                                                        "{0}        }}\n" +
                                                                        "{0}    }}\n\n" +
                                                                        "{0}    return bRet;\n" +
                                                                        "{0}}}\n";

        private const string CODE_REPORT_VERIFY_VALUE_SPECIAL_CONTENT = "{0}        case {1}:\n" +
                                                                        "{0}            {{\n" +
                                                                        "{0}                MinValue64bits = 0x{2};\n" +
                                                                        "{0}                MaxValue64bits = 0x{3};\n" +
                                                                        "{0}            }}\n" +
                                                                        "{0}            break;\n\n";

        private ICodeFileProcessor m_FileProcessor = null;
        private CSourceCodeTemplate m_SourceTemplate = null;
        private bool m_bFileComplete = false;

        private string m_sOutputFolder = null;
        private string m_sOutputFileName = null;

        private string m_sItemNumVarName = null;
        private string m_sSizeVarName = null;

        private string m_sSpecialVerifyRangeFuncType = null;
        private string m_sSpecialVerifyRangeFuncName = null;
        private List<string[]> m_lsSpecialVerifyRangeFuncParams = null;
        private string m_sUsingNamespace = null;
        private string m_sDeclareNamespace = null;

        private string m_sDecleareNamespaceHeaderFormat = null;
        private string m_sDecleareNamespaceFooterFormat = null;
        private string m_sUsingNamespaceFormat = null;

        private uint m_uiIndent = 0;

        public CCodeSpecialVerifyRangeHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_VERIFY_RANGE_FILE_PROCESSOR);
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
                    m_sSpecialVerifyRangeFuncType = (oData as ISpecialVerifyRangeFuncProperties).GetSpecialVerifyRangeFuncType();
                    m_sSpecialVerifyRangeFuncName = (oData as ISpecialVerifyRangeFuncProperties).GetSpecialVerifyRangeFuncName();
                    m_lsSpecialVerifyRangeFuncParams = (oData as ISpecialVerifyRangeFuncProperties).GetSpecialVerifyRangeFuncParams();
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
                                m_sSizeVarName = element.m_sDataName;
                            }
                        }
                    }

                    if (m_FileProcessor != null)
                    {
                        if (VerifyConfigInfo() == true)
                        {
                            m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_VERIFY_VALUE_SPECIAL_FILE_NAME);

                            if (m_sDeclareNamespace != null)
                            {
                                m_uiIndent += 1;
                                m_sDecleareNamespaceHeaderFormat = string.Format(m_SourceTemplate.m_sDecleareNamespaceHeader,
                                                               m_sDeclareNamespace);
                                m_sDecleareNamespaceFooterFormat = m_SourceTemplate.m_sDecleareNamespaceFooter;
                            }

                            if (m_sUsingNamespace != null)
                            {
                                m_sUsingNamespaceFormat = string.Format(m_SourceTemplate.m_sUsingNamespace,
                                                                                    m_sUsingNamespace);
                            }

                            AddFuncionHeader();
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

                        string sMinValue = SpecialConstTable.GetMinValue();
                        string sMaxValue = SpecialConstTable.GetMaxValue();

                        if ((string.IsNullOrEmpty(sMinValue) == false) &&
                            (string.IsNullOrEmpty(sMaxValue) == false))
                        {
                            string sNewItem = null;
                            string sTagName = CommonConstTable.GetTagName();

                            sNewItem = string.Format(CODE_REPORT_VERIFY_VALUE_SPECIAL_CONTENT,
                                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                                        sTagName,
                                                        sMinValue,
                                                        sMaxValue);

                            bRet = m_FileProcessor.AddNewItem(sNewItem);
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
                AddFuncionFooter();
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
                string sParamsContent = string.Format(m_SourceTemplate.m_sVerifyRangeParamTemplate,
                                                        m_lsSpecialVerifyRangeFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                                        m_lsSpecialVerifyRangeFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        m_lsSpecialVerifyRangeFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                        m_lsSpecialVerifyRangeFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);

                sTemp = string.Format(m_SourceTemplate.m_sIncludeHeader,
                                       m_sOutputFileName);

                sTemp += m_sUsingNamespaceFormat;
                sTemp += m_sDecleareNamespaceHeaderFormat;

                sTemp += string.Format(m_SourceTemplate.m_sFuncTemplate,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_sSpecialVerifyRangeFuncType,
                                        m_sSpecialVerifyRangeFuncName,
                                        sParamsContent,
                                        null);

                sTemp += string.Format(CODE_REPORT_VERIFY_VALUE_SPECIAL_HEADER,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_lsSpecialVerifyRangeFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_sItemNumVarName);

                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private void AddFuncionFooter()
        {
            if (m_FileProcessor != null)
            {
                string sTemp;

                sTemp = string.Format(CODE_REPORT_VERIFY_VALUE_SPECIAL_FOOTER,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_lsSpecialVerifyRangeFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_sSizeVarName,
                                        m_lsSpecialVerifyRangeFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                sTemp += m_sDecleareNamespaceFooterFormat;

                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sOutputFolder) == false)&&
                (string.IsNullOrEmpty(m_sOutputFileName) == false) &&
                (string.IsNullOrEmpty(m_sItemNumVarName) == false) &&
                (string.IsNullOrEmpty(m_sSizeVarName) == false) &&
                (string.IsNullOrEmpty(m_sSpecialVerifyRangeFuncType) == false) &&
                (string.IsNullOrEmpty(m_sSpecialVerifyRangeFuncName) == false) &&
                (m_lsSpecialVerifyRangeFuncParams != null) && (m_lsSpecialVerifyRangeFuncParams.Count == 2/*NumberOfParam*/))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bFileComplete = false;
            m_sUsingNamespace = null;
            m_sDeclareNamespace = null;
            m_sDecleareNamespaceHeaderFormat = null;
            m_sDecleareNamespaceFooterFormat = null;
            m_sUsingNamespaceFormat = null;

            m_uiIndent = 0;
        }
    }
}
