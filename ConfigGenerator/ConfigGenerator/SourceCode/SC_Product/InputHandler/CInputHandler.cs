using System;
using System.Collections.Generic;

namespace ConfigGenerator
{

    public class Event_InputHandler
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[] 
        { 
            EVENT_TYPE.EVENT_REQUEST_CHECK_INPUT_FILE,
            EVENT_TYPE.EVENT_REQUEST_GET_ONE_DATA_FROM_INPUT_FILE,
        };
    }

    class CInputHandler : AEventReceiver
    {
        //----------------------
        // ATTRIBUTES
        //----------------------
        private List<IInputChecker>     m_InputCheckerList = null;
        private List<IInputReader>      m_InputReaderList = null;

        //----------------------
        // PUBLIC FUNCTIONS
        //----------------------
        public CInputHandler()
            : base(new Event_InputHandler().m_MyEvent)
        {
            m_InputCheckerList = CFactoryInputChecker.GetInstance().GetInputCheckerList();
            m_InputReaderList = CFactoryInputReader.GetInstance().GetInputReaderList();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            EVENT_TYPE tempEvent = Event;

            switch (tempEvent)
            {
                case EVENT_TYPE.EVENT_REQUEST_CHECK_INPUT_FILE:
                    if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                    {
                        if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                        {
                            Initialize();
                            CheckInputFile((oData as CStringObject));
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_GET_ONE_DATA_FROM_INPUT_FILE:
                    if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
                    {
                        if ((oData as CStringObject).GetNumberOfStringMember() > 0)
                        {
                            string sTemp = (oData as CStringObject).GetStringData();
                            GetAndSendOneData(Convert.ToInt32(sTemp));
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private bool CheckInputFile(CStringObject oFilePath)
        {
            bool bRet = false;

            string sFilePath = null;

            if (m_InputCheckerList != null)
            {
                if (oFilePath.GetNumberOfStringMember() > m_InputCheckerList.Count)
                {
                    bRet = false;
                }
                else
                {

                    int[] tempResult = new int[m_InputCheckerList.Count];
                    for (int i = 0; i < m_InputCheckerList.Count; i++)
                    {
                        tempResult[i] = 0;
                    }

                    for (int i = 0; i < oFilePath.GetNumberOfStringMember(); i++)
                    {
                        sFilePath = oFilePath.GetStringData(i);
                        foreach (IInputChecker CheckerElement in m_InputCheckerList)
                        {
                            if (CheckerElement != null)
                            {
                                if (CheckerElement.CheckingInputFile(sFilePath) == true)
                                {
                                    tempResult[m_InputCheckerList.IndexOf(CheckerElement)]++;
                                }
                            }
                        }
                    }

                    bRet = true;
                    for (int i = 0; i < m_InputCheckerList.Count; i++)
                    {
                        switch (tempResult[i])
                        {
                            case 0:
                                // TODO Add Log missing files
                                bRet = false;
                                break;

                            case 1:
                                break;

                            default:
                                // TODO Add Log duplicate files
                                bRet = false;
                                break;
                        }
                    }
                }

                CInputInfoObject oInfoObject = new CInputInfoObject();
                if (bRet == true)
                {
                    int iTemp = 0;
                    foreach (IInputReader ReaderElement in m_InputReaderList)
                    {
                        if (ReaderElement != null)
                        {
                            iTemp = Math.Max(ReaderElement.GetNumberParameter(), iTemp);
                            ReaderElement.GetConfigInfo(oInfoObject);
                        }
                    }
                    oInfoObject.SetNumberItems(iTemp.ToString());
                }
                else
                {
                    oInfoObject.SetNumberItems("0");
                }

                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_CHECK_INPUT_FILE_DONE, oInfoObject);
            }
            else
            {
                // TODO
            }

            return bRet;
        }

        private bool GetAndSendOneData(int iIndex)
        {
            bool bRet = false;

            if (m_InputReaderList != null)
            {
                CInputParameterObject oOutObject = new CInputParameterObject();

                foreach (IInputReader element in m_InputReaderList)
                {
                    if (element != null)
                    {
                        if (oOutObject.IsEmpty() == true)
                        {
                            element.GetInputParameter(iIndex, oOutObject);
                        }
                        else
                        {
                            element.UpdateInputParameter(oOutObject);
                        }
                    }
                }
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_PARSE_INPUT_DATA, oOutObject);
            }

            return bRet;
        }

        private void Initialize()
        {
            if (m_InputCheckerList != null)
            {
                foreach (IInputChecker CheckerElement in m_InputCheckerList)
                {
                    CheckerElement.Initialize();
                }
            }
        }
        //----------------------
        // PRIVATE FUNCTIONS
        //----------------------
    }

    
}
