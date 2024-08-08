using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeCIEnumTableCopyHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_CI_ENUM_FILE_NAME  = "config_item_ci_enumeration.h";
        private ICodeFileProcessor  m_FileProcessor         = null;
        private CSourceCodeTemplate m_SourceTemplate        = null;
        private CNamespaceFormatter m_NamespaceFormatter    = null;
        private string              m_sOutputFolder         = null;
        List<CI_ENUM_INFO>[]        m_lsEnumInfo            = null;
        private uint                m_uiIntent              = 0;


        public CCodeCIEnumTableCopyHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_CI_ENUMERATION_FILE_PROCESSOR);
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
                    m_sOutputFolder = (oData as ICommonProperties).GetOutputFolder();
                    m_lsEnumInfo = (oData as CSourceInfoObject).GetCIEnumInfo();

                    string sCIEnumFilePath = null;
                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        if (string.IsNullOrEmpty(m_sOutputFolder) == false)
                        {
                            sCIEnumFilePath = m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_CI_ENUM_FILE_NAME;
                        }
                        else
                        {
                            sCIEnumFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + CODE_REPORT_CI_ENUM_FILE_NAME;
                        }

                        if (m_FileProcessor.CreateNewFile(sCIEnumFilePath) == false)
                        {
                            return false;
                        }

                        string sContent = null;
                        sContent = m_NamespaceFormatter.GetExternCUsingNamespace();
                        sContent += m_NamespaceFormatter.GetExternCDeclareNamespaceHeader();

                        if (String.IsNullOrEmpty(m_NamespaceFormatter.GetExternCDeclareNamespaceHeader()) == false)
                        {
                            m_uiIntent = 1;
                        }

                        // temporary clear using namespace/declare namespace
                        sContent = string.Empty;

                        sContent += string.Format(m_SourceTemplate.m_sTypedefHeader,
                                                m_SourceTemplate.GetIndent(m_uiIntent));

                        foreach (List<CI_ENUM_INFO> element in m_lsEnumInfo)
                        {
                            sContent += ProcessEnumList(element);
                        }

                        sContent += string.Format(m_SourceTemplate.m_sTypedefFooter,
                                                    m_SourceTemplate.GetIndent(m_uiIntent),
                                                    m_SourceTemplate.m_sCINumberType);

                        // sContent += m_NamespaceFormatter.GetExternCDeclareNamespaceFooter();
                        if (sContent != null)
                        {
                            m_FileProcessor.AddNewItem(sContent);
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

        private void ReInitProperties()
        {
            m_NamespaceFormatter    = null;
            m_sOutputFolder         = null;
            m_lsEnumInfo            = null;
            m_uiIntent              = 0;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = true;

            if (m_lsEnumInfo == null)
            {
                bRet = false;
            }

            return bRet;
        }

        private string ProcessEnumList(List<CI_ENUM_INFO> lsEnumList)
        {
            string sResult = string.Empty;
            foreach (CI_ENUM_INFO element in lsEnumList)
            {
                if (string.IsNullOrEmpty(element.m_sCIEnumValue) == false)
                {
                    sResult += string.Format(m_SourceTemplate.m_sTypedefEnumItemWithComment,
                                                m_SourceTemplate.GetIndent(m_uiIntent),
                                                string.Format("{0, -70}", element.m_sCITagName),
                                                element.m_sCIEnumValue,
                                                element.m_sCITagCode);
                }
                else
                {
                    sResult += string.Format(m_SourceTemplate.m_sTypedefEnumItemWithCommentWithoutValue,
                                                m_SourceTemplate.GetIndent(m_uiIntent),
                                                string.Format("{0, -79}", element.m_sCITagName),
                                                element.m_sCITagCode);
                }
            }
            return sResult;
        }
    }
}
