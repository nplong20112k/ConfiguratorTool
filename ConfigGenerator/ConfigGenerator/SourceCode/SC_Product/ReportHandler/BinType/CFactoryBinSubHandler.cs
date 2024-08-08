using System.Collections.Generic;

namespace ConfigGenerator
{
    class CFactoryBinSubHandler
    {
        private List<ABinSubHandler> m_SubHandlerList = null;

        private readonly static CFactoryBinSubHandler m_Instance = new CFactoryBinSubHandler();
        public CFactoryBinSubHandler()
        {
            m_SubHandlerList = new List<ABinSubHandler>();

            // Create sub handler
            CBinNormalConstantHandler               BinNormalConstantHandler                = new CBinNormalConstantHandler();
            CBinSpecialConstantHandler              BinSpecialConstantHandler               = new CBinSpecialConstantHandler();
            CBinNormalDeltaConstantHandler          BinNormalDeltaConstantHandler           = new CBinNormalDeltaConstantHandler();
            CBinSpecialDeltaConstantHandler         BinSpecialDeltaConstantHandler          = new CBinSpecialDeltaConstantHandler();
            CBinSupportInterfaceConstantHandler     BinSupportInterfaceConstantHandler      = new CBinSupportInterfaceConstantHandler();
            CBinNormalExceptionValueHandler         BinNormalExceptionValueHandler          = new CBinNormalExceptionValueHandler();
            CBinSpecialReadableAsciiValueHandler    BinSpecialReadableAsciiValueHandler     = new CBinSpecialReadableAsciiValueHandler();
            CBinSpecialSelectiveValueHandler        BinSpecialSelectiveValueHandler         = new CBinSpecialSelectiveValueHandler();

            // add into list of Factory
            m_SubHandlerList.Add(BinNormalConstantHandler);
            m_SubHandlerList.Add(BinSpecialConstantHandler);
            m_SubHandlerList.Add(BinNormalDeltaConstantHandler);
            m_SubHandlerList.Add(BinSpecialDeltaConstantHandler);
            m_SubHandlerList.Add(BinSupportInterfaceConstantHandler);
            m_SubHandlerList.Add(BinNormalExceptionValueHandler);
            m_SubHandlerList.Add(BinSpecialReadableAsciiValueHandler);
            m_SubHandlerList.Add(BinSpecialSelectiveValueHandler);
        }

        public static CFactoryBinSubHandler GetInstance()
        {
            return m_Instance;
        }

        public List<ABinSubHandler> GetSubHandlerList()
        {
            return m_SubHandlerList;
        }
    }
}
