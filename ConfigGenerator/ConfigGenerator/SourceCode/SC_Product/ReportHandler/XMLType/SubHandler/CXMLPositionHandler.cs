using System;
using System.Collections.Generic;
using System.Xml;

namespace ConfigGenerator
{
    class CXMLPositionHandler : AXMLSubHandler
    {
        private const int    ROOT_PAGE_IDX          = 0;
        private const int    START_PAGE_IDX         = 1;

        private const string CONFIG_PAGE_NAME       = "Configuration";

        private const string PAGE_NODE_NAME         = "page";
        private const string PAGE_NODE_TAIL         = ".pnl";
        private const string POSITION_NODE_NAME     = "field";
        private const string LABEL_GROUP_NAME       = "label";

        private const string MESSAGE_NODE_NAME      = "message";
        private const string PAGE_MESSAGE_NODE_NAME = "pageMessage";

        private const string KEYWORD_NAME           = "name";
        private const string KEYWORD_TITLE          = "title";
        private const string KEYWORD_TOCID          = "tocId";
        private const string KEYWORD_TEXT           = "text";
        private const string KEYWORD_INDEX          = "index";
        private const string KEYWORD_PANEL          = "panel";

        private const string FM_SELECTION_NODES     = "//{0}//{1}[@{2}='{3}']";
        private const string FW_SELECTION_NAME_NODES = "//{0}//{1}";
        
        private IXMLFileProcess             m_XMLFileProcess        = null;
        private CXmlHardContentProcessor    m_XMLHardContentProcess = null;

        private REF_NODE_TYPE               m_RootRefNode;
        private REF_NODE_TYPE               m_BasisRootRefNode;
        private REF_NODE_TYPE               m_ExpertRootRefNode;

        private List<XmlNode>               m_HardContentPositionBasicList  = null;
        private List<XmlNode>               m_HardContentPositionExpertList = null;

        public CXMLPositionHandler()
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
                    bRet = true;
                    List<REF_NODE_TYPE> TempRefNodeList;
                    TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(CXMLNodeName.GetInstance().GetRootChildName(XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_BASIC_PAGE), m_RootRefNode);
                    if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                    {
                        m_BasisRootRefNode = TempRefNodeList[0];
                    }
                    else
                    {
                        bRet = false;
                    }

