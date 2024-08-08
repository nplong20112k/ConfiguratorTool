using System.Collections.Generic;

namespace ConfigGenerator
{
    class CFactoryXMLSubhandlerAutoTest
    {
        private List<AXMLSubHandler> m_AutoTestSubHandlerList = null;

        private readonly static CFactoryXMLSubhandlerAutoTest m_Instance = new CFactoryXMLSubhandlerAutoTest();

        public CFactoryXMLSubhandlerAutoTest()
        {
            m_AutoTestSubHandlerList = new List<AXMLSubHandler>();

            CXMLAutoTestTagItemHandler TagItemsHandler = new CXMLAutoTestTagItemHandler();

            m_AutoTestSubHandlerList.Add(TagItemsHandler);
        }

        public static CFactoryXMLSubhandlerAutoTest GetInstance()
        {
            return m_Instance;
        }

        public List<AXMLSubHandler> GetAutoTestSubHanlerList()
        {
            return m_AutoTestSubHandlerList;
        }
    }
}
