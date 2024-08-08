using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigGenerator
{
    
    public class CEventForwarder:IEventForwarder
    {
        //----------------------
        // ATTRIBUTES
        //----------------------
        static readonly CEventForwarder m_EventForwarder = new CEventForwarder();

        private List<IEventReceiver>    m_ReceiverList = new List<IEventReceiver>();

        private Queue<DATA_QUEUE>       m_QueueList = new Queue<DATA_QUEUE>();
        private bool                    m_bInProcess = false;

        //----------------------
        // INTERFACE FUNCTIONS
        //----------------------
        public static CEventForwarder GetInstance ()
        {
            return m_EventForwarder;
        }

        public CEventForwarder()
        {
        }

        public bool Register(IEventReceiver receiver)
        {
            bool bRet = false;
            if (receiver != null)
            { 
                m_ReceiverList.Add(receiver);
                bRet = true;
            }

            return bRet;
        }

        public bool Unregister(IEventReceiver receiver)
        {
            bool bRet = false;

            return bRet;
        }

        public void Notify(EVENT_TYPE eventName, IShareObject oData)
        {
            lock (m_QueueList)
            {
                DATA_QUEUE NewEvent = new DATA_QUEUE(eventName, oData);
                m_QueueList.Enqueue(NewEvent);
            }

            CLogger.GetInstance().Log("Forwarder_" + eventName.ToString());

            if (m_bInProcess == false)
            {
                m_bInProcess = true;
                Task.Factory.StartNew(UpdateForReceivers);
            }
        }

        //----------------------
        // INTERNAL FUNCTION
        //----------------------
        public void UpdateForReceivers()
        {
            DATA_QUEUE TempEvent;

            do
            {
                TempEvent = null;
                lock (m_QueueList)
                {
                    if (m_QueueList.Count > 0)
                    {
                        TempEvent = m_QueueList.Dequeue();
                    }
                }
                if (TempEvent != null)
                {
                    try
                    {
                        if (TempEvent.GetEvent() != EVENT_TYPE.EVENT_UNKNOW)
                        {
                            foreach (IEventReceiver element in m_ReceiverList)
                            {
                                element.HandleEvent(TempEvent.GetEvent(), TempEvent.GetData());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.ToString());
                    }
                }
            } while (TempEvent != null);

            m_bInProcess = false;
        }

        private IEventReceiver PickReceiverAsID(RECEIVER_ID id)
        {
            IEventReceiver retReceiver = null;

            return retReceiver;
        }
    }
}
