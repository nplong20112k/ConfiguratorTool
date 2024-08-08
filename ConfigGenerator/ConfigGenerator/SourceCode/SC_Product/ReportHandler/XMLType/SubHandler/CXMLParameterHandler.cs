using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ConfigGenerator
{
    class CXMLParameterHandler : AXMLSubHandler
    {
        private struct VALID_INTERFACE_TYPE
        {
            public string m_sInterfaceName;
            public string m_sCommand;
        }

        private const string PARAMETER_NORMAL_NODE_DUMMY   = "PARAMETERS_NORMAL_DUMMY";
        private const string PARAMETER_MULTI_NODE_DUMMY    = "PARAMETERS_MULTI_DUMMY";

        private const string PARAMETER_NORMAL_NODE_NAME    = "parameter";
        private const string PARAMETER_MULTIPLE_NODE_NAME  = "multiParameter";
        private const string PARAMETER_SINGLE_NODE_NAME    = "singleParameter";

        private const string KEYWORD_NAME                  = "name";
        private const string KEYWORD_VALUE                 = "value";
        private const string KEYWORD_TYPE                  = "type";
        private const string KEYWORD_PROTECTION            = "protection";
        private const string KEYWORD_CODE                  = "code";
        private const string KEYWORD_TABLEREF              = "tableRef";
        private const string KEYWORD_SIZELEN               = "sizeLen";
        private const string KEYWORD_FILL_CHAR             = "fillChar";
        private const string KEYWORD_MIN_LEN               = "minLen";
        private const string KEYWORD_MAX_LEN               = "maxLen";
        private const string KEYWORD_MIN_VALUE             = "min";
        private const string KEYWORD_MAX_VALUE             = "max";
        private const string KEYWORD_CONTEXT               = "context";
        private const string KEYWORD_HIDE_VALUE            = "hideValue";

        private const string KEYWORD_READ                  = "read";
        private const string KEYWORD_WRITE                 = "write";
        private const string KEYWORD_ITF_CMD_READ          = "$hA";
        private const string KEYWORD_ITF_CMD_WRITE         = "$HA";

        private const string KEYWORD_ACTION                = "action";
        private const string KEYWORD_ACTION_INSERT         = "insert";
        private const string KEYWORD_ACTION_REMOVE         = "remove";

        public static readonly List<string> KW_INTERFACE_NAME = new List<string>() { "INTERFACE_", "IF_" };

        private const string SPECIAL_PARAM_GROUP_NAME      = "Special Param Group";
        private const string FM_SELECTION_NODES            = "//{0}[@{1}='{2}']";
        
        private IXMLFileProcess             m_XMLFileProcess        = null;
        private CXmlHardContentProcessor    m_XMLHardContentProcess = null;
        private List<XmlNode>               m_HardContentParamList  = null;
        private List<VALID_INTERFACE_TYPE>  m_InterfaceSupportList  = null;

        private REF_NODE_TYPE               m_RootRefNode;
        private REF_NODE_TYPE               m_ParameterRootRefNode;
        private REF_NODE_TYPE               m_ParamNormalSiblingRefNode;
        private REF_NODE_TYPE               m_ParamMultiSiblingRefNode;
        
        public CXMLParameterHandler()
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
                    TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(CXMLNodeName.GetInstance().GetRootChildName(XML_ROOT_CHILD_NODE_ID_TYPE.PARAMETERS), m_RootRefNode);
                    if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                    {
                        m_ParameterRootRefNode = TempRefNodeList[0];
                    }

                    if (m_ParameterRootRefNode.Node != null)
                    {
                        bRet = true;
                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(PARAMETER_NORMAL_NODE_DUMMY, m_ParameterRootRefNode);
                        if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                        {
                            m_ParamNormalSiblingRefNode = TempRefNodeList[0];
                        }
                        else
                        {
                            bRet = false;
                        }
                        
                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(PARAMETER_MULTI_NODE_DUMMY, m_ParameterRootRefNode);
                        if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                        {
                            m_ParamMultiSiblingRefNode = TempRefNodeList[0];
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
                    if (oData != null)
                    {
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT)
                        {
                            m_InterfaceSupportList = null;
                            m_InterfaceSupportList = ProcessInterfaceSupportInfo(oData as CSourceInfoObject);
                        }
                        else if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT)
                        {
                            if (ProcessConfigInfo(oData as CXmlConfigInfoObject) == false)
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (bRet == true)
            {
                m_HardContentParamList = UpdateInterfaceSupport(m_InterfaceSupportList, m_HardContentParamList);

                if ((m_HardContentParamList != null) && (m_HardContentParamList.Count > 0))
                {
                    foreach (XmlNode HardParamElement in m_HardContentParamList)
                    {
                        if (HandlingHardParameter(HardParamElement, m_ParameterRootRefNode.Node))
                        {
                            bRet = false;
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
                    List<CIntegrateParamObject> oParaCoverInteList = null;
                    List<CIntegrateParamContainObject> oParaContainList = null;

                    oParaCoverInteList = (oDataIn as CIntegratedDataObject).GetParameterList();
                    if ((oParaCoverInteList != null) && (oParaCoverInteList.Count > 0))
                    {
                        bRet = true;
                        foreach (CIntegrateParamObject element in oParaCoverInteList)
                        {
                            if (element != null)
                            {
                                oParaContainList = element.GetParameterList();
                                if ((oParaContainList != null) && (oParaContainList.Count != 0))
                                {
                                    if (oParaContainList.Count == 1)
                                    {
                                        if (HandlingParameterNormal(element) == false)
                                        {
                                            bRet = false;
                                        }
                                    }
                                    else
                                    {
                                        if (HandlingParameterMultiple(element) == false)
                                        {
                                            bRet = false;
                                        }
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
            return bRet;
        }

        private bool HandlingHardParameter(XmlNode HardParamNode, XmlNode ParentNode)
        {
            bool bRet = false;

            if (m_XMLFileProcess != null)
            {
                if ((HardParamNode != null) && (ParentNode != null))
                {
                    string sTempTagCode = null;
                    string sTempAction = null;
                    REF_NODE_TYPE TempParentNode;
                    XmlNode TempParamNode = m_XMLFileProcess.XMLImportNode(HardParamNode);
                    
                    try
                    {
                        sTempTagCode = (TempParamNode as XmlElement).GetAttribute(KEYWORD_CODE);
                        sTempAction = (TempParamNode as XmlElement).GetAttribute(KEYWORD_ACTION);
                    }
                    catch { }

                    switch (sTempAction)
                    {
                        case KEYWORD_ACTION_INSERT:
                            {
                                XmlNodeList TempParamNodeList = ParentNode.SelectNodes(string.Format(   FM_SELECTION_NODES,
                                                                                                        PARAMETER_NORMAL_NODE_NAME,
                                                                                                        KEYWORD_CODE,
                                                                                                        sTempTagCode));
                                foreach (XmlNode ParamNodeElement in TempParamNodeList)
                                {
                                    TempParentNode = new REF_NODE_TYPE() { Node = ParamNodeElement};
                                    foreach (XmlNode SubContentElement in TempParamNode.ChildNodes)
                                    {
                                        if (SubContentElement != null)
                                        {
                                            if (m_XMLFileProcess.XMLAddNode(SubContentElement.CloneNode(true), TempParentNode, true) == false)
                                            {
                                                bRet = false;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case KEYWORD_ACTION_REMOVE:
                            {
                                XmlNodeList TempParamNodeList = ParentNode.SelectNodes(string.Format(   FM_SELECTION_NODES,
                                                                                                        PARAMETER_NORMAL_NODE_NAME,
                                                                                                        KEYWORD_CODE,
                                                                                                        sTempTagCode));
                                foreach (XmlNode ParamNodeElement in TempParamNodeList)
                                {
                                    if (m_XMLFileProcess.XMLRemoveNode(ParamNodeElement) == false)
                                    {
                                        bRet = false;
                                    }
                                }
                            }
                            break;

                        default:
                            {
                                TempParentNode = new REF_NODE_TYPE() { Node = ParentNode};
                                if (m_XMLFileProcess.XMLAddNode(TempParamNode, TempParentNode, true) == false)
                                {
                                    bRet = false;
                                }
                            }
                            break;
                    }
                }
            }

            return bRet;
        }

        private bool HandlingParameterNormal(CIntegrateParamObject ParameterIntegrated)
        {
            bool bRet = false;
            XmlNode NewNode = null;

            if (ParameterIntegrated != null)
            {
                List<CIntegrateParamContainObject> ParameterList = null;
                ParameterList = ParameterIntegrated.GetParameterList();

                NewNode = CreateParameterNode(PARAMETER_NORMAL_NODE_NAME, ParameterList[0]);
                if (NewNode != null)
                {
                    bRet = m_XMLFileProcess.XMLInsertNode(NewNode, m_ParameterRootRefNode, m_ParamNormalSiblingRefNode);
                    m_ParamNormalSiblingRefNode.Node = NewNode;
                }
            }

            return bRet;
        }

        private bool HandlingParameterMultiple(CIntegrateParamObject ParameterIntegrated)
        {
            bool bRet = false;
            XmlNode NewNode = null;

            if (ParameterIntegrated != null)
            {
                List<CIntegrateParamContainObject> ParameterList = null;
                ParameterList = ParameterIntegrated.GetParameterList();
                if (ParameterList.Count > 1)
                {
                    foreach (CIntegrateParamContainObject element in ParameterList)
                    {
                        if (element != null)
                        {
                            if (element == ParameterList.First())
                            {
                                NewNode = CreateParameterNode(PARAMETER_MULTIPLE_NODE_NAME, element);
                            }
                            else
                            {
                                if (NewNode != null)
                                {
                                    XmlNode SubNode = CreateParameterNode(PARAMETER_SINGLE_NODE_NAME, element);
                                    if (SubNode != null)
                                    {
                                        NewNode.AppendChild(SubNode);
                                    }
                                }
                            }
                        }
                    }

                    bRet = m_XMLFileProcess.XMLInsertNode(NewNode, m_ParameterRootRefNode, m_ParamMultiSiblingRefNode);
                    m_ParamMultiSiblingRefNode.Node = NewNode;

                }
            }

            return bRet;
        }

        private XmlNode CreateParameterNode(string ParamNodeName, CIntegrateParamContainObject ParameterIntegrated)
        {
            XmlNode bNodeRet = null;

            if ((m_XMLFileProcess != null) &&  (ParamNodeName != null) && (ParameterIntegrated != null))
            {
                bNodeRet = m_XMLFileProcess.XMLCreateNode(ParamNodeName);

                string sName = ParameterIntegrated.GetParamName();
                if (sName != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_NAME, sName);
                }

                string sValue = ParameterIntegrated.GetParamDefaultValue();
                if (sValue != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_VALUE, sValue);
                }

                string sType = ParameterIntegrated.GetParamTypeValue();
                if (sType != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_TYPE, sType);
                }

                string sProtection = ParameterIntegrated.GetParamProtection();
                if (sProtection != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_PROTECTION, sProtection);
                }

                string sCode = ParameterIntegrated.GetParamCode();
                if (sCode != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_CODE, sCode);
                }

                string sTableRefName = ParameterIntegrated.GetParamTableRefName();
                if (sTableRefName != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_TABLEREF, sTableRefName);
                }

                string sSizeLen = ParameterIntegrated.GetParamSizeLen();
                if (sSizeLen != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_SIZELEN, sSizeLen);
                }

                string sMinValue = ParameterIntegrated.GetParamMinValue();
                if (sMinValue != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_MIN_VALUE, sMinValue);
                }

                string sMaxValue = ParameterIntegrated.GetParamMaxValue();
                if (sMaxValue != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_MAX_VALUE, sMaxValue);
                }

                string sMinLen = ParameterIntegrated.GetParamMinLen();
                if (sMinLen != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_MIN_LEN, sMinLen);
                }

                string sMaxLen = ParameterIntegrated.GetParamMaxLen();
                if (sMaxLen != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_MAX_LEN, sMaxLen);
                }

                string sFillChar = ParameterIntegrated.GetParamFillChar();
                if (sMaxLen != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_FILL_CHAR, sFillChar);
                }

                string sHiddenValue = ParameterIntegrated.GetParamHiddenValue();
                if (sHiddenValue != null)
                {
                    (bNodeRet as XmlElement).SetAttribute(KEYWORD_HIDE_VALUE, sHiddenValue);
                }

                string sContext = null;
                switch (ParamNodeName)
                {
                    case PARAMETER_MULTIPLE_NODE_NAME:
                        sContext = ParameterIntegrated.GetParamUserName();
                        (bNodeRet as XmlElement).SetAttribute(KEYWORD_CONTEXT, sContext);
                        break;

                    case PARAMETER_NORMAL_NODE_NAME:
                    case PARAMETER_SINGLE_NODE_NAME:
                        sContext = ParameterIntegrated.GetParamUserName();
                        XmlNode ContextNode = m_XMLFileProcess.XMLCreateNode(KEYWORD_CONTEXT);
                        if (ContextNode != null)
                        {
                            (ContextNode as XmlElement).InnerText = sContext;
                            bNodeRet.AppendChild(ContextNode);
                        }
                        break;

                    default:
                        break;
                }
            }

            return bNodeRet;
        }

        private bool ProcessConfigInfo(CXmlConfigInfoObject oConfigData)
        {
            bool bRet = false;

            if (oConfigData != null)
            {
                XmlNode GroupConfigInfo = oConfigData.GetConfigFeatureInfo(CXMLConfigKeywords.KW_CONFIG_GROUP);
                if ((GroupConfigInfo != null) && (GroupConfigInfo.HasChildNodes == true))
                {
                    XmlNode ParamContent = null;
                    m_HardContentParamList = new List<XmlNode>();
                    foreach (XmlNode ElementNode in GroupConfigInfo.ChildNodes)
                    {
                        if (ElementNode != null)
                        {
                            string sGroupName = (ElementNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_NAME);
                            ParamContent = m_XMLHardContentProcess.GetParameters(sGroupName);
                            if ((ParamContent != null) && (ParamContent.HasChildNodes == true))
                            {
                                XmlNode TempNode = ParamContent.CloneNode(true);
                                m_HardContentParamList.AddRange(TempNode.ChildNodes.Cast<XmlNode>());
                            }
                            else
                            {
                                // missing detected !! TODO
                            }
                        }
                    }

                    ParamContent = m_XMLHardContentProcess.GetParameters(SPECIAL_PARAM_GROUP_NAME);
                    if ((ParamContent != null) && (ParamContent.HasChildNodes == true))
                    {
                        XmlNode TempNode = ParamContent.CloneNode(true);
                        m_HardContentParamList.AddRange(TempNode.ChildNodes.Cast<XmlNode>());
                    }

                    bRet = true;
                }
            }

            return bRet;
        }

        private List<VALID_INTERFACE_TYPE> ProcessInterfaceSupportInfo(CSourceInfoObject oConfigData)
        {
            List<VALID_INTERFACE_TYPE> InterfaceListRet = null;

            if (oConfigData != null)
            {
                InterfaceListRet = new List<VALID_INTERFACE_TYPE>();
                List<INTERFACE_CLASS> InterfaceClassList = oConfigData.GetInterfaceClass();
                foreach (INTERFACE_CLASS InfClassElement in InterfaceClassList)
                {
                    foreach (INTERFACE_CLASS_MEMBER ItfMemberElement in InfClassElement.m_lsInterfaceMember)
                    {
                        if ((ItfMemberElement.m_bInterfaceValid == true) &&
                            (ItfMemberElement.m_sInterfaceName  != null) &&
                            (ItfMemberElement.m_sCommand    != null) )
                        {
                            VALID_INTERFACE_TYPE TempInterface = new VALID_INTERFACE_TYPE();

                            string sInterfaceName = ItfMemberElement.m_sInterfaceName;
                            foreach (string sElement in KW_INTERFACE_NAME)
                            {
                                if (sInterfaceName.Contains(sElement) == true)
                                {
                                    sInterfaceName = sInterfaceName.Replace(sElement, string.Empty);
                                    break;
                                }
                            }
                            sInterfaceName = sInterfaceName.Replace("_"," ");

                            TempInterface.m_sInterfaceName = sInterfaceName;
                            TempInterface.m_sCommand = ItfMemberElement.m_sCommand;

                            InterfaceListRet.Add(TempInterface);
                        }
                    }
                }
            }

            return InterfaceListRet;
        }

        private List<XmlNode> UpdateInterfaceSupport(List<VALID_INTERFACE_TYPE> InfSupportList, List<XmlNode> HardContentParamList)
        {
            List<XmlNode> InfListRet = HardContentParamList;

            if ((InfSupportList != null) && (HardContentParamList != null))
            {
                foreach (XmlNode ElementNode in InfListRet)
                {
                    // Check node hard content for Interface Selection by detect code="$hA".
                    if ((ElementNode != null) && 
                        (ElementNode.NodeType != XmlNodeType.Comment) && 
                        ((ElementNode as XmlElement).GetAttribute(KEYWORD_CODE) == KEYWORD_ITF_CMD_READ))
                    {
                        XmlNode InfSelectionNode = ElementNode.CloneNode(true);
                        InfSelectionNode = m_XMLFileProcess.XMLImportNode(InfSelectionNode);

                        foreach (VALID_INTERFACE_TYPE ItfMemberElement in InfSupportList)
                        {
                            string sInterfaceName = ItfMemberElement.m_sInterfaceName;
                            string sReadContent   = ItfMemberElement.m_sCommand.Replace("HA", string.Empty);
                            string sWriteCommand  = KEYWORD_ITF_CMD_WRITE + sReadContent;

                            XmlNode InfMemberNode = m_XMLFileProcess.XMLCreateNode(KEYWORD_VALUE);
                            (InfMemberNode as XmlElement).SetAttribute(KEYWORD_CONTEXT, sInterfaceName);
                            (InfMemberNode as XmlElement).SetAttribute(KEYWORD_READ, sReadContent);
                            (InfMemberNode as XmlElement).SetAttribute(KEYWORD_WRITE, sWriteCommand);

                            InfSelectionNode.AppendChild(InfMemberNode);
                        }

                        InfListRet[InfListRet.IndexOf(ElementNode)] = InfSelectionNode;
                        break;
                    }
                }
            }

            return InfListRet;
        }

        private void ReInitProperties()
        {
            m_HardContentParamList  = null;
        }
    }
}
