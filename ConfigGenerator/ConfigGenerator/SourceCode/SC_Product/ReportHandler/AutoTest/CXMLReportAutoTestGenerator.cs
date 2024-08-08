using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ConfigGenerator
{
    public class Event_AutoTestReportGenerator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CXMLReportAutoTestGenerator : AReportGenerator
    {
        private CXMLFileProcess m_XMLAutoTestFileProcess = null;

        private const string AUTO_TEST_FOLDER_NAME = "AutoTestPackage";

        private const string PRODUCT_NAME = "_PRODUCT_NAME_";
        private const string RELEASE_SOFTWARE_NUMBER = "__APPL_SW_VERSION__";
        private const string Date = "_TIME_CREATED_";

        private const string ROOT_CHILD_NODE_OF_INTERFACETYPES_NODE = "interface";
        private const string CONST_VALUE_OF_MODEL_NODE = "PC";
        private const string CONST_VALUE_OF_DEFAULT_NODE = "None";

        private const string CHILD_NODE_OF_INTERFACECLASS_NODE = "interface";
        private const string ATTRIBUTE_OF_INTERFACE_NODE = "index";
        private const string INNER_TEXT_INTERFACE_NODE_PREFIX = "Class-";

        private string m_sResoureFile = "ConfigGenerator.ResourceFiles.XMLTemplateAutoTestFile.xml";
        private Assembly m_Assembly = null;
        private Stream m_FileStream = null;
        private XmlDocument m_XMLFileApp = null;

        private string m_ProductName = null;
        private string m_ReleaseVersion = null;
        private string m_ReleaseNumber = null;
        private string m_MergeRelease = null;
        private string m_DateRelease = null;
        private string m_ReportFilePath = null;

        private const string INTERFACE_NAME_PREFIX = "IF_";
        private const string VALUE_INTERFACE_MEMBER_PREFIX = "HA";

        private int NUMBER_OF_CONFIG_INFO_OBJECT = 2;

        private string[] m_cRootChildNodeNameList = CXMLAutoTestNodeName.GetInstance().GetRootChildNameList();

        private List<INTERFACE_CLASS> m_lsInterfaceClass = null;

        private List<AXMLSubHandler> m_SubHandlerList = null;

        private List<IShareObject> m_ShareObjectList = new List<IShareObject>();

        public CXMLReportAutoTestGenerator() : base(new Event_AutoTestReportGenerator().m_MyEvent)
        {
            m_XMLAutoTestFileProcess = CFactoryXmlFileProcessor.GetInstance().GetAutoTestXmlFileProcessor();
            m_SubHandlerList = CFactoryXMLSubhandlerAutoTest.GetInstance().GetAutoTestSubHanlerList();

            try
            {
                m_XMLFileApp = new XmlDocument();
                m_Assembly = Assembly.GetExecutingAssembly();
                m_FileStream = m_Assembly.GetManifestResourceStream(m_sResoureFile);
                m_XMLFileApp.Load(m_FileStream);
            }
            catch { }
        }
        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT))
                    {
                        ProcessConfigInfo((CXmlConfigInfoObject)oData);
                        ReportFileGenerator();
                        m_ShareObjectList.Add(oData);
                    }
                    else if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                    {
                        ProcessConfigInfo((CSourceInfoObject)oData);
                        UpdateInterfacePartXMLFile();
                        m_ShareObjectList.Add(oData);
                    }

                    if (m_ShareObjectList.Count == NUMBER_OF_CONFIG_INFO_OBJECT)
                    {
                        Initialize(m_ShareObjectList);
                        m_ShareObjectList.RemoveRange(0, NUMBER_OF_CONFIG_INFO_OBJECT);
                    }

                    break;
            }
        }

        public override bool ReportDataHandling(CIntegratedDataObject oDataIn)
        {
            bool bRet = false;
            if ((m_SubHandlerList != null) && (m_SubHandlerList.Count > 0))
            {
                bRet = true;
                for (int i = 0; i < m_SubHandlerList.Count; i++)
                {
                    if (!m_SubHandlerList[0].DataHandling(oDataIn))
                    {
                        bRet = false;
                    }
                }
            }
            return bRet;
        }

        private void ProcessConfigInfo(CXmlConfigInfoObject oDataIn)
        {
            m_ProductName    = oDataIn.GetProductName();
            m_ReleaseNumber  = oDataIn.GetReleaseNumber();
            m_ReleaseVersion = oDataIn.GetReleaseVersion();
            m_DateRelease    = oDataIn.GetDateRelease();
            m_MergeRelease   = oDataIn.GetMergeRelease();
            m_ReportFilePath = CreateAutoTestReportFilePath(m_ProductName, m_ReleaseNumber);
        }

        private string CreateAutoTestReportFilePath(string ProductName, string ReleaseNumber)
        {
            // string sTempPath = Directory.GetCurrentDirectory();
            string sTempPath = System.AppDomain.CurrentDomain.BaseDirectory;
            sTempPath += Path.DirectorySeparatorChar + AUTO_TEST_FOLDER_NAME;
            sTempPath += Path.DirectorySeparatorChar + string.Format("{0}_{1}", ProductName, ReleaseNumber);
            sTempPath += Path.DirectorySeparatorChar + string.Format("{0}_{1}.xml", ProductName, ReleaseNumber);

            return sTempPath;
        }

        private void ReportFileGenerator()
        {         
            if (m_XMLFileApp != null)
            {
                // Check to create folder
                string sTempPath = Path.GetDirectoryName(m_ReportFilePath);
                if (!Directory.Exists(sTempPath))
                {
                    Directory.CreateDirectory(sTempPath);
                    Program.SetUnixFileFullPermissions(sTempPath);
                }

                // Remove file if it already exist
                if (File.Exists(m_ReportFilePath))
                {
                    File.Delete(m_ReportFilePath);
                }

                m_XMLFileApp.Save(m_ReportFilePath);
                ProcessReportFileInfo(m_ReportFilePath);
            }
        }

        private bool ProcessReportFileInfo(string sFilePath)
        {
            bool bRet = false;

            CTextFileProcess TextAutoTestFile = CFactoryXmlFileProcessor.GetInstance().GetAutoTestTextFileProcessor();
            if ((TextAutoTestFile != null) && (string.IsNullOrEmpty(sFilePath) == false))
            {
                if (TextAutoTestFile.Open(sFilePath))
                {
                    TextAutoTestFile.FindAndReplace(PRODUCT_NAME, m_ProductName);
                    TextAutoTestFile.FindAndReplace(RELEASE_SOFTWARE_NUMBER, m_ReleaseNumber);
                    TextAutoTestFile.FindAndReplace(Date, m_DateRelease);

                    TextAutoTestFile.Close();
                    bRet = true;
                }
            }

            return bRet;
        }

        private void ProcessConfigInfo(CSourceInfoObject oDataIn)
        {
            m_lsInterfaceClass = oDataIn.GetInterfaceClass();
        }

        private void UpdateInterfacePartXMLFile()
        {
            bool bUpdateSuccessFlag = true;
            CStringObject ResultObj = new CStringObject();
            string sContent = "KO";

            if (m_XMLAutoTestFileProcess.XMLLoadingFile(m_ReportFilePath, false))
            {
                List<REF_NODE_TYPE> lsReturnNode = null;
                REF_NODE_TYPE RefNode = new REF_NODE_TYPE()
                {
                    Node = m_XMLAutoTestFileProcess.XMLGetRootNode(),
                    RefPathNameList = new List<string>()
                };

                if (RefNode.Node.HasChildNodes)
                {
                    lsReturnNode = m_XMLAutoTestFileProcess.XMLSearchNodes(m_cRootChildNodeNameList[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.INTERFACE_TYPES], RefNode);
                    if ((lsReturnNode != null) && (1 == lsReturnNode.Count))
                    {
                        if (UpdateInterfaceTypesNode(lsReturnNode[0].Node) == false)
                        {
                            bUpdateSuccessFlag = false;
                        }
                    }

                    lsReturnNode = m_XMLAutoTestFileProcess.XMLSearchNodes(m_cRootChildNodeNameList[(int)AUTO_TEST_XML_ROOT_CHILD_NODE_ID_TYPE.INTERFACE_CLASS], RefNode);
                    if ((lsReturnNode != null) && (1 == lsReturnNode.Count))
                    {
                        if (UpdateInterfaceClassNode(lsReturnNode[0].Node) == false)
                        {
                            bUpdateSuccessFlag = false;
                        }
                    }
                }
            }

            if (bUpdateSuccessFlag == true)
            {
                sContent = "OK";
            }
            ResultObj.AddStringData(sContent);
            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_UPDATE_CONFIG_INFO, ResultObj);
        }

        private bool UpdateInterfaceTypesNode(XmlNode ParentNode)
        {
            bool bRet = false;
            XmlNode newNode = null;
            REF_NODE_TYPE Ref_Node = new REF_NODE_TYPE();
            Ref_Node.Node = ParentNode;

            if (ParentNode != null)
            {
                foreach (INTERFACE_CLASS InterfaceClassElement in m_lsInterfaceClass)
                {
                    foreach (INTERFACE_CLASS_MEMBER InterfaceClassMemberElement in InterfaceClassElement.m_lsInterfaceMember)
                    {
                        newNode = CreateInterfaceNode(InterfaceClassElement, InterfaceClassMemberElement);
                        if (m_XMLAutoTestFileProcess.XMLAddNode(newNode, Ref_Node) == true)
                        {
                            bRet = true;
                        }
                        else
                        {
                            bRet = false;
                            break;
                        }
                    }
                    if (bRet == false) break;
                }
            }        
            return bRet;
        }

        private XmlNode CreateInterfaceNode(INTERFACE_CLASS InterfaceClassElement, INTERFACE_CLASS_MEMBER InterfaceClassElementMember)
        {
            XmlNode tempNode = null;
            XmlNode childNode = null;
            string sValueOfClassMember = null;
            string sNameOfClassMember = null;
            string sClassNumber = null;
            string[] s_InterfaceChildNodeNameList = CXMLAutoTestNodeName.GetInstance().GetInterfaceChildNodeNameList();

            sNameOfClassMember = InterfaceClassElementMember.m_sInterfaceName.Replace(INTERFACE_NAME_PREFIX, "");
            sNameOfClassMember = sNameOfClassMember.Replace('_', '-');
            sValueOfClassMember = InterfaceClassElementMember.m_sCommand.TrimStart(VALUE_INTERFACE_MEMBER_PREFIX.ToCharArray());
            sClassNumber = InterfaceClassElement.m_uiClassNumber.ToString();

            tempNode = m_XMLAutoTestFileProcess.XMLCreateNode(ROOT_CHILD_NODE_OF_INTERFACETYPES_NODE);
            for (int i = 0; i < (int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.TOTAL_CHILDE_NODE_IN_INTERFACE_NODE; i++)
            {
                childNode = m_XMLAutoTestFileProcess.XMLCreateNode(s_InterfaceChildNodeNameList[i]);
                switch (i)
                {
                    case (int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.NAME:
                        childNode.InnerText = sNameOfClassMember;
                        break;
                    case (int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.VALUE:
                        childNode.InnerText = sValueOfClassMember;
                        break;
                    case (int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.CLASS:
                        childNode.InnerText = sClassNumber;
                        break;
                    case (int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.MODEL:
                        childNode.InnerText = CONST_VALUE_OF_MODEL_NODE;
                        break;
                    case (int)AUTO_INTERFACE_CHILD_NODE_ID_TYPE.DEFAULT:
                        childNode.InnerText = CONST_VALUE_OF_DEFAULT_NODE;
                        break;
                    default:
                        childNode.InnerText = "";
                        break;
                }
                tempNode.AppendChild(childNode);
            }    
            return tempNode;
        }

        private bool UpdateInterfaceClassNode(XmlNode ParentNode)
        {
            bool bRet = false;
            string InterfaceClassName = null;
            XmlNode newNode = null;
            REF_NODE_TYPE Ref_Node = new REF_NODE_TYPE();
            Ref_Node.Node = ParentNode;

            if (ParentNode != null)
            {
                uint uiIndex = 0;
                foreach (INTERFACE_CLASS InterfaceClassElement in m_lsInterfaceClass)
                {
                    InterfaceClassName = INNER_TEXT_INTERFACE_NODE_PREFIX + InterfaceClassElement.m_uiClassNumber.ToString();
                    newNode = m_XMLAutoTestFileProcess.XMLCreateNode(CHILD_NODE_OF_INTERFACECLASS_NODE);
                    (newNode as XmlElement).SetAttribute(ATTRIBUTE_OF_INTERFACE_NODE, uiIndex.ToString());
                    newNode.InnerText = InterfaceClassName;
                    if (m_XMLAutoTestFileProcess.XMLAddNode(newNode, Ref_Node) == true)
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
                        break;
                    }
                    uiIndex++;
                }
            }
            return bRet;
        }

        public override bool ReportDataFinalizing(CIntegratedDataObject oDataIn)
        {
            bool bRet = true;
            //To Do
            return bRet;
        }

        private bool Initialize(List<IShareObject> oDataList)
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
    }
}
