using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CFactoryInputFileProcessor
    {
        private List<IInputFileProcessor> m_SubProcessorList = null;

        private readonly static CFactoryInputFileProcessor  m_Instance = new CFactoryInputFileProcessor();

        public CFactoryInputFileProcessor()
        {
            m_SubProcessorList = new List<IInputFileProcessor>();

            // Create file processor
            //m_FileProcessor = new CExcelFileProcessor();
            CMainInputFileProcessor             MainInputFileProcessor = new CMainInputFileProcessor();
            CSupportedModelInputFileProcessor   SupportedModelInputFileProcessor = new CSupportedModelInputFileProcessor();

            // add into list of Factory
            m_SubProcessorList.Add(MainInputFileProcessor);
            m_SubProcessorList.Add(SupportedModelInputFileProcessor);
        }

        public static CFactoryInputFileProcessor GetInstance()
        {
            return m_Instance;
        }

        public IInputFileProcessor GetInputFileProcessor(INPUT_FILE_PROCESSOR_TYPE ProcessType)
        {
            foreach (IInputFileProcessor element in m_SubProcessorList)
            {
                if (element.GetProcessorType() == ProcessType)
                {
                    return element;
                }
            }
            return null;
        }
    }
}
