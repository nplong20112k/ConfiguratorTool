namespace ConfigGenerator
{
    public class Factory
    {
        //----------------------
        // ATTRIBUTES
        //----------------------
        IEventForwarder EventForwarder;
        IEventReceiver DataParserHandler;
        IEventReceiver ReportGenerator;
        IEventReceiver ReportHandler;
        IEventReceiver InputFileHandler;
        IEventReceiver IntergrateHandler;
        IEventReceiver CStatisticHandler;
        IEventReceiver InputPreHandler;
        IEventReceiver ParameterFileParser;

        //----------------------
        // INTERFACE FUNCTIONS
        //----------------------
        public Factory()
        {
            EventForwarder = CEventForwarder.GetInstance();

            // Initialize
            DataParserHandler = new CDataParser();
            ReportGenerator = new CReportGeneratorDirector();
            ReportHandler = new CReportHandler();
            InputFileHandler = new CInputHandler();
            IntergrateHandler = new CDataIntegrator();
            CStatisticHandler = new CStatisticHandler();
            InputPreHandler = new CInputPreHandler();
            ParameterFileParser = new CParameterFileParser();

            // Register to Event Forwarder
            EventForwarder.Register(DataParserHandler);
            EventForwarder.Register(ReportGenerator);
            EventForwarder.Register(ReportHandler);
            EventForwarder.Register(InputFileHandler);
            EventForwarder.Register(IntergrateHandler);
            EventForwarder.Register(CStatisticHandler);
            EventForwarder.Register(InputPreHandler);
            EventForwarder.Register(ParameterFileParser);
        }
    }
}
