using System.Reflection;

namespace ConfigGenerator
{
    public class Event_ReportGenerator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[] 
        {
            EVENT_TYPE.EVENT_REQUEST_GENERATE_REPORT,
            EVENT_TYPE.EVENT_REQUEST_UPDATE_SYSTEM_INFO,
            EVENT_TYPE.EVENT_REQUEST_UPDATE_FINAL_INFO,

            EVENT_TYPE.EVENT_RESPONSE_PARSE_PARAMETER_FILE_DONE,
            EVENT_TYPE.EVENT_REQUEST_MERGE_INPUT_FILE,
            EVENT_TYPE.EVENT_RESPOND_MERGE_INPUT_FILE_DONE,
            EVENT_TYPE.EVENT_REQUEST_CHECK_INPUT_FILE,
            EVENT_TYPE.EVENT_RESPOND_CHECK_INPUT_FILE_DONE,
            EVENT_TYPE.EVENT_REQUEST_CHECK_TEMPLATE_FILE,
            EVENT_TYPE.EVENT_RESPOND_CHECK_TEMPLATE_FILE_DONE,
            EVENT_TYPE.EVENT_RESPOND_UPDATE_CONFIG_INFO_DONE,
            EVENT_TYPE.EVENT_RESPOND_WRITE_TO_REPORT_FILE_DONE,
            EVENT_TYPE.EVENT_RESPOND_FINALIZE_REPORT_FILE_DONE,
        };
    }

    class CReportGeneratorDirector : AEventReceiver
    {
        //----------------------
        // DATA TYPE DEFINITION
        //----------------------
        private struct PROCESS_GENERATE_INFO
        {
            public bool m_bInprocess;
            public bool m_bFileInputReady;
            public bool m_bFileTemplateReady;
            public bool m_bFileReportReady;

            public int  m_iTotalNumberParInput;
            public int  m_iTotalNumberParProcessed;
            public int  m_iCurrentParProcess;
        }

        private enum EVENT_PROCESS_TYPE
        {
            EVENT_PROCESS_UNKNOW = 0,
            EVENT_PROCESS_RESET_INFO,
            EVENT_PROCESS_SET_INFO_FROM_INPUT_FILE,
            EVENT_PROCESS_SET_INFO_FROM_TEMPLATE_FILE,
            EVENT_PROCESS_SET_INFO_FROM_REPORT_FILE,
            EVENT_PROCESS_INCREASE_NUMBER_PAR_PROCESSED,
        }

        //----------------------
        // ATTRIBUTES
        //----------------------
        private string m_sMainInputPath         = null;
        private string m_sModelFilePath         = null;
        private string m_sTemplateFilePath      = null;
        private string m_sReportFilePath        = null;
        private bool   m_bFinalInfoStatus       = false;

        private PROCESS_GENERATE_INFO   m_ProcessGenerateInfo;

        //----------------------
        // PUBLIC FUNCTIONS
        //----------------------
        public CReportGeneratorDirector()
            : base(new Event_ReportGenerator().m_MyEvent)
        {
        }

        //----------------------
        // PRIVATE FUNCTIONS
        //----------------------
        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            EVENT_TYPE tempEvent = Event;

            switch (tempEvent)
            {
                case EVENT_TYPE.EVENT_RESPONSE_PARSE_PARAMETER_FILE_DONE:
                    {
                        // For GUI only
                        string sTemp = "Could not parse parameter file.\r\n\r\nPlease check again and rerun.\r\n";
                        Program.SystemHandleStatusInfo(sTemp);
                        Program.SystemResetStateInput();
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_GENERATE_REPORT:
                    {                        
                        ReInitProperties();

                        // string sTemp = "Current directory\r\n" + Directory.GetCurrentDirectory().ToString() + "\r\n\r\n";
                        string sTemp = new string('*', 40) + "\r\n";
                        // sTemp += "Current directory\r\n" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\r\n";
                        sTemp += "Current directory\r\n" + System.AppDomain.CurrentDomain.BaseDirectory + "\r\n";
                        Program.SystemHandleStatusInfo(sTemp);

                        UpdateProcessGenerateInfo(EVENT_PROCESS_TYPE.EVENT_PROCESS_RESET_INFO);

                        // Trigger check template file
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                        {
                            if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                            {
                                // Mandatory files
                                m_sMainInputPath = (oData as CStringObject).GetStringData(0);
                                m_sModelFilePath = (oData as CStringObject).GetStringData(1);
                                m_sTemplateFilePath = (oData as CStringObject).GetStringData(2);
                                m_sReportFilePath = (oData as CStringObject).GetStringData(3);

                                CStringObject InputFilePath = new CStringObject();
                                InputFilePath.AddStringData(m_sMainInputPath);
                                InputFilePath.AddStringData(m_sModelFilePath);

                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_MERGE_INPUT_FILE, InputFilePath);
                            }
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_MERGE_INPUT_FILE:
                    {
                        string sTemp = new string('*', 40) + "\r\n";
                        Program.SystemHandleStatusInfo(sTemp);
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_MERGE_INPUT_FILE_DONE:
                    if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                    {
                        if ((oData as CStringObject).GetNumberOfStringMember() == 2)
                        {
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_CHECK_INPUT_FILE, oData);
                        }
                        else
                        {
                            // For GUI only
                            string sTemp = "ERROR::Could not combine single input files.\r\n\r\nPlease check again and rerun.\r\n";
                            Program.SystemHandleStatusInfo(sTemp);
                            Program.SystemResetStateInput();
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_CHECK_INPUT_FILE:
                    {
                        string sTemp = new string('*', 40) + "\r\n";
                        Program.SystemHandleStatusInfo(sTemp);
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_CHECK_INPUT_FILE_DONE:
                    if ((oData != null)  && 
                        (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_INPUT_INFO_OBJECT))
                    {
                        if (UpdateProcessGenerateInfo(EVENT_PROCESS_TYPE.EVENT_PROCESS_SET_INFO_FROM_INPUT_FILE, oData) == false)
                        {
                            // For GUI only
                            string sTemp = "ERROR::Input files have incorrect structure!\r\n\r\nPlease check again and rerun.\r\n";
                            Program.SystemHandleStatusInfo(sTemp);
                            Program.SystemResetStateInput();
                        }
                        else
                        {
                            // For GUI only
                            string sTemp = "Input files contain: ";
                            sTemp += (oData as CInputInfoObject).GetNumberItems() + " items.\r\n";
                            Program.SystemHandleStatusInfo(sTemp);

                            // Trigger check template file
                            CStringObject TemplateFilePath = new CStringObject();

                            TemplateFilePath.AddStringData(m_sTemplateFilePath);
                            TemplateFilePath.AddStringData(m_sReportFilePath);

                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_CHECK_TEMPLATE_FILE, TemplateFilePath);
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_CHECK_TEMPLATE_FILE:
                    {
                        string sTemp = new string('*', 40) + "\r\n";
                        Program.SystemHandleStatusInfo(sTemp);
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_CHECK_TEMPLATE_FILE_DONE:
                    if ((oData != null) &&
                        (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT))
                    {
                        if (UpdateProcessGenerateInfo(EVENT_PROCESS_TYPE.EVENT_PROCESS_SET_INFO_FROM_TEMPLATE_FILE, oData) == false)
                        {
                            // For GUI only
                            string sTemp = "\r\nPlease check again and rerun.\r\n";
                            Program.SystemHandleStatusInfo(sTemp);
                            Program.SystemResetStateInput();
                        }
                        else
                        {
                            Program.SystemHandleStatusInfo("Check Template done: ");
                            if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                            {
                                for (int i = 0; i < (oData as CStringObject).GetNumberOfStringMember(); i++)
                                {
                                    string sTemp = (oData as CStringObject).GetStringData(i);
                                    Program.SystemHandleStatusInfo(sTemp + "\r\n");
                                }
                            }
                        }
                    }

                    break;

                case EVENT_TYPE.EVENT_RESPOND_UPDATE_CONFIG_INFO_DONE:
                    if ((oData != null) &&
                        (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT))
                    {
                        if (UpdateProcessGenerateInfo(EVENT_PROCESS_TYPE.EVENT_PROCESS_SET_INFO_FROM_REPORT_FILE, oData) == false)
                        {
                            // For GUI only
                            string sTemp = "Template files contain invalid value!\r\n\r\nPlease check again and rerun.\r\n";
                            Program.SystemHandleStatusInfo(sTemp);
                            Program.SystemResetStateInput();
                        }
                        else
                        {
                            // For GUI only
                            if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                            {
                                string sTemp = new string('*', 40) + "\r\n";
                                Program.SystemHandleStatusInfo(sTemp);
                                sTemp = (oData as CStringObject).GetStringData();
                                Program.SystemHandleStatusInfo("Empty report file created: " + sTemp + "\r\n");
                            }
                                                        
                            if (IsResourceReady())
                            {
                                Program.SystemHandleStatusInfo("Please waiting for all items are populating !!!\r\n");
                                ProcessGenerateReportManage();
                            }
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_WRITE_TO_REPORT_FILE_DONE:
                    if (UpdateProcessGenerateInfo(EVENT_PROCESS_TYPE.EVENT_PROCESS_INCREASE_NUMBER_PAR_PROCESSED) == true)
                    {
                        if (ProcessGenerateReportManage() == false)
                        {
                            Program.SystemHandleStatusInfo("All items are populated, finalizing all report files! \r\n");
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_FINALIZE_REPORT_FILE, null);
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_FINALIZE_REPORT_FILE_DONE:
                    {
                        Program.SystemEndingProgram(m_bFinalInfoStatus);
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_UPDATE_FINAL_INFO:
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_SYSTEM_INFO:
                    if ((oData != null) && 
                        (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT))
                    {
                        if (Event == EVENT_TYPE.EVENT_REQUEST_UPDATE_FINAL_INFO)
                        {
                            m_bFinalInfoStatus = true;
                        }

                        if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                        {
                            for(int i = 0; i < (oData as CStringObject).GetNumberOfStringMember(); i++)
                            {
                                string sTemp = (oData as CStringObject).GetStringData(i);
                                Program.SystemHandleStatusInfo(sTemp);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void ReInitProperties()
        {
            m_sMainInputPath     = null;
            m_sModelFilePath     = null;
            m_sTemplateFilePath  = null;
            m_sReportFilePath    = null;
            m_bFinalInfoStatus   = false;
        }

        private bool IsResourceReady()
        {
            bool bRet = false;
            if ((m_ProcessGenerateInfo.m_bFileInputReady == true) &&
                (m_ProcessGenerateInfo.m_bFileTemplateReady == true) &&
                (m_ProcessGenerateInfo.m_bFileReportReady == true))
            {
                bRet = true;
            }

            return bRet;
        }

        private bool UpdateProcessGenerateInfo(EVENT_PROCESS_TYPE Event, object oData = null)
        {
            bool bRet = true;

            switch (Event)
            { 
                case EVENT_PROCESS_TYPE.EVENT_PROCESS_RESET_INFO:
                    {
                        m_ProcessGenerateInfo.m_bInprocess = false;
                        m_ProcessGenerateInfo.m_bFileInputReady = false;
                        m_ProcessGenerateInfo.m_bFileTemplateReady = false;
                        m_ProcessGenerateInfo.m_bFileReportReady = false;
                        m_ProcessGenerateInfo.m_iTotalNumberParInput = 0;
                        m_ProcessGenerateInfo.m_iTotalNumberParProcessed = 0;
                        m_ProcessGenerateInfo.m_iCurrentParProcess = 1;
                    }
                    
                    break;

                case EVENT_PROCESS_TYPE.EVENT_PROCESS_SET_INFO_FROM_INPUT_FILE:
                    if (oData != null)
                    {
                        m_ProcessGenerateInfo.m_iTotalNumberParInput = int.Parse((oData as CInputInfoObject).GetNumberItems());
                        if (m_ProcessGenerateInfo.m_iTotalNumberParInput > 0)
                        {
                            m_ProcessGenerateInfo.m_bFileInputReady = true;
                        }
                        else
                        {
                            bRet = false;
                        }
                    }
                    else
                    {
                        bRet = false;
                    }
                    break;

                case EVENT_PROCESS_TYPE.EVENT_PROCESS_SET_INFO_FROM_TEMPLATE_FILE:
                    if (oData != null)
                    {
                        if ((oData as CStringObject).GetStringData() == "OK")
                        {
                            m_ProcessGenerateInfo.m_bFileTemplateReady = true;
                        }
                        else
                        {
                            bRet = false;
                        }
                    }
                    else
                    {
                        bRet = false;
                    }
                    break;

                case EVENT_PROCESS_TYPE.EVENT_PROCESS_SET_INFO_FROM_REPORT_FILE:
                    if (oData != null)
                    {
                        if ((oData as CStringObject).GetStringData() == "OK")
                        {
                            m_ProcessGenerateInfo.m_bFileReportReady = true;
                        }
                        else
                        {
                            bRet = false;
                        }
                    }
                    else
                    {
                        bRet = false;
                    }
                    break;

                case EVENT_PROCESS_TYPE.EVENT_PROCESS_INCREASE_NUMBER_PAR_PROCESSED:
                    m_ProcessGenerateInfo.m_iTotalNumberParProcessed++;
                    break;

                default:
                    bRet = false;
                    break;
            }
            

            return bRet;
        }

        private bool ProcessGenerateReportManage()
        {
            bool bRet = false;

            if (m_ProcessGenerateInfo.m_iTotalNumberParProcessed < m_ProcessGenerateInfo.m_iTotalNumberParInput)
            {
                CStringObject tempObject = new CStringObject();
                tempObject.AddStringData(m_ProcessGenerateInfo.m_iCurrentParProcess.ToString());

                // MainForm._MainForm.UpdateDataForGui("Process in parameter: " + m_ProcessGenerateInfo.m_iCurrentParProcess.ToString()+ "\r\n");

                // Use for DEBUG at START POINT of parameter.
                CLogger.GetInstance().Log("Process in parameter: " + m_ProcessGenerateInfo.m_iCurrentParProcess.ToString());

                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_GET_ONE_DATA_FROM_INPUT_FILE, tempObject);
                m_ProcessGenerateInfo.m_iCurrentParProcess++;

                bRet = true;
            }

            return bRet;
        }
        
    }
}
