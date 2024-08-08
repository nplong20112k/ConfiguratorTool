namespace ConfigGenerator
{
    public abstract class AReportChecker : AEventReceiver
    {
        public enum REPORT_FILE_TYPE
        {
            REPORT_FILE_UNKNOW = 0,
            REPORT_FILE_XML,
            REPORT_FILE_SOURCE_CODE,
            REPORT_FILE_SOURCE_BIN,
        }

        private string m_FileExtension = null;

        public AReportChecker(REPORT_FILE_TYPE FileType, EVENT_TYPE[] myEvent = null)
                : base(myEvent)
        {
            switch (FileType)
            {
                case REPORT_FILE_TYPE.REPORT_FILE_XML:
                    m_FileExtension = ".xml";
                    break;

                case REPORT_FILE_TYPE.REPORT_FILE_SOURCE_CODE:
                    m_FileExtension = ".h";
                    break;
                
                case REPORT_FILE_TYPE.REPORT_FILE_SOURCE_BIN:
                    m_FileExtension = ".h";
                    break;

                case REPORT_FILE_TYPE.REPORT_FILE_UNKNOW:
                default:
                    break;
            }
        }

        public string GetReportFileExtension()
        {
            return m_FileExtension;
        }

        public abstract void Initialize();

        public abstract bool CheckTemplateFileValid(string sFilePath);
        public abstract bool CheckTemplateFileStructure();
        public abstract bool ProcessTemplateFile();

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData) {; }

    }
}
