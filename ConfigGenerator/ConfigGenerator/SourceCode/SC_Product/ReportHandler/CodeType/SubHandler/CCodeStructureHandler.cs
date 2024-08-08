using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{
    class CCodeStructureHandler : ACodeSubHandler
    {
        private const string CODE_REPORT_STRUCTURE_FILE_NAME            = "config_item_structure.h";

        private const string CODE_REPORT_STRUCTURE_LESS_BYTE_ITEM       = "    unsigned char {0,-56}:{1};\n";

        private const string CODE_REPORT_STRUCTURE_EQUAL_BYTE_ITEM      = "    unsigned char {0};\n";

        private const string CODE_REPORT_STRUCTURE_GREATER_BYTE_ITEM    = "    unsigned char {0}[{1}];\n";

        private const string CODE_REPORT_STRUCTURE_HEADER               = "typedef struct\n" +
                                                                          "{\n";

        private const string CODE_REPORT_STRUCTURE_TERMINATOR           = "}} {0};\n";

        private const string CODE_REPORT_STRUCTURE_OPTIMIZE             = "    unsigned char {0}[{1}];\n";

        private ICodeFileProcessor      m_FileProcessor             = null;
        private CSourceCodeTemplate     m_SourceTemplate            = null;
        private bool                    m_bFileComplete             = false;

        private uint[]                  m_uiTempIndex               = new uint[8];
        private uint                    m_uiTempIndexByteType       = 0;
        
        private string                  m_sOutputFolder             = null;
        private string                  m_sStructureType            = null;

        private string                  m_sSpecialItemByteType      = null;
        private List<EXCEPTION_CI_ITEM> m_lsExceptionEnumStruct     = null;

        private CNamespaceFormatter     m_NamespaceFormatter = null;

        public CCodeStructureHandler()
            :base ()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_STRUCTURE_FILE_PROCESSOR);
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
                    m_sStructureType = (oData as ICodeStructure).GetCodeStructureType();
                    m_lsExceptionEnumStruct = (oData as CSourceInfoObject).GetExceptionEnumStructItem();
                    m_NamespaceFormatter = new CNamespaceFormatter((oData as CSourceInfoObject).GetDecleareNamespace(), (oData as CSourceInfoObject).GetUsingNamespace());

                    if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
                    {
                        m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_STRUCTURE_FILE_NAME);
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

            if (m_FileProcessor != null)
            {
                if (oDataIn != null)
                {
                    CStructure StructureObj = null;
                    StructureObj = (oDataIn as CIntegratedDataObject).GetStructure();

                    if (StructureObj != null)
                    {
                        string sNewItem = null;                        
                        uint uiSizeInBit = uint.Parse(StructureObj.GetTagSizeInBit());
                        if (uiSizeInBit > 8)
                        {
                            uiSizeInBit = uiSizeInBit / 8;
                            sNewItem = string.Format( CODE_REPORT_STRUCTURE_GREATER_BYTE_ITEM,
                                                        StructureObj.GetTagName(),
                                                        uiSizeInBit.ToString());

                                                    sNewItem = m_NamespaceFormatter.FormatStringWithIndent(sNewItem);
                        }
                        else if ((uiSizeInBit > 0) && (uiSizeInBit <= 8))
                        {
                            // This contain the length of bit array size while structure only return index => plus 1
                            m_uiTempIndex[(uiSizeInBit - 1)] = StructureObj.GetIndex(uiSizeInBit) + 1;
                        }


                        bRet = m_FileProcessor.AddNewItem(sNewItem);
                    }
                }
            }

            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            // Do not support handle oDataIn yet!

            if ((m_bFileComplete == false) && (m_FileProcessor != null))
            {           
                string sTemp = null;

                for (int i = 1; i <= 8; i++)
                {
                    // Generate at least 1 item for any category dont have any items to avoid compiler error
                    if (0 == m_uiTempIndex[(i - 1)])
                    {
                        m_uiTempIndex[(i - 1)] = 1;
                    }

                    sTemp = string.Format(CODE_REPORT_STRUCTURE_OPTIMIZE,
                                            string.Format(m_SourceTemplate.m_sConfigStructureExtend, i.ToString()),
                                            m_uiTempIndex[(i - 1)]);

                    sTemp = m_NamespaceFormatter.FormatStringWithIndent(sTemp);

                    m_FileProcessor.AddNewItem(sTemp);
                }

                AddFuncionFooter();
                m_FileProcessor.CloseFile();
                m_bFileComplete = true;
            }

            return true;
        }

        private void AddFuncionHeader()
        {
            if (m_FileProcessor != null)
            {
                string sTemp = null;

                sTemp = m_NamespaceFormatter.GetExternCUsingNamespace();
                sTemp += m_NamespaceFormatter.GetExternCDeclareNamespaceHeader();

                sTemp += m_NamespaceFormatter.FormatStringWithIndent(CODE_REPORT_STRUCTURE_HEADER);

                m_FileProcessor.AddNewItem(sTemp);

                foreach (EXCEPTION_CI_ITEM element in m_lsExceptionEnumStruct)
                {
                    if (element.m_sValuesize == "1")
                    {
                        sTemp = m_NamespaceFormatter.FormatStringWithIndent(string.Format(CODE_REPORT_STRUCTURE_EQUAL_BYTE_ITEM,
                                                                                            element.m_sNameCIItem));
                    }
                    else
                    {
                        sTemp = m_NamespaceFormatter.FormatStringWithIndent(string.Format(CODE_REPORT_STRUCTURE_GREATER_BYTE_ITEM,
                                                                                            element.m_sNameCIItem,
                                                                                            element.m_sValuesize));
                    }

                    m_FileProcessor.AddNewItem(sTemp);
                }
            }
        }

        private void AddFuncionFooter()
        {
            if (m_FileProcessor != null)
            {
                string sTemp = m_NamespaceFormatter.FormatStringWithIndent(string.Format( CODE_REPORT_STRUCTURE_TERMINATOR,
                                                                                            m_sStructureType));

                sTemp += m_NamespaceFormatter.GetExternCDeclareNamespaceFooter();

                m_FileProcessor.AddNewItem(sTemp);
            }
        }
        
        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((m_sOutputFolder  != null) && (m_sOutputFolder  != string.Empty) &&
                (m_sStructureType != null) && (m_sStructureType != string.Empty) &&
                (m_lsExceptionEnumStruct != null) && (m_lsExceptionEnumStruct.Count > 0))
            {
                bRet = true;
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_bFileComplete             = false;

            m_uiTempIndex               = new uint[8];
            m_uiTempIndexByteType       = 0;
            m_sOutputFolder             = null;
            m_sStructureType            = null;
            m_sSpecialItemByteType      = null;
            m_lsExceptionEnumStruct     = null; 
        }
    }
}