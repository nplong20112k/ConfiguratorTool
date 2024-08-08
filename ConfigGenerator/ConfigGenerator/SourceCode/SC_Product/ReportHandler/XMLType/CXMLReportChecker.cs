using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Globalization;

namespace ConfigGenerator
{
    public class Event_XMLReportChecker
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_RESPOND_CHECK_INPUT_FILE_DONE,
        };
    }


    class CXMLReportChecker : AReportChecker
    {
        private const string ALADDIN_FOLDER_NAME        = "Aladdin_Package";
        private const string TEMPLATE_NODE_INFO_NAME    = "info";
        private const string TEMPLATE_NODE_CONTENT_NAME = "device";

        private const string FOLDER_PATH                = "__OUTPUT_FOLDER_PATH__";
        private const string PRODUCT_NAME               = "__PROD_NAME__";
        private const string RELEASE_SW_NUMBER          = "__APPL_SW_VERSION__";
        private const string MERGE_RELEASE              = "__MERGE_RELEASE__";
        private const string RELEASE_VERSION            = "__RELEASE_VERSION__";
        private const string DATE                       = "__APPL_SW_DATE__";
        private const string IMAGE_PATH                 = "__IMAGE_FILE_PATH__";
        private const string HELP_FILE_PATH             = "__HELP_URL_FILE_PATH__";
        private const string HARD_CONTENT_FILE_PATH     = "__HARD_CONTENT_FILE_PATH__";

        private const string XML_REPORT_PREFIX          = "config";
        private const string XML_REPORT_TYPE            = ".xml";
        private const string XML_SPLIT_CHAR             = "_";

        private const string KW_CONFIG_ROOT             = "INFO_CONFIG";

        private const string KW_CONFIG_STATE            = "state";
        private const string KW_CONFIG_VALUE_ENABLE     = "enable";

        private const string FM_SELECTION_NODES         = "//{0}//{1}[@{2}='{3}']";
        private const string FW_SELECTION_NAME_NODES    = "//{0}//{1}";
        private const string FW_SELECTION_ATTRIBUTE_NODE = "//*[@{0}='{1}']";
        private const string PAGE_NODE_NAME             = "page";
        private const string KEYWORD_TITLE              = "title";
        private const string CONFIG_PAGE_NAME           = "Configuration";
        private const string COMMENT_KEYWORD            = "comment()";
        private const string REMOVE_DETECT_KEYWORD      = "page name";
        private const string GROUP_KEYWORD              = "group";

        private IXMLFileProcess m_XMLFileProcess = null;
        private XmlNode m_XMLReportInfo = null;
        private XmlNode m_XMLReportContent = null;

        private string m_TemplateFilePath = null;
        private string m_ReportFilePath = null;

        private string m_ReportFolderPath = null;
        private string m_ProductName = null;
        private string m_ReleaseNumber = null;
        private string m_MergeRelease = null;
        private string m_ReleaseVersion = null;
        private string m_DateRelease = null;
        private string m_ImagePath = null;
        private string m_HelpFilePath = null;
        private string m_HardContentFilePath = null;

        private CXmlConfigInfoObject m_XmlConfigInfo = null;

        private List<string> m_lsModelName = null;

        public CXMLReportChecker()
            : base(AReportChecker.REPORT_FILE_TYPE.REPORT_FILE_XML, new Event_XMLReportChecker().m_MyEvent)
        {
            m_XMLFileProcess = CFactoryXmlFileProcessor.GetInstance().GetXmlFileProcessor();
        }

        public override bool CheckTemplateFileValid(string sFilePath)
        {
            bool bRet = false;
            Initialize();

            if (m_XMLFileProcess != null)
            {
                // load XML template with mode read only
                bRet = m_XMLFileProcess.XMLLoadingFile(sFilePath, true);
                if (bRet == true)
                {
                    m_TemplateFilePath = sFilePath;
                }
            }

            return bRet;
        }

        public override bool CheckTemplateFileStructure()
        {
            bool bRet = false;
            int[] tempResult = null;

            if (m_XMLFileProcess != null)
            {
                string[] XMLRootChildList = CXMLNodeName.GetInstance().GetRootChildNameList();
                tempResult = new int[XMLRootChildList.Length];
                for (int i = 0; i < XMLRootChildList.Length; i++)
                {
                    tempResult[i] = 0;
                }

                XmlNode TemplateRootNode = m_XMLFileProcess.XMLGetRootNode();

                if (TemplateRootNode != null)
                {
                    if (TemplateRootNode.HasChildNodes)
                    {
                        foreach (XmlNode element in TemplateRootNode.ChildNodes)
                        {
                            if (element != null)
                            {
                                switch (element.Name)
                                {
                                    case TEMPLATE_NODE_INFO_NAME:
                                        m_XMLReportInfo = element;
                                        break;

                                    case TEMPLATE_NODE_CONTENT_NAME:
                                        m_XMLReportContent = element;
                                        for (int i = 0; i < m_XMLReportContent.ChildNodes.Count; i++)
                                        {
                                            for (int j = 0; j < XMLRootChildList.Length; j++)
                                            {
                                                if (m_XMLReportContent.ChildNodes[i].Name == XMLRootChildList[j])
                                                {
                                                    tempResult[j]++;
                                                }
                                            }
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    bRet = true;
                    for (int i = 0; i < XMLRootChildList.Length; i++)
                    {
                        switch (tempResult[i])
                        {
                            case 0:
                                // TODO Add Log missing collumn
                                bRet = false;
                                break;

                            case 1:
                                break;

                            default:
                                // TODO Add Log duplicate collumn
                                bRet = false;
                                break;
                        }
                    }
                }
            }

            return bRet;
        }

        public override bool ProcessTemplateFile()
        {
            bool bRet = false;

            if (PrepareReportFileInfo())
            {
                if (CheckHardContentFile() == true)
                {
                    if (m_ReportFilePath != null)
                    {
                        CleanCommentedPageName(m_XMLReportContent);
                        bRet = ProcessModelName(m_XMLReportContent);
                        if (bRet == true)
                        {
                            UpdateXmlConFigInfoObject(m_XmlConfigInfo,
                                                        m_XMLReportContent,
                                                        m_ProductName,
                                                        m_ReleaseNumber,
                                                        m_MergeRelease,
                                                        m_DateRelease,
                                                        m_ReleaseVersion,
                                                        m_ImagePath,
                                                        m_HelpFilePath,
                                                        m_ReportFilePath,
                                                        m_HardContentFilePath);
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO, m_XmlConfigInfo);
                        }
                    }
                }
            }

            return bRet;
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_RESPOND_CHECK_INPUT_FILE_DONE:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_INPUT_INFO_OBJECT))
                    {
                        m_lsModelName = (oData as CInputInfoObject).GetModelName();
                    }
                    else
                    {
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, null);
                    }
                    break;

                default:
                    break;
            }
        }

        public override void Initialize()
        {
            m_XMLReportInfo = null;
            m_XMLReportContent = null;
            m_TemplateFilePath = null;
            m_ReportFilePath = null;
            m_ReportFolderPath = null;
            m_ProductName = null;
            m_ReleaseNumber = null;
            m_MergeRelease = null;
            m_ReleaseVersion = null;
            m_DateRelease = null;
            m_XmlConfigInfo = null;
            m_HardContentFilePath = null;
        }

        private bool PrepareReportFileInfo()
        {
            bool bRet = false;

            if (m_XMLFileProcess != null)
            {
                if (m_XMLReportInfo != null)
                {
                    if ((m_XMLReportInfo.HasChildNodes) && (m_XMLReportInfo.Name == TEMPLATE_NODE_INFO_NAME))
                    {
                        foreach (XmlNode ElementNode in m_XMLReportInfo.ChildNodes)
                        {
                            switch (ElementNode.Name)
                            {
                                case FOLDER_PATH:
                                    m_ReportFolderPath = ElementNode.InnerText;
                                    break;

                                case PRODUCT_NAME:
                                    m_ProductName = ElementNode.InnerText;
                                    break;

                                case RELEASE_SW_NUMBER:
                                    m_ReleaseNumber = ElementNode.InnerText;
                                    break;

                                case MERGE_RELEASE:
                                    m_MergeRelease = ElementNode.InnerText;
                                    break;

                                case RELEASE_VERSION:
                                    m_ReleaseVersion = ElementNode.InnerText;
                                    break;

                                case DATE:
                                    m_DateRelease = ElementNode.InnerText;
                                    if ((m_DateRelease == null) || (m_DateRelease == ""))
                                    {
                                        CultureInfo culture = new CultureInfo(@"en-GB");
                                        m_DateRelease = DateTime.Today.ToString("D", culture);
                                    }
                                    break;

                                case IMAGE_PATH:
                                    m_ImagePath = ElementNode.InnerText;
                                    break;

                                case HELP_FILE_PATH:
                                    m_HelpFilePath = ElementNode.InnerText;
                                    break;

                                case HARD_CONTENT_FILE_PATH:
                                    m_HardContentFilePath = ElementNode.InnerText;
                                    break;

                                case KW_CONFIG_ROOT:
                                    m_XmlConfigInfo = ParseConfigInfo(ElementNode);
                                    break;

                                default:
                                    break;
                            }
                        }

                        if ((String.IsNullOrEmpty(m_ProductName) == false) &&
                            (String.IsNullOrEmpty(m_ReleaseNumber) == false))
                        {
                            // m_ReportFilePath = Directory.GetCurrentDirectory();
                            m_ReportFilePath = System.AppDomain.CurrentDomain.BaseDirectory;
                            
                            if (String.IsNullOrEmpty(m_ReportFolderPath) == false)
                            {
                                if (Path.IsPathRooted(m_ReportFolderPath))
                                {
                                    m_ReportFilePath = m_ReportFolderPath;
                                }
                                else
                                {
                                    m_ReportFilePath += Path.DirectorySeparatorChar + m_ReportFolderPath;
                                }
                            }

                            m_ReportFilePath += Path.DirectorySeparatorChar + ALADDIN_FOLDER_NAME;
                            m_ReportFilePath += Path.DirectorySeparatorChar + m_ProductName + XML_SPLIT_CHAR + m_ReleaseNumber;
                            m_ReportFilePath += Path.DirectorySeparatorChar + XML_REPORT_PREFIX + XML_SPLIT_CHAR + m_ProductName + XML_SPLIT_CHAR + m_ReleaseNumber + XML_REPORT_TYPE;

                            m_ImagePath = CheckCorrectFileNameAndFullfillPath(m_ImagePath);
                            if (String.IsNullOrEmpty(m_ImagePath) == true)
                            {
                                Program.SystemHandleStatusInfo("\r\nWARNING:: Missing device picture or invalid file name !!!\r\n\r\n");
                            }

                            m_HelpFilePath = CheckCorrectFileNameAndFullfillPath(m_HelpFilePath);
                            if (String.IsNullOrEmpty(m_HelpFilePath) == true)
                            {
                                Program.SystemHandleStatusInfo("\r\nWARNING:: Missing help url file or invalid file name !!!\r\n\r\n");
                            }

                            m_HardContentFilePath = CheckCorrectFileNameAndFullfillPath(m_HardContentFilePath);
                            if (String.IsNullOrEmpty(m_HardContentFilePath) == true)
                            {
                                Program.SystemHandleStatusInfo("\r\nERROR:: Missing hard content file or invalid file name !!!\r\n\r\n");
                                return false;
                            }

                            m_XMLReportInfo = null;
                            bRet = true;
                        }
                    }
                }
            }

            return bRet;
        }

        private string CheckCorrectFileNameAndFullfillPath(string sFileName)
        {
            string sResult = null;
            if (String.IsNullOrEmpty(sFileName) == false)
            {
                if (String.IsNullOrEmpty(Path.GetFileName(sFileName)) == false)
                {
                    if (Path.IsPathRooted(sFileName) == true)
                    {
                        sResult = sFileName;
                    }
                    else
                    {
                        if (Path.IsPathRooted(Path.GetDirectoryName(m_TemplateFilePath)) == true)
                        {
                            sResult = Path.GetDirectoryName(m_TemplateFilePath) + Path.DirectorySeparatorChar + sFileName;
                        }
                        else
                        {
                            // sResult = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetDirectoryName(m_TemplateFilePath) + Path.DirectorySeparatorChar + sFileName;
                            sResult = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + Path.GetDirectoryName(m_TemplateFilePath) + Path.DirectorySeparatorChar + sFileName;
                            
                        }
                    }

                    if (File.Exists (sResult) == false)
                    {
                        sResult = null;
                    }
                }
            }
            return sResult;
        }

        private CXmlConfigInfoObject ParseConfigInfo(XmlNode ConfigNode)
        {
            CXmlConfigInfoObject ConfigInfoRet = null;

            if ((ConfigNode != null) && (ConfigNode.HasChildNodes == true))
            {
                ConfigInfoRet = new CXmlConfigInfoObject();

                foreach (XmlNode ElementNode in ConfigNode.ChildNodes)
                {
                    if ((ElementNode != null) &&
                        (ElementNode.NodeType != XmlNodeType.Comment) &&
                        (ElementNode.HasChildNodes == true))
                    {
                        XmlNode GroupNode = ElementNode.CloneNode(true);
                        if (ElementNode.Name != "_FUNCTION_SUPPORT_")
                        {
                            GroupNode.RemoveAll();

                            // loop for fillter setting in DISABLE state.
                            foreach (XmlNode ChildElementNode in ElementNode.ChildNodes)
                            {
                                if ((ChildElementNode as XmlElement).GetAttribute(KW_CONFIG_STATE) == KW_CONFIG_VALUE_ENABLE)
                                {
                                    GroupNode.AppendChild(ChildElementNode.CloneNode(true));
                                }
                            }
                        }

                        if (GroupNode.HasChildNodes == true)
                        {
                            ConfigInfoRet.SetConfigFeatureInfo(GroupNode.Name, GroupNode);
                        }
                    }
                }
            }

            return ConfigInfoRet;
        }

        private bool ProcessModelName(XmlNode XmlContent)
        {
            bool bRet = false;

            string[] XMLRootChildList = CXMLNodeName.GetInstance().GetRootChildNameList();
            XmlNode ExpertPageNodeRef = XmlContent.SelectSingleNode(XMLRootChildList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_PAGE]);
            XmlNode BasicPageNodeRef = XmlContent.SelectSingleNode(XMLRootChildList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_BASIC_PAGE]);

            bRet = ReplaceModelName(ExpertPageNodeRef, m_lsModelName);
            if (bRet == true)
            {
                bRet = ReplaceModelName(BasicPageNodeRef, m_lsModelName);
            }
            return bRet;
        }

        private bool ReplaceModelName(XmlNode XmlParentNode, List<string> lsModelName)
        {
            bool bRet = false;
            bool bNotRun = true;

            if ((null != lsModelName) && (0 < lsModelName.Count))
            {
                bRet = true;
                do
                {
                    XmlNode ModelNode = XmlParentNode.SelectSingleNode(".//page[contains(@*,'##MODEL##')]");
                   
                    if (null == ModelNode)
                    {
                        if (true == bNotRun)
                        {
                            bRet = false;
                        }
                        break;
                    }
                    
                    foreach (string Element in lsModelName)
                    {
                        Program.SystemDisplayDebugInfo("##MODEL## found: " + Element + "\r\n");

                        XmlNode NewNode = ModelNode.CloneNode(true);
                        NewNode.InnerXml = NewNode.InnerXml.ToString().Replace("##MODEL##", Element);
                        foreach (XmlAttribute attribute in NewNode.Attributes)
                        {
                            if (attribute.Value.Contains("##MODEL##"))
                            {
                                attribute.Value = attribute.Value.Replace("##MODEL##", Element);
                            }
                        }
                        ModelNode.ParentNode.InsertAfter(NewNode, ModelNode);
                    }
                    ModelNode.ParentNode.RemoveChild(ModelNode);
                    bNotRun = false;
                }
                while (true);
            }
            else
            {
                XmlNode ModelNode = XmlParentNode.SelectSingleNode(".//page[contains(@*,'##MODEL##')]");
                if (null == ModelNode)
                {
                    bRet = true;
                }
                else
                {
                    bRet = false;
                }
            }
            return bRet;
        }

        private void UpdateXmlConFigInfoObject(CXmlConfigInfoObject InputObject,
                                               XmlNode ReportContent, 
                                               string ProductName,
                                               string ReleaseNumber,
                                               string MergeNumber,
                                               string DateRelease,
                                               string ReleaseVersion, 
                                               string ImagePath, 
                                               string HelpFilePath,
                                               string ReportFolderPath,
                                               string HardContentFilePath)
        {
            InputObject.SetXmlReportContent(ReportContent);
            InputObject.SetProductName(ProductName);
            InputObject.SetReleaseNumber(ReleaseNumber);
            InputObject.SetMergeRelease(MergeNumber);
            InputObject.SetDateRelease(DateRelease);
            InputObject.SetReleaseVersion(ReleaseVersion);
            InputObject.SetReportFilePath(ReportFolderPath);
            InputObject.SetImagePath(ImagePath);
            InputObject.SetHelpFilePath(HelpFilePath);
            InputObject.SetHardContentFilePath(HardContentFilePath);
        }

        private void CleanCommentedPageName(XmlNode XmlContent)
        {
            string[] XMLRootChildList = CXMLNodeName.GetInstance().GetRootChildNameList();
            XmlNode ExpertPageNodeRef = XmlContent.SelectSingleNode(XMLRootChildList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_PAGE]);
            XmlNode BasicPageNodeRef = XmlContent.SelectSingleNode(XMLRootChildList[(int)XML_ROOT_CHILD_NODE_ID_TYPE.ROOT_BASIC_PAGE]);

            RemoveCommentedPageName(ExpertPageNodeRef);
            RemoveCommentedPageName(BasicPageNodeRef);
        }

        private void RemoveCommentedPageName(XmlNode rootPageNode)
        {
            string sSearchInfo = string.Format(FM_SELECTION_NODES,
                                  rootPageNode.Name,
                                  PAGE_NODE_NAME,
                                  KEYWORD_TITLE,
                                  CONFIG_PAGE_NAME);

            XmlNode ConfigurationPage = rootPageNode.SelectSingleNode(sSearchInfo);
            if (ConfigurationPage != null)
            {
                sSearchInfo = string.Format(FW_SELECTION_NAME_NODES,
                         ConfigurationPage.Name,
                         COMMENT_KEYWORD);

                XmlNodeList PageRefList = ConfigurationPage.SelectNodes(sSearchInfo);

                if ((PageRefList != null) && (PageRefList.Count > 0))
                {
                    foreach (XmlNode pageNode in PageRefList)
                    {
                        if (pageNode.InnerText.Contains(REMOVE_DETECT_KEYWORD))
                        {
                            pageNode.ParentNode.RemoveChild(pageNode);
                        }
                    }
                }
            }
        }

        private bool CheckHardContentFile()
        {
            bool bRet = false;

            if (String.IsNullOrEmpty(m_HardContentFilePath) == false)
            {
                IXMLFileProcess XmlFileReader = CFactoryXmlFileProcessor.GetInstance().GetHardContentReader();
                if (XmlFileReader != null)
                {
                    if (XmlFileReader.XMLLoadingFile(m_HardContentFilePath, true))
                    {
                        bRet = true;
                        XmlNode RootNode = XmlFileReader.XMLGetRootNode();
                        XmlNode GroupConfigInfo = m_XmlConfigInfo.GetConfigFeatureInfo(CXMLConfigKeywords.KW_CONFIG_GROUP);
                        if ((GroupConfigInfo != null) && (GroupConfigInfo.HasChildNodes == true))
                        {
                            foreach (XmlNode ElementNode in GroupConfigInfo.ChildNodes)
                            {
                                if (ElementNode != null)
                                {
                                    string sGroupName = (ElementNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_NAME);
                                    string sSearchInfo = string.Format(FW_SELECTION_ATTRIBUTE_NODE,
                                                                        GROUP_KEYWORD,
                                                                        sGroupName);
                                    XmlNode FoundNode = RootNode.SelectSingleNode(sSearchInfo);
                                    if (FoundNode == null)
                                    {
                                        string sMessage = "\r\nERROR:: Missing hard content section \"" + sGroupName + "\" !!!\r\n\r\n";
                                        Program.SystemHandleStatusInfo(sMessage);
                                        bRet = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return bRet;
        }
    }
}
