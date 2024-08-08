using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CFactoryInputChecker
    {
        private List<IInputChecker> m_InputCheckerList = null;

        private readonly static CFactoryInputChecker m_Instance = new CFactoryInputChecker();
        public CFactoryInputChecker()
        {
            m_InputCheckerList = new List<IInputChecker>();

            // Create Report Handler
            IInputChecker MainInputChecker              = new CMainInputChecker();
            IInputChecker SupportedModelInputChecker    = new CSpupportedModelInputChecker();

            // Add into list of Factory
            m_InputCheckerList.Add(MainInputChecker);
            m_InputCheckerList.Add(SupportedModelInputChecker);
        }

        public static CFactoryInputChecker GetInstance()
        {
            return m_Instance;
        }

        public List<IInputChecker> GetInputCheckerList()
        {
            return m_InputCheckerList;
        }

    }
}
