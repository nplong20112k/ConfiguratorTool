namespace ConfigGenerator
{
    public class CFactoryOutputMergedFileProcessor
    {
        private IOutputMergedFileProcessor m_InputFileProcessor = null;

        private readonly static CFactoryOutputMergedFileProcessor m_Instance = new CFactoryOutputMergedFileProcessor();
        public CFactoryOutputMergedFileProcessor()
        {
            m_InputFileProcessor = new COutputMergedFileProcessor();
        }

        public static CFactoryOutputMergedFileProcessor GetInstance()
        {
            return m_Instance;
        }

        public IOutputMergedFileProcessor GetInputFileProcessor()
        {
            return m_InputFileProcessor;
        }
    }
}
