namespace ConfigGenerator
{
    public class CFactoryCommonInputFileProcessor
    {
        private IInputFileProcessor m_InputFileProcessor = null;

        private readonly static CFactoryCommonInputFileProcessor m_Instance = new CFactoryCommonInputFileProcessor();
        public CFactoryCommonInputFileProcessor()
        {
            m_InputFileProcessor = new CCommonInputFileProcessor();
        }

        public static CFactoryCommonInputFileProcessor GetInstance()
        {
            return m_Instance;
        }

        public IInputFileProcessor GetInputFileProcessor()
        {
            return m_InputFileProcessor;
        }

    }
}
