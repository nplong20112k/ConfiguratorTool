namespace ConfigGenerator
{
    public class CFactoryXmlIntegrator : AFactoryIntegrator
    {
        readonly static CFactoryXmlIntegrator m_Instance = new CFactoryXmlIntegrator();
        public CFactoryXmlIntegrator()
        {
            // New integrate component
            AIntegerator TableRefIntegrator = new CTableRefIntegrator();
            AIntegerator ParameterIntegrator = new CParameterIntegerator();
            AIntegerator PositionIntegrator = new CPositionIntegrator();
            AIntegerator RuleIntegrator = new CRuleIntegrator();

            // Add integrate component
            AddComponentToList(PositionIntegrator);
            AddComponentToList(ParameterIntegrator);
            AddComponentToList(RuleIntegrator);
            AddComponentToList(TableRefIntegrator);

            CEventForwarder.GetInstance().Register((IEventReceiver)RuleIntegrator);
            CEventForwarder.GetInstance().Register((IEventReceiver)TableRefIntegrator);
            CEventForwarder.GetInstance().Register((IEventReceiver)ParameterIntegrator);
        }

        public static CFactoryXmlIntegrator GetInstance()
        {
            return m_Instance;
        }
    }
}
