using System.Collections.Generic;
using System.Linq;
using System.Xml;


namespace ConfigGenerator
{
    class CXMLTableRefHandler : AXMLSubHandler
    {
        const string ENUM_NODE_NAME         = "table";
        const string HEX_RANGE_NODE_NAME    = "exeNumericRange";
        const string INT_RANGE_NODE_NAME    = "integerRangeTable";

        const string KEYWORD_ELEMENT        = "element";
        const string KEYWORD_NAME           = "name";
        const string KEYWORD_MIN            = "min";
        const string KEYWORD_MAX            = "max";

        private IXMLFileProcess             m_XMLFileProcess        = null;
        private CXmlHardContentProcessor    m_XMLHardContentProcess = null;

        private REF_NODE_TYPE               m_RootRefNode;
        private REF_NODE_TYPE               m_TableRefRootRefNode;
        private REF_NODE_TYPE               m_EnumSiblingRefNode;
        private REF_NODE_TYPE               m_HexRangeSiblingRefNode;
        private REF_NODE_TYPE               m_IntRangeSiblingRefNode;

        private List<XmlNode>               m_HardContentTableList = null;

        public CXMLTableRefHandler()
        {
            m_XMLFileProcess = CFactoryXmlFileProcessor.GetInstance().GetXmlFileProcessor();
            m_XMLHardContentProcess = CFactoryXmlFileProcessor.GetInstance().GetHardContentProcessor();
        }

        public override bool Initialize(List<IShareObject> oDataList)
        {
            bool bRet = false;
            ReInitProperties();

            if (m_XMLFileProcess != null)
            {
                m_RootRefNode = new REF_NODE_TYPE()
                {
                    Node = m_XMLFileProcess.XMLGetRootNode(),
                    RefPathNameList = new List<string>()
                };

                if (m_RootRefNode.Node != null)
                {
                    List<REF_NODE_TYPE> TempRefNodeList;
                    TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(CXMLNodeName.GetInstance().GetRootChildName(XML_ROOT_CHILD_NODE_ID_TYPE.TABLE_REFS), m_RootRefNode);
                    if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                    {
                        m_TableRefRootRefNode = TempRefNodeList[0];
                    }

                    if (m_TableRefRootRefNode.Node != null)
                    {
                        bRet = true;
                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(CXMLNodeName.GetInstance().GetTableRefChildName(XML_TABLEREF_CHILD_NODE_ID_TYPE.ENUM_DUMMY), m_TableRefRootRefNode);
                        if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                        {
                            m_EnumSiblingRefNode = TempRefNodeList[0];
                        }
                        else
                        {
                            bRet = false;
                        }

                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(CXMLNodeName.GetInstance().GetTableRefChildName(XML_TABLEREF_CHILD_NODE_ID_TYPE.HEX_RANGE_DUMMY), m_TableRefRootRefNode);
                        if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                        {
                            m_HexRangeSiblingRefNode = TempRefNodeList[0];
                        }
                        else
                        {
                            bRet = false;
                        }

                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(CXMLNodeName.GetInstance().GetTableRefChildName(XML_TABLEREF_CHILD_NODE_ID_TYPE.INT_RANGE_DUMMY), m_TableRefRootRefNode);
                        if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                        {
                            m_IntRangeSiblingRefNode = TempRefNodeList[0];
                        }
                        else
                        {
                            bRet = false;
                        }
                    }
                }
            }

            if (bRet == true)
            {
                foreach (IShareObject oData in oDataList)
                {
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT))
                    {
                        if (ProcessConfigInfo(oData as CXmlConfigInfoObject) == false)
                        {
                            bRet = false;
                            break;
                        }
                    }
                }
            }

