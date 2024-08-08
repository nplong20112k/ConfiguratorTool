using System.Collections.Generic;
using System.IO;
namespace ConfigGenerator
{
    class CCodeConvertHexIntHandler : ACodeSubHandler
    {
        public enum CONVERT_TYPE
        {
            CONVERT_HEX_TO_INT = 0,
            CONVERT_INT_TO_HEX,
            CONVERT_TOTAL,
        }
        private const string CODE_REPORT_CONVERT_HEX_INT_FILE_NAME = "config_item_function_convert_hex_int.cpp";
        private const string CODE_REPORT_CONVERT_HEX_INT_HEADER =           "{0}{{\n"                                                                                   +
                                                                            "{0}    int iNumChar = 0;\n"                                                                +
                                                                            "{0}    switch ((CI_NUMBER_TYPE){1})\n"                                                     +
                                                                            "{0}    {{\n";
        private const string CODE_REPORT_CONVERT_INT_HEX_HEADER =           "{0}{{\n"                                                                                   +
                                                                            "{0}    int iResult = 0;\n"                                                                 +
                                                                            "{0}    bool bCheck = true;\n"                                                              +
                                                                            "{0}    unsigned long long ullMinValue = 0;\n"                                              +
                                                                            "{0}    unsigned long long ullMaxValue = 0;\n"                                              +
                                                                            "{0}    switch ((CI_NUMBER_TYPE){1})\n"                                                     +
                                                                            "{0}    {{\n";
        private const string CODE_REPORT_CONVERT_HEX_INT_CONTENT_HEADING =  "{0}        case {1}:\n";
        private const string CODE_REPORT_CONVERT_HEX_INT_VALUE_CONTENT =    "{0}            {{\n"                                                                       +
                                                                            "{0}                unsigned long long ullNumber = 0;\n"                                    +
                                                                            "{0}                int uiCount[] = {{5,8,10,13,15,17,20}};\n"                              +
                                                                            "{0}                for (unsigned int i = 0; i < {1}; i++)\n"                               +
                                                                            "{0}                {{\n"                                                                   +
                                                                            "{0}                    ullNumber = (ullNumber << 8) + {2}[i];\n"                           +
                                                                            "{0}                }}\n"                                                                   +
                                                                            "{0}                memset({3},'0',(uiCount[{1} - 2] - 1));\n"                              +
                                                                            "{0}                for (int j = (uiCount[{1} - 2] - 1) ; j >= 0; j--)\n"                   +
                                                                            "{0}                {{\n"                                                                   +
                                                                            "{0}                    {3}[j] = (ullNumber % 10) + '0';\n"                                 +
                                                                            "{0}                    ullNumber = ullNumber / 10;\n"                                      +
                                                                            "{0}                    iNumChar ++;\n"                                                     +
                                                                            "{0}                }}\n"                                                                   +
                                                                            "{0}                {3}[uiCount[{1} - 2]] = '\\0';\n"                                       +
                                                                            "{0}            }}\n"                                                                       +
                                                                            "{0}            break;\n";
        private const string CODE_REPORT_CONVERT_INT_HEX_MAX_MIN_CONTENT =  "{0}        {{\n"                                                                           +
                                                                            "{0}            ullMinValue = 0x{1};\n"                                                     +
                                                                            "{0}            ullMaxValue = 0x{2};\n"                                                     +
                                                                            "{0}            bCheck = true;\n"                                                           +
                                                                            "{0}        }}\n"                                                                           +
                                                                            "{0}        break;\n\n";
        private const string CODE_REPORT_CONVERT_INT_HEX_FOOTER =           "{0}        default:\n"                                                                     +
                                                                            "{0}            bCheck = false;\n"                                                          +
                                                                            "{0}            break;\n"                                                                   +
                                                                            "{0}\n"                                                                                     +
                                                                            "{0}    }}\n\n"                                                                             +
                                                                            "{0}    if ((true == bCheck))\n"                                                            +
                                                                            "{0}    {{\n"                                                                               +
                                                                            "{0}        unsigned long long ullNumber = 0;\n"                                            +
                                                                            "{0}        int count = 0;\n"                                                               +
                                                                            "{0}        while ((*({2} + count) != ',') && (*({2} + count) != 0x0D))\n"                  +
                                                                            "{0}        {{\n"                                                                           +
                                                                            "{0}            count++;\n"                                                                 +
                                                                            "{0}        }}\n"                                                                           +
                                                                            "{0}        int uiCount[] = {{5,8,10,13,15,17,20}};\n"                                      +
                                                                            "{0}        if (count != uiCount[{1} - 2])\n"                                               +
                                                                            "{0}        {{\n"                                                                           +
                                                                            "{0}            iResult = -1;\n"                                                            +
                                                                            "{0}            return iResult;\n"                                                          +
                                                                            "{0}        }}\n"                                                                           +
                                                                            "{0}        for (int i = 0; i < count; i++)\n"                                              +
                                                                            "{0}        {{\n"                                                                           +
                                                                            "{0}            unsigned int uiTemp = (unsigned int) Ascii2Hex({2}[i]);\n"                  +
                                                                            "{0}            if (uiTemp == 0xFF)\n"                                                      +
                                                                            "{0}            {{\n"                                                                       +
                                                                            "{0}                iResult = -1;\n"                                                        +
                                                                            "{0}                return iResult;\n"                                                      +
                                                                            "{0}            }}\n"                                                                       +
                                                                            "{0}            ullNumber = ullNumber * 10;\n"                                              +
                                                                            "{0}            ullNumber += (unsigned int) uiTemp;\n"                                      +
                                                                            "{0}        }}\n"                                                                           +
                                                                            "{0}        if ((ullNumber >= ullMinValue) && (ullNumber <= ullMaxValue))\n"                +
                                                                            "{0}        {{\n"                                                                           +
                                                                            "{0}            memcpy({3}, (unsigned char *) &ullNumber, {1});\n"                          +
                                                                            "{0}            for (unsigned int i = 0; i < {1} /2; i++)\n"                                +
                                                                            "{0}            {{\n"                                                                       +
                                                                            "{0}                unsigned char ucTemp = {3}[i];\n"                                       +
                                                                            "{0}                {3}[i] = {3}[({1} - 1) - i];\n"                                         +
                                                                            "{0}                {3}[({1} - 1) - i] = ucTemp;\n"                                         +
                                                                            "{0}            }}\n"                                                                       +
                                                                            "{0}            iResult = 1;\n"                                                             +
                                                                            "{0}        }}\n"                                                                           +
                                                                            "{0}        else\n"                                                                         +
                                                                            "{0}        {{\n"                                                                           +
                                                                            "{0}            iResult = -1;\n"                                                            +
                                                                            "{0}        }}\n"                                                                           +
                                                                            "{0}    }}\n"                                                                               +
                                                                            "{0}    return iResult;\n"                                                                  +
                                                                            "{0}}}\n\n";

