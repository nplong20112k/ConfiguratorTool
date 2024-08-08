namespace ConfigGenerator
{
    public abstract class AParser : AEventReceiver
    {
        public AParser(EVENT_TYPE[] myEvent = null)
            : base(myEvent)
        {
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData) {; }

        public abstract bool ParserProcessing(IShareObject oData, ref CParsedDataObject oShareObject);
    }
}
