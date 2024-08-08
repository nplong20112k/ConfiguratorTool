namespace ConfigGenerator
{
    public class CFactoryAutoTestIntegrator : AFactoryIntegrator
    {
        readonly static CFactoryAutoTestIntegrator m_Instance = new CFactoryAutoTestIntegrator();
        public CFactoryAutoTestIntegrator()
        {
            // New integrate component
            AIntegerator AutoTestParamIntegrator = new CAutoTestParamIntegrator();
            AIntegerator AutoTestTableRefIntegrator = new CAutoTestTableRefIntegrator();

            // Add integrate component
            AddComponentToList(AutoTestParamIntegrator);
            AddComponentToList(AutoTestTableRefIntegrator);

            CEventForwarder.GetInstance().Register((IEventReceiver)AutoTestParamIntegrator);
        }

        public static CFactoryAutoTestIntegrator GetInstance()
        {
            return m_Instance;
        }
    }
}
