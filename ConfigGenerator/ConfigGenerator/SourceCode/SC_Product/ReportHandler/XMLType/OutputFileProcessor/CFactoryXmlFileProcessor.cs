namespace ConfigGenerator
{
    class CFactoryXmlFileProcessor
    {
        private CTextFileProcess            m_TextFileProcessor         = null;
        private CXMLFileProcess             m_XmlFileProcessor          = null;
        private CXmlHardContentProcessor    m_XmlHardContentProcessor   = null;
        private CXMLFileProcess             m_XmlHardContentReader      = null;

        private CTextFileProcess            m_AutoTestTextFileProcessor = null;
        private CXMLFileProcess             m_XmlAutoTestFileProcessor = null;

        private readonly static CFactoryXmlFileProcessor m_Instance = new CFactoryXmlFileProcessor();
        private CFactoryXmlFileProcessor()
        {
            m_TextFileProcessor         = new CTextFileProcess();
            m_XmlFileProcessor          = new CXMLFileProcess();
            m_XmlHardContentProcessor   = new CXmlHardContentProcessor();
            m_XmlHardContentReader      = new CXMLFileProcess();

            m_AutoTestTextFileProcessor = new CTextFileProcess();
            m_XmlAutoTestFileProcessor  = new CXMLFileProcess(); 
        }

        public static CFactoryXmlFileProcessor GetInstance()
        {
            return m_Instance;
        }

        public CTextFileProcess GetTextFileProcessor()
        {
            return m_TextFileProcessor;
        }

        public IXMLFileProcess GetXmlFileProcessor()
        {
            return m_XmlFileProcessor;
        }

        public CXmlHardContentProcessor GetHardContentProcessor()
        {
            return m_XmlHardContentProcessor;
        }

        public CXMLFileProcess GetHardContentReader()
        {
            return m_XmlHardContentReader;
        }

        public CXMLFileProcess GetAutoTestXmlFileProcessor()
        {
            return m_XmlAutoTestFileProcessor;
        }

        public CTextFileProcess GetAutoTestTextFileProcessor()
        {
            return m_AutoTestTextFileProcessor;
        }
    }
}
