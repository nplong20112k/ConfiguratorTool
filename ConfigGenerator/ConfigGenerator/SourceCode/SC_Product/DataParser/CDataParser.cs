using System.Collections.Generic;

namespace ConfigGenerator
{
    public class Event_DataParser
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[] 
        { 
            EVENT_TYPE.EVENT_REQUEST_PARSE_INPUT_DATA,
        };
    }

    class CDataParser : AEventReceiver
    {
        //----------------------
        // ATTRIBUTES
        //----------------------
        private List<AParser>       m_ParserList = null;
        private CParsedDataObject   m_DataParserObject = null;

        // SOURCE CODE PARSER

        //----------------------
        // PUBLIC FUNCTIONS
        //----------------------
        public CDataParser()
            : base(new Event_DataParser().m_MyEvent)
        {
            m_ParserList = CFactoryExcelParser.GetInstance().GetLComponentsList();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            EVENT_TYPE tempEvent = Event;

            switch (tempEvent)
            {
                case EVENT_TYPE.EVENT_REQUEST_PARSE_INPUT_DATA:
                    if (oData != null)
                    {
                        if (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_INPUT_DATA_OBJECT)
                        {
                            m_DataParserObject = null;
                            ProcessParserData(oData);
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
        private bool ProcessParserData(IShareObject oData)
        {
            bool bRet = true;
            m_DataParserObject = new CParsedDataObject();

            foreach (AParser element in m_ParserList)
            {
                if (element != null)
                {
                    element.ParserProcessing(oData, ref m_DataParserObject);
                }
            }

            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_INTEGRATE_PARSED_DATA, m_DataParserObject);
            return bRet;
        }

    }
}
