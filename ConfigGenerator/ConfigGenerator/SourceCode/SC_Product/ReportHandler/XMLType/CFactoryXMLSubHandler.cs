using System.Collections.Generic;

namespace ConfigGenerator
{
    class CFactoryXMLSubHandler
    {
        private List<AXMLSubHandler> m_SubHandlerList = null;

        private readonly static CFactoryXMLSubHandler m_Instance = new CFactoryXMLSubHandler();
        public CFactoryXMLSubHandler()
        {
            m_SubHandlerList = new List<AXMLSubHandler>();

            // Create sub handler
            CXMLTableRefHandler     TableRefHandler     = new CXMLTableRefHandler();
            CXMLParameterHandler    ParameterHandler    = new CXMLParameterHandler();
            CXMLRuleHandler         RuleHandler         = new CXMLRuleHandler();
            CXMLPositionHandler     PositionHandler     = new CXMLPositionHandler();

            // add into list of Factory
            m_SubHandlerList.Add(TableRefHandler);
            m_SubHandlerList.Add(ParameterHandler);
            m_SubHandlerList.Add(RuleHandler);
            m_SubHandlerList.Add(PositionHandler);
        }

        public static CFactoryXMLSubHandler GetInstance()
        {
            return m_Instance;
        }

        public List<AXMLSubHandler> GetSubHandlerList()
        {
            return m_SubHandlerList;
        }
    }
}
