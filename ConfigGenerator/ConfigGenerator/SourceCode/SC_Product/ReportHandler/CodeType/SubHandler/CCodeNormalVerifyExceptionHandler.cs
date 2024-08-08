using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeNormalVerifyExceptionHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_VERIFY_VALUE_NORMAL_FILE_NAME  = "config_item_verify_value.cpp";

        private const string CODE_REPORT_VERIFY_VALUE_NORMAL_HEADER     = "{0}{{\n" +
                                                                          "{0}    bool bRet = true;\n\n" +
                                                                          "{0}    if ({1} != NULL)\n" +
                                                                          "{0}    {{\n" +
                                                                          "{0}        if (({2} > {1}->{3}) || ({2} < {1}->{4}))\n" +
                                                                          "{0}        {{\n" +
                                                                          "{0}            bRet = false;\n" +
                                                                          "{0}            return bRet;\n" +
                                                                          "{0}        }}\n\n" +
                                                                          "{0}        switch ({1}->{5})\n" +
                                                                          "{0}        {{\n";

        private const string CODE_REPORT_VERIFY_VALUE_NORMAL_FOOTER     = "{0}            default:\n" +
                                                                          "{0}                break;\n" +
                                                                          "{0}        }}\n" +
                                                                          "{0}    }}\n\n" +
                                                                          "{0}    return bRet;\n" +
                                                                          "{0}}}\n";
        
        private const string CODE_REPORT_VERIFY_VALUE_NORMAL_CONTENT    = "{0}            case {1}:\n" +
                                                                          "{0}                if ({2})\n" +
                                                                          "{0}                {{\n" +
                                                                          "{0}                    bRet = false;\n" +
                                                                          "{0}                }}\n" +
                                                                          "{0}                break;\n\n";

        private const string CODE_REPORT_EQUAL_EXPRESSION               = "({0} == {1})";
        private const string CODE_REPORT_GREATER_LESS_EXPRESSION        = "(({0} >= {1}) && ({0} <= {2}))";
        private const string CODE_REPORT_OR_OPERATOR                    = " || ";

        private ICodeFileProcessor  m_FileProcessor     = null;           
        private CSourceCodeTemplate m_SourceTemplate    = null;           
        private bool                m_bFileComplete     = false;          
                                                                          
        private string              m_sOutputFolder     = null;
        private string              m_sOutputFileName   = null;

        private string              m_sMinVarName       = null;
        private string              m_sMaxVarName       = null;
        private string              m_sItemNumVarName   = null;

        private string              m_sNormalVerifyExceptFuncType       = null;
        private string              m_sNormalVerifyExceptFuncName       = null;
        private List<string[]>      m_lsNormalVerifyExceptFuncParams    = null;
        private string              m_sUsingNamespace                   = null;
        private string              m_sDeclareNamespace                 = null;

        private string              m_sDecleareNamespaceHeaderFormat    = null;
        private string              m_sDecleareNamespaceFooterFormat    = null;
        private string              m_sUsingNamespaceFormat             = null;

        private uint                m_uiIndent                          = 0;

        public CCodeNormalVerifyExceptionHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_VERIFY_EXCEPTION_FILE_PROCESSOR);
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
                    m_sNormalVerifyExceptFuncType = (oData as INormalVerifyExceptFuncProperties).GetNormalVerifyExceptFuncType();
                    m_sNormalVerifyExceptFuncName = (oData as INormalVerifyExceptFuncProperties).GetNormalVerifyExceptFuncName();
                    m_lsNormalVerifyExceptFuncParams = (oData as INormalVerifyExceptFuncProperties).GetNormalVerifyExceptFuncParams();
                    m_sDeclareNamespace = (oData as CSourceInfoObject).GetDecleareNamespace();
                    m_sUsingNamespace = (oData as CSourceInfoObject).GetUsingNamespace();

                    List<TABLE_STRUCT_DATA_TYPE> NormalTableElementOrder = (oData as INormalTableProperties).GetNormalTableElementOrder();
                    if ((NormalTableElementOrder != null) && (NormalTableElementOrder.Count > 0))
                    {
                        foreach (TABLE_STRUCT_DATA_TYPE element in NormalTableElementOrder)
                        {
                            if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.CI_NAME_ID)
                            {
                                m_sItemNumVarName = element.m_sDataName;
                            }
                            else if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.MIN_VALUE_ID)
                            {
                                m_sMinVarName = element.m_sDataName;
                            }
                            else if (element.m_DatID == TABLE_STRUCT_MEMBER_ID.MAX_VALUE_ID)
                            {
                                m_sMaxVarName = element.m_sDataName;
                            }
                        }
                    }

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_VERIFY_VALUE_NORMAL_FILE_NAME);

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
                    if (CommonConstTable.GetComConstTableType() == COMMON_CONST_TABLE_TYPE.NORMAL_TABLE)
                    {
                        CNormalConstItem NormalConstTable = CommonConstTable as CNormalConstItem;
                        List<string> sExceptionValueList = NormalConstTable.GetExceptionValueList();

                        if (sExceptionValueList != null && (sExceptionValueList.Count > 0))
                        {
                            string sNewItem = null;
                            string sFirstFormatValue = null;
                            string sTagName = CommonConstTable.GetTagName();

                            foreach (string ExceptionValueElement in sExceptionValueList)
                            {
                                if (sFirstFormatValue != null)
                                {
                                    sFirstFormatValue += CODE_REPORT_OR_OPERATOR;
                                }
                                sFirstFormatValue += string.Format(CODE_REPORT_EQUAL_EXPRESSION,
                                                                    m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                                                    ExceptionValueElement);
                            }

                            string sSecondFormatValue = HandleArrayOfValue(sExceptionValueList);

                            string sFormatValue;
                            if (sFirstFormatValue.Length > sSecondFormatValue.Length)
                            {
                                sFormatValue = sSecondFormatValue;
                            }
                            else
                            {
                                sFormatValue = sFirstFormatValue;
                            }
                            sNewItem = string.Format(CODE_REPORT_VERIFY_VALUE_NORMAL_CONTENT,
                                                      m_SourceTemplate.GetIndent(m_uiIndent),
                                                      sTagName,
                                                      sFormatValue);

                            bRet = m_FileProcessor.AddNewItem(sNewItem);
                        }
                    }
                }
            }

            return bRet;
        }

        private string HandleArrayOfValue(List<string> sExceptionValueList)
        {
            string sResult = null;

            List<List<int>> consecutiveList = SeparateToConsecutiveList(sExceptionValueList);

            foreach (List<int> subList in consecutiveList)
            {
                if (sResult != null)
                {
                    sResult += CODE_REPORT_OR_OPERATOR;
                }

                if (subList.Count > 1)
                {
                    sResult += string.Format(CODE_REPORT_GREATER_LESS_EXPRESSION,
                                    m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                    subList[0].ToString(),
                                    subList[subList.Count - 1].ToString());
                }
                else
                {
                    sResult += string.Format(CODE_REPORT_EQUAL_EXPRESSION,
                                    m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                    subList[0].ToString());
                }
            }

            return sResult;
        }

        private static List<List<int>> SeparateToConsecutiveList(List<string> sExceptionValueList)
        {
            List<List<int>> consecutiveList = new List<List<int>>();
            List<int> resultList = new List<int>(); ;
            foreach (string ExceptionValueElement in sExceptionValueList)
            {
                int iCurrentValue = int.Parse(ExceptionValueElement);
                if ((resultList.Count == 0) || (iCurrentValue - (resultList[resultList.Count - 1]) <= 1))
                {
                    resultList.Add(iCurrentValue);
                }
                else
                {
                    consecutiveList.Add(resultList);
                    resultList = new List<int> { iCurrentValue };
                }
            }
            consecutiveList.Add(resultList);
            return consecutiveList;
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
                string sParamsContent = string.Format(  m_SourceTemplate.m_sVerifyExceptParamTemplate,
                                                        m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                                        m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                        m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);

                sTemp = string.Format( m_SourceTemplate.m_sIncludeHeader,
                                       m_sOutputFileName);

                sTemp += m_sUsingNamespaceFormat;
                sTemp += m_sDecleareNamespaceHeaderFormat;

                sTemp += string.Format( m_SourceTemplate.m_sFuncTemplate, 
                                        m_SourceTemplate.GetIndent(m_uiIndent), 
                                        m_sNormalVerifyExceptFuncType, 
                                        m_sNormalVerifyExceptFuncName,
                                        sParamsContent, 
                                        null);
                
                sTemp += string.Format( CODE_REPORT_VERIFY_VALUE_NORMAL_HEADER,
                                        m_SourceTemplate.GetIndent(m_uiIndent),
                                        m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                        m_lsNormalVerifyExceptFuncParams[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                        m_sMaxVarName,
                                        m_sMinVarName,
                                        m_sItemNumVarName);
                                            
                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private void AddFuncionFooter ()
        {
            if (m_FileProcessor != null)
            {
                string sTemp;

                sTemp = string.Format(CODE_REPORT_VERIFY_VALUE_NORMAL_FOOTER,
                                        m_SourceTemplate.GetIndent(m_uiIndent));
                sTemp += m_sDecleareNamespaceFooterFormat;

                m_FileProcessor.AddNewItem(sTemp);
            }     
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ( (m_sOutputFolder                   != null) && (m_sOutputFolder                        != string.Empty) &&
                 (m_sOutputFileName                 != null) && (m_sOutputFileName                      != string.Empty) && 
                 (m_sMinVarName                     != null) && (m_sMinVarName                          != string.Empty) &&
                 (m_sMaxVarName                     != null) && (m_sMaxVarName                          != string.Empty) &&
                 (m_sItemNumVarName                 != null) && (m_sItemNumVarName                      != string.Empty) &&
                 (m_sNormalVerifyExceptFuncType     != null) && (m_sNormalVerifyExceptFuncType          != string.Empty) &&
                 (m_sNormalVerifyExceptFuncName     != null) && (m_sNormalVerifyExceptFuncName          != string.Empty) &&
                 (m_lsNormalVerifyExceptFuncParams  != null) && (m_lsNormalVerifyExceptFuncParams.Count == 2/*NumberOfParam*/))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bFileComplete                     = false;
            m_sUsingNamespace                   = null;
            m_sDeclareNamespace                 = null;
            m_sDecleareNamespaceHeaderFormat    = null;
            m_sDecleareNamespaceFooterFormat    = null;
            m_sUsingNamespaceFormat             = null;

            m_uiIndent                          = 0;
        }
    }
}
