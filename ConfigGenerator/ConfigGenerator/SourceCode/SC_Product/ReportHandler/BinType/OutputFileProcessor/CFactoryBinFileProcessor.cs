using System.Collections.Generic;

namespace ConfigGenerator
{
    class CFactoryBinFileProcessor
    {
        private List<IBinFileProcessor> m_SubProcessorList = null;

        private readonly static CFactoryBinFileProcessor m_Instance = new CFactoryBinFileProcessor();
        public CFactoryBinFileProcessor()
        {
            m_SubProcessorList = new List<IBinFileProcessor>();

            // Create sub handler
            CBinNormalConstantFileProcessor             BinNormalConstantFileProcessor              = new CBinNormalConstantFileProcessor();
            CBinSpecialConstantFileProcessor            BinSpecialConstantFileProcessor             = new CBinSpecialConstantFileProcessor();
            CBinNormalDeltaConstantFileProcessor        BinNormalDeltaConstantFileProcessor         = new CBinNormalDeltaConstantFileProcessor();
            CBinSpecialDeltaConstantFileProcessor       BinSpecialDeltaConstantFileProcessor        = new CBinSpecialDeltaConstantFileProcessor();
            CBinSupportInterfaceConstantFileProcessor   BinSupportInterfaceConstantFileProcessor    = new CBinSupportInterfaceConstantFileProcessor();
            CBinNormalExceptionValueFileProcessor       BinNormalExceptionValueFileProcessor        = new CBinNormalExceptionValueFileProcessor();
            CBinSpecialReadableAsciiValueFileProcessor  BinSpecialReadableAsciiValueFileProcessor   = new CBinSpecialReadableAsciiValueFileProcessor();
            CBinSpecialSelectiveValueFileProcessor      BinSpecialSelectiveValueFileProcessor       = new CBinSpecialSelectiveValueFileProcessor();
            CBinCheckerFileProcessor                    binCheckerFileProcessor                     = new CBinCheckerFileProcessor();

            // add into list of Factory
            m_SubProcessorList.Add(BinNormalConstantFileProcessor);
            m_SubProcessorList.Add(BinSpecialConstantFileProcessor);
            m_SubProcessorList.Add(BinNormalDeltaConstantFileProcessor);
            m_SubProcessorList.Add(BinSpecialDeltaConstantFileProcessor);
            m_SubProcessorList.Add(BinSupportInterfaceConstantFileProcessor);
            m_SubProcessorList.Add(BinNormalExceptionValueFileProcessor);
            m_SubProcessorList.Add(BinSpecialReadableAsciiValueFileProcessor);
            m_SubProcessorList.Add(BinSpecialSelectiveValueFileProcessor);
            m_SubProcessorList.Add(binCheckerFileProcessor);
        }

        public static CFactoryBinFileProcessor GetInstance()
        {
            return m_Instance;
        }

        public IBinFileProcessor GetFileProcessor(BIN_FILE_PROCESSOR_TYPE ProcessType)
        {
            foreach (IBinFileProcessor element in m_SubProcessorList)
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
