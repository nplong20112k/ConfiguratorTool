using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ConfigGenerator
{
    class CXMLRuleHandler : AXMLSubHandler
    {
        private const string RULE_NODE_DUMMY      = "RULES_DUMMY";
        private const string RULE_SET_NODE_DUMMY  = "RULESETS_DUMMY";

        private const string RULE_NODE_NAME       = "rule";
        private const string RULE_SET_NODE_NAME   = "ruleSet";

        private const string KEYWORD_SENDER       = "sender";
        private const string KEYWORD_CONDITION    = "condition";
        private const string KEYWORD_TARGET       = "target";
        private const string KEYWORD_ACTION       = "action";

        private const string KEYWORD_READ         = "read";
        private const string KEYWORD_NAME         = "name";
        private const string KEYWORD_VALUE        = "value";
        private const string KEYWORD_SPLIT_MEMBER = ";";

        private IXMLFileProcess             m_XMLFileProcess        = null;
        private CXmlHardContentProcessor    m_XMLHardContentProcess = null;
        private List<XmlNode>               m_HardContentRuleList   = null;

        private REF_NODE_TYPE               m_RootRefNode;
        private REF_NODE_TYPE               m_ParameterRootRefNode;
        private REF_NODE_TYPE               m_RuleRefNode;
        private REF_NODE_TYPE               m_RuleSetRefNode;

        public CXMLRuleHandler()
        {
            m_XMLFileProcess = CFactoryXmlFileProcessor.GetInstance().GetXmlFileProcessor();
            m_XMLHardContentProcess = CFactoryXmlFileProcessor.GetInstance().GetHardContentProcessor();
        }

        public override bool Initialize(List<IShareObject> oDataList)
        {
            bool bRet = true;
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
                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(RULE_NODE_DUMMY, m_ParameterRootRefNode);
                        if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                        {
                            m_RuleRefNode = TempRefNodeList[0];
                        }
                        else
                        {
                            bRet = false;
                        }

                        TempRefNodeList = m_XMLFileProcess.XMLSearchNodes(RULE_SET_NODE_DUMMY, m_ParameterRootRefNode);
                        if ((TempRefNodeList != null) && (TempRefNodeList.Count == 1))
                        {
                            m_RuleSetRefNode = TempRefNodeList[0];
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
                if ((m_HardContentRuleList != null) && (m_HardContentRuleList.Count > 0))
                {
                    foreach (XmlNode RuleElement in m_HardContentRuleList)
                    {
                        if (RuleElement != null)
                        {
                            XmlNode TempParamNode = m_XMLFileProcess.XMLImportNode(RuleElement);
                            if (m_XMLFileProcess.XMLAddNode(TempParamNode, m_ParameterRootRefNode, false) == false)
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
            bool bRet = true;
            // body empty
            return bRet;
        }

        public override bool Finalize(IShareObject oDataIn)
        {
            bool bRet = false;
            
            bRet = DataFinalizing(oDataIn);

            return bRet;
        }

        private bool DataFinalizing(IShareObject oDataIn)
        {
            bool bRet = true;

            if ((m_XMLFileProcess != null) && ((oDataIn != null)))
            {
                CIntegratedRuleObject RuleObject = (oDataIn as CIntegratedDataObject).GetRules();
                if (RuleObject != null)
                {
                    List<CIntegratedRuleContent> RuleList = RuleObject.GetRuleList();
                    foreach (CIntegratedRuleContent RuleElement in RuleList)
                    {
                        switch (RuleElement.m_RuleType)
                        {
                            case RULE_TYPE.RULE:
                                if (HandlingRule(RuleElement, m_ParameterRootRefNode) == false)
                                {
                                    bRet = false;
                                }
                                break;

                            case RULE_TYPE.RULE_SET:
                                if (HandlingRuleSet(RuleElement, m_ParameterRootRefNode) == false)
                                {
                                    bRet = false;
                                }
                                break;

                            default:
                                break;
                        }
                    }
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
                    m_HardContentRuleList = new List<XmlNode>();
                    foreach (XmlNode ElementNode in GroupConfigInfo.ChildNodes)
                    {
                        if (ElementNode != null)
                        {
                            string sGroupName = (ElementNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_NAME);
                            XmlNode TableContent = m_XMLHardContentProcess.GetRules(sGroupName);
                            if ((TableContent != null) && (TableContent.HasChildNodes == true))
                            {
                                XmlNode TempNode = TableContent.CloneNode(true);
                                m_HardContentRuleList.AddRange(TempNode.ChildNodes.Cast<XmlNode>());
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

        private bool HandlingRule(CIntegratedRuleContent RuleContent, REF_NODE_TYPE ParentRefNode)
        {
            bool bRet = false;

            if ((RuleContent != null) && (ParentRefNode.Node != null))
            {
                XmlNode NewNode = m_XMLFileProcess.XMLCreateNode(RULE_NODE_NAME);

                // Handle condition
                (NewNode as XmlElement).SetAttribute(KEYWORD_CONDITION, RuleContent.m_sCondition);

                // Handle action
                (NewNode as XmlElement).SetAttribute(KEYWORD_ACTION, RuleContent.m_sAction);

                // Handler sender
                String SenderColection = null;
                foreach (RULE_MEMBER_TYPE SenderElement in RuleContent.m_SenderList)
                {
                    if ((SenderElement.m_sName != null) && (SenderElement.m_sName != string.Empty))
                    {
                        // Check dupplicated
                        if ((SenderColection == null) || (SenderColection.Contains(SenderElement.m_sName) == false))
                        {
                            if (SenderColection != null)
                            {
                                SenderColection += KEYWORD_SPLIT_MEMBER;
                            }
                            SenderColection += SenderElement.m_sName;
                        }
                    }

                    if ((SenderElement.m_sValue != null) && (SenderElement.m_sValue != string.Empty))
                    {
                        XmlNode SubNode = m_XMLFileProcess.XMLCreateNode(KEYWORD_VALUE);
                        (SubNode as XmlElement).InnerText = SenderElement.m_sValue;

                        if ((SenderElement.m_sName != null) && (SenderElement.m_sName != string.Empty))
                        {
                            (SubNode as XmlElement).SetAttribute(KEYWORD_READ, SenderElement.m_sName);
                        }
                            
                        NewNode.AppendChild(SubNode);
                    }
                }
                (NewNode as XmlElement).SetAttribute(KEYWORD_SENDER, SenderColection);

                // Handle target
                String TargetColection = null;
                foreach (RULE_MEMBER_TYPE TargetElement in RuleContent.m_TargetList)
                {
                    if ((TargetElement.m_sName != null) && (TargetElement.m_sName != string.Empty))
                    {
                        // Check dupplicated
                        if ((TargetColection == null) || (SenderColection.Contains(TargetElement.m_sName) == false))
                        {
                            if (TargetColection != null)
                            {
                                TargetColection += KEYWORD_SPLIT_MEMBER;
                            }
                            TargetColection += TargetElement.m_sName;
                        }
                    }
                }
                (NewNode as XmlElement).SetAttribute(KEYWORD_TARGET, TargetColection);

                // Write rule node to report
                bRet = m_XMLFileProcess.XMLInsertNode(NewNode, ParentRefNode, m_RuleRefNode);
            }

            return bRet;
        }

        private bool HandlingRuleSet(CIntegratedRuleContent RuleSetContent, REF_NODE_TYPE RefNode)
        {
            bool bRet = false;

             if ((RuleSetContent != null) && (RefNode.Node != null))
            {
                XmlNode NewNode = m_XMLFileProcess.XMLCreateNode(RULE_SET_NODE_NAME);

                // Handle condition
                (NewNode as XmlElement).SetAttribute(KEYWORD_CONDITION, RuleSetContent.m_sCondition);

                // Handle sender
                String SenderColection = null;
                foreach (RULE_MEMBER_TYPE SenderElement in RuleSetContent.m_SenderList)
                {
                    if ((SenderElement.m_sName != null) && (SenderElement.m_sName != string.Empty))
                    {
                        // Check dupplicated
                        if ((SenderColection == null) || (SenderColection.Contains(SenderElement.m_sName) == false))
                        {
                            if (SenderColection != null)
                            {
                                SenderColection += KEYWORD_SPLIT_MEMBER;
                            }
                            SenderColection += SenderElement.m_sName;
                        }
                    }

                    if ((SenderElement.m_sValue != null) && (SenderElement.m_sValue != string.Empty))
                    {
                        XmlNode SubNode = m_XMLFileProcess.XMLCreateNode(KEYWORD_VALUE);
                        (SubNode as XmlElement).InnerText = SenderElement.m_sValue;
                        if ((SenderElement.m_sName != null) && (SenderElement.m_sName != string.Empty))
                        {
                            (SubNode as XmlElement).SetAttribute(KEYWORD_READ, SenderElement.m_sName);
                        }
                            
                        NewNode.AppendChild(SubNode);
                    }
                }
                (NewNode as XmlElement).SetAttribute(KEYWORD_SENDER, SenderColection);
                
                // Handle target
                foreach (RULE_MEMBER_TYPE TargetElement in RuleSetContent.m_TargetList)
                {
                    if ((TargetElement.m_sName != null) && (TargetElement.m_sName != string.Empty) &&
                        (TargetElement.m_sValue != null) && (TargetElement.m_sValue != string.Empty))
                    {
                        XmlNode SubNode = m_XMLFileProcess.XMLCreateNode(KEYWORD_TARGET);
                        (SubNode as XmlElement).SetAttribute(KEYWORD_NAME, TargetElement.m_sName);
                        (SubNode as XmlElement).SetAttribute(KEYWORD_ACTION, RuleSetContent.m_sAction);
                        (SubNode as XmlElement).SetAttribute(KEYWORD_VALUE, TargetElement.m_sValue);
                            
                        NewNode.AppendChild(SubNode);
                    }
                }

                // Write rule node to report
                bRet = m_XMLFileProcess.XMLInsertNode(NewNode, RefNode, m_RuleSetRefNode);
            }
            return bRet;
        }

        private void ReInitProperties()
        {
            m_HardContentRuleList   = null;
        }
    }
}
