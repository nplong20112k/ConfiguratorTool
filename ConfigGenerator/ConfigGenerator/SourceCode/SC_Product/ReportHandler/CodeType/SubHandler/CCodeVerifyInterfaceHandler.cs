using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeVerifyInterfaceHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_VERIFY_INTERFACE_FILE_NAME        =    "config_item_verify_interface.cpp";

        private const string CODE_REPORT_VERIFY_INTERFACE_HEADER           =    "{0}{{\n" +
                                                                                "{0}    bool bRet = true;\n" +
                                                                                "\n" +
                                                                                "{0}    switch ({1})\n" +
                                                                                "{0}    {{\n" +
                                                                                "{0}        default:\n" +
                                                                                "{0}             bRet = false;\n" +
                                                                                "{0}             break;\n" +
                                                                                "\n";

        private const string CODE_REPORT_VERIFY_INTERFACE_BODY_VALID_ITF   =    "{0}        case {1}: /* {2} */\n";
        private const string CODE_REPORT_VERIFY_INTERFACE_BODY_INVALID_ITF =    "{0}        // case {1}: /* {2} */\n";

        private const string CODE_REPORT_VERIFY_INTERFACE_FOOTER           =    "{0}             break;\n" +
                                                                                "{0}    }}\n" +
                                                                                "\n" +
                                                                                "{0}    return bRet;\n" +
                                                                                "{0}}}\n";

        private ICodeFileProcessor      m_FileProcessor                 = null;
        private CSourceCodeTemplate     m_SourceTemplate                = null;
        private CNamespaceFormatter     m_NamespaceFormatter            = null;

        private List<INTERFACE_CLASS>   m_lsInterfaceClass              = null;
        private string                  m_sOutputFolder                 = null;
        private string                  m_sOutputFileName               = null;
        private string                  m_sVerifyInterfaceFuncType      = null;
        private string                  m_sVerifyInterfaceFuncName      = null;
        private List<string[]>          m_lsVerifyInterfaceFuncParams   = null;

        public CCodeVerifyInterfaceHandler()
            : base ()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_VERIFY_INTERFACE_FILE_PROCESSOR);
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
                    m_NamespaceFormatter = new CNamespaceFormatter((oData as CSourceInfoObject).GetDecleareNamespace(), (oData as CSourceInfoObject).GetUsingNamespace());
                    m_lsInterfaceClass = (oData as CSourceInfoObject).GetInterfaceClass();
                    m_sOutputFolder = (oData as ICommonProperties).GetOutputFolder();
                    m_sOutputFileName = (oData as CSourceInfoObject).GetOutputFileName();
                    m_sVerifyInterfaceFuncType = (oData as IVerifyInterfaceValueFuncProperties).GetVerifyInterfaceValueFuncType();
                    m_sVerifyInterfaceFuncName = (oData as IVerifyInterfaceValueFuncProperties).GetVerifyInterfaceValueFuncName();
                    m_lsVerifyInterfaceFuncParams = (oData as IVerifyInterfaceValueFuncProperties).GetVerifyInterfaceValueFuncParams();

                    string sVerifyInterfaceFilePath = null;
                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        if (string.IsNullOrEmpty(m_sOutputFolder) == false)
                        {
                            sVerifyInterfaceFilePath = m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_VERIFY_INTERFACE_FILE_NAME;
                        }
                        else
                        {
                            sVerifyInterfaceFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + CODE_REPORT_VERIFY_INTERFACE_FILE_NAME;
                        }

                        if (m_FileProcessor.CreateNewFile(sVerifyInterfaceFilePath) == false)
                        {
                            return false;
                        }

                        string sVerifyInterface = null;
                        sVerifyInterface = string.Format(m_SourceTemplate.m_sIncludeHeader,
                                                            m_sOutputFileName);
                        sVerifyInterface += m_NamespaceFormatter.GetExternCUsingNamespace();
                        sVerifyInterface += m_NamespaceFormatter.GetExternCDeclareNamespaceHeader();

                        uint uiIndent = 0;
                        if (String.IsNullOrEmpty(m_NamespaceFormatter.GetExternCDeclareNamespaceHeader()) == false)
                        {
                            uiIndent = 1;
                        }

                        string sVerifyItfFunction = null;
                        string sParamsContent = string.Format(m_SourceTemplate.m_sVerifyInterfaceParamTemplate,
                                                                m_lsVerifyInterfaceFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                                                m_lsVerifyInterfaceFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);

                        sVerifyItfFunction = string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                            m_SourceTemplate.GetIndent(uiIndent),
                                                            m_sVerifyInterfaceFuncType,
                                                            m_sVerifyInterfaceFuncName,
                                                            sParamsContent,
                                                            null);

                        sVerifyItfFunction += string.Format(CODE_REPORT_VERIFY_INTERFACE_HEADER,
                                                                m_SourceTemplate.GetIndent(uiIndent),
                                                                m_lsVerifyInterfaceFuncParams[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);

                        foreach (INTERFACE_CLASS element in m_lsInterfaceClass)
                        {
                            foreach (INTERFACE_CLASS_MEMBER subElement in element.m_lsInterfaceMember)
                            {
                                string sItfValue = "0x" + subElement.m_sCommand.Substring(2);
                                if (subElement.m_bInterfaceValid == true)
                                {
                                    sVerifyItfFunction += String.Format(CODE_REPORT_VERIFY_INTERFACE_BODY_VALID_ITF,
                                                                    m_SourceTemplate.GetIndent(uiIndent),
                                                                    sItfValue,
                                                                    subElement.m_sInterfaceName);
                                }
                                else
                                {
                                    sVerifyItfFunction += String.Format(CODE_REPORT_VERIFY_INTERFACE_BODY_INVALID_ITF,
                                                                    m_SourceTemplate.GetIndent(uiIndent),
                                                                    sItfValue,
                                                                    subElement.m_sInterfaceName);
                                }
                            }
                            sVerifyItfFunction += "\n";
                        }

                        sVerifyItfFunction += string.Format(CODE_REPORT_VERIFY_INTERFACE_FOOTER,
                                                    m_SourceTemplate.GetIndent(uiIndent));

                        sVerifyInterface += sVerifyItfFunction;
                        sVerifyInterface += m_NamespaceFormatter.GetExternCDeclareNamespaceFooter();
                        if (sVerifyInterface != null)
                        {
                            m_FileProcessor.AddNewItem(sVerifyInterface);
                            m_FileProcessor.CloseFile();
                            bRet = true;
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            return true;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            return true;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((m_lsInterfaceClass != null) &&
                (string.IsNullOrEmpty(m_sOutputFolder) == false) &&
                (string.IsNullOrEmpty(m_sOutputFileName) == false) &&
                (string.IsNullOrEmpty(m_sVerifyInterfaceFuncType) == false) &&
                (string.IsNullOrEmpty(m_sVerifyInterfaceFuncName) == false) &&
                (m_lsVerifyInterfaceFuncParams != null))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_NamespaceFormatter = null;
            m_lsInterfaceClass = null;
            m_sOutputFolder = null;
            m_sOutputFileName = null;
            m_sVerifyInterfaceFuncType = null;
            m_sVerifyInterfaceFuncName = null;
            m_lsVerifyInterfaceFuncParams = null;
        }
    }
}
