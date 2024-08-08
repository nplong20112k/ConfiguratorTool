using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeInterfaceTypedefHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_INTERFACE_TYPEDEF_FILE_NAME        = "config_item_interface_typedef.h";
        private const string INTERFACE_ALL_ENUM                             = "IF_ALL";
        private const string INTERFACE_ALL_VALUE                            =  "0xFFFFFFFFFFFFFFFF";

        private ICodeFileProcessor              m_FileProcessor             = null;
        private CSourceCodeTemplate             m_SourceTemplate            = null;
        private CNamespaceFormatter             m_NamespaceFormatter        = null;

        private List<INTERFACE_CLASS>           m_lsInterfaceClass          = null;
        private List<TABLE_STRUCT_DATA_TYPE>    m_NormalTableElementOrder   = null;

        private string                          m_sOutputFolder             = null;

        private uint                            m_uiIntent                  = 0;

        public CCodeInterfaceTypedefHandler()
            : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_INTERFACE_TYPEDEF_FILE_PROCESSOR);
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
                    m_lsInterfaceClass =  (oData as CSourceInfoObject).GetInterfaceClass();
                    m_NormalTableElementOrder = (oData as INormalTableProperties).GetNormalTableElementOrder();
                    m_sOutputFolder = (oData as ICommonProperties).GetOutputFolder();
                    m_NamespaceFormatter = new CNamespaceFormatter((oData as CSourceInfoObject).GetDecleareNamespace(), (oData as CSourceInfoObject).GetUsingNamespace());

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        string sItfTypedefFilePath = null;
                        if (string.IsNullOrEmpty(m_sOutputFolder) == false)
                        {
                            sItfTypedefFilePath = m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_INTERFACE_TYPEDEF_FILE_NAME;
                        }
                        else
                        {
                            sItfTypedefFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + CODE_REPORT_INTERFACE_TYPEDEF_FILE_NAME;
                        }

                        if (m_FileProcessor.CreateNewFile(sItfTypedefFilePath) == false)
                        {
                            return false;
                        }

                        if ((oData as CSourceInfoObject).GetDecleareNamespace() != null)
                        {
                            m_uiIntent += 1;
                        }

                        string sInterfaceTypedef = null;
                        sInterfaceTypedef = m_NamespaceFormatter.GetExternCUsingNamespace();
                        sInterfaceTypedef += m_NamespaceFormatter.GetExternCDeclareNamespaceHeader();

                        string sEnum = null;
                        UInt64 uiEnumValue = 1;
                        sEnum = string.Format(m_SourceTemplate.m_sTypedefHeader,
                                                m_SourceTemplate.GetIndent(m_uiIntent));
                        foreach (INTERFACE_CLASS element in m_lsInterfaceClass)
                        {
                            foreach (INTERFACE_CLASS_MEMBER subElement in element.m_lsInterfaceMember)
                            {
                                sEnum += string.Format(m_SourceTemplate.m_sTypedefEnumItem,
                                                        m_SourceTemplate.GetIndent(m_uiIntent),
                                                        string.Format("{0,-25}", subElement.m_sInterfaceName),
                                                        "0x" + string.Format("{0:X16}", uiEnumValue));
                                uiEnumValue *= 2;
                            }
                        }
                        sEnum += string.Format(m_SourceTemplate.m_sTypedefEnumItem,
                                                m_SourceTemplate.GetIndent(m_uiIntent),
                                                string.Format("{0,-25}", INTERFACE_ALL_ENUM),
                                                INTERFACE_ALL_VALUE);

                        string sInterfaceTypedefName = null;
                        foreach (TABLE_STRUCT_DATA_TYPE element1 in m_NormalTableElementOrder)
                        {
                            if (element1.m_DatID == TABLE_STRUCT_MEMBER_ID.INTERFACE_MASK_ID)
                            {
                                sInterfaceTypedefName = element1.m_sDataType;
                            }
                        }
                        sEnum += string.Format(m_SourceTemplate.m_sTypedefFooter,
                                                m_SourceTemplate.GetIndent(m_uiIntent),
                                                sInterfaceTypedefName);

                        sInterfaceTypedef += sEnum;
                        sInterfaceTypedef += m_NamespaceFormatter.GetExternCDeclareNamespaceFooter();

                        if (sInterfaceTypedef != null)
                        {
                            m_FileProcessor.AddNewItem(sInterfaceTypedef);
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
                (m_NormalTableElementOrder != null) &&
                (String.IsNullOrEmpty(m_sOutputFolder) == false))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_NamespaceFormatter        = null;
            m_lsInterfaceClass          = null;
            m_NormalTableElementOrder   = null;
            m_sOutputFolder             = null;
            m_uiIntent                  = 0;
        }
    }
}
