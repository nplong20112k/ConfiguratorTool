namespace ConfigGenerator
{
    public enum XML_ROOT_CHILD_NODE_ID_TYPE
    {
        TABLE_REFS = 0,
        PRINT_CONF,
        MESSAGES,
        PARAMETERS,
        ROOT_PAGE,
        ROOT_BASIC_PAGE,

        TOTAL_NODE_ID, // ALWAYS AT LAST!
    }

    public enum XML_TABLEREF_CHILD_NODE_ID_TYPE
    {
        ENUM_DUMMY = 0,
        HEX_RANGE_DUMMY,
        INT_RANGE_DUMMY,

        TOTAL_NODE_ID, // ALWAYS AT LAST!
    }

    public class CXMLNodeName
    {
        private string   m_RootNodeName = null;
        private string[] m_cRootChildNodeNameList = null;
        private string[] m_cTableRefChildNodeNameList = null;

        readonly static CXMLNodeName m_Instance = new CXMLNodeName();
        public static CXMLNodeName GetInstance()
        {
            return m_Instance;
        }

        public CXMLNodeName()
        {
            m_cRootChildNodeNameList = new string[(int)XML_ROOT_CHILD_NODE_ID_TYPE.TOTAL_NODE_ID];
            m_cTableRefChildNodeNameList = new string[(int)XML_TABLEREF_CHILD_NODE_ID_TYPE.TOTAL_NODE_ID];

            // Root child nodes name 
            m_RootNodeName = "device";
            m_cRootChildNodeNameList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.TABLE_REFS]        = "tableList";
            m_cRootChildNodeNameList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.PARAMETERS]        = "parameters";
            m_cRootChildNodeNameList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.PRINT_CONF]        = "printConf";
            m_cRootChildNodeNameList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.MESSAGES]          = "messages";
            m_cRootChildNodeNameList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_PAGE]         = "rootPage";
            m_cRootChildNodeNameList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_BASIC_PAGE]   = "rootBasicPage";

            // Tabler ref dummy child nodes name
            m_cTableRefChildNodeNameList[(int)XML_TABLEREF_CHILD_NODE_ID_TYPE.ENUM_DUMMY]       = "ENUM_TABLE_REF_LIST";
            m_cTableRefChildNodeNameList[(int)XML_TABLEREF_CHILD_NODE_ID_TYPE.HEX_RANGE_DUMMY]  = "HEX_RANGE_TABLE_REF_LIST";
            m_cTableRefChildNodeNameList[(int)XML_TABLEREF_CHILD_NODE_ID_TYPE.INT_RANGE_DUMMY]  = "INT_RANGE_TABLE_REF_LIST";
        }

        public string[] GetRootChildNameList()
        {
            return m_cRootChildNodeNameList;
        }

        public string GetRootChildName(XML_ROOT_CHILD_NODE_ID_TYPE ChildNodeID)
        {
            string sRet = null;
            if (ChildNodeID < XML_ROOT_CHILD_NODE_ID_TYPE.TOTAL_NODE_ID)
            {
                sRet = m_cRootChildNodeNameList[(int)ChildNodeID];
            }

            return sRet;
        }

        public string GetTableRefChildName(XML_TABLEREF_CHILD_NODE_ID_TYPE ChildNodeID)
        {
            string sRet = null;
            if (ChildNodeID < XML_TABLEREF_CHILD_NODE_ID_TYPE.TOTAL_NODE_ID)
            {
                sRet = m_cTableRefChildNodeNameList[(int)ChildNodeID];
            }

            return sRet;
        }

        public string GetRootNodeName()
        {
            return m_RootNodeName;
        }
    }
}