        private const string CODE_REPORT_CONVERT_HEX_INT_FOOTER =           "{0}        default:\n"                                                                     +
                                                                            "{0}            break;\n"                                                                   +
                                                                            "{0}\n"                                                                                     +
                                                                            "{0}    }}\n"                                                                               +
                                                                            "{0}    return iNumChar;\n"                                                                 +
                                                                            "{0}}}\n\n";

        private const string CODE_REPORT_CONVERT_FUNCTION_ASCII2HEX = "{0}unsigned char Ascii2Hex(const unsigned char x)\n" +
                                                                        "{0}{{\n"                                           +
                                                                        "{0}    if (x >= '0' && x <= '9')\n"                +
                                                                        "{0}    {{\n"                                       +
                                                                        "{0}        return (x & 0x0F);\n"                   +
                                                                        "{0}    }}\n"                                       +
                                                                        "{0}    else if (x >= 'A' && x <= 'F')\n"           +
                                                                        "{0}    {{\n"                                       +
                                                                        "{0}        return ((x - 7) & 0x0F);\n"             +
                                                                        "{0}    }}\n"                                       +
                                                                        "{0}    else if (x >= 'a' && x <= 'f')\n"           +
                                                                        "{0}    {{\n"                                       +
                                                                        "{0}        return ((x - 39) & 0x0F);\n"            +
                                                                        "{0}    }}\n"                                       +
                                                                        "{0}    else\n"                                     +
                                                                        "{0}    {{\n"                                       +
                                                                        "{0}        return 0xFF;\n"                         +
                                                                        "{0}    }}\n"                                       +
                                                                        "{0}}}\n\n";

