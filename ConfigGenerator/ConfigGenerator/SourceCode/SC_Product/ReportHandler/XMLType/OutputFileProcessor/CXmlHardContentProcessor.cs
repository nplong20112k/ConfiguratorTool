using System.IO;
using System.Xml;

namespace ConfigGenerator
{
    class CXmlHardContentProcessor
    {
        private const string KEYWORD_ROOT_NODE          = "hard_content";
        private const string KEYWORD_MESSAGE_NODE       = "hard_messages";
        private const string KEYWORD_TABLE_LIST_NODE    = "hard_tableList";
        private const string KEYWORD_PARAMETER_NODE     = "hard_parameters";
        private const string KEYWORD_RULE_NODE          = "hard_rules";
        private const string KEYWORD_POSITION_NODE      = "hard_position";

        private const string KEYWORD_GROUP_ATTRIBUTE    = "group";
        private const string KEYWORD_MESSAGE_ATTRIBUTE  = "name";

        private XmlDocument  m_XMLFileApp               = null;

        private XmlNode      m_MessageRoot              = null;
        private XmlNode      m_TableListRoot            = null;
        private XmlNode      m_ParamerRoot              = null;
        private XmlNode      m_RuleRoot                 = null;
        private XmlNode      m_PositionRoot             = null;
        
        public CXmlHardContentProcessor()
        {
            m_XMLFileApp = new XmlDocument();
        }

        public bool XMLLoadingFile(string sFilePath)
        {
            bool bRet = false;

            if (File.Exists(sFilePath))
            {
                try
                {
                    m_XMLFileApp.Load(sFilePath);
                    Initialize();
                    bRet = true;
                }
                catch { }
            }

            return bRet;
        }

        private bool Initialize()
        {
            bool bRet = false;

            if ((m_XMLFileApp != null) && (m_XMLFileApp.DocumentElement.Name == KEYWORD_ROOT_NODE))
            {
                if (m_XMLFileApp.DocumentElement.HasChildNodes)
                {
                    foreach (XmlNode NodeElement in m_XMLFileApp.DocumentElement.ChildNodes)
                    {
                        switch (NodeElement.Name)
                        {
                            case KEYWORD_MESSAGE_NODE:
                                m_MessageRoot = NodeElement;
                                break;

                            case KEYWORD_TABLE_LIST_NODE:
                                m_TableListRoot = NodeElement;
                                break;

                            case KEYWORD_PARAMETER_NODE:
                                m_ParamerRoot = NodeElement;
                                break;

                            case KEYWORD_RULE_NODE:
                                m_RuleRoot = NodeElement;
                                break;

                            case KEYWORD_POSITION_NODE:
                                m_PositionRoot = NodeElement;
                                break;

                            default:
                                break;
                        }
                    }
                    bRet = true;
                }
            }

            return bRet;
        }

        public XmlNode GetMessages(string sMessageName)
        {
            XmlNode NodeRet = null;
            NodeRet = GetConfigNode(sMessageName, m_MessageRoot, KEYWORD_MESSAGE_ATTRIBUTE);
            return NodeRet;
        }

        public XmlNode GetTableList(string sGroupName)
        {
            XmlNode NodeRet = null;
            NodeRet = GetConfigNode(sGroupName, m_TableListRoot, KEYWORD_GROUP_ATTRIBUTE);
            return NodeRet;
        }

        public XmlNode GetParameters(string sGroupName)
        {
            XmlNode NodeRet = null;
            NodeRet = GetConfigNode(sGroupName, m_ParamerRoot, KEYWORD_GROUP_ATTRIBUTE);
            return NodeRet;
        }

        public XmlNode GetRules(string sGroupName)
        {
            XmlNode NodeRet = null;
            NodeRet = GetConfigNode(sGroupName, m_RuleRoot, KEYWORD_GROUP_ATTRIBUTE);
            return NodeRet;
        }

        public XmlNode GetPositions(string sGroupName)
        {
            XmlNode NodeRet = null;
            NodeRet = GetConfigNode(sGroupName, m_PositionRoot, KEYWORD_GROUP_ATTRIBUTE);
            return NodeRet;
        }

        private XmlNode GetConfigNode(string sAttributeValue, XmlNode RootNode, string sAttributeName)
        {
            XmlNode NodeRet = null;

            if ((RootNode != null) && (RootNode.HasChildNodes) &&
                (sAttributeValue != null) && (sAttributeValue != string.Empty) &&
                (sAttributeName != null) && (sAttributeName != string.Empty))
            {
                foreach (XmlNode NodeElement in RootNode.ChildNodes)
                {
                    if ((NodeElement.NodeType != XmlNodeType.Comment) &&
                        (NodeElement as XmlElement).GetAttribute(sAttributeName) == sAttributeValue)
                    {
                        NodeRet = NodeElement;
                        break;
                    }
                }
            }

            return NodeRet;
        }
    }
}
