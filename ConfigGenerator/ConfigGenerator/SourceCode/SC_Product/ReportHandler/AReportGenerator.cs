namespace ConfigGenerator
{
    public abstract class AReportGenerator : AEventReceiver
    {
        public AReportGenerator(EVENT_TYPE[] m_MyEvent)
            :base (m_MyEvent)
        {
        }

        public abstract bool ReportDataHandling(CIntegratedDataObject oDataIn);
        public abstract bool ReportDataFinalizing(CIntegratedDataObject oDataIn);
    }
}
