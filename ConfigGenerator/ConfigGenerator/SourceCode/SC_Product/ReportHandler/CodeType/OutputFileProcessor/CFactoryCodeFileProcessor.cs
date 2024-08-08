using System.Collections.Generic;

namespace ConfigGenerator
{
    class CFactoryCodeFileProcessor
    {
        private List<ICodeFileProcessor> m_SubProcessorList = null;

        private readonly static CFactoryCodeFileProcessor m_Instance = new CFactoryCodeFileProcessor();
        public CFactoryCodeFileProcessor()
        {
            m_SubProcessorList = new List<ICodeFileProcessor>();

            // Create sub handler
            CCodeNormalTableFileProcessor                   NormalTableFileProcessor                = new CCodeNormalTableFileProcessor();
            CCodeNormalDeltaTableFileProcessor              NormalDeltaTableFileProcessor           = new CCodeNormalDeltaTableFileProcessor();
            CCodeNormalAccessFuncFileProcessor              NormalAccessFuncFileProcessor           = new CCodeNormalAccessFuncFileProcessor();
            CCodeNormalVerifyExceptionProcessor             NormalVerifyValueFileProcessor          = new CCodeNormalVerifyExceptionProcessor();
            CCodeSearchNormalItemIndexFileProcessor         NormalSearchItemIndexProcessor          = new CCodeSearchNormalItemIndexFileProcessor();

            CCodeSpecialTableFileProcessor                  SpecialTableFileProcessor               = new CCodeSpecialTableFileProcessor();
            CCodeSpecialDeltaTableFileProcessor             SpecialDeltaTableFileProcessor          = new CCodeSpecialDeltaTableFileProcessor();
            CCodeSpecialVerifyRangeProcessor                SpecialVerifyRangeValueFileProcessor    = new CCodeSpecialVerifyRangeProcessor();

            CCodeCheckerFileProcessor                       CheckerFileProcessor                    = new CCodeCheckerFileProcessor();
            CCodeStructureFileProcessor                     StructureFileProcessor                  = new CCodeStructureFileProcessor();
            CCodeCIEnumTableGenFileProcessor                CIEnumTableFileProcessor                = new CCodeCIEnumTableGenFileProcessor();
            CCodeCIEnumTableCopyFileProcessor               CIEnumerationTableFileProcessor         = new CCodeCIEnumTableCopyFileProcessor();
            CCodeInterfaceTypedefFileProcessor              InterfaceTypdefFileProcessor            = new CCodeInterfaceTypedefFileProcessor();
            CCodeVerifyInterfaceFileProcessor               VerifyInterfaceFileProcessor            = new CCodeVerifyInterfaceFileProcessor();
            CCodeSpecialVerifySelectiveValueFileProcessor   SpecialVerifySelectiveFileProcessor     = new CCodeSpecialVerifySelectiveValueFileProcessor();
            CCodeSpecialVerifyReadableAsciiFileProcessor    SpecialVerifyReadableAsciiFileProcessor = new CCodeSpecialVerifyReadableAsciiFileProcessor();
            CCodeConvertHexIntProcessor                     ConvertHexIntFileProcessor              = new CCodeConvertHexIntProcessor();

            // add into list of Factory
            m_SubProcessorList.Add(NormalTableFileProcessor);
            m_SubProcessorList.Add(NormalDeltaTableFileProcessor);
            m_SubProcessorList.Add(NormalAccessFuncFileProcessor);
            m_SubProcessorList.Add(NormalVerifyValueFileProcessor);
            m_SubProcessorList.Add(NormalSearchItemIndexProcessor);

            m_SubProcessorList.Add(SpecialTableFileProcessor);
            m_SubProcessorList.Add(SpecialDeltaTableFileProcessor);
            m_SubProcessorList.Add(SpecialVerifyRangeValueFileProcessor);

            m_SubProcessorList.Add(CheckerFileProcessor);
            m_SubProcessorList.Add(StructureFileProcessor);
            m_SubProcessorList.Add(CIEnumTableFileProcessor);
            m_SubProcessorList.Add(CIEnumerationTableFileProcessor);
            m_SubProcessorList.Add(InterfaceTypdefFileProcessor);
            m_SubProcessorList.Add(VerifyInterfaceFileProcessor);
            m_SubProcessorList.Add(SpecialVerifySelectiveFileProcessor);
            m_SubProcessorList.Add(SpecialVerifyReadableAsciiFileProcessor);
            m_SubProcessorList.Add(ConvertHexIntFileProcessor);
        }

        public static CFactoryCodeFileProcessor GetInstance()
        {
            return m_Instance;
        }

        public ICodeFileProcessor GetFileProcessor(CODE_FILE_PROCESSOR_TYPE ProcessType)
        {
            foreach (ICodeFileProcessor element in m_SubProcessorList)
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
