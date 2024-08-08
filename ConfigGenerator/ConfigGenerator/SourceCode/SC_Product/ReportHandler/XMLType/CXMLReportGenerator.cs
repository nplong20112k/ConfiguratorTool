using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace ConfigGenerator
{
    public class Event_XMLReportGenerator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CXMLReportGenerator: AReportGenerator
    {
        private List<AXMLSubHandler> m_SubHandlerList = null;

        private XmlNode m_XmlReportContent = null;
        private IXMLFileProcess m_XMLFileProcess = null;
        private CXmlConfigInfoObject m_XmlConfigInfo = null;
        private CXmlHardContentProcessor m_XMLHardContentProcess = null;

        private string m_ProductName = null;
        private string m_ReleaseVersion = null;
        private string m_ReleaseNumber = null;
        private string m_MergeRelease = null;
        private string m_DateRelease = null;
        private string m_ReportFilePath = null;
        private string m_ImagePath = null;
        private string m_HelpFilePath = null;

        private List<IShareObject> m_ShareObjectList = new List<IShareObject>();

        private const string PRODUCT_NAME = "__PROD_NAME__";
        private const string RELEASE_SW_NUMBER = "__APPL_SW_VERSION__";
        private const string MERGE_RELEASE = "__MERGE_RELEASE__";
        private const string RELEASE_VERSION = "__RELEASE_VERSION__";
        private const string DATE = "__APPL_SW_DATE__";

        private const string KW_CONFIG_PRINCONF = "_PRINTCONF_";
        private const string KW_CONFIG_FUNCTION_SUPPORT = "_FUNCTION_SUPPORT_";
        private const string KW_SUPPORT_2D = "Support Label 2D";
        private const string KW_SUPPORT_IMAGE_CAPTURE = "Support Image Capture";
        private const string KW_SUPPORT_IMAGE_CAPTURE_RS232 = "Support Image Capture RS232";
        private const string KW_SUPPORT_IMAGE_CAPTURE_USB = "Support Image Capture USB";
        private const string KW_SUPPORT_IMAGE_CAPTURE_PREVIEW = "Support Image Capture Preview";
        private const string KW_SUPPORT_IMAGE_CAPTURE_SENSOR_OPTION = "Support Image Capture Sensor Option";
        private const string KW_SUPPORT_CAPTURE_ON_DECODE_LABEL = "Support Capture On Decode Label";

        private const string KW_CONFIG_VALUE_PRE = "pre";
        private const string KW_CONFIG_VALUE_POST = "post";
        private const string KW_CONFIG_VALUE_BOTH = "both";

        private const string KW_ROOT_PRINCONF_NAME = "printConf";
        private const string KW_PRINCONF_NODE_NAME = "value";
        private const string KW_PRINCONF_ATTRIBUTE_NAME = "name";
        private const string KW_PRINCONF_ATTRIBUTE_TITLE = "title";
        private const string KW_PRINCONF_ATTRIBUTE_PROTECTION = "protection";
        private const string KW_PRINCONF_ATTRIBUTE_VALUE = "value";
        private const string KW_PRINCONF_ATTRIBUTE_TEXT = "text";
        private const string KW_PRINCONF_ATTRIBUTE_POSITION = "position";
        private const string KW_PRINCONF_ATTRIBUTE_LABEL = "label";

        private const string KW_SHA256 = "sha256";

        private int NUMBER_OF_CONFIG_INFO_OBJECT = 2;

        public CXMLReportGenerator()
            : base(new Event_XMLReportGenerator().m_MyEvent)
        {
            m_SubHandlerList = CFactoryXMLSubHandler.GetInstance().GetSubHandlerList();
            m_XMLHardContentProcess = CFactoryXmlFileProcessor.GetInstance().GetHardContentProcessor();
            m_XMLFileProcess = CFactoryXmlFileProcessor.GetInstance().GetXmlFileProcessor();
          
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {                
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    {
                        CStringObject ResultObj = new CStringObject();
                        string sContent = "FAIL";
                        if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT))
                        {
                            bool bRet = false;
                            
                            UpdatePreProcess(oData);

                            string sPath = (oData as CXmlConfigInfoObject).GetHardContentFilePath();
                            if (m_XMLHardContentProcess.XMLLoadingFile(sPath))
                            {
                                bRet = true;
                            }

                            if (bRet == true)
                            {
                                if (m_XMLFileProcess.XMLCreateFile(m_ReportFilePath, m_XmlReportContent))
                                {
                                    bRet = ProcessReportFileInfo();
                                }
                            }

                            if (bRet == true)
                            {
                                if (String.IsNullOrEmpty(Path.GetFileName(m_ImagePath)) == false)
                                {
                                    CopyFile(m_ImagePath, Path.GetDirectoryName(m_ReportFilePath));
                                }
                                if (String.IsNullOrEmpty(Path.GetFileName(m_HelpFilePath)) == false)
                                {
                                    CopyFile(m_HelpFilePath, Path.GetDirectoryName(m_ReportFilePath));
                                }

                                m_ShareObjectList.Add(oData);
                            }
                        }
                        else if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                        {
                            m_ShareObjectList.Add(oData);
                        }

                        if (m_ShareObjectList.Count == NUMBER_OF_CONFIG_INFO_OBJECT)
                        {
                            if (Initialize(m_ShareObjectList) == true)
                            {
                                sContent = "OK";
                            }
                            ResultObj.AddStringData(sContent);

                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_UPDATE_CONFIG_INFO, ResultObj);
                            m_ShareObjectList.RemoveRange(0, m_ShareObjectList.Count); 
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private  bool Initialize(List<IShareObject> oDataList)
        {
            bool bRet = false;

            if ((m_SubHandlerList != null) && (m_SubHandlerList.Count > 0))
            {
                bRet = true;
                for (int i = 0; i < m_SubHandlerList.Count; i++)
                {
                    if (m_SubHandlerList[i].Initialize(oDataList) == false)
                    {
                        bRet = false;
                    }
                }
            }

            return bRet;
        }

        public override bool ReportDataHandling(CIntegratedDataObject oDataIn)
        {
            bool bRet = false;
            
            if ((m_SubHandlerList != null) && (m_SubHandlerList.Count > 0))
            {
                bRet = true;
                for (int i = 0; i < m_SubHandlerList.Count; i++)
                {
                    if (!m_SubHandlerList[i].DataHandling(oDataIn))
                    {
                        bRet = false;
                    }
                }
            }

            return bRet;
        }

        public override bool ReportDataFinalizing(CIntegratedDataObject oDataIn)
        {
            bool bRet = false;

            if ((m_SubHandlerList != null) && (m_SubHandlerList.Count > 0))
            {
                bRet = true;
                for (int i = 0; i < m_SubHandlerList.Count; i++)
                {
                    if (!m_SubHandlerList[i].Finalize(oDataIn))
                    {
                        bRet = false;
                    }
                }
            }

            AddSHA256();

            return bRet;
        }

        private void AddSHA256()
        {
            string sResult;
            string sDate = CConfigTool.GetInstance().sDateRelease;
            using (SHA256 SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(m_ReportFilePath))
                {
                    byte[] fileData = new byte[fileStream.Length + sDate.Length];
                    fileStream.Read(fileData, 0, (int)fileStream.Length);
                    byte[] dateArray = Encoding.ASCII.GetBytes(sDate);
                    dateArray.CopyTo(fileData, (int)fileStream.Length);
                    byte[] hashValue = SHA256.ComputeHash(fileData);
                    sResult = BitConverter.ToString(hashValue).Replace("-", String.Empty).ToLower();
                }
            }

            XmlNode RootNode = m_XMLFileProcess.XMLGetRootNode();
            REF_NODE_TYPE ParentNodeRet = new REF_NODE_TYPE() { Node = RootNode };
            XmlNode SHA256Node = m_XMLFileProcess.XMLCreateNode(KW_SHA256);
            SHA256Node.InnerText = sResult;
            m_XMLFileProcess.XMLAddNode(SHA256Node, ParentNodeRet);
        }

        private bool ProcessReportFileInfo()
        {
            bool bRet = false;
            if (m_XMLFileProcess != null)
            {
                CTextFileProcess TextFileApp = CFactoryXmlFileProcessor.GetInstance().GetTextFileProcessor();
                if (TextFileApp != null)
                {
                    if ((TextFileApp != null) && (m_ReportFilePath != null))
                    {
                        if (TextFileApp.Open(m_ReportFilePath))
                        {
                            TextFileApp.FindAndReplace(PRODUCT_NAME, m_ProductName);
                            TextFileApp.FindAndReplace(RELEASE_SW_NUMBER, m_ReleaseNumber);
                            TextFileApp.FindAndReplace(MERGE_RELEASE, m_MergeRelease);
                            TextFileApp.FindAndReplace(RELEASE_VERSION, m_ReleaseVersion);
                            TextFileApp.FindAndReplace(DATE, m_DateRelease);

                            TextFileApp.Close();
                            bRet = true;
                        }
                    }
                }

                if (bRet == true)
                {
                    m_XMLFileProcess.XMLLoadingFile(m_ReportFilePath, false);
                    bRet = ProcessConfigInfo(m_XmlConfigInfo);
                }
            }

            return bRet;
        }

        private void CopyFile(string sSrcFileFullPath, string sDestFolderPath)
        {
            string sDestFileFullPath = Path.Combine(sDestFolderPath, Path.GetFileName(sSrcFileFullPath));
            File.Copy(sSrcFileFullPath, sDestFileFullPath, true);
        }

        private bool ProcessConfigInfo(CXmlConfigInfoObject XmlConfigInfoObject)
        {
            bool bRet = false;

            if (XmlConfigInfoObject != null)
            {
                XmlNode FuncSupportConfigInfoList = XmlConfigInfoObject.GetConfigFeatureInfo(KW_CONFIG_FUNCTION_SUPPORT);
                XmlNode PrinconfConfigInfoList = XmlConfigInfoObject.GetConfigFeatureInfo(KW_CONFIG_PRINCONF);
                XmlNode MessageConfigInfoList = XmlConfigInfoObject.GetConfigFeatureInfo(CXMLConfigKeywords.KW_CONFIG_MESSAGE);

                if ((m_XMLFileProcess.XMLGetRootNode() != null) &&
                    (m_XMLFileProcess.XMLGetRootNode().HasChildNodes == true))
                {
                    bRet = true;
                    XmlNode RootNode = m_XMLFileProcess.XMLGetRootNode();
                    XmlNode PrinconfRootNodeRef = m_XMLFileProcess.XMLGetRootNode().SelectSingleNode(KW_ROOT_PRINCONF_NAME);
                    XmlNode MessageRootNodeRef = m_XMLFileProcess.XMLGetRootNode().SelectSingleNode(CXMLConfigKeywords.KW_ROOT_MESSAGE_NAME);

                    if (ProcessFunctSupportConfigInfo(FuncSupportConfigInfoList, RootNode) == false)
                    {
                        bRet = false;
                    }

                    if (ProcessPrintconfConfigInfo(PrinconfConfigInfoList, PrinconfRootNodeRef) == false)
                    {
                        bRet = false;
                    }

                    if (ProcessMessageConfigInfo(MessageConfigInfoList, MessageRootNodeRef) == false)
                    {
                        bRet = false;
                    }
                }
            }

            return bRet;
        }

        private bool ProcessFunctSupportConfigInfo(XmlNode FuncSupportConfigInfo, XmlNode RootNode)
        {
            bool bRet = false;

            if ((FuncSupportConfigInfo != null) && (RootNode != null))
            {
                string sNameFunction = null;
                string sValueFunction = null;
                List<string[]> FuncSupportList = new List<string[]>();

                foreach (XmlNode ElementNode in FuncSupportConfigInfo.ChildNodes)
                {
                    string sFunctionName = (ElementNode as XmlElement).GetAttribute("name");
                    string sFunctionState = (ElementNode as XmlElement).GetAttribute("state");
                    if (sFunctionState == "disable")
                    {
                        sValueFunction = "true";
                    }
                    else
                    {
                        sValueFunction = "false";
                    }
                    switch (sFunctionName)
                    {
                        case KW_SUPPORT_IMAGE_CAPTURE:
                            sNameFunction = "disableImageCapture";
                            FuncSupportList.Add(new string[] { sNameFunction, sValueFunction });
                            break;

                        case KW_SUPPORT_IMAGE_CAPTURE_RS232:
                            sNameFunction = "disableImageCaptureOnRS232";
                            FuncSupportList.Add(new string[] { sNameFunction, sValueFunction });
                            break;

                        case KW_SUPPORT_IMAGE_CAPTURE_USB:
                            sNameFunction = "disableImageCaptureOnUSB";
                            FuncSupportList.Add(new string[] { sNameFunction, sValueFunction });
                            break;

                        case KW_SUPPORT_CAPTURE_ON_DECODE_LABEL:
                            sNameFunction = "disableOnDecodeLabel";
                            FuncSupportList.Add(new string[] { sNameFunction, sValueFunction });
                            break;

                        case KW_SUPPORT_IMAGE_CAPTURE_PREVIEW:
                            sNameFunction = "disableImageCaptureWithPreview";
                            FuncSupportList.Add(new string[] { sNameFunction, sValueFunction });
                            break;

                        case KW_SUPPORT_IMAGE_CAPTURE_SENSOR_OPTION:
                            sNameFunction = "disableSensorOptions";
                            FuncSupportList.Add(new string[] { sNameFunction, sValueFunction });
                            break;

                        case KW_SUPPORT_2D:
                            sNameFunction = "subtype";
                            sValueFunction = "2D";
                            FuncSupportList.Add(new string[] { sNameFunction, sValueFunction });
                            break;

                        default:
                            break;
                    }
                }

                bRet = m_XMLFileProcess.XMLChangeAttributes(RootNode, FuncSupportList);
            }

            return bRet;
        }

        private bool ProcessPrintconfConfigInfo(XmlNode PrintconfRawConfigInfo, XmlNode PrintconfRootNodeRef)
        {
            bool bRet = false;

            if ((PrintconfRawConfigInfo != null) && (PrintconfRootNodeRef != null) && (PrintconfRootNodeRef.Name == KW_ROOT_PRINCONF_NAME))
            {
                bRet = true;
                XmlNode PreRootNodeRef = PrintconfRootNodeRef.SelectSingleNode(KW_CONFIG_VALUE_PRE);
                XmlNode PostRootNodeRef = PrintconfRootNodeRef.SelectSingleNode(KW_CONFIG_VALUE_POST);
                REF_NODE_TYPE RefNode = new REF_NODE_TYPE();

                foreach (XmlNode ElementNode in PrintconfRawConfigInfo.ChildNodes)
                {
                    if ((ElementNode != null) && (ElementNode.NodeType != XmlNodeType.Comment))
                    {
                        string sPrintconfText = (ElementNode as XmlElement).GetAttribute(KW_PRINCONF_ATTRIBUTE_TEXT);
                        string sPrintconfPosition = (ElementNode as XmlElement).GetAttribute(KW_PRINCONF_ATTRIBUTE_POSITION);
                        string sPrintconfProtection = (ElementNode as XmlElement).GetAttribute(KW_PRINCONF_ATTRIBUTE_PROTECTION);
                        string sPrintconfLabel = (ElementNode as XmlElement).GetAttribute(KW_PRINCONF_ATTRIBUTE_LABEL);

                        if ((sPrintconfText != null) && (sPrintconfText != string.Empty) &&
                             (sPrintconfPosition != null) && (sPrintconfPosition != string.Empty) &&
                             (sPrintconfProtection != null) && (sPrintconfProtection != string.Empty) &&
                             (sPrintconfLabel != null) && (sPrintconfLabel != string.Empty))
                        {
                            XmlNode PrinconfContentNode = m_XMLFileProcess.XMLCreateNode(KW_PRINCONF_NODE_NAME);
                            if (PrinconfContentNode != null)
                            {
                                (PrinconfContentNode as XmlElement).SetAttribute(KW_PRINCONF_ATTRIBUTE_NAME, sPrintconfText);
                                (PrinconfContentNode as XmlElement).SetAttribute(KW_PRINCONF_ATTRIBUTE_TITLE, sPrintconfText);
                                (PrinconfContentNode as XmlElement).SetAttribute(KW_PRINCONF_ATTRIBUTE_PROTECTION, sPrintconfProtection);
                                (PrinconfContentNode as XmlElement).SetAttribute(KW_PRINCONF_ATTRIBUTE_VALUE, sPrintconfLabel);

                                if ((sPrintconfPosition == KW_CONFIG_VALUE_PRE) || (sPrintconfPosition == KW_CONFIG_VALUE_BOTH))
                                {
                                    RefNode.Node = PreRootNodeRef;
                                    if (m_XMLFileProcess.XMLAddNode(PrinconfContentNode.CloneNode(true), RefNode) == false)
                                    {
                                        bRet = false;
                                    }
                                }

                                if ((sPrintconfPosition == KW_CONFIG_VALUE_POST) || (sPrintconfPosition == KW_CONFIG_VALUE_BOTH))
                                {
                                    RefNode.Node = PostRootNodeRef;
                                    if (m_XMLFileProcess.XMLAddNode(PrinconfContentNode.CloneNode(true), RefNode) == false)
                                    {
                                        bRet = false;
                                    }
                                }
                            }
                            else
                            {
                                bRet = false;
                            }
                        }
                        else
                        {
                            // missing detected !! TODO
                        }
                    }
                }
            }

            return bRet;
        }

        private bool ProcessMessageConfigInfo(XmlNode MessageRawConfigInfo, XmlNode MessageRootNodeRef)
        {
            bool bRet = false;

            if ((MessageRawConfigInfo != null) &&
                (MessageRootNodeRef != null) &&
                (MessageRootNodeRef.Name == CXMLConfigKeywords.KW_ROOT_MESSAGE_NAME))
            {
                bRet = true;
                List<XmlNode> MessageSupportList = new List<XmlNode>();
                REF_NODE_TYPE RefNode = new REF_NODE_TYPE() { Node = MessageRootNodeRef };

                foreach (XmlNode ElementNode in MessageRawConfigInfo.ChildNodes)
                {
                    if ((ElementNode != null) && (ElementNode.NodeType != XmlNodeType.Comment))
                    {
                        string sMessageName = (ElementNode as XmlElement).GetAttribute(CXMLConfigKeywords.KW_ATTRIBUTE_NAME);
                        XmlNode MessageContent = m_XMLHardContentProcess.GetMessages(sMessageName);
                        if (MessageContent != null)
                        {
                            XmlNode MessageNode = m_XMLFileProcess.XMLImportNode(MessageContent.CloneNode(true));
                            if (m_XMLFileProcess.XMLAddNode(MessageNode, RefNode) == false)
                            {
                                bRet = false;
                            }
                        }
                        else
                        {
                            // missing detected !! TODO
                        }
                    }
                }
            }

            return bRet;
        }

        private void UpdatePreProcess(IShareObject oData)
        {
            m_XmlConfigInfo = (CXmlConfigInfoObject)oData;
            m_XmlReportContent = m_XmlConfigInfo.GetXMlReportContent();

            m_ProductName = m_XmlConfigInfo.GetProductName();
            m_ReleaseNumber = m_XmlConfigInfo.GetReleaseNumber();
            m_ReleaseVersion = m_XmlConfigInfo.GetReleaseVersion();
            m_ReportFilePath = m_XmlConfigInfo.GetReportFilePath();
            m_DateRelease = m_XmlConfigInfo.GetDateRelease();
            m_MergeRelease = m_XmlConfigInfo.GetMergeRelease();
            m_ImagePath = m_XmlConfigInfo.GetImagePath();
            m_HelpFilePath = m_XmlConfigInfo.GetHelpFilePath();
        }
    }
}
