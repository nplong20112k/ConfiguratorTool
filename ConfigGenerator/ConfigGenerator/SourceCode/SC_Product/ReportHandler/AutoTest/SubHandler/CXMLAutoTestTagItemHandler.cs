using System.Collections.Generic;
using System.Xml;

namespace ConfigGenerator
{
    public enum ATTRIBUTES_OF_VALUE_NODE
    {
        KW_ATTRIBUTE_VALUES_TYPE =0,
        KW_ATTRIBUTE_VALUE_SIZE,
        KW_ATTRIBUTE_VALUE_UNITS,

        TOTAL_ATTRIBUTE_OF_VALUE_NODE,
    }

    class CXMLAutoTestTagItemHandler : AXMLSubHandler
    {
        private IXMLFileProcess m_XmlFileProcessor = null;

        private List<CIntegrateParamObject> m_lsIntegrateParamObject = null;
        private List<AXmlTable> m_lsXmlTable = null;
        private CAutoTestParamObject m_AutoTestParamObject = null;

        private string[] m_cRootChildNodeNameList = null;
        private string[] m_cTagItemChildNodeNameList = null;
        private string[] m_AttributeOfValueNameList = null;

        private REF_NODE_TYPE m_TagItemRefNode;

        private const string KW_ROOT_NODE_OF_TAGITEMS = "tagitem";

        private const string KW_VALUE_ROOT_NODE = "value";
        private const string KW_VALUE_ATTRIBUTE = "setting";
        private const string KW_VALUE_TYPE_ENUM = "Enumeration";
        private const string KW_VAUE_TYPE_RANGE = "Range";

        private const string KW_NAME_OF_MINIMUM_NODE = "minimum";
        private const string KW_NAME_OF_MAXIMUM_NODE = "maximum";
        private const string KW_NAME_OF_STEP_NODE    = "increment";

        private const string KW_CATEGORY_ROOT_NODE  = "category";
        private const string KW_CATEGORY_CHILD_NODE = "subcategory";
        private const string KW_CATEGORY_ATTRIBUTE  = "primary";

        private const string KW_CLASS_DEFAULT_PREFIX = "Class-";

        public CXMLAutoTestTagItemHandler()
        {
            m_XmlFileProcessor = CFactoryXmlFileProcessor.GetInstance().GetAutoTestXmlFileProcessor();
            m_cRootChildNodeNameList = CXMLAutoTestNodeName.GetInstance().GetRootChildNameList();
            m_cTagItemChildNodeNameList = CXMLAutoTestNodeName.GetInstance().GetTagItemNodeNameList();

            m_AttributeOfValueNameList = new string[(int)ATTRIBUTES_OF_VALUE_NODE.TOTAL_ATTRIBUTE_OF_VALUE_NODE];
            m_AttributeOfValueNameList[(int)ATTRIBUTES_OF_VALUE_NODE.KW_ATTRIBUTE_VALUES_TYPE]  = "type";
            m_AttributeOfValueNameList[(int)ATTRIBUTES_OF_VALUE_NODE.KW_ATTRIBUTE_VALUE_SIZE]   = "size";
            m_AttributeOfValueNameList[(int)ATTRIBUTES_OF_VALUE_NODE.KW_ATTRIBUTE_VALUE_UNITS]  = "uint";
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            bool bRet = false;
            if ((oDataIn != null) && (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_INTEGRATED_DATA_OBJECT))
            {
                if (UpdatePreProcess((CIntegratedDataObject)oDataIn))
                {
                    bRet = UpdateConfigItemPart();
                }
            }
            return bRet;
        }

        private bool UpdatePreProcess(CIntegratedDataObject oDataIn)
        {
            bool bRet = false;
            m_lsIntegrateParamObject = oDataIn.GetParameterList();
            if (m_lsIntegrateParamObject != null)
            {
                m_AutoTestParamObject = oDataIn.GetAutoTestParamObject();
                m_lsXmlTable = oDataIn.GetTableRefAutoTestList();
                bRet = true;
            }
            return bRet;
        }

        private bool UpdateConfigItemPart()
        {
            bool bRet = false;

            if ((m_TagItemRefNode.RefPathNameList != null) && (m_TagItemRefNode.Node != null))
            {
                XmlNode newNode = null;

                newNode = CreateTagItemNode();
                bRet = m_XmlFileProcessor.XMLAddNode(newNode, m_TagItemRefNode);
            }

            return bRet;
        }

