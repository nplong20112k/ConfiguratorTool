namespace ConfigGenerator
{
    public enum RECEIVER_ID
    { 
        ID_UNKNOW = 0,
        ID_DATA_INTEGRATOR,
        ID_DATA_PARSER,
        ID_REPORT_GENERATOR,
        ID_OUTPUT_HANDLER,
        ID_INPUT_HANDLER,
    }

    public class DATA_QUEUE
    { 
        private EVENT_TYPE       m_eEvent;
        private IShareObject     m_oData;

        public DATA_QUEUE(EVENT_TYPE eEvent, IShareObject oData)
        {
            m_eEvent = eEvent;
            m_oData = oData;
        }

        public EVENT_TYPE GetEvent ()
        {
            return m_eEvent;
        }

        public IShareObject GetData ()
        {
            return m_oData;
        }
    }

    public interface IEventReceiver
    {
        //-------------------
        // Operations Interface
        //-------------------
        bool HandleEvent(EVENT_TYPE eventName, IShareObject oData);
    }
}
