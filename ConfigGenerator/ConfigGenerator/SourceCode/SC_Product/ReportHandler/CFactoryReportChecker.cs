using System.Collections.Generic;
using static ConfigGenerator.CFactoryReportGenerator;

namespace ConfigGenerator
{
    public class CFactoryReportChecker
    {
        public enum CHECK_TYPE
        {
            CHECK_NONE = 0b_0000_0000,
            CHECK_ALADDIN_PACKAGE = 0b_0000_0001,
            CHECK_SOURCE_CODE = 0b_0000_0010,
            CHECK_BIN_FILES = 0b_0000_0100,
            CHECK_ALADDIN_PACKAGE_AND_SOURCE_CODE = CHECK_ALADDIN_PACKAGE | CHECK_SOURCE_CODE,
            CHECK_ALADDIN_PACKAGE_AND_BIN_FILES = CHECK_ALADDIN_PACKAGE | CHECK_BIN_FILES,
        };

        private List<AReportChecker> m_ReportCheckerList = null;

        private static CFactoryReportChecker m_Instance = null;
        public CFactoryReportChecker(CHECK_TYPE eCheckType)
        {
            m_ReportCheckerList = new List<AReportChecker>();
            // Register as a event receiver
            if ((eCheckType & CHECK_TYPE.CHECK_ALADDIN_PACKAGE) == CHECK_TYPE.CHECK_ALADDIN_PACKAGE)
            {
                AReportChecker XmlFileChecking = new CXMLReportChecker();
                m_ReportCheckerList.Add(XmlFileChecking);
                CEventForwarder.GetInstance().Register(XmlFileChecking);
            }
            if ((eCheckType & CHECK_TYPE.CHECK_SOURCE_CODE) == CHECK_TYPE.CHECK_SOURCE_CODE)
            {
                AReportChecker CodeFileChecking = new CCodeReportChecker();
                m_ReportCheckerList.Add(CodeFileChecking);
                CEventForwarder.GetInstance().Register(CodeFileChecking);
            }
            if ((eCheckType & CHECK_TYPE.CHECK_BIN_FILES) == CHECK_TYPE.CHECK_BIN_FILES)
            {
                AReportChecker BinFileChecking = new CBinReportChecker();
                m_ReportCheckerList.Add(BinFileChecking);
                CEventForwarder.GetInstance().Register(BinFileChecking);
            }
        }

        public static CFactoryReportChecker GetInstance(CHECK_TYPE eCheckType)
        {
            if(m_Instance == null)
            {
                m_Instance = new CFactoryReportChecker(eCheckType);
            }
            return m_Instance;            
        }

        public List<AReportChecker> GetReportFileCheckerList()
        {
            return m_ReportCheckerList;
        }
    }
}
