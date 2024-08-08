using System.Collections.Generic;

namespace ConfigGenerator
{
    class CFactoryCodeSubHandler
    {
        private List<ACodeSubHandler> m_SubHandlerList = null;

        private readonly static CFactoryCodeSubHandler m_Instance = new CFactoryCodeSubHandler();
        public CFactoryCodeSubHandler()
        {
            m_SubHandlerList = new List<ACodeSubHandler>();

            // Create sub handler
            CCodeNormalConstTableHandler            NormalTableHandler                          = new CCodeNormalConstTableHandler();
            CCodeDeltaNormalTableHandler            DeltaTableNormalHandler                     = new CCodeDeltaNormalTableHandler();
            CCodeNormalAccessFuncHandler            NormalAccessFuncHandler                     = new CCodeNormalAccessFuncHandler();
            CCodeNormalVerifyExceptionHandler       NormalVerifyExceptionHandler                = new CCodeNormalVerifyExceptionHandler();
            CCodeSpecialConstTableHandler           SpecialTableHandler                         = new CCodeSpecialConstTableHandler();
            CCodeSpecialVerifyRangeHandler          SpecialVerifyRangeHandler                   = new CCodeSpecialVerifyRangeHandler();
            CCodeDeltaSpecialTableHandler           DeltaTableSpecialHandler                    = new CCodeDeltaSpecialTableHandler();
            CCodeCIEnumTableGenHandler              CIEnumTableGenHandler                       = new CCodeCIEnumTableGenHandler();
            CCodeStructureHandler                   StructureHandler                            = new CCodeStructureHandler();
            CCodeInterfaceTypedefHandler            InterfaceTypedefHandler                     = new CCodeInterfaceTypedefHandler();
            CCodeVerifyInterfaceHandler             VerifyInterfaceHandler                      = new CCodeVerifyInterfaceHandler();
            CCodeSpecialVerifyReadableAsciiHandler  SpecialVerifyReadableAsciiHandler           = new CCodeSpecialVerifyReadableAsciiHandler();
            CCodeSpecialVerifySelectiveHandler      SpecialVerifySelectiveHandler               = new CCodeSpecialVerifySelectiveHandler();
            CCodeCIEnumTableCopyHandler             CIEnumTableCopyHandler                      = new CCodeCIEnumTableCopyHandler();
            CCodeSearchNormalItemIndex              SearchNormalItemHandler                     = new CCodeSearchNormalItemIndex();
            CCodeConvertHexIntHandler               ConvertHexIntHandler                        = new CCodeConvertHexIntHandler();

            // add into list of Factory
            m_SubHandlerList.Add(NormalTableHandler);
            m_SubHandlerList.Add(NormalAccessFuncHandler);        
            m_SubHandlerList.Add(NormalVerifyExceptionHandler);
            m_SubHandlerList.Add(DeltaTableNormalHandler);
            m_SubHandlerList.Add(SpecialTableHandler);
            m_SubHandlerList.Add(SpecialVerifyRangeHandler);
            m_SubHandlerList.Add(DeltaTableSpecialHandler);
            m_SubHandlerList.Add(CIEnumTableGenHandler);
            m_SubHandlerList.Add(StructureHandler);
            m_SubHandlerList.Add(InterfaceTypedefHandler);
            m_SubHandlerList.Add(VerifyInterfaceHandler);
            m_SubHandlerList.Add(SpecialVerifySelectiveHandler);
            m_SubHandlerList.Add(CIEnumTableCopyHandler);
            m_SubHandlerList.Add(SearchNormalItemHandler);
            m_SubHandlerList.Add(SpecialVerifyReadableAsciiHandler);
            m_SubHandlerList.Add(ConvertHexIntHandler);
        }

        public static CFactoryCodeSubHandler GetInstance()
        {
            return m_Instance;
        }

        public List<ACodeSubHandler> GetSubHandlerList()
        {
            return m_SubHandlerList;
        }
    }
}
