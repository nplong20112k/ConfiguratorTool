using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeCIEnumTableGenHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_CI_ITEM_TABLE_FILE_NAME    = "config_item_ci_enum.h";
        private const string CI_HARDCODE_SECTION                    = "    CI_TERMINATOR";
        private const string CI_SPECIAL_ITEM_SEPARATION             = "    /****Special Items Part****/\n";
        private const string CI_HARDCODE_ITEM_SEPARATION            = "    /****Exception Items Part****/\n";

        private const string CI_ITEM_TABLE                          = "typedef enum \n" +
                                                                      "{{\n" +
                                                                      "{0}\n" +
                                                                      "{1}\n" +
                                                                      "{2}\n" +
                                                                      "}} {3};\n";

        private const string CI_ENUM_NORMAL_PART                    = "{0}" +
                                                                      "    {1},\n";

        private const string CI_ENUM_FIRST_ITEM                     = "    {0,-54}= 0,\n";
        private const string CI_ENUM_NORMAL_ITEM                    = "    {0},\n";

        private ICodeFileProcessor          m_FileProcessor             = null;
        private CSourceCodeTemplate         m_SourceTemplate            = null;

        private bool                        m_bCheckFirstItem           = true;
        private string                      m_sSpecialItem              = null;
        private string                      m_sNormalItem               = null;
        private CNamespaceFormatter         m_NamespaceFormatter        = null;

        private List<EXCEPTION_CI_ITEM>     m_lsExceptionEnumStruct     = null;
        private List<string>                m_lsExceptionEnumOnly       = null;
        
        private bool                        m_bFileComplete             = false;
        private string                      m_sOutputFolder             = null;
 
        public CCodeCIEnumTableGenHandler()
             : base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_CI_ENUM_FILE_PROCESSOR);
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
                    m_lsExceptionEnumOnly = (oData as CSourceInfoObject).GetExceptionEnumItem();
                    m_lsExceptionEnumStruct = (oData as CSourceInfoObject).GetExceptionEnumStructItem();
                    m_NamespaceFormatter = new CNamespaceFormatter((oData as CSourceInfoObject).GetDecleareNamespace(), (oData as CSourceInfoObject).GetUsingNamespace());

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        bRet = true;
                    }
                }
            }

            return bRet;
        }
        
        public override bool DataHandling(IShareObject oDataIn)
        {
            bool bRet = false;

            if (m_FileProcessor != null)
            {
                if (oDataIn != null)
                {

                }
            }

            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            if ((m_bFileComplete == false) && (m_FileProcessor != null))
            {

            }

            return true;
        }

        private void AddFuncionHeader()
        {
            string sTemp = null;

            if (m_FileProcessor != null)
            {
                sTemp = m_NamespaceFormatter.GetExternCUsingNamespace();
                sTemp += m_NamespaceFormatter.GetExternCDeclareNamespaceHeader();

                m_FileProcessor.AddNewItem(sTemp);
            }
        }

        private void AddFuncionFooter()
        {
            string sTemp = null;

            sTemp = m_NamespaceFormatter.GetExternCDeclareNamespaceFooter();
            m_FileProcessor.AddNewItem(sTemp);
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((m_sOutputFolder != null)           && (m_sOutputFolder != string.Empty) &&
                (m_lsExceptionEnumOnly != null)     && (m_lsExceptionEnumOnly.Count > 0) &&
                (m_lsExceptionEnumStruct != null)   && (m_lsExceptionEnumStruct.Count > 0))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bCheckFirstItem           = true;
            m_sSpecialItem              = null;
            m_sNormalItem               = null;
            m_lsExceptionEnumStruct     = null;
            m_lsExceptionEnumOnly       = null;
            m_bFileComplete             = false;
            m_sOutputFolder             = null;
        }
    }
}