        private ICodeFileProcessor m_FileProcessor = null;
        private CSourceCodeTemplate m_SourceTemplate = null;
        private bool m_bFileComplete = false;
        private string m_sOutputFolder = null;
        private string m_sOutputFileName = null;
        private string m_sHexIntContentHeading = null;
        private string m_sIntHexContentHeading = null;
        private string m_sDeclareNamespace = null;
        private string m_sUsingNamespace = null;
        private string m_sItemNumVarName = null;
        private string m_sSizeVarName = null;
        private string m_sDecleareNamespaceHeaderFormat = null;
        private string m_sDecleareNamespaceFooterFormat = null;
        private string m_sUsingNamespaceFormat = null;
        private uint m_uiIndent = 0;


        public CCodeConvertHexIntHandler()
             : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_CONVERT_HEX_INT_FILE_PROCESSOR);
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
                            m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_CONVERT_HEX_INT_FILE_NAME);

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

                        bool bIsDecimalValue = SpecialConstTable.CheckSpecialValueIsDecimal();
                        if (bIsDecimalValue)
                        {
                            string sNewItem;
                            sNewItem = string.Format(CODE_REPORT_CONVERT_HEX_INT_CONTENT_HEADING,
                                                     m_SourceTemplate.GetIndent(m_uiIndent),
                                                     CommonConstTable.GetTagName());

                            m_sHexIntContentHeading += sNewItem;
                            m_sIntHexContentHeading += sNewItem;

                            sNewItem = string.Format(CODE_REPORT_CONVERT_INT_HEX_MAX_MIN_CONTENT,
                                                     m_SourceTemplate.GetIndent(m_uiIndent),
                                                     SpecialConstTable.GetMinValue(),
                                                     SpecialConstTable.GetMaxValue());

                            m_sIntHexContentHeading += sNewItem;

                            bRet = true;
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

        private void AddFuncionHeader()
        {
            string sTemp;

            if (m_FileProcessor != null)
            {
                sTemp = string.Format(m_SourceTemplate.m_sIncludeHeader,
                                      m_sOutputFileName);
                sTemp += m_sUsingNamespaceFormat;
                sTemp += m_sDecleareNamespaceHeaderFormat;
                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private void AddFunctionFooter()
        {
            if (m_FileProcessor != null)
            {
                string sParamsContent = string.Format(m_SourceTemplate.m_sConvertHexIntParamTemplate,
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.TYPE],
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.TYPE],
                                                      m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME]);
                string sTemp = null;
                sTemp += string.Format(CODE_REPORT_CONVERT_FUNCTION_ASCII2HEX,
                                m_SourceTemplate.GetIndent(m_uiIndent));
                for (int i = 0; i < (int)CONVERT_TYPE.CONVERT_TOTAL; i++)
                {
                    sTemp += string.Format(m_SourceTemplate.m_sFuncTemplate,
                                           m_SourceTemplate.GetIndent(m_uiIndent),
                                           m_SourceTemplate.m_sConvertHexIntFunctionType[i],
                                           m_SourceTemplate.m_sConvertHexIntFuncName[i],
                                           sParamsContent,
                                           null);


                    if (i == (int)CONVERT_TYPE.CONVERT_HEX_TO_INT)
                    {
                        sTemp += string.Format(CODE_REPORT_CONVERT_HEX_INT_HEADER,
                                               m_SourceTemplate.GetIndent(m_uiIndent),
                                               m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);
                        if (!string.IsNullOrEmpty(m_sHexIntContentHeading))
                        {
                            sTemp += m_sHexIntContentHeading;
                            sTemp += ConvertParamFormat(CODE_REPORT_CONVERT_HEX_INT_VALUE_CONTENT);
                        }
                        sTemp += string.Format(CODE_REPORT_CONVERT_HEX_INT_FOOTER,
                                               m_SourceTemplate.GetIndent(m_uiIndent));
                    }
                    else
                    {
                        sTemp += string.Format(CODE_REPORT_CONVERT_INT_HEX_HEADER,
                                               m_SourceTemplate.GetIndent(m_uiIndent),
                                               m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);
                        if (!string.IsNullOrEmpty(m_sIntHexContentHeading))
                        {
                            sTemp += m_sIntHexContentHeading;
                        }
                        sTemp += string.Format(CODE_REPORT_CONVERT_INT_HEX_FOOTER,
                                               m_SourceTemplate.GetIndent(m_uiIndent),
                                               m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                               m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                               m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME]);
                    }
                }
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
                (string.IsNullOrEmpty(m_sSizeVarName) == false))
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

        private string ConvertParamFormat(string a)
        {
            string sParameter = string.Format(a,
                                              m_SourceTemplate.GetIndent(m_uiIndent),
                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME]);
            return sParameter;
        }
    }
}
