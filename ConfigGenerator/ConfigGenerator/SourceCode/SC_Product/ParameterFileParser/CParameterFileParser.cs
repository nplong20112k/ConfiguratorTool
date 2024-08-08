using System;
using System.IO;
using System.Xml;

namespace ConfigGenerator
{
    public class Event_ParameterParser
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_PARSE_PARAMETER_FILE,
        };
    }

    class CParameterFileParser : AEventReceiver
    {
        private const int NUM_OF_TYPE_FILE = 4;

        private const string ITEM_FILES             = "item_files";
        private const string ITEM                   = "item";
        private const string MODEL_MAPS_FILES       = "model_maps";
        private const string MODEL_MAP              = "model_map";
        private const string ALADDIN_TEMPLATE_FILE  = "aladdin_template";
        private const string SRC_TEMPLATE_FILE      = "sourcecode_template";

        private IXMLFileProcess m_XMLFileProcess = null;
        private string m_sMainInputPath = null;
        private string m_sModelFilePath = null;
        private string m_sTemplateFilePath = null;
        private string m_sReportFilePath = null;
        private string m_sParameterFilePath = null;

        public CParameterFileParser()
            : base(new Event_ParameterParser().m_MyEvent)
        {
            m_XMLFileProcess = CFactoryXmlFileProcessor.GetInstance().GetXmlFileProcessor();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch(Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_PARSE_PARAMETER_FILE:
                    if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                    {
                        if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                        {
                            string sFilePath = (oData as CStringObject).GetStringData(0);

                            CStringObject resultObject = MultiInputFilesParse(sFilePath);
                            if (resultObject.GetNumberOfStringMember() == NUM_OF_TYPE_FILE)
                            {
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_GENERATE_REPORT, resultObject);
                            }
                            else
                            {
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPONSE_PARSE_PARAMETER_FILE_DONE, resultObject);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private CStringObject MultiInputFilesParse(string sFilePath)
        {
            CStringObject result = new CStringObject();

            if (m_XMLFileProcess.XMLLoadingFile(sFilePath) == true)
            {
                m_sParameterFilePath = Path.GetDirectoryName(sFilePath);
                XmlNode RootNode = m_XMLFileProcess.XMLGetRootNode();
                if (RootNode != null)
                {
                    if (RootNode.HasChildNodes == true)
                    {

                        m_sMainInputPath = null;
                        m_sModelFilePath = null;
                        m_sTemplateFilePath = null;
                        m_sReportFilePath = null;

                        foreach (XmlNode element in RootNode.ChildNodes)
                        {
                            if (element != null)
                            {
                                switch(element.Name)
                                {
                                    case ITEM_FILES:
                                        if (m_sMainInputPath == null)
                                        {
                                            m_sMainInputPath = ParseMultiple(element, ITEM);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                        break;

                                    case MODEL_MAPS_FILES:
                                        if (m_sModelFilePath == null)
                                        {
                                            m_sModelFilePath = ParseMultiple(element, MODEL_MAP);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                        break;

                                    case ALADDIN_TEMPLATE_FILE:
                                        if (m_sTemplateFilePath == null)
                                        {
                                            m_sTemplateFilePath = ParseSingle(element);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                        break;

                                    case SRC_TEMPLATE_FILE:
                                        if (m_sReportFilePath == null)
                                        {
                                            m_sReportFilePath = ParseSingle(element);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }

                        if (String.IsNullOrEmpty(m_sMainInputPath) == false)
                        {
                            m_sMainInputPath = m_sMainInputPath.Replace('\\', Path.DirectorySeparatorChar);
                            result.AddStringData(m_sMainInputPath);
                        }

                        if (String.IsNullOrEmpty(m_sModelFilePath) == false)
                        {
                            m_sModelFilePath = m_sModelFilePath.Replace('\\', Path.DirectorySeparatorChar);
                            result.AddStringData(m_sModelFilePath);
                        }

                        if (String.IsNullOrEmpty(m_sTemplateFilePath) == false)
                        {
                            m_sTemplateFilePath = m_sTemplateFilePath.Replace('\\', Path.DirectorySeparatorChar);
                            result.AddStringData(m_sTemplateFilePath);
                        }

                        if (String.IsNullOrEmpty(m_sReportFilePath) == false)
                        {
                            m_sReportFilePath = m_sReportFilePath.Replace('\\', Path.DirectorySeparatorChar);
                            result.AddStringData(m_sReportFilePath);
                        }
                    }
                }
            }
            return result;
        }

        private string ParseSingle(XmlNode element)
        {
            String sResult = "";

            if ((element.HasChildNodes == true) && (element.ChildNodes.Count == 1))
            {
                sResult = element.InnerText;
                sResult = CheckAndConvertToAbsolutePath(sResult, m_sParameterFilePath);
            }
            return sResult;
        }

        private string ParseMultiple(XmlNode Node, string sKeyword)
        {
            String sResult = "";
            if (Node.HasChildNodes == true)
            {
                foreach (XmlNode element in Node.ChildNodes)
                {
                    if (element != null)
                    {
                        if (element.Name == sKeyword)
                        {
                            // null, whitespace, empty are checked here
                            if (String.IsNullOrWhiteSpace(element.InnerText) == false)
                            {
                                string sText = element.InnerText;
                                sResult += CheckAndConvertToAbsolutePath(sText, m_sParameterFilePath);
                                sResult += "|";
                            }
                        }
                    }
                }
                sResult = sResult.Trim('|');
            }
            return sResult;
        }

        private string CheckAndConvertToAbsolutePath(string sPath, string sRefPath)
        {
            String sResult = "";

            if (String.IsNullOrEmpty(sPath) == false)
            {
                if (Path.IsPathRooted(sPath) == true)
                {
                    sResult = sPath;
                }
                else
                {
                    if (Path.IsPathRooted(sRefPath) == true)
                    {
                        sResult = sRefPath + Path.DirectorySeparatorChar + sPath;
                    }
                    else
                    {
                        // sResult = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + sRefPath + Path.DirectorySeparatorChar + sPath;
                        sResult = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + sRefPath + Path.DirectorySeparatorChar + sPath;
                    }
                }
            }
            return sResult;
        }

    }
}
