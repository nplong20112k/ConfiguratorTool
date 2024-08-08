using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CFactoryReportGenerator
    {
        public enum GENERATE_TYPE
        {
            GENERATE_NONE = 0b_0000_0000,
            GENERATE_ALADDIN_PACKAGE = 0b_0000_0001,
            GENERATE_SOURCE_CODE = 0b_0000_0010,
            GENERATE_BIN_FILES = 0b_0000_0100,
            GENERATE_ALADDIN_PACKAGE_AND_SOURCE_CODE = GENERATE_ALADDIN_PACKAGE | GENERATE_SOURCE_CODE,
            GENERATE_ALADDIN_PACKAGE_AND_BIN_FILES = GENERATE_ALADDIN_PACKAGE | GENERATE_BIN_FILES,
        };
        private List<AReportGenerator> m_ReportHandlerList = null;

        private static CFactoryReportGenerator m_Instance = null;

        public CFactoryReportGenerator(GENERATE_TYPE eGenType)
        {
            // Create Report Handler
            // Add into list of Factory
            // Register as a event receiver
            m_ReportHandlerList = new List<AReportGenerator>();
            
            if ((eGenType & GENERATE_TYPE.GENERATE_ALADDIN_PACKAGE) == GENERATE_TYPE.GENERATE_ALADDIN_PACKAGE)
            {
                CXMLReportGenerator XmlGenerator = new CXMLReportGenerator();
                m_ReportHandlerList.Add(XmlGenerator);
                CEventForwarder.GetInstance().Register(XmlGenerator);
            }
            if ((eGenType & GENERATE_TYPE.GENERATE_SOURCE_CODE) == GENERATE_TYPE.GENERATE_SOURCE_CODE)
            {
                CCodeReportGenerator CodeGenerator = new CCodeReportGenerator();
                m_ReportHandlerList.Add(CodeGenerator);
                CEventForwarder.GetInstance().Register(CodeGenerator);
            }
            if ((eGenType & GENERATE_TYPE.GENERATE_BIN_FILES) == GENERATE_TYPE.GENERATE_BIN_FILES)
            {
                CBinReportGenerator BinGenerator = new CBinReportGenerator();
                m_ReportHandlerList.Add(BinGenerator);
                CEventForwarder.GetInstance().Register(BinGenerator);
            }
            // CXMLReportAutoTestGenerator XmlAutoTestGenerator = new CXMLReportAutoTestGenerator();
            // m_ReportHandlerList.Add(XmlAutoTestGenerator);
            // CEventForwarder.GetInstance().Register(XmlAutoTestGenerator);
        }

        public static CFactoryReportGenerator GetInstance(GENERATE_TYPE eGenType)
        {
            if(m_Instance == null)
            {
                m_Instance = new CFactoryReportGenerator(eGenType);
            }
            return m_Instance;
        }

        public List<AReportGenerator> GetReportGeneratorList()
        {
            return m_ReportHandlerList;
        }

    }
}
