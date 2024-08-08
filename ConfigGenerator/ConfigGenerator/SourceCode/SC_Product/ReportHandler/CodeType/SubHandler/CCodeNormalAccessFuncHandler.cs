using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeNormalAccessFuncHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_FILE_NAME                   = "config_item_access_func.cpp";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_VARIABLE_DECLARATION        = "{0}{1} {2};\n\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_SWITCH_CASE_HEADER          = "{0}{{\n"                     +
                                                                                          "{0}    {1} {2};\n"           +
                                                                                          "{0}    bool {3} = true;\n"   +
                                                                                          "\n"                          +
                                                                                          "{0}    switch ({4})\n"       +
                                                                                          "{0}    {{\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_SWITCH_CASE_BODY            = "{0}case {1}:\n"                      +
                                                                                          "{0}    if ({2} == GET)\n"            +
                                                                                          "{0}    {{\n"                         +
                                                                                          "{0}        {3} = ({4}) {5}.{6};\n"   +
                                                                                          "{0}    }}\n"                         +
                                                                                          "{0}    else\n"                       +
                                                                                          "{0}    {{\n"                         +
                                                                                          "{0}        {5}.{6} = {7};\n"         +
                                                                                          "{0}    }}\n"                         +
                                                                                          "{0}     break;\n"                    +
                                                                                          "\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_SWITCH_CASE_FOOTER          = "{0}    default:\n"           +
                                                                                          "{0}        {1} = false;\n"   +
                                                                                          "{0}        break;\n"         +
                                                                                          "{0}}}\n"                     +
                                                                                          "\n"                          +
                                                                                          "{0}if ({2} != NULL)\n"       +
                                                                                          "{0}    *{2} = {1};\n"        +
                                                                                          "\n"                          +
                                                                                          "{0}    return {3};\n"        +
                                                                                          "{0}}}\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_BIT_HEADER                  = "{0}{{\n"                             +
                                                                                          "{0}    {1} {2} = 0;\n"               +
                                                                                          "{0}    bool {3} = false;\n"          +
                                                                                          "\n"                                  +
                                                                                          "{0}    unsigned char * {4}[8];\n"    +
                                                                                          "\n"                                  +
                                                                                          "{5}"                                 +
                                                                                          "\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_BIT_BODY                    = "{0}int iIndex                  = 0;\n"                                                       +
                                                                                          "{0}unsigned char ucLocation    = 0;\n"                                                       +
                                                                                          "{0}unsigned char ucBitLength   = 0;\n"                                                       +
                                                                                          "{0}unsigned char ucBitMask     = 0;\n"                                                       +
                                                                                          "{0}unsigned char ucTempMove    = 0;\n"                                                       +
                                                                                          "\n"                                                                                          +
                                                                                          "{0}if (pNormalItem != NULL)\n"                                                               +
                                                                                          "{0}{{\n"                                                                                     +
                                                                                          "{0}    iIndex      = pNormalItem->{1};\n"                                                    +
                                                                                          "{0}    ucLocation  = pNormalItem->{2};\n"                                                    +
                                                                                          "{0}    ucBitLength = pNormalItem->{3};\n"                                                    +
                                                                                          "{0}    {8} = true;\n"                                                                       +
                                                                                          "{0}}}\n"                                                                                     +
                                                                                          "{0}else\n"                                                                                   +
                                                                                          "{0}{{\n"                                                                                     +
                                                                                          "{0}{9}"                                                                                 +
                                                                                          "{0}}}\n"                                                                                     +
                                                                                          "\n"                                                                                          +
                                                                                          "{0}if ({8} == true)\n"                                                                      +
                                                                                          "{0}{{\n"                                                                                     +
                                                                                          "{0}    ucTempMove = ( 8 - (ucLocation + ucBitLength));\n"                                    +
                                                                                          "{0}    for (int j = 0; j < ucBitLength; j++)\n"                                              +
                                                                                          "{0}    {{\n"                                                                                 +
                                                                                          "{0}        ucBitMask |= (0x01 << j); \n"                                                     +
                                                                                          "{0}    }}\n"                                                                                 +
                                                                                          "\n"                                                                                          +
                                                                                          "{0}    if ({4} == GET)\n"                                                                    +
                                                                                          "{0}    {{\n"                                                                                 +
                                                                                          "{0}        ValueRet = ({5})(({6}[(ucBitLength - 1)][iIndex] >> ucTempMove) & ucBitMask);\n"  +
                                                                                          "{0}    }}\n"                                                                                 +
                                                                                          "{0}    else\n"                                                                               +
                                                                                          "{0}    {{\n"                                                                                 +
                                                                                          "{0}        {6}[(ucBitLength - 1)][iIndex] &= (~(ucBitMask << ucTempMove));\n"                +
                                                                                          "{0}        {6}[(ucBitLength - 1)][iIndex] |= ({5})({7} << ucTempMove);\n"                   +
                                                                                          "{0}    }}\n"                                                                                 +
                                                                                          "{0}}}\n"                                                                                     +
                                                                                          "\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_SEARCH_INDEX                = "{0}int iCIIndex = {2}({3});\n"                  +
                                                                                          "{1}    if(iCIIndex != -1) \n"                   +
                                                                                          "{1}    {{\n"                                    +
                                                                                          "{1}        iIndex      = {4}[iCIIndex].{5};\n"  +
                                                                                          "{1}        ucLocation  = {4}[iCIIndex].{6};\n"  +
                                                                                          "{1}        ucBitLength = {4}[iCIIndex].{7};\n"  +
                                                                                          "{1}        {8} = true;\n"                       +
                                                                                          "{1}    }}\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_BYTE_BIT_FOOTER             = "{0}    if ({1} != NULL)\n"                       +
                                                                                          "{0}    {{\n"                                     +
                                                                                          "{0}        *{1} = {2};\n"                        +
                                                                                          "{0}    }}\n\n"                                   +
                                                                                          "{0}    return {3};\n"                            +
                                                                                          "{0}}}\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_BYTE_HEADER                 = "{0}{{\n"                                         +
                                                                                          "{0}    {1} {2};\n"                               +
                                                                                          "{0}    bool {3} = false;\n"                      +
                                                                                          "\n";

        private const string CODE_REPORT_ACCESS_FUNC_NORMAL_BYTE_BODY                   = "{0}if (({1}){2} < {3}) \n"                       +
                                                                                          "{0}{{\n"                                         +
                                                                                          "{0}    if ({4} == GET)\n"                        +
                                                                                          "{0}    {{\n"                                     +
                                                                                          "{0}        {5} = ({6}) {7}.{8}[{2}];\n"          +
                                                                                          "{0}    }}\n"                                     +
                                                                                          "{0}    else\n"                                   +
                                                                                          "{0}    {{\n"                                     +
                                                                                          "{0}        {7}.{8}[{2}] = (unsigned char) {9};\n"+
                                                                                          "{0}    }}\n"                                     +
                                                                                          "\n"                                              +
                                                                                          "{0}    {10} = true; \n"                          +
                                                                                          "{0}}}\n"                                         +
                                                                                          "\n";


        private const string CODE_REPORT_ADDRESS_VAR_NAME                               = "p_ucAddressCI";

        private const string CODE_REPORT_VALID_RET_VAR_NAME                             = "bValidRet";

        private const string CODE_REPORT_VALUE_RET_VAR_NAME                             = "ValueRet";
        
        private ICodeFileProcessor  m_FileProcessor         = null;
        private CSourceCodeTemplate m_SourceTemplate        = null;
        private bool                m_bFileComplete         = false;

        // properties of normal access function section
        private string         m_sOutputFolder              = null;
        private string         m_sOutputFileName            = null;
        private string         m_sNormalAccessFuncType      = null;
        private string         m_sNormalAccessFuncName      = null;
        private List<string[]> m_lsNormalAccessFuncParams   = null;

        private string         m_sUsingNamespace            = null;
        private string         m_sDecleareNamespace         = null;

        private string         m_sSearchNormalItemIndexFuncName = null;
        
        // properties of other section
        private string                       m_sNormalConfigTableName   = null;
        private string                       m_sCmVariableType          = null;
        private string                       m_sCmVariableName          = null;
        private string                       m_NormalTableElementCiName = null;

        // Indent information
        private uint m_uiIndentAccessFuncDecleare       = 0;

        private uint m_uiIndentSwitchCaseHeader         = 0;
        private uint m_uiIndentSwitchCaseBody           = 2;
        private uint m_uiIndentSwitchCaseFooter         = 0;

        private uint m_uiIndentByteArrayHeader          = 0;
        private uint m_uiIndentByteArrayBody            = 1;

        private uint m_uiIndentBitArrayHeader           = 0;       
        private uint m_uiIndentBitArrayBody             = 1;
        private uint m_uiIndentByteBitFooter            = 0;

        private uint m_uiIndentTempBitArrayDecleare     = 1;

        private string m_sDecleareNamespaceHeaderFormat = null;
        private string m_sDecleareNamespaceFooterFormat = null;
        private string m_sUsingNamespaceFormat          = null;

        public CCodeNormalAccessFuncHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_ACCESS_FUNCTION_FILE_PROCESSOR);
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
                    m_sNormalAccessFuncType = (oData as INormalAccessFuncProperties).GetNormalAccessFuncType();
                    m_sNormalAccessFuncName = (oData as INormalAccessFuncProperties).GetNormalAccessFuncName();
                    m_lsNormalAccessFuncParams = (oData as INormalAccessFuncProperties).GetNormalAccessFuncParams();
                    m_sNormalConfigTableName = (oData as INormalTableProperties).GetNormalTableName();
                    m_sCmVariableType = (oData as ICodeStructure).GetCodeStructureType();
                    m_sCmVariableName = (oData as ISpecialTableProperties).GetCmVariableName();
                    m_sDecleareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();
                    m_sUsingNamespace = (oData as CSourceInfoObject).GetUsingNamespace();
                    m_sSearchNormalItemIndexFuncName = (oData as CSourceInfoObject).GetSearchNormalItemIndexFuncName();

                    List<TABLE_STRUCT_DATA_TYPE> NormalTableElementOrder = (oData as INormalTableProperties).GetNormalTableElementOrder();
                    if ((NormalTableElementOrder != null) && (NormalTableElementOrder.Count > 0))
                    {
                        foreach (TABLE_STRUCT_DATA_TYPE element in NormalTableElementOrder)
                        {
                            if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.CI_NAME_ID)
                            {
                                m_NormalTableElementCiName = element.m_sDataName;
                            }
                        }
                    }

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_ACCESS_FUNC_NORMAL_FILE_NAME);

                        if (m_sDecleareNamespace != null)
                        {
                            m_uiIndentAccessFuncDecleare += 1;
                            m_uiIndentSwitchCaseHeader += 1;
                            m_uiIndentSwitchCaseBody += 1;
                            m_uiIndentSwitchCaseFooter += 1;

                            m_uiIndentByteArrayHeader += 1;
                            m_uiIndentByteArrayBody += 1;

                            m_uiIndentBitArrayHeader += 1;
                            m_uiIndentBitArrayBody += 1;
                            m_uiIndentByteBitFooter += 1;

                            m_uiIndentTempBitArrayDecleare += 1;


                            m_sDecleareNamespaceHeaderFormat = string.Format(m_SourceTemplate.m_sDecleareNamespaceHeader,
                                                                               m_sDecleareNamespace);
                            m_sDecleareNamespaceFooterFormat = m_SourceTemplate.m_sDecleareNamespaceFooter;
                        }

                        if (m_sUsingNamespace != null)
                        {
                            m_sUsingNamespaceFormat = string.Format(m_SourceTemplate.m_sUsingNamespace,
                                                                                m_sUsingNamespace);
                        }

                        AddFunctionHeader();
                        bRet = true;
                    }
                }
            }

            return bRet;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            bool bRet = false;           
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

        private void AddFunctionHeader()
        {
            string sTemp            = null;
            string sParamsContent   = null;
            
            if (m_lsNormalAccessFuncParams.Count == (int)PARAMS_ID.PARAM_TOTAL)
            {
                sParamsContent += m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE] + " " + m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME] + ", ";
                sParamsContent += m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE] + " " + m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME] + ", ";
                sParamsContent += m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_3][(int)VAR_ID.TYPE] + " " + m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME] + ", ";
                sParamsContent += m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_4][(int)VAR_ID.TYPE] + " " + m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME] + ", ";
                sParamsContent += m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_5][(int)VAR_ID.TYPE] + " " + m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_5][(int)VAR_ID.NAME];
            }

            //----Common Part----
            sTemp = string.Format(m_SourceTemplate.m_sIncludeHeader,
                                    m_sOutputFileName);

            sTemp += m_sUsingNamespaceFormat;
            sTemp += m_sDecleareNamespaceHeaderFormat;
          
            sTemp += string.Format(CODE_REPORT_ACCESS_FUNC_NORMAL_VARIABLE_DECLARATION,
                                    m_SourceTemplate.GetIndent(m_uiIndentAccessFuncDecleare),
                                    m_sCmVariableType,
                                    m_sCmVariableName);

            sTemp += string.Format(m_SourceTemplate.m_sFuncTemplate,
                                    m_SourceTemplate.GetIndent(m_uiIndentAccessFuncDecleare),
                                    m_sNormalAccessFuncType,
                                    m_sNormalAccessFuncName,
                                    sParamsContent,
                                    null);

            //----Header of using Bit-Array type----
            string sTemplate = "{0}{1}[{2}]    =    {3}.{4};\n";
            string sTempHeader = null;

            for (int i = 0; i < 8; i++)
            {
                sTempHeader += string.Format(sTemplate,
                                            m_SourceTemplate.GetIndent(m_uiIndentTempBitArrayDecleare),
                                            CODE_REPORT_ADDRESS_VAR_NAME,
                                            i.ToString(),
                                            m_sCmVariableName,
                                            string.Format(m_SourceTemplate.m_sConfigStructureExtend, (i + 1).ToString()));
            }

            sTemp += string.Format(CODE_REPORT_ACCESS_FUNC_NORMAL_BIT_HEADER,
                                    m_SourceTemplate.GetIndent(m_uiIndentBitArrayHeader),
                                    m_sNormalAccessFuncType,
                                    CODE_REPORT_VALUE_RET_VAR_NAME,
                                    CODE_REPORT_VALID_RET_VAR_NAME,
                                    CODE_REPORT_ADDRESS_VAR_NAME,
                                    sTempHeader);

            string sSearchCode;

            sSearchCode = string.Format(CODE_REPORT_ACCESS_FUNC_NORMAL_SEARCH_INDEX,
                                        m_SourceTemplate.GetIndent(m_uiIndentBitArrayHeader),
                                        m_SourceTemplate.GetIndent(m_uiIndentBitArrayBody),
                                        m_sSearchNormalItemIndexFuncName,
                                        m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                        m_sNormalConfigTableName,
                                        m_SourceTemplate.m_sNorTableMemExtend[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_SourceTemplate.m_sNorTableMemExtend[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                        m_SourceTemplate.m_sNorTableMemExtend[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                        CODE_REPORT_VALID_RET_VAR_NAME);                                            

            sTemp += string.Format(CODE_REPORT_ACCESS_FUNC_NORMAL_BIT_BODY,
                                    m_SourceTemplate.GetIndent(m_uiIndentBitArrayBody),                    
                                    m_SourceTemplate.m_sNorTableMemExtend[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                    m_SourceTemplate.m_sNorTableMemExtend[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                    m_SourceTemplate.m_sNorTableMemExtend[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                    m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                    m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                    CODE_REPORT_ADDRESS_VAR_NAME,
                                    m_lsNormalAccessFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                    CODE_REPORT_VALID_RET_VAR_NAME,
                                    sSearchCode);

            m_FileProcessor.AddNewItem(sTemp);
        }

        private void AddFuncionFooter ()
        {
            string sTemp;
            sTemp = string.Format(CODE_REPORT_ACCESS_FUNC_NORMAL_BYTE_BIT_FOOTER,
                                    m_SourceTemplate.GetIndent(m_uiIndentByteBitFooter),
                                    m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME].Replace("*", string.Empty),
                                    CODE_REPORT_VALID_RET_VAR_NAME,
                                    CODE_REPORT_VALUE_RET_VAR_NAME);

            sTemp += m_sDecleareNamespaceFooterFormat;
            m_FileProcessor.AddNewItem(sTemp);            
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ( (m_sOutputFolder               != null) && (m_sOutputFolder                    != string.Empty) &&
                 (m_sOutputFileName             != null) && (m_sOutputFileName                  != string.Empty) &&
                 (m_sNormalAccessFuncType       != null) && (m_sNormalAccessFuncType            != string.Empty) &&
                 (m_sNormalAccessFuncName       != null) && (m_sNormalAccessFuncName            != string.Empty) &&
                 (m_sNormalConfigTableName      != null) && (m_sNormalConfigTableName           != string.Empty) &&
                 (m_NormalTableElementCiName    != null) && (m_NormalTableElementCiName         != string.Empty) &&
                 (m_sCmVariableType             != null) && (m_sCmVariableType                  != string.Empty) &&
                 (m_sCmVariableName             != null) && (m_sCmVariableName                  != string.Empty) &&
                 (m_sSearchNormalItemIndexFuncName != null) && (m_sSearchNormalItemIndexFuncName != string.Empty) &&
                 (m_lsNormalAccessFuncParams    != null) && (m_lsNormalAccessFuncParams.Count   == (int)PARAMS_ID.PARAM_TOTAL))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bFileComplete             = false;

            m_sOutputFolder            = null;
            m_sOutputFileName          = null;
            m_sNormalAccessFuncType    = null;
            m_sNormalAccessFuncName    = null;
            m_lsNormalAccessFuncParams = null;
            m_sUsingNamespace          = null;
            m_sDecleareNamespace       = null;

            m_sNormalConfigTableName   = null;
            m_sCmVariableType          = null;
            m_sCmVariableName          = null;
            m_NormalTableElementCiName = null;
            m_sSearchNormalItemIndexFuncName = null;

            m_uiIndentAccessFuncDecleare    = 0;
            m_uiIndentSwitchCaseHeader      = 0;
            m_uiIndentSwitchCaseBody        = 2;
            m_uiIndentSwitchCaseFooter      = 0;
            m_uiIndentByteArrayHeader       = 0;
            m_uiIndentByteArrayBody         = 1;
            m_uiIndentBitArrayHeader        = 0;
            m_uiIndentBitArrayBody          = 1;
            m_uiIndentByteBitFooter         = 0;
            m_uiIndentTempBitArrayDecleare  = 1;

            m_sDecleareNamespaceHeaderFormat = null;
            m_sDecleareNamespaceFooterFormat = null;
            m_sUsingNamespaceFormat          = null;
        }
    }
}