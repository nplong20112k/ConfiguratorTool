namespace ConfigGenerator
{
    class CCodeDeltaConstTableHandler : ACodeSubHandler 
    {        
        private const string CODE_REPORT_DELTA_TABLE_FILE_NAME      = "ConfigItemDeltaTable.cpp";

        private const string EUGENE_TYPE                            = "eugene_";
        private const string BOLOGNA_TYPE                           = "bologna_";
        private const string EUGENE_SPECIAL_TYPE                    = "eugene_special_";
        private const string BOLOGNA_SPECIAL_TYPE                   = "bologna_special_";

        const string DELTA_TABLE_FORMAT =   "const {0,-30} {1}[] = \n" +
                                            "{{\n" +
                                            "{2}" +
                                            "}};\n" +
                                            "\n";

        private string[] m_sContentNormalDeltaTable     = null;
        private string[] m_sContentSpecialDeltaTable    = null;

        private struct INTERFACE_MAP
        {
            public string m_sInterfaceName;
            public string m_sEugeneName;
            public string m_sBolognaName;
            public string m_sEugeneSpecialName;
            public string m_sBolognaSpecialName;
            //public string m_sContentNormalDeltaTable;
            //public string m_sContentSpecialDeltaTable;
        }

        private List<INTERFACE_MAP> m_lsInterfaceMap        = null;
        private string              m_sOutputFolder         = null;
        ACodeFileProcessor          m_FileProcessor         = null;
   
        private string   m_sNormalDeltaTableName            = null;
        private string   m_sSpecialDeltaTableName           = null;
        private List<INTERFACE_CLASS> m_lsInterfaceClass    = null;       

        private const string CODE_REPORT_DELTA_TABLE_NORMAL_ITEM    = "    {{ {0,-54},0x{1} }},\n";
        private const string CODE_REPORT_DELTA_TABLE_SPECIAL_ITEM   = "    {{ {0,-54},\"{1}\" }},\n";
        private const string DELTA_SPECIAL_ITEM                     = "\"{0}\"";
        private const char KW_SPLIT_CHAR                            = '_';
      
        public CCodeDeltaConstTableHandler()
            :base()
        {
            m_FileProcessor = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_DELTA_TABLE_FILE_PROCESSOR);

            if (m_FileProcessor == null)
            {
                return;
            }
        }

        public override bool DataHandling(object oDataIn)
        {
            if (m_FileProcessor != null)
            {
                if (oDataIn != null)
                {                    
                    CDeltaConstTable DeltaTableObj = (oDataIn as CIntegratedDataObject).GetDeltaTable();
                    if (DeltaTableObj != null)
                    {
                        string sNameCIItem                          = DeltaTableObj.GetCIItemName();
                        string sSizeValue                           = DeltaTableObj.GetSizeValue();
                        List<DELTA_TABLE_ITEM> lsDeltaTableGroup    = DeltaTableObj.GetDeltaTableGroup();
                   
                        foreach (DELTA_TABLE_ITEM element in lsDeltaTableGroup)
                        {
                            for (int i = 0; i < m_lsInterfaceMap.Count; i++)
                            {
                                if (element.m_sInterfaceName == m_lsInterfaceMap[i].m_sInterfaceName)
                                {
                                    if (sSizeValue == "1")
                                    {
                                        m_sContentNormalDeltaTable[i] += string.Format(CODE_REPORT_DELTA_TABLE_NORMAL_ITEM,
                                                                                        sNameCIItem,
                                                                                        element.m_sValue.PadLeft(2,'0'));
                                        // fail syntax
                                        //m_lsInterfaceMap[i].m_sContentNormalDeltaTable += string.Format(CODE_REPORT_DELTA_TABLE_NORMAL_ITEM,
                                        //                                                            sNameCIItem,
                                        //                                                            element.m_sValue);
                                    }
                                    else
                                    {
                                        m_sContentSpecialDeltaTable[i] += string.Format(CODE_REPORT_DELTA_TABLE_SPECIAL_ITEM,
                                                                                        sNameCIItem,
                                                                                        element.m_sValue);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            return true;
        }

        public override bool Initialize(IShareObject oData)
        {
            bool bRet = false;
            m_sOutputFolder = (oData as ICommonProperties).GetOutputFolder();

            if ((m_FileProcessor != null) && (VerifyConfigInfo() == true))
            {
                m_FileProcessor.CreateNewFile(m_sOutputFolder + Path.DirectorySeparatorChar + CODE_REPORT_DELTA_TABLE_FILE_NAME);
                m_lsInterfaceClass          = (oData as CSourceInfoObject).GetInterfaceClass();
                m_sNormalDeltaTableName     = (oData as CSourceInfoObject).GetNormalDeltaTableName();
                m_sSpecialDeltaTableName    = (oData as CSourceInfoObject).GetSpecialDeltaTableName();

                CreateDeltaTableName(m_lsInterfaceClass);
                m_sContentNormalDeltaTable  = new string[m_lsInterfaceMap.Count];
                m_sContentSpecialDeltaTable = new string[m_lsInterfaceMap.Count];
                   
                bRet = true;
            }

            return bRet;
        }

        public override bool Finalize()
        {
            string sTempNormalItem  = null;
            string sTempSpecialItem = null;
            string sOutput          = null;

            for (int i = 0; i < m_lsInterfaceMap.Count ; i++)
            {
                sTempNormalItem += string.Format(   DELTA_TABLE_FORMAT,
                                                    m_sNormalDeltaTableName,
                                                    m_lsInterfaceMap[i].m_sEugeneName,
                                                    m_sContentNormalDeltaTable[i]);

                sTempSpecialItem += string.Format(  DELTA_TABLE_FORMAT,
                                                    m_sSpecialDeltaTableName,
                                                    m_lsInterfaceMap[i].m_sEugeneSpecialName,
                                                    m_sContentSpecialDeltaTable[i]);
            }

            sOutput = sTempNormalItem + sTempSpecialItem;
            m_FileProcessor.AddNewItem(sOutput);
            return true;
        }

        private bool VerifyConfigInfo()
        {
            bool bRet = false;

            if ((m_sOutputFolder != null) && (m_sOutputFolder != string.Empty))
            {
                bRet = true;
            }

            return bRet;
        }

        private void CreateDeltaTableName(List<INTERFACE_CLASS> lsInterfaceClass)
        {
            m_lsInterfaceMap = new List<INTERFACE_MAP>();
            INTERFACE_MAP MemberInterfaceMap = new INTERFACE_MAP()
            {
                m_sInterfaceName            = null,
                m_sEugeneName               = null,
                m_sBolognaName              = null,
                m_sEugeneSpecialName        = null,
                m_sBolognaSpecialName       = null,
            };

            foreach (INTERFACE_CLASS elementClass in lsInterfaceClass)
            {
                foreach (INTERFACE_CLASS_MEMBER elementMember in elementClass.m_lsInterfaceMember)
                {
                    if (elementMember.m_bInterfaceValid)
                    {
                        int tempIndex           = elementMember.m_sInterfaceName.IndexOf(KW_SPLIT_CHAR);
                        string sNameInterface   = elementMember.m_sInterfaceName.Substring((tempIndex + 1)).ToLower();

                        MemberInterfaceMap.m_sInterfaceName         = elementMember.m_sInterfaceName;
                        MemberInterfaceMap.m_sEugeneName            = EUGENE_TYPE + sNameInterface;
                        MemberInterfaceMap.m_sEugeneSpecialName     = EUGENE_SPECIAL_TYPE + sNameInterface;
                        MemberInterfaceMap.m_sBolognaName           = BOLOGNA_TYPE + sNameInterface;
                        MemberInterfaceMap.m_sBolognaSpecialName    = BOLOGNA_SPECIAL_TYPE + sNameInterface;

                        m_lsInterfaceMap.Add(MemberInterfaceMap);
                    }
                }
            }
        }
    }
}