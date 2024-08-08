using System.Collections.Generic;
using System.Xml.Linq;

namespace ConfigGenerator
{
    public class Event_ReportHandler
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[] 
        {
            EVENT_TYPE.EVENT_UPDATE_GENERATE_TYPE,
            EVENT_TYPE.EVENT_REQUEST_CHECK_TEMPLATE_FILE,
            EVENT_TYPE.EVENT_REQUEST_WRITE_TO_REPORT_FILE,
            EVENT_TYPE.EVENT_REQUEST_WRITE_LAST_PART_TO_REPORT_FILE,

            EVENT_TYPE.EVENT_RESPOND_UPDATE_CONFIG_INFO,
        };
    }

    class CReportHandler : AEventReceiver
    {
        //----------------------
        // ATTRIBUTES
        //----------------------
        public RECEIVER_ID                  m_ReceiverID                = RECEIVER_ID.ID_OUTPUT_HANDLER;
        private List<AReportChecker>        m_ReportCheckingList        = null;
        private List<AReportGenerator>      m_ReportGeneratorList       = null;
        private uint                        m_uiCountGeneratorDone      = 0;

        //----------------------
        // PUBLIC FUNCTIONS
        //----------------------
        public CReportHandler()
            : base (new Event_ReportHandler().m_MyEvent)
        {
            m_ReportCheckingList = null;// CFactoryReportChecker.GetInstance(CFactoryReportChecker.CHECK_TYPE.CHECK_ALL).GetReportFileCheckerList();
            m_ReportGeneratorList = null;// CFactoryReportGenerator.GetInstance(CFactoryReportGenerator.GENERATE_TYPE.GENERATE_ALL).GetReportGeneratorList();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_UPDATE_GENERATE_TYPE:
                    if (oData != null)
                    {
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                        {
                            string sGenType = ((CStringObject)oData).GetStringData();
                            CFactoryReportGenerator.GENERATE_TYPE eGenType = CFactoryReportGenerator.GENERATE_TYPE.GENERATE_NONE;
                            CFactoryReportChecker.CHECK_TYPE eCheckType = CFactoryReportChecker.CHECK_TYPE.CHECK_ALADDIN_PACKAGE_AND_SOURCE_CODE;
                            if (sGenType == "GENERATE_ALADDIN_PACKAGE")
                            {
                                eGenType = CFactoryReportGenerator.GENERATE_TYPE.GENERATE_ALADDIN_PACKAGE;
                            }
                            else if (sGenType == "GENERATE_SOURCE_CODE")
                            {
                                eGenType = CFactoryReportGenerator.GENERATE_TYPE.GENERATE_SOURCE_CODE;
                            }
                            else if (sGenType == "GENERATE_BIN_FILES")
                            {
                                eGenType = CFactoryReportGenerator.GENERATE_TYPE.GENERATE_BIN_FILES;
                                eCheckType = CFactoryReportChecker.CHECK_TYPE.CHECK_ALADDIN_PACKAGE_AND_BIN_FILES;
                            }
                            else if (sGenType == "GENERATE_ALADDIN_PACKAGE_AND_BIN_FILES")
                            {
                                eGenType = CFactoryReportGenerator.GENERATE_TYPE.GENERATE_ALADDIN_PACKAGE_AND_BIN_FILES;
                                eCheckType = CFactoryReportChecker.CHECK_TYPE.CHECK_ALADDIN_PACKAGE_AND_BIN_FILES;
                            }
                            else if (sGenType == "GENERATE_ALADDIN_PACKAGE_AND_SOURCE_CODE")
                            {
                                eGenType = CFactoryReportGenerator.GENERATE_TYPE.GENERATE_ALADDIN_PACKAGE_AND_SOURCE_CODE;
                            }

                            m_ReportGeneratorList = CFactoryReportGenerator.GetInstance(eGenType).GetReportGeneratorList();
                            m_ReportCheckingList = CFactoryReportChecker.GetInstance(eCheckType).GetReportFileCheckerList();
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_CHECK_TEMPLATE_FILE:
                    if (oData != null)
                    {
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                        {
                            ReportFileChecking((CStringObject)oData);
                        }
                    }
                    else
                    {
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, null);
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_UPDATE_CONFIG_INFO:
                    if (CheckGeneratorUpdateInfoDone(oData) == false)
                    {
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, null);
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_WRITE_TO_REPORT_FILE:
                    if (oData != null)
                    {
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_INTEGRATED_DATA_OBJECT)
                        {
                            ReportDataHandling((CIntegratedDataObject)oData);
                        }
                    }
                    else
                    {
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, null);
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_WRITE_LAST_PART_TO_REPORT_FILE:
                    if (oData != null)
                    {
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_INTEGRATED_DATA_OBJECT)
                        {
                            ReportDataFinalizing((CIntegratedDataObject)oData);
                        }
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

        //----------------------
        // PRIVATE FUNCTIONS
        //----------------------
        private bool ReportFileChecking(CStringObject oFilePathObject)
        {
            bool                bRet = false;
            string              sFilePath = null;
            CStringObject       ResultObj = new CStringObject();
            List<string>        sContentList = new List<string>() {"FAIL"};

            ReInitProperties();

            if (
                    (oFilePathObject != null) && 
                    (oFilePathObject.GetNumberOfStringMember() > 0) &&
                    (m_ReportCheckingList != null) &&
                    (m_ReportCheckingList.Count > 0)
               )
            {
                bRet = true;
                for (int i = 0; i < oFilePathObject.GetNumberOfStringMember(); i++)
                {
                    sFilePath = oFilePathObject.GetStringData(i);
                    
                    if (sFilePath != null)
                    {
                        foreach (AReportChecker element in m_ReportCheckingList)
                        {
                            if ((element != null) && (sFilePath.Contains(element.GetReportFileExtension())))
                            {
                                string sInfo = string.Format("Processing \"{0}\" template file.\r\n", element.GetReportFileExtension());
                                Program.SystemHandleStatusInfo(sInfo);
                                // check template file exist
                                if (!element.CheckTemplateFileValid(sFilePath))
                                {
                                    Program.SystemHandleStatusInfo("ERROR:: Template file is invalid!!!\r\n");
                                    bRet = false;
                                    break;
                                }
                                else
                                {
                                }

                                // check template file structure
                                if (!element.CheckTemplateFileStructure())
                                {
                                    Program.SystemHandleStatusInfo("ERROR:: Template file has invalid structure!!!\r\n");
                                    bRet = false;
                                    break;
                                }
                                else
                                {
                                }

                                // create report file
                                if (element.ProcessTemplateFile() == false)
                                {
                                    Program.SystemHandleStatusInfo("ERROR:: Detail/unknown error detected!!!\r\n");
                                    bRet = false;
                                    break;
                                }
                                else
                                {
                                }
                            }
                        }
                    }
                }
            }

            if (bRet == true)
            {
                sContentList = new List<string>() {"OK"};
            }

            foreach (string Element in sContentList)
            {
                if ((Element != null) && (Element != string.Empty))
                {
                    ResultObj.AddStringData(Element);
                }
            }
            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_CHECK_TEMPLATE_FILE_DONE, ResultObj);

            return bRet;
        }

        private bool CheckGeneratorUpdateInfoDone(IShareObject oData)
        {
            bool            bRet        = false;
            CStringObject   ResultObj   = null;
            string          sContent    = null;

            if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT))
            {
                sContent = (oData as CStringObject).GetStringData();
                if (sContent == "OK")
                {
                    bRet = true;
                    m_uiCountGeneratorDone++;
                }
            }

            if (m_ReportGeneratorList != null)
            {
                if (m_uiCountGeneratorDone == m_ReportGeneratorList.Count)
                {
                    ResultObj = new CStringObject();
                    ResultObj.AddStringData("OK");
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_UPDATE_CONFIG_INFO_DONE, ResultObj);
                }
            }

            return bRet;
        }

        private bool ReportDataHandling(CIntegratedDataObject oDataIn)
        {
            bool bRet = false;

            if (m_ReportGeneratorList != null)
            {
                bRet = true;
                foreach (AReportGenerator element in m_ReportGeneratorList)
                {
                    if (element != null)
                    {
                        if (!element.ReportDataHandling(oDataIn))
                        {
                            bRet = false;
                        }
                    }
                }
            }

            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_WRITE_TO_REPORT_FILE_DONE, null);

            return bRet;
        }

        private bool ReportDataFinalizing(CIntegratedDataObject oDataIn)
        {
            bool bRet = false;

            if (m_ReportGeneratorList != null)
            {
                bRet = true;
                foreach (AReportGenerator element in m_ReportGeneratorList)
                {
                    if (element != null)
                    {
                        if (!element.ReportDataFinalizing(oDataIn))
                        {
                            bRet = false;
                        }
                    }
                }
            }

            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_WRITE_LAST_PART_TO_REPORT_FILE_DONE, null);

            return bRet;
        }

        private void ReInitProperties()
        {
            m_uiCountGeneratorDone = 0;
        }

    }
}
