using System.Collections.Generic;
using System.IO;

namespace ConfigGenerator
{

    public class Event_StatisticHandler
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
            EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED,
            EVENT_TYPE.EVENT_REQUEST_PARSE_INPUT_DATA,
            EVENT_TYPE.EVENT_REQUEST_WRITE_TO_REPORT_FILE,
            EVENT_TYPE.EVENT_REQUEST_FINALIZE_REPORT_FILE,
            EVENT_TYPE.EVENT_REQUEST_WRITE_LAST_PART_TO_REPORT_FILE
        };
    }

    class CStatisticHandler : AEventReceiver
    {
        private enum STATISTIC_COLUMN
        {
            STATISTIC_COL_TAG_CODE = 0,
            STATISTIC_COL_TAG_NAME,
            STATISTIC_COL_STATUS,
            STATISTIC_COL_CUSTOM_DATA,
            STATISTIC_COL_MAX,
        }

        //----------------------
        // ATTRIBUTES
        //----------------------
        private const string                STATISTIC_FILE_PATH         = "{0}{1}Statistic{1}{2}_{3}.csv";
        private const string                STATISTIC_DELIMITER         = ",";
        private static readonly string[]    STATISTIC_COL_HEADER        = {"Configuration Item", "CI_TagName", "Status", "Detail info"};
        private static readonly string[]    STATISTIC_COL_STATUS_STRING = {"Unknow Item",
                                                                           "Good Item",
                                                                           "Missing mandatory information",
                                                                           "Information is incorrect format",
                                                                           "Information is incorrect Hex format",
                                                                           "Information is incorrect Decimal format",
                                                                           "Duplicate data"};

        private string              m_sFilePath            = null;
        private bool                m_bFileExist           = false;
                                                           
        private string              m_sCurrentTagName      = null;
        private string              m_sCurrentTagCode      = null;
        private bool                m_bStartStatistic      = false;
        List<STATISTIC_DATA_TYPE>   m_StatisticDataList    = null;
        private int                 m_iTotalNumberItem     = 0;
        private int                 m_iNumberItemError     = 0;
        private int                 m_iNumberItemCompleted = 0;

        //----------------------
        // PUBLIC FUNCTIONS
        //----------------------
        public CStatisticHandler()
             : base(new Event_StatisticHandler().m_MyEvent)
        {
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            EVENT_TYPE tempEvent = Event;
            
            switch (tempEvent)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT))
                    {
                        m_sFilePath = string.Format(STATISTIC_FILE_PATH,
                                                    // Directory.GetCurrentDirectory(),
                                                    System.AppDomain.CurrentDomain.BaseDirectory,
                                                    Path.DirectorySeparatorChar,
                                                    (oData as CXmlConfigInfoObject).GetProductName(),
                                                    (oData as CXmlConfigInfoObject).GetReleaseNumber());
                        ReInitProperties();
                        StatisticNewSession(m_sFilePath);
                        Program.SetUnixFileFullPermissions(m_sFilePath);
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_PARSE_INPUT_DATA:
                    if (oData != null)
                    {
                        m_sCurrentTagName = (oData as IGetInputParameterObject).GetCITagName();
                        m_sCurrentTagCode = (oData as IGetInputParameterObject).GetCITagCode();
                        if ((m_sCurrentTagName != null) || (m_sCurrentTagName != ""))
                        {
                            m_bStartStatistic = true;
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(m_sFilePath)) 
                                File.AppendAllText(m_sFilePath, "Line is invalid !\n");
                        }
                        m_iTotalNumberItem++;
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STATISTIC_DATA_OBJECT))
                    {
                        List<STATISTIC_DATA_TYPE> TempDataList = (oData as CStatisticObject).GetStatisticDataList();
                        if ((TempDataList != null) && (TempDataList.Count > 0))
                        {
                            if (m_bStartStatistic == true)
                            {
                                if (m_StatisticDataList == null)
                                {
                                    m_StatisticDataList = new List<STATISTIC_DATA_TYPE>();
                                }

                                m_StatisticDataList.AddRange(TempDataList);
                            }
                            else
                            {
                                string sTagInfo = (oData as CStatisticObject).GetTagCode() + STATISTIC_DELIMITER + (oData as CStatisticObject).GetTagName() + "\n";
                                if (!String.IsNullOrEmpty(m_sFilePath)) 
                                    File.AppendAllText(m_sFilePath, sTagInfo);
                                HandleStatisticData(TempDataList);
                                m_iNumberItemError++;
                                m_iNumberItemCompleted--;
                            }
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_WRITE_TO_REPORT_FILE:
                    if ((m_StatisticDataList != null) && (m_StatisticDataList.Count > 0))
                    {
                        string sTagInfo = m_sCurrentTagCode + STATISTIC_DELIMITER + m_sCurrentTagName + "\n";
                        if (!String.IsNullOrEmpty(m_sFilePath)) 
                            File.AppendAllText (m_sFilePath, sTagInfo);
                        HandleStatisticData (m_StatisticDataList);
                        m_StatisticDataList = null;
                        m_bStartStatistic = false;
                        m_iNumberItemError++;
                    }
                    else
                    {
                        m_iNumberItemCompleted++;
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_FINALIZE_REPORT_FILE:
                    {
                        m_bStartStatistic = false;
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_WRITE_LAST_PART_TO_REPORT_FILE:
                    {
                        if (!String.IsNullOrEmpty(m_sFilePath))
                        {
                            File.AppendAllText(m_sFilePath, "=============================================================\n");
                            File.AppendAllText(m_sFilePath, "Number Item Error Detected:," + m_iNumberItemError.ToString() + "\n");
                            File.AppendAllText(m_sFilePath, "Number Item Completed:," + m_iNumberItemCompleted.ToString() + "\n");
                            File.AppendAllText(m_sFilePath, "Total Item:," + m_iTotalNumberItem.ToString() + "\n");
                            File.AppendAllText(m_sFilePath, "=============================================================\n");
                        }
                        if (m_iNumberItemError > 0)
                        {
                            CStringObject tempObject = new CStringObject();
                            tempObject.AddStringData("\r\n");
                            tempObject.AddStringData("Number Item Error Detected: " + m_iNumberItemError.ToString() + "\r\n");
                            tempObject.AddStringData("Number Item Completed: " + m_iNumberItemCompleted.ToString() + "\r\n");
                            tempObject.AddStringData("Total Item: " + m_iTotalNumberItem.ToString() + "\r\n\r\n");
                            tempObject.AddStringData("Check input files and rerun again\r\n");
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_FINAL_INFO, tempObject);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void StatisticNewSession(string sFilePath)
        {
            if (sFilePath == null)
            {
                return;
            }

            string FolderName = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(FolderName))
            {
                try
                {
                    Directory.CreateDirectory(FolderName);
                    Program.SetUnixFileFullPermissions(FolderName);
                }
                catch
                {
                    return;
                }
            }

            if (File.Exists(sFilePath))
            {
                try
                {
                    File.Delete(sFilePath);
                }
                catch
                {
                    return;
                }
            }

            try
            {
                string sHeader = FormatStatistic(STATISTIC_COL_HEADER);
                File.AppendAllText(sFilePath, sHeader);
            }
            catch
            {
                return;
            }
            m_bFileExist = true;
        }

        private string FormatStatistic (string[] sData)
        {
            string sTemp = "";

            foreach (string sElement in sData)
            {
                sTemp = sTemp + sElement + STATISTIC_DELIMITER;
            }
            sTemp = sTemp.Remove(sTemp.Length - 1) + "\n";

            return sTemp;
        }

        private void HandleStatisticData(List<STATISTIC_DATA_TYPE> StatisticDataList)
        {
            if ((m_bFileExist == true) && (StatisticDataList != null))
            {
                List<STATISTIC_DATA_TYPE> TempProcessList = new List<STATISTIC_DATA_TYPE>();

                foreach (STATISTIC_DATA_TYPE element in StatisticDataList)
                {
                    bool        bFlagDuplicateDetect = false;
                    
                    foreach (STATISTIC_DATA_TYPE element_1 in TempProcessList)
                    {
                        if ((element.m_StatisticType == element_1.m_StatisticType) &&
                            (element.m_sCustomString == element_1.m_sCustomString))
                        {
                            bFlagDuplicateDetect = true;
                            break;
                        }
                    }

                    if (bFlagDuplicateDetect == false)
                    {
                        string[] sTemp = new string[(int)(STATISTIC_COLUMN.STATISTIC_COL_MAX)];
                        sTemp[(int)(STATISTIC_COLUMN.STATISTIC_COL_STATUS)] = STATISTIC_COL_STATUS_STRING[(int)element.m_StatisticType];
                        sTemp[(int)(STATISTIC_COLUMN.STATISTIC_COL_CUSTOM_DATA)] = element.m_sCustomString;
                        string sOutTemp = FormatStatistic(sTemp);

                        File.AppendAllText(m_sFilePath, sOutTemp);
                        TempProcessList.Add(element);
                    }
                }
            }
        }

        private void ReInitProperties()
        {
            m_bFileExist           = false;
            m_sCurrentTagName      = null;
            m_sCurrentTagCode      = null;
            m_bStartStatistic      = false;
            m_StatisticDataList    = null;
            m_iTotalNumberItem     = 0;
            m_iNumberItemError     = 0;
            m_iNumberItemCompleted = 0;
        }
    }
}
