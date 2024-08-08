using System.Collections.Generic;

namespace ConfigGenerator
{
    public class Event_CodeReportGenerator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_CHECK_TEMPLATE_FILE,
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CCodeReportGenerator : AReportGenerator
    {
        private List<ACodeSubHandler> m_SubHandlerList = null;
        private List<IShareObject> m_ShareObjectList = new List<IShareObject>();
        private int NUMBER_OF_CONFIG_INFO_OBJECT = 2;

        public CCodeReportGenerator()
            : base(new Event_CodeReportGenerator().m_MyEvent)
        {
            m_SubHandlerList = CFactoryCodeSubHandler.GetInstance().GetSubHandlerList();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_CHECK_TEMPLATE_FILE:
                    {
                        if (m_ShareObjectList.Count > 0)
                        {
                            m_ShareObjectList.RemoveRange(0, m_ShareObjectList.Count);
                        }
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    {
                        if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                        {
                            m_ShareObjectList.Add(oData);
                        }
                        else if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_XML_CONFIG_INFO_OBJECT))
                        {
                            m_ShareObjectList.Add(oData);
                        }

                        if (m_ShareObjectList.Count == NUMBER_OF_CONFIG_INFO_OBJECT)
                        {
                            CStringObject ResultObj = new CStringObject();
                            string sContent = null;
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

        private bool Initialize(List<IShareObject> oDataList)
        {
            bool bRet = true;

            if ((m_SubHandlerList != null) && (m_SubHandlerList.Count > 0))
            {
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

            return bRet;
        }
}
}