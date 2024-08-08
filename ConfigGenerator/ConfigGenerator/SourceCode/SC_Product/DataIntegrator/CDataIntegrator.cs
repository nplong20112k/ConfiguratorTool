using System.Collections.Generic;

namespace ConfigGenerator
{
    public class Event_IntegratorHandler
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_INTEGRATE_PARSED_DATA,
            EVENT_TYPE.EVENT_REQUEST_FINALIZE_REPORT_FILE,
            EVENT_TYPE.EVENT_RESPOND_WRITE_LAST_PART_TO_REPORT_FILE_DONE,
        };
    }

    class CDataIntegrator: AEventReceiver
    {

        //----------------------
        // ATTRIBUTES
        //----------------------
        private List<AIntegerator>  m_IntegratorList = null;

        //----------------------
        // PUBLIC FUNCTIONS
        //----------------------
        public CDataIntegrator()
            : base (new Event_IntegratorHandler().m_MyEvent)
        {
            m_IntegratorList = CFactoryIntegrator.GetInstance().GetIntegratorList();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_INTEGRATE_PARSED_DATA:
                    if (oData != null)
                    {
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                        {
                            IntegratingProcess(oData);
                        }
                    }
                    else
                    {
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, null);
                    }
                    break;

                case EVENT_TYPE.EVENT_REQUEST_FINALIZE_REPORT_FILE:
                    {
                        FinializeIntegratingProcess();
                    }
                    break;

                case EVENT_TYPE.EVENT_RESPOND_WRITE_LAST_PART_TO_REPORT_FILE_DONE:
                    {
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_FINALIZE_REPORT_FILE_DONE, null);
                    }
                    break;

                default:
                    break;
            }
        }

        private void IntegratingProcess(IShareObject oData = null)
        {
            if (m_IntegratorList != null)
            {
                CIntegratedDataObject IntegrateData = new CIntegratedDataObject();

                for (int i = 0; i < m_IntegratorList.Count; i++)
                {
                    m_IntegratorList[i].IntegratingProcess(ref oData, ref IntegrateData);
                }
                
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_WRITE_TO_REPORT_FILE, IntegrateData);
            }
        }

        private void FinializeIntegratingProcess()
        {
            if (m_IntegratorList != null)
            {
                CIntegratedDataObject IntegrateData = new CIntegratedDataObject();

                for (int i = 0; i < m_IntegratorList.Count; i++)
                {
                    m_IntegratorList[i].FinalizeIntegratingProcess(ref IntegrateData);
                }

                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_WRITE_LAST_PART_TO_REPORT_FILE, IntegrateData);
            }
        }

    }
}
