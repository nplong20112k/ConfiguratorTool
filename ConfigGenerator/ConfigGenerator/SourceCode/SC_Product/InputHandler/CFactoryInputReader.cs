using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CFactoryInputReader
    {
        private List<IInputReader> m_InputReaderList = null;

        private readonly static CFactoryInputReader m_Instance = new CFactoryInputReader();
        public CFactoryInputReader()
        {
            m_InputReaderList = new List<IInputReader>();

            // Create Report Handler
            IInputReader MainInputReader = new CMainInputReader();
            IInputReader SupportedModelInputReader = new CSpupportedModelInputReader();

            // Add into list of Factory
            m_InputReaderList.Add(MainInputReader);
            m_InputReaderList.Add(SupportedModelInputReader);
        }

        public static CFactoryInputReader GetInstance()
        {
            return m_Instance;
        }

        public List<IInputReader> GetInputReaderList()
        {
            return m_InputReaderList;
        }

    }
}
