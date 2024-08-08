using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    public class Event_InputPreHandler
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_MERGE_INPUT_FILE,
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
            EVENT_TYPE.EVENT_REQUEST_WRITE_LAST_PART_TO_REPORT_FILE,
        };
    }

    class CInputPreHandler : AEventReceiver
    {
        private IInputFileProcessor         m_InputFileProcessor    = null;
        private IOutputMergedFileProcessor  m_OutputFileProcessor   = null;

        private static readonly string[]    m_sReferenceMergedFileName = { "ConfigurationItems", "ProductModels" };
        private string                      m_sProductName = null;
        private List<string>                m_lsMergedFileName = new List<string>();

        public CInputPreHandler()
            : base(new Event_InputPreHandler().m_MyEvent)
        {
            m_InputFileProcessor = CFactoryCommonInputFileProcessor.GetInstance().GetInputFileProcessor();
            m_OutputFileProcessor = CFactoryOutputMergedFileProcessor.GetInstance().GetInputFileProcessor();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            EVENT_TYPE tempEvent = Event;

            switch (tempEvent)
            {
                case EVENT_TYPE.EVENT_REQUEST_MERGE_INPUT_FILE:
                    if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                    {
                        if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                        {
                            int iNumOfString = (oData as CStringObject).GetNumberOfStringMember();

                            CStringObject MergedInputFilePath = new CStringObject();
                            for (int i = 0; i < iNumOfString; i++)
                            {
                                string sInputFilePath = (oData as CStringObject).GetStringData(i);
                                string sMergedFilePath = MergeInputFile(sInputFilePath, m_sReferenceMergedFileName[i]);
                                if (string.IsNullOrEmpty(sMergedFilePath) == false)
                                {
                                    MergedInputFilePath.AddStringData(sMergedFilePath);
                                    m_lsMergedFileName.Add(sMergedFilePath);
                                }
                            }

                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_MERGE_INPUT_FILE_DONE, MergedInputFilePath);
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT))
                    {
                        m_sProductName = (oData as CXmlConfigInfoObject).GetProductName();

                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_WRITE_LAST_PART_TO_REPORT_FILE:
                    {
                        foreach (string sFilename in m_lsMergedFileName)
                        {
                            m_OutputFileProcessor.MoveAndRenameFile(sFilename, m_sProductName);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private string MergeInputFile(string sInputFilePathList, string sFileName)
        {
            string sReturn = null;
            string[] sPathList = sInputFilePathList.Split('|');
            sReturn = m_OutputFileProcessor.CreateNewFile(sFileName);
            if (sReturn == null)
            {
                return null;
            }

            string[] sFirstLineRef = FindMostPopularLayout(sPathList);
            if (sFirstLineRef == null)
            {
                return null;
            }

            foreach (string sPath in sPathList)
            {
                if (m_OutputFileProcessor.IsFileExistent(sPath) == true)
                {
                    if (ArrangeAndMerge(sPath, sFirstLineRef) == false)
                    {
                        sReturn = null;
                    }
                }
                else
                {
                    // Log error on main screen
                    CStringObject stringObject = new CStringObject();
                    string sString = "Input file " + sPath + " doesn't exist!\r\n";
                    stringObject.AddStringData(sString);
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_SYSTEM_INFO, stringObject);
                    sReturn = null;
                }
            }

            m_OutputFileProcessor.CloseFile();
            return sReturn;
        }

        private string[] FindMostPopularLayout(string[] sPathList)
        {
            string sLastPath = null;
            int iNumberValidFieldRef = 0;
            List<CLayoutData> LayoutDataList = new List<CLayoutData>();

            foreach (string sPath in sPathList)
            {
                if (m_InputFileProcessor.OpenFile(sPath) == true)
                {
                    int iNumberValidField = m_InputFileProcessor.GetNumberValidField();
                    string[] sElementList = GetLineData(0);

                    CLayoutData cLayoutData = new CLayoutData(sElementList, m_InputFileProcessor.GetNumberParamValid());
                    LayoutDataList.Add(cLayoutData);
                    m_InputFileProcessor.CloseFile();

                    if (iNumberValidFieldRef != iNumberValidField)
                    {
                        if (iNumberValidFieldRef == 0)
                        {
                            iNumberValidFieldRef = iNumberValidField;
                        }
                        else
                        {
                            // Log error on main screen
                            CStringObject stringObject = new CStringObject();
                            string sString = "Input files:\r\n" + "_" + sLastPath + "\r\n_" + sPath + "\r\nhave different number of columns!\r\n";
                            stringObject.AddStringData(sString);
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_SYSTEM_INFO, stringObject);
                            return null;
                        }
                    }
                    sLastPath = sPath;
                }
            }

            int nIndex = 0;
            int[] iFrequency = new int[LayoutDataList.Count];
            for (int i = 0; i < LayoutDataList.Count; i++)
            {
                iFrequency[i] = LayoutDataList[i].m_iCountLine;
            }

            for (int i = 0; i < (LayoutDataList.Count - 1); i++)
            {
                if (iFrequency[i] != 0)
                {
                    for (int j = i + 1; j < LayoutDataList.Count; j++)
                    {
                        if (iFrequency[j] != 0)
                        {
                            if (LayoutDataList[i].m_sFirstLine.SequenceEqual(LayoutDataList[j].m_sFirstLine) == true)
                            {
                                iFrequency[i] += iFrequency[j];
                                iFrequency[j] = 0;
                            }

                            var sResult = LayoutDataList[i].m_sFirstLine.Except(LayoutDataList[j].m_sFirstLine);
                            if (sResult.Count() != 0)
                            {
                                // Log error on main screen
                                CStringObject stringObject = new CStringObject();
                                string sString = "Input file " + sPathList[i] + " and " + sPathList[j] + " has different columns!\r\n";
                                stringObject.AddStringData(sString);
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_SYSTEM_INFO, stringObject);
                                return null;
                            }
                        }
                    }
                }
            }

            int iFrequencyMax = 0;
            for (int i = 0; i < iFrequency.Length; i++)
            {
                if (iFrequency[i] > iFrequencyMax)
                {
                    iFrequencyMax = iFrequency[i];
                    nIndex = i;
                }
            }
            nIndex = LayoutDataList.Count - 1;

            m_OutputFileProcessor.AddNewItem(LayoutDataList[nIndex].m_sFirstLine);
            return LayoutDataList[nIndex].m_sFirstLine;
        }

        private bool ArrangeAndMerge(string sInputFilePath, string[] sFieldListRef)
        {
            if (m_InputFileProcessor.OpenFile(sInputFilePath) == true)
            {
                int iNumberValidField = m_InputFileProcessor.GetNumberValidField();
                string[] sCurrentFieldList = GetLineData(0);

                bool bIsSameFirstLine = sFieldListRef.SequenceEqual(sCurrentFieldList);
                int iNumberOfLine = m_InputFileProcessor.GetNumberParamValid();
                for (int i = 1; i <= iNumberOfLine; i++)
                {
                    string[] sFieldListToSort = (string[])sCurrentFieldList.Clone();
                    // Read element from input file
                    string[] sFieldList = GetLineData(i);

                    if (sFieldList.Length == sFieldListToSort.Length)
                    {

                        // Arrange and merge
                        string[] sResultLine = new string[iNumberValidField];
                        if (bIsSameFirstLine == true)
                        {
                            sResultLine = sFieldList;
                        }
                        else
                        {
                            // Use Array.Sort with custom comparer
                            FieldComparer comparer = new FieldComparer(sFieldListRef);
                            Array.Sort(sFieldListToSort, sFieldList, comparer);
                            sResultLine = sFieldList;
                        }

                        // Use OutputFileProcessor to write data in appropriate format
                        m_OutputFileProcessor.AddNewItem(sResultLine);
                    }
                    else
                    {
                        // Log error on main screen
                        CStringObject stringObject = new CStringObject();
                        string sString = "Input file \r\n" + sInputFilePath + " has different number of column at line " + i.ToString() + " !\r\n";
                        stringObject.AddStringData(sString);
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_SYSTEM_INFO, stringObject);
                        return false;
                    }
                }

                m_InputFileProcessor.CloseFile();
            }
            return true;
        }

        private string[] GetLineData(int iLine)
        {
            int iNumberValidField = m_InputFileProcessor.GetNumberTotalField(iLine);
            string[] sResult = new string[iNumberValidField];
            for (int i = 0; i < iNumberValidField; i++)
            {
                sResult[i] = m_InputFileProcessor.ReadElement(i, iLine);
            }
            return sResult;
        }
    }
}
