namespace ConfigGenerator
{
    public enum AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE
    {
        PRODUCTS = 0,
        INTERFACE_TYPES,
        INTERFACE_CLASS,
        TAG_ITEMS,

        TOTAL_NODE_AUTO_TEST_ID,
    }

    public enum AUTO_INTERFACE_CHILD_NODE_ID_TYPE
    {
        NAME = 0,
        VALUE,
        DESC,
        CLASS,
        MODEL,
        DEFAULT,

        TOTAL_CHILDE_NODE_IN_INTERFACE_NODE,
    }

    public enum AUTO_TAG_ITEM_CHILD_NODE_ID
    {
        NUMBER = 0,
        USER_NAME,
        DESC,
        VALUES,
        NOTES,
        CATEGORIES,
        PRODUCTMASK,
        DEFAULT,

        TOTAL_CHILDE_NODE_IN_TAG_ITEM_NODE,
    }

    class CXMLAutoTestNodeName
    {
        private string m_RootName = null;
        private string[] m_RootAutoTestChildNodeNameList = null;
        private string[] m_ChildNodeOfInterfaceNodeList = null;
        private string[] m_ChildNodeOfTagItemNodeList = null;

        readonly static CXMLAutoTestNodeName m_Instance = new CXMLAutoTestNodeName();
        public static CXMLAutoTestNodeName GetInstance()
        {
            return m_Instance;
        }

        public CXMLAutoTestNodeName()
        {
            m_RootAutoTestChildNodeNameList = new string[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.TOTAL_NODE_AUTO_TEST_ID];
            m_ChildNodeOfInterfaceNodeList = new string[(int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.TOTAL_CHILDE_NODE_IN_INTERFACE_NODE];
            m_ChildNodeOfTagItemNodeList = new string[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.TOTAL_CHILDE_NODE_IN_TAG_ITEM_NODE];

            //Root Child Node Name
            m_RootName = "cfgmaster";
            m_RootAutoTestChildNodeNameList[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.PRODUCTS]        = "product";
            m_RootAutoTestChildNodeNameList[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.INTERFACE_TYPES] = "interfacetypes";
            m_RootAutoTestChildNodeNameList[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.INTERFACE_CLASS] = "interfaceclass";
            m_RootAutoTestChildNodeNameList[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.TAG_ITEMS]       = "tagitems";

            //Interface Child Node Name:
            m_ChildNodeOfInterfaceNodeList[(int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.NAME]    = "name";
            m_ChildNodeOfInterfaceNodeList[(int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.VALUE]   = "value";
            m_ChildNodeOfInterfaceNodeList[(int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.DESC]    = "desc";
            m_ChildNodeOfInterfaceNodeList[(int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.CLASS]   = "class";
            m_ChildNodeOfInterfaceNodeList[(int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.MODEL]   = "model";
            m_ChildNodeOfInterfaceNodeList[(int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.DEFAULT] = "default";

            //TagItem Child Node Name:
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.NUMBER]        = "number";
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.USER_NAME]     = "name";
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.DESC]          = "desc";
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.VALUES]        = "values";
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.NOTES]         = "note";
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.CATEGORIES]    = "categories";
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.PRODUCTMASK]   = "productmask";
            m_ChildNodeOfTagItemNodeList[(int)AUTO_TAG_ITEM_CHILD_NODE_ID.DEFAULT]       = "default";
        }

        public string[] GetRootChildNameList()
        {
            return m_RootAutoTestChildNodeNameList;
        }

        public string GetRootName()
        {
            return m_RootName;
        }

        public string[] GetInterfaceChildNodeNameList()
        {
            return m_ChildNodeOfInterfaceNodeList;
        }

        public string[] GetTagItemNodeNameList()
        {
            return m_ChildNodeOfTagItemNodeList;
        }
    }
}
