using System.Collections.Generic;
using System.Xml;

namespace ConfigGenerator
{
    class CXmlConfigInfoObject : AShareObject
    {
        private XmlNode m_XmlReportContent;

        private string m_ProductName    = null;
        private string m_ReleaseNumber  = null;
        private string m_MergeRelease   = null;
        private string m_DateRealease   = null;
        private string m_ReportFilePath = null;
        private string m_ReleaseVersion = null;
        private string m_ImagePath      = null;
        private string m_HelpFilePath   = null;
        private string m_HardContentFilePath = null;

        private struct CONFIG_DATA_TYPE
        {
            public string m_FeatureName;
            public XmlNode m_FeatureContent;
        }

        private List<CONFIG_DATA_TYPE> m_ConfigFeatureList = null;

        public CXmlConfigInfoObject()
            : base(SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT)
        {
            m_ConfigFeatureList = new List<CONFIG_DATA_TYPE>();
        }

        public void SetConfigFeatureInfo(string sName, XmlNode ConfigNode)
        {
            if ((ConfigNode != null) && (sName != null))
            {
                CONFIG_DATA_TYPE TempData = new CONFIG_DATA_TYPE() {
                    m_FeatureName = sName,
                    m_FeatureContent = ConfigNode.CloneNode(true),
                };

                m_ConfigFeatureList.Add(TempData);
            }
        }

        public XmlNode GetConfigFeatureInfo(string sName)
        {
            XmlNode RetNode = null;

            if (m_ConfigFeatureList != null)
            {
                foreach (CONFIG_DATA_TYPE Element in m_ConfigFeatureList)
                {
                    if (Element.m_FeatureName == sName)
                    {
                        RetNode = Element.m_FeatureContent;
                    }
                }
            }

            return RetNode;
        }

        public void SetXmlReportContent(XmlNode XmlReportContent)
        {
            m_XmlReportContent = XmlReportContent;
        }

        public XmlNode GetXMlReportContent()
        {
            return m_XmlReportContent;
        }

        public void SetProductName(string ProductName)
        {
            m_ProductName = ProductName;
        }

        public string GetProductName()
        {
            return m_ProductName;
        }

        public void SetReleaseNumber(string ReleaseNumber)
        {
            m_ReleaseNumber = ReleaseNumber;
        }

        public string GetReleaseNumber()
        {
            return m_ReleaseNumber;
        }

        public void SetMergeRelease(string MergeRelease)
        {
            m_MergeRelease = MergeRelease;
        }

        public string GetMergeRelease()
        {
            return m_MergeRelease;
        }

        public void SetDateRelease(string DateRelease)
        {
            m_DateRealease = DateRelease;
        }

        public string GetDateRelease()
        {
            return m_DateRealease;
        }

        public void SetReportFilePath(string ReportFilePath)
        {
            m_ReportFilePath = ReportFilePath;
        }

        public string GetReportFilePath()
        {
            return m_ReportFilePath;
        }

        public void SetReleaseVersion(string ReleaseVersion)
        {
            m_ReleaseVersion = ReleaseVersion;
        }

        public string GetReleaseVersion()
        {
            return m_ReleaseVersion;
        }

        public void SetImagePath(string ImagePath)
        {
            m_ImagePath = ImagePath;
        }

        public string GetImagePath()
        {
            return m_ImagePath;
        }

        public void SetHelpFilePath(string HelpFilePath)
        {
            m_HelpFilePath = HelpFilePath;
        }

        public string GetHelpFilePath()
        {
            return m_HelpFilePath;
        }
        
        public void SetHardContentFilePath(string HelpFilePath)
        {
            m_HardContentFilePath = HelpFilePath;
        }

        public string GetHardContentFilePath()
        {
            return m_HardContentFilePath;
        }
    }
}