        private XmlNode CreateTagItemNode()
        {
            XmlNode tempNode = null;
            XmlNode childNode = null;
            string[] s_TagItemChildNodeNameList = CXMLAutoTestNodeName.GetInstance().GetTagItemNodeNameList();

            string s_TagCode = m_AutoTestParamObject.GetTagCode();
            string s_TagName = m_AutoTestParamObject.GetTagName();
            string s_ValueType = m_AutoTestParamObject.GetAutoTestParamValue();
            string s_ValueSize = m_AutoTestParamObject.GetValueSizeByte();

            List<string> ls_Category = m_AutoTestParamObject.GetCategoryList();
            string s_DefaultValue = m_AutoTestParamObject.GetDefaultValue();
            List<CIClassDefault> lsClassDefaults = m_AutoTestParamObject.GetListCICLassDefault();

            tempNode = m_XmlFileProcessor.XMLCreateNode(KW_ROOT_NODE_OF_TAGITEMS);
            for (int i = 0; i < (int)AUTO_TAG_ITEM_CHILD_NODE_ID.TOTAL_CHILDE_NODE_IN_TAG_ITEM_NODE; i++)
            {
                childNode = m_XmlFileProcessor.XMLCreateNode(s_TagItemChildNodeNameList[i]);
                switch (i)
                {
                    case (int)AUTO_TAG_ITEM_CHILD_NODE_ID.NUMBER:
                        childNode.InnerText = s_TagCode;
                        break;

                    case (int)AUTO_TAG_ITEM_CHILD_NODE_ID.USER_NAME:
                        childNode.InnerText = s_TagName;
                        break;

                    case (int)AUTO_TAG_ITEM_CHILD_NODE_ID.VALUES:
                        // Create attribute
                        for (int j = 0; j < (int)ATTRIBUTES_OF_VALUE_NODE.TOTAL_ATTRIBUTE_OF_VALUE_NODE; j++)
                        {
                            string AttributeName = m_AttributeOfValueNameList[j];
                            switch (j)
                            {
                                case (int)ATTRIBUTES_OF_VALUE_NODE.KW_ATTRIBUTE_VALUES_TYPE:
                                    (childNode as XmlElement).SetAttribute(AttributeName, s_ValueType);
                                    break;
                                case (int)ATTRIBUTES_OF_VALUE_NODE.KW_ATTRIBUTE_VALUE_SIZE:
                                    (childNode as XmlElement).SetAttribute(AttributeName, s_ValueSize);
                                    break;
                                case (int)ATTRIBUTES_OF_VALUE_NODE.KW_ATTRIBUTE_VALUE_UNITS:
                                    (childNode as XmlElement).SetAttribute(AttributeName, "");
                                    break;
                            }
                        }
                        // Create child node
                        if ((m_lsXmlTable != null) && (m_lsXmlTable).Count > 0)
                        {
                            foreach (AXmlTable XmlTableElement in m_lsXmlTable)
                            {
                                if (s_ValueType == KW_VALUE_TYPE_ENUM)
                                {
                                    List<CTableRefSelection.VALUE_TYPE> ls_ValueList = (XmlTableElement as CEnumTable).GetTableRefValueList();
                                    for (int j = 0; j < ls_ValueList.Count; j++)
                                    {
                                        childNode.AppendChild(HandleEnumValueNode(ls_ValueList[j]));
                                    }
                                }
                                else if (s_ValueType == KW_VAUE_TYPE_RANGE)
                                {
                                    string sMinValue = null;
                                    string sMaxValue = null;
                                    string sStepValue = m_AutoTestParamObject.GetStepRangeValue();

                                    XmlNode minNode = m_XmlFileProcessor.XMLCreateNode(KW_NAME_OF_MINIMUM_NODE);
                                    XmlNode maxNode = m_XmlFileProcessor.XMLCreateNode(KW_NAME_OF_MAXIMUM_NODE);
                                    XmlNode incNode = m_XmlFileProcessor.XMLCreateNode(KW_NAME_OF_STEP_NODE);

                                    if ((XmlTableElement.GetXmlTableType() == AXmlTable.XML_TABLE_TYPE.XML_TABLE_INT))
                                    {
                                        sMinValue = (XmlTableElement as CIntRangeTable).GetTableRefMinValue();
                                        sMaxValue = (XmlTableElement as CIntRangeTable).GetTableRefMaxValue();
                                    }
                                    else if ((XmlTableElement.GetXmlTableType() == AXmlTable.XML_TABLE_TYPE.XML_TABLE_HEX))
                                    {
                                        sMinValue = (XmlTableElement as CHexRangeTable).GetTableRefMinValue();
                                        sMaxValue = (XmlTableElement as CHexRangeTable).GetTableRefMaxValue();
                                    }
                                    if ((sMinValue != null) && (sMaxValue != null))
                                    {
                                        minNode.InnerText = sMinValue;
                                        childNode.AppendChild(minNode);
                                        maxNode.InnerText = sMaxValue;
                                        childNode.AppendChild(maxNode);
                                        incNode.InnerText = sStepValue;
                                        childNode.AppendChild(incNode);
                                    }
                                }
                            }
                        }
                        break;

                    case (int)AUTO_TAG_ITEM_CHILD_NODE_ID.CATEGORIES:
                        if (ls_Category != null)
                        {
                            XmlNode CategoryNode = m_XmlFileProcessor.XMLCreateNode(KW_CATEGORY_ROOT_NODE);
                            XmlNode SubcategoryNode = m_XmlFileProcessor.XMLCreateNode(KW_CATEGORY_CHILD_NODE);
                            for (int j = 0; j < ls_Category.Count; j++)
                            {
                                string CategoryElement = ls_Category[j];
                                string CategoryPrimary = CategoryElement.Split('.')[0];
                                string SubCategory = CategoryElement.Replace(CategoryPrimary + ".", "");
                                SubcategoryNode.InnerText = SubCategory;
                                (CategoryNode as XmlElement).SetAttribute(KW_CATEGORY_ATTRIBUTE, CategoryPrimary);
                                CategoryNode.AppendChild(SubcategoryNode);
                                childNode.AppendChild(CategoryNode);
                            }
                        }
                        break;

                    case (int)AUTO_TAG_ITEM_CHILD_NODE_ID.DEFAULT:
                        childNode.InnerText = s_DefaultValue;
                        break;

                    default:
                        childNode.InnerText = "";
                        break;
                }
                tempNode.AppendChild(childNode);
            }
            if (lsClassDefaults != null)
            {
                foreach(CIClassDefault CIClassDefaultElement in lsClassDefaults)
                {
                    uint index = CIClassDefaultElement.m_uiClassNumber;
                    XmlNode ClassValueNode = m_XmlFileProcessor.XMLCreateNode(KW_CLASS_DEFAULT_PREFIX + index.ToString());
                    if (CIClassDefaultElement.sDefaultValue != null)
                    {
                        ClassValueNode.InnerText = CIClassDefaultElement.sDefaultValue;
                    }
                    else
                    {
                        ClassValueNode.InnerText = s_DefaultValue;
                    }
                    tempNode.AppendChild(ClassValueNode);
                }
            }
            return tempNode;
        }