                    TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(CXMLNodeName.GetInstance().GetRootChildName(XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_PAGE), m_RootRefNode);
                    if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                    {
                        m_ExpertRootRefNode = TempRefNodeList[0];
                    }
                    else
                    {
                        bRet = false;
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
                        }
                    }
                }
            }

            if (bRet == true)
            {
                if (HandlingHardPosition(m_BasisRootRefNode, m_HardContentPositionBasicList, true) == false)
                {
                    bRet = false;
                }

                if (HandlingHardPosition(m_ExpertRootRefNode, m_HardContentPositionExpertList, true) == false)
                {
                    bRet = false;
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
                    CIntegratedPositionObject oPositionIntegratedObject = null;

                    oPositionIntegratedObject = (oDataIn as CIntegratedDataObject).GetPosition();
                    if (oPositionIntegratedObject != null)
                    {
                        List<POSTION_INTEGRATED_TYPE> oPositionDataList = oPositionIntegratedObject.GetPositionDataList();
                        if ((oPositionDataList != null) && (oPositionDataList.Count > 0))
                        {
                            bRet = true;
                            foreach (POSTION_INTEGRATED_TYPE element in oPositionDataList)
                            {
                                if (RootPageHandling(m_ExpertRootRefNode, element) == false)
                                {
                                    bRet = false;
                                }

                                if(oPositionIntegratedObject.GetRootPage() == CIntegratedPositionObject.ROOT_PAGE_TYPE.ROOT_PAGE_BASIC)
                                {
                                    if (RootPageHandling(m_BasisRootRefNode, element) == false)
                                    {
                                        bRet = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            bool bRet = true;

            if (CleanEmptyLabelAndPage(m_BasisRootRefNode) == false)
            {
                bRet = false;
            }

            if (CleanEmptyLabelAndPage(m_ExpertRootRefNode) == false)
            {
                bRet = false;
            }

            return bRet;
        }

        private bool HandlingHardPosition(REF_NODE_TYPE PageRefNode, List<XmlNode> HardContentPositionList, bool bFlagRevertOrder = false)
        {
            bool bRet = false;

            if ((HardContentPositionList != null) && (HardContentPositionList.Count > 0))
            {
                bRet = true;
                string sRootInfo = string.Format( FM_SELECTION_NODES, 
                                                  PageRefNode.Node.Name,
                                                  PAGE_NODE_NAME,
                                                  KEYWORD_TITLE,
                                                  CONFIG_PAGE_NAME );

                XmlNode ConfigurationPage = PageRefNode.Node.SelectSingleNode(sRootInfo);

                if (ConfigurationPage != null)
                {
                    if (bFlagRevertOrder == true)
                    {
                        HardContentPositionList.Reverse();
                    }

                    foreach (XmlNode ElementHardPosition in HardContentPositionList)
                    {
                        if ((ElementHardPosition != null) && (ElementHardPosition.Name == MESSAGE_NODE_NAME))
                        {
                            REF_NODE_TYPE ParentNodeRet = new REF_NODE_TYPE() { Node = ConfigurationPage };

                            XmlNode MessagePositionNode = m_XMLFileProcess.XMLCreateNode(PAGE_MESSAGE_NODE_NAME);
                            string sMessageName = (ElementHardPosition as XmlElement).GetAttribute(KEYWORD_NAME);
                            (MessagePositionNode as XmlElement).SetAttribute(KEYWORD_NAME, sMessageName);

                            if (m_XMLFileProcess.XMLAddNode(MessagePositionNode, ParentNodeRet, true) == false)
                            {
                                bRet = false;
                            }
                        }
                        else if ((ElementHardPosition != null) && (ElementHardPosition.Name == PAGE_NODE_NAME))
                        {
                            string sHardContentNodeTitle = (ElementHardPosition as XmlElement).GetAttribute(KEYWORD_TITLE);

                            string sRefInfo = string.Format( FM_SELECTION_NODES, 
                                                             PageRefNode.Node.Name,
                                                             PAGE_NODE_NAME,
                                                             KEYWORD_TITLE,
                                                             sHardContentNodeTitle);

                            XmlNodeList PageRefList = ConfigurationPage.SelectNodes(sRefInfo);
                            if ((PageRefList != null) && (PageRefList.Count > 0))
                            {
                                foreach (XmlNode ElementNode in PageRefList)
                                {
                                    REF_NODE_TYPE ParentNodeRet = new REF_NODE_TYPE() { Node = ElementNode };

                                    foreach (XmlNode HardContentChildNode in ElementHardPosition.ChildNodes)
                                    {
                                        XmlNode ContentNode = m_XMLFileProcess.XMLImportNode(HardContentChildNode);
                                        if (m_XMLFileProcess.XMLAddNode(ContentNode, ParentNodeRet) == false)
                                        {
                                            bRet = false;
                                        }
                                    }

                                    bool bCompare = ElementNode.Attributes.Equals(ElementHardPosition.Attributes);
                                    if (bCompare == false)
                                    {
                                        CopyMissingAttribute(ElementHardPosition, ElementNode);
                                    }
                                }
                            }
                            else
                            {
                                XmlNode ContentNode = m_XMLFileProcess.XMLImportNode(ElementHardPosition);
                                REF_NODE_TYPE ParentNodeRet = new REF_NODE_TYPE() { Node = ConfigurationPage };

                                if (m_XMLFileProcess.XMLAddNode(ContentNode, ParentNodeRet) == false)
                                {
                                    bRet = false;
                                }
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        private void CopyMissingAttribute(XmlNode XmlNodeSource, XmlNode XmlNodeDest)
        {
            XmlAttribute[] xmlAttribute = new XmlAttribute[10];
            int index = 0;
            XmlNodeSource.Attributes.CopyTo(xmlAttribute, index);
            foreach (XmlAttribute newAttribute in xmlAttribute)
            {
                if (newAttribute != null)
                {
                    if (XmlNodeDest.Attributes.GetNamedItem(newAttribute.Name.ToString()) == null)
                    {
                        XmlNodeDest.Attributes.SetNamedItem(newAttribute);
                    }
                }
            }
        }

        private bool RootPageHandling(REF_NODE_TYPE RootPageRefNode, POSTION_INTEGRATED_TYPE oPositionData)
        {
            bool bRet = false;

            XmlNode             PositionNode = null;
            List<REF_NODE_TYPE> TempRefNodeList;
            REF_NODE_TYPE       TempParentRefNode;
            XmlNode             TempPageNode = null;

            if ((RootPageRefNode.Node != null) && (oPositionData.m_sPositionPath != null) && (oPositionData.m_sPositionPath.Count > START_PAGE_IDX))
            {
                TempParentRefNode = RootPageRefNode;
                PositionNode = m_XMLFileProcess.XMLCreateNode(POSITION_NODE_NAME);
                if (PositionNode != null)
                {
                    (PositionNode as XmlElement).SetAttribute(KEYWORD_NAME, oPositionData.m_sFieldName);
                    (PositionNode as XmlElement).SetAttribute(KEYWORD_INDEX, oPositionData.m_sIndex);
                    foreach (string element in oPositionData.m_sPositionPath)
                    {
                        if ((element != null) && (element != ""))
                        {
                            List<string[]> sAttributes = new List<string[]> { new string[] { KEYWORD_TITLE, element } };
                            TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(PAGE_NODE_NAME, TempParentRefNode, sAttributes);

                            if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                            {
                                TempParentRefNode = TempRefNodeList[0];
                            }
                            else if ((TempRefNodeList == null) || (TempRefNodeList.Count == 0))
                            {
                                TempPageNode = m_XMLFileProcess.XMLCreateNode(PAGE_NODE_NAME);

                                string sPageNameProcess = element.Replace(" ", string.Empty);
                                // sPageNameProcess = Char.ToLowerInvariant(sPageNameProcess[0]) + sPageNameProcess.Substring(1);

                                if ((oPositionData.m_sModelName != null) && (oPositionData.m_sModelName != string.Empty))
                                {
                                    sPageNameProcess = oPositionData.m_sModelName.Replace(" ", string.Empty) + "_" + sPageNameProcess;
                                }

                                (TempPageNode as XmlElement).SetAttribute(KEYWORD_NAME, sPageNameProcess + PAGE_NODE_TAIL);
                                (TempPageNode as XmlElement).SetAttribute(KEYWORD_TITLE, element);

                                if ((oPositionData.m_sPositionPath.IndexOf(element) == CPositionIntegrator.MODEL_IDX) &&
                                    ((oPositionData.m_sModelName == null) || (oPositionData.m_sModelName == string.Empty)))
                                {
                                    m_XMLFileProcess.XMLAddNode(TempPageNode, TempParentRefNode, true /*add first!*/);
                                }
                                else
                                {
                                    m_XMLFileProcess.XMLAddNode(TempPageNode, TempParentRefNode);
                                }
                                
                                TempParentRefNode.RefPathNameList.Add(TempParentRefNode.Node.Name);
                                TempParentRefNode.Node = TempPageNode;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    if ((oPositionData.m_sGroup != null) && (oPositionData.m_sGroup != string.Empty))
                    {
                        List<string[]> sAttributes = new List<string[]> { new string[] { KEYWORD_TEXT, oPositionData.m_sGroup } };
                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(LABEL_GROUP_NAME, TempParentRefNode, sAttributes);

                        if ((TempRefNodeList == null) || (TempRefNodeList.Count == 0))
                        {
                            XmlNode LabelGroupNode = m_XMLFileProcess.XMLCreateNode(LABEL_GROUP_NAME);
                            (LabelGroupNode as XmlElement).SetAttribute(KEYWORD_NAME, "");
                            (LabelGroupNode as XmlElement).SetAttribute(KEYWORD_TEXT, oPositionData.m_sGroup);

                            if (m_XMLFileProcess.XMLAddNode(LabelGroupNode, TempParentRefNode))
                            {
                                bRet = m_XMLFileProcess.XMLAddNode(PositionNode, TempParentRefNode);
                            }
                        }
                        else
                        {
                            // make sure PositionNode and TempRefNodeList[0].Node.NextSibling have valid index
                            if ((DoesNextSiblingHaveAttribute(TempRefNodeList[0].Node, KEYWORD_INDEX) == false) ||
                                (String.IsNullOrEmpty(PositionNode.Attributes[KEYWORD_INDEX].Value) == true))
                            {
                                bRet = m_XMLFileProcess.XMLInsertNode(PositionNode, TempParentRefNode, TempRefNodeList[0], true);
                            }
                            else
                            {
                                XmlNode compareNode = TempRefNodeList[0].Node.NextSibling;
                                bRet = AddNodeInOrderOfAttribute(PositionNode, compareNode, TempParentRefNode);
                            }
                        }
                    }
                    else
                    {
                        // add logic to sort item right under page without label
                        if (TempParentRefNode.Node.HasChildNodes)
                        {
                            XmlNode firstChild = TempParentRefNode.Node.FirstChild;
                            // make sure PositionNode and firstChild have valid index
                            if ((firstChild.Name == LABEL_GROUP_NAME) ||
                                (firstChild.Attributes == null) ||
                                (firstChild.Attributes[KEYWORD_INDEX] == null) ||
                                (String.IsNullOrEmpty(firstChild.Attributes[KEYWORD_INDEX].Value) == true) ||
                                (String.IsNullOrEmpty(PositionNode.Attributes[KEYWORD_INDEX].Value) == true))
                            {
                                m_XMLFileProcess.XMLAddNode(PositionNode, TempParentRefNode, true);
                            }
                            else
                            {
                                bRet = AddNodeInOrderOfAttribute(PositionNode, firstChild, TempParentRefNode);
                            }
                        }
                        else
                        {
                            m_XMLFileProcess.XMLAddNode(PositionNode, TempParentRefNode);
                        }
                    }

                }
            }
            return bRet;
        }

        private bool AddNodeInOrderOfAttribute(XmlNode NodeToAdd, XmlNode FirstFieldNode, REF_NODE_TYPE ParentRefNode)
        {
            bool bRet = false;
            // find appropriate position to insert
            XmlNode compareNode = FirstFieldNode;
            do
            {
                if ((DoesNextSiblingHaveAttribute(compareNode, KEYWORD_INDEX) == false) ||
                    (Convert.ToInt16(compareNode.Attributes[KEYWORD_INDEX].Value) >= Convert.ToInt16(NodeToAdd.Attributes[KEYWORD_INDEX].Value)))
                {
                    REF_NODE_TYPE refNode = new REF_NODE_TYPE() { Node = compareNode };
                    if (Convert.ToInt16(compareNode.Attributes[KEYWORD_INDEX].Value) >= Convert.ToInt16(NodeToAdd.Attributes[KEYWORD_INDEX].Value))
                    {
                        bRet = m_XMLFileProcess.XMLInsertNode(NodeToAdd, ParentRefNode, (REF_NODE_TYPE)refNode, false);
                    }
                    else
                    {
                        bRet = m_XMLFileProcess.XMLInsertNode(NodeToAdd, ParentRefNode, (REF_NODE_TYPE)refNode, true);
                    }
                    break;
                }
                else
                {
                    compareNode = compareNode.NextSibling;
                }
            }
            while (compareNode != null);

            return bRet;
        }

        private bool DoesNextSiblingHaveAttribute(XmlNode checkNode, string sAttributeName)
        {
            if ((checkNode.NextSibling == null)
                || (checkNode.NextSibling.Attributes == null)
                || (checkNode.NextSibling.Attributes[sAttributeName] == null)
                || (String.IsNullOrEmpty(checkNode.NextSibling.Attributes[sAttributeName].Value) == true))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ProcessConfigInfo(CXmlConfigInfoObject oConfigData)
        {
            bool bRet = false;

            if (oConfigData != null)
            {
                m_HardContentPositionBasicList = new List<XmlNode>();
                m_HardContentPositionExpertList = new List<XmlNode>();
                
                // process hard messages position 
                XmlNode MessageConfigInfoList = oConfigData.GetConfigFeatureInfo(CXMLConfigKeywords.KW_CONFIG_MESSAGE);
                if ((MessageConfigInfoList != null) && (MessageConfigInfoList.HasChildNodes == true))
                {
                    foreach (XmlNode ElementNode in MessageConfigInfoList.ChildNodes)
                    {
                        if (ElementNode != null)
                        {
                            string sMessageName = (ElementNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_NAME);
                            XmlNode MessageContent = m_XMLHardContentProcess.GetMessages(sMessageName);
                            if (MessageContent != null)
                            {
                                XmlNode PositionData = MessageContent.CloneNode(true);
                                m_HardContentPositionExpertList.Add(PositionData);

                                if ((ElementNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_MODE) == CXMLConfigKeywords.KW_VALUE_BASIC)
                                {
                                    m_HardContentPositionBasicList.Add(PositionData);
                                }
                            }
                            else
                            {
                                // missing detected !! TODO
                            }
                        }
                    }
                }

                // process hard page position
                XmlNode GroupConfigInfo = oConfigData.GetConfigFeatureInfo(CXMLConfigKeywords.KW_CONFIG_GROUP);
                if ((GroupConfigInfo != null) && (GroupConfigInfo.HasChildNodes == true))
                {
                    foreach (XmlNode ConfigGroupNode in GroupConfigInfo.ChildNodes)
                    {
                        if (ConfigGroupNode != null)
                        {
                            // process postion hard content
                            string sConfigGroupName = (ConfigGroupNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_NAME);
                            XmlNode TableContent = m_XMLHardContentProcess.GetPositions(sConfigGroupName);
                            if ((TableContent != null) && (TableContent.HasChildNodes == true))
                            {
                                foreach (XmlNode PositionNode in TableContent.ChildNodes)
                                {
                                    XmlNode PositionData = PositionNode.CloneNode(true);
                                    m_HardContentPositionExpertList.Add(PositionData);

                                    if ((ConfigGroupNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_MODE) == CXMLConfigKeywords.KW_VALUE_BASIC)
                                    {
                                        m_HardContentPositionBasicList.Add(PositionData);
                                    }
                                }
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

        private bool CleanEmptyLabelAndPage (REF_NODE_TYPE RootPageRefNode)
        {
            bool bRet = true;

            string sSearchInfo = string.Format(FM_SELECTION_NODES,
                                  RootPageRefNode.Node.Name,
                                  PAGE_NODE_NAME,
                                  KEYWORD_TITLE,
                                  CONFIG_PAGE_NAME);

            XmlNode ConfigurationPage = RootPageRefNode.Node.SelectSingleNode(sSearchInfo);

            if (ConfigurationPage != null)
            {
                sSearchInfo = string.Format(FW_SELECTION_NAME_NODES,
                                 ConfigurationPage.Name,
                                 LABEL_GROUP_NAME);

                XmlNodeList PageRefList = ConfigurationPage.SelectNodes(sSearchInfo);

                if ((PageRefList != null) && (PageRefList.Count > 0))
                {
                    foreach(XmlNode labelNode in PageRefList)
                    {
                        if ((labelNode.NextSibling == null) ||
                            (labelNode.NextSibling.Name.Contains(LABEL_GROUP_NAME) == true) ||
                            (labelNode.NextSibling.Name.Contains(PAGE_NODE_NAME) == true))
                        {
                            m_XMLFileProcess.XMLRemoveNode(labelNode);
                        }
                    }
                }

                sSearchInfo = string.Format(FW_SELECTION_NAME_NODES,
                                 ConfigurationPage.Name,
                                 PAGE_NODE_NAME);

                PageRefList = ConfigurationPage.SelectNodes(sSearchInfo);

                if ((PageRefList != null) && (PageRefList.Count > 0))
                {
                    foreach (XmlNode pageNode in PageRefList)
                    {
                        if ((pageNode.HasChildNodes == false) &&
                                ((pageNode.Attributes == null) ||
                                ((pageNode.Attributes != null) && (pageNode.Attributes[KEYWORD_PANEL] == null))))
                        {
                            m_XMLFileProcess.XMLRemoveNode(pageNode);
                        }
                    }
                }
            }

                return bRet;
        }

        private void ReInitProperties()
        {
            m_HardContentPositionBasicList  = null;
            m_HardContentPositionExpertList = null;
        }
    }
}
