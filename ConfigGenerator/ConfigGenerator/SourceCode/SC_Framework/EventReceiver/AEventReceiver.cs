using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace ConfigGenerator
{
    public abstract class AEventReceiver : IEventReceiver
    {
        //---------------------
        // ATTRIBUTES
        //---------------------
        private EVENT_TYPE[] m_EventList = null;
        private Queue<DATA_QUEUE> m_QueueList = new Queue<DATA_QUEUE>();
        private bool m_InProcess = false;

        //---------------------
        // INTERFACE FUNCTIONS
        //---------------------
        public AEventReceiver(EVENT_TYPE[] EventList)
        {
            if (m_EventList == null)
            {
                if (EventList != null)
                {
                    m_EventList = new EVENT_TYPE[EventList.Length];
                    m_EventList = EventList;
                }
            }
            else
            {
                // TODO::
            }
        }
        
        public bool HandleEvent(EVENT_TYPE eventName, IShareObject oData)
        {
            bool bRet = false;
            if (m_EventList != null)
            {
                foreach (EVENT_TYPE element in m_EventList)
                {
                    if (eventName == element)
                    {
                        DATA_QUEUE QueueData = new DATA_QUEUE(eventName, oData) ;

                        lock (m_QueueList)
                        {
                            m_QueueList.Enqueue(QueueData);
                        }

                        if (m_InProcess == false)
                        {
                            m_InProcess = true;
                            Task.Factory.StartNew(TaskHandler);
                        }

                        bRet = true;
                        break;
                    }
                }
            }
            return bRet;
        }

        //---------------------
        // INTERNAL FUNCTIONS
        //---------------------

        protected void TaskHandler()
        {
            DATA_QUEUE tempData;

            do
            {
                tempData = null;
                lock (m_QueueList)
                {
                    if (m_QueueList.Count > 0)
                    {
                        tempData = m_QueueList.Dequeue();
                    }
                }
                if (tempData != null)
                {
                    try
                    {
                        CLogger.GetInstance().Log("Receiver_" + tempData.GetEvent().ToString());

                        EventHandler(tempData.GetEvent(), tempData.GetData());
                    }
                    catch (Exception e)
                    {
                         CLogger.GetInstance().Log("Exception: " + e.ToString());
                         Program.SystemHandleStatusInfo("Exception: " + e.ToString());
                    }
                }
            } while (tempData != null);

            m_InProcess = false;
        }

        protected abstract void EventHandler(EVENT_TYPE Event, IShareObject oData);
    }

}