        private XmlNode HandleEnumValueNode(CTableRefSelection.VALUE_TYPE ValueType)
        {
            XmlNode SubNewNode = m_XmlFileProcessor.XMLCreateNode(KW_VALUE_ROOT_NODE);
            (SubNewNode as XmlElement).SetAttribute(KW_VALUE_ATTRIBUTE, "");
            if ((ValueType.sDescript != null) && (ValueType.sValue != null))
            {
                SubNewNode.Attributes[KW_VALUE_ATTRIBUTE].Value = ValueType.sValue;
                SubNewNode.InnerText = ValueType.sDescript;
            }
            return SubNewNode;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            bool bRet = true;
            //TO DO
            return bRet;
        }

        public override bool Initialize(List<IShareObject> oDataList)
        {
            bool bRet = false;

            if (m_XmlFileProcessor != null)
            {
                List<REF_NODE_TYPE> lsReturnNode = null;
                REF_NODE_TYPE RefNode = new REF_NODE_TYPE()
                {
                    Node = m_XmlFileProcessor.XMLGetRootNode(),
                    RefPathNameList = new List<string>()
                };

                if ((null != RefNode.Node) && (RefNode.Node.HasChildNodes))
                {
                    lsReturnNode = m_XmlFileProcessor.XMLSearchNodes(m_cRootChildNodeNameList[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.TAG_ITEMS], RefNode);
                    if ((lsReturnNode != null) && (1 == lsReturnNode.Count))
                    {
                        m_TagItemRefNode = lsReturnNode[0];
                        bRet = true;
                    }
                }
            }
            return bRet;
        }
    }
}