            if (bRet == true)
            {
                if ((m_HardContentTableList != null) && (m_HardContentTableList.Count > 0))
                {
                    foreach (XmlNode TableElement in m_HardContentTableList)
                    {
                        if (TableElement != null)
                        {
                            XmlNode TempNode = m_XMLFileProcess.XMLImportNode(TableElement);
                            if (m_XMLFileProcess.XMLAddNode(TempNode, m_TableRefRootRefNode, true) == false)
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool DataHandling(IShareObject oDataIn)
        {
            bool bRet = false;
            if (m_XMLFileProcess != null)
            {
                if (oDataIn != null)
                {
                    List<AXmlTable> XmlTableList = null;
                    XmlTableList = (oDataIn as CIntegratedDataObject).GetTableRefList();
                
                    if ((XmlTableList != null) && (XmlTableList.Count > 0))
                    {
                        foreach (AXmlTable element in XmlTableList)
                        {
                            bRet = HandlingTableRef(element);
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            bool bRet = true;
            return bRet;
        }

        private bool HandlingTableRef(AXmlTable TableRefIn)
        {
            bool bRet = false;
            if (TableRefIn != null)
            {
                switch (TableRefIn.GetXmlTableType())
                {
                    case AXmlTable.XML_TABLE_TYPE.XML_TABLE_ENUM:
                        bRet = HandlingTableRefEnum(TableRefIn);
                        break;

                    case AXmlTable.XML_TABLE_TYPE.XML_TABLE_HEX:
                        bRet = HandlingTableRefHexRange(TableRefIn);
                        break;

                    case AXmlTable.XML_TABLE_TYPE.XML_TABLE_INT:
                        bRet = HandlingTableRefIntRange(TableRefIn); 
                        break;

                    default:
                        break;
                }
            }

            return bRet;
        }

        private bool HandlingTableRefEnum(AXmlTable TableRefIn)
        {
            bool bRet = false;
            XmlNode NewNode = null;
            List<CTableRefSelection.VALUE_TYPE> TempValueList = null;

            if ((TableRefIn != null) && (TableRefIn.GetXmlTableType() == AXmlTable.XML_TABLE_TYPE.XML_TABLE_ENUM))
            {
                NewNode = m_XMLFileProcess.XMLCreateNode(ENUM_NODE_NAME);
                if (NewNode != null)
                {
                    (NewNode as XmlElement).SetAttribute(KEYWORD_NAME, TableRefIn.GetName());
                    TempValueList = (TableRefIn as CEnumTable).GetTableRefValueList();

                    if ((TempValueList != null) && (TempValueList.Count > 0))
                    {
                        foreach (CTableRefSelection.VALUE_TYPE element in TempValueList)
                        {
                            if ((element.sDescript != null) && (element.sValue != null))
                            {
                                XmlNode TempNode = m_XMLFileProcess.XMLCreateNode(KEYWORD_ELEMENT);
                                (TempNode as XmlElement).SetAttribute(KEYWORD_NAME, element.sDescript);
                                (TempNode as XmlElement).InnerText = element.sValue;

                                NewNode.AppendChild(TempNode);
                            }
                        }
                        
                        bRet = m_XMLFileProcess.XMLInsertNode(NewNode, m_TableRefRootRefNode, m_EnumSiblingRefNode);
                        m_EnumSiblingRefNode.Node = NewNode;
                    }
                }
            }

            return bRet;
        }

        private bool HandlingTableRefHexRange(AXmlTable TableRefIn)
        {
            bool bRet = false;
            XmlNode NewNode = null;

            if ((TableRefIn != null) && (TableRefIn.GetXmlTableType() == AXmlTable.XML_TABLE_TYPE.XML_TABLE_HEX) )
            {
                NewNode = m_XMLFileProcess.XMLCreateNode(HEX_RANGE_NODE_NAME);
                if (NewNode != null)
                {
                    (NewNode as XmlElement).SetAttribute(KEYWORD_NAME, TableRefIn.GetName());
                    (NewNode as XmlElement).SetAttribute(KEYWORD_MIN, (TableRefIn as CHexRangeTable).GetTableRefMinValue());
                    (NewNode as XmlElement).SetAttribute(KEYWORD_MAX,(TableRefIn as CHexRangeTable).GetTableRefMaxValue());
                    
                    bRet = m_XMLFileProcess.XMLInsertNode(NewNode, m_TableRefRootRefNode, m_HexRangeSiblingRefNode);
                    m_HexRangeSiblingRefNode.Node = NewNode;
                }
            }

            return bRet;
        }

        private bool HandlingTableRefIntRange(AXmlTable TableRefIn)
        {
            bool bRet = false;
            XmlNode NewNode = null;

            if ((TableRefIn != null) && (TableRefIn.GetXmlTableType() == AXmlTable.XML_TABLE_TYPE.XML_TABLE_INT))
            {
                NewNode = m_XMLFileProcess.XMLCreateNode(INT_RANGE_NODE_NAME);
                if (NewNode != null)
                {
                    (NewNode as XmlElement).SetAttribute(KEYWORD_NAME, TableRefIn.GetName());
                    (NewNode as XmlElement).SetAttribute(KEYWORD_MIN, (TableRefIn as CIntRangeTable).GetTableRefMinValue());
                    (NewNode as XmlElement).SetAttribute(KEYWORD_MAX, (TableRefIn as CIntRangeTable).GetTableRefMaxValue());
                    
                    bRet = m_XMLFileProcess.XMLInsertNode(NewNode, m_TableRefRootRefNode, m_IntRangeSiblingRefNode);
                    m_IntRangeSiblingRefNode.Node = NewNode;
                }
            }

            return bRet;
        }

        private bool ProcessConfigInfo(CXmlConfigInfoObject oConfigData)
        {
            bool bRet = false;

            if (oConfigData != null)
            {
                XmlNode GroupConfigInfo = oConfigData.GetConfigFeatureInfo(CXMLConfigKeywords.KW_CONFIG_GROUP);
                if ((GroupConfigInfo != null) && (GroupConfigInfo.HasChildNodes == true))
                {
                    m_HardContentTableList = new List<XmlNode>();

                    // update common table hard list
                    XmlNode TableContent = m_XMLHardContentProcess.GetTableList(CXMLConfigKeywords.KW_VALUE_COMMON);
                    if ((TableContent != null) && (TableContent.HasChildNodes == true))
                    {
                        XmlNode TempNode = TableContent.CloneNode(true);
                        m_HardContentTableList.AddRange(TempNode.ChildNodes.Cast<XmlNode>());
                    }

                    // update table hard list
                    foreach (XmlNode ElementNode in GroupConfigInfo.ChildNodes)
                    {
                        if (ElementNode != null)
                        {
                            string sGroupName = (ElementNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_NAME);
                            TableContent = m_XMLHardContentProcess.GetTableList(sGroupName);
                            if ((TableContent != null) && (TableContent.HasChildNodes == true))
                            {
                                XmlNode TempNode = TableContent.CloneNode(true);
                                m_HardContentTableList.AddRange(TempNode.ChildNodes.Cast<XmlNode>());
                            }
                            else
                            {
                                // missing detected !! TODO
                            }
                        }
                    }
                    bRet = true;
                }
            }

            return bRet;
        }

        private void ReInitProperties()
        {
            m_HardContentTableList = null;
        }
    }
}
