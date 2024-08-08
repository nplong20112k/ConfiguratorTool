namespace ConfigGenerator
{
    public class CFactorySourceCodeIntegrator : AFactoryIntegrator
    {
        readonly static CFactorySourceCodeIntegrator m_Instance = new CFactorySourceCodeIntegrator();
        public CFactorySourceCodeIntegrator()
        {
            // New integrate component
            AIntegerator CommonConstTableIntegrator     = new CCommonConstTableIntegrator();
            AIntegerator DeltaConstTableIntegrator      = new CDeltaConstTableIntegrator();
            AIntegerator AccessFunctionIntegrator       = new CAccessFunctionIntegrator();   
            AIntegerator CIEnumTableIntegrator          = new CCIEnumTableIntegrator();            

            // Add integrate component
            AddComponentToList(CommonConstTableIntegrator);
            AddComponentToList(DeltaConstTableIntegrator);
            AddComponentToList(AccessFunctionIntegrator);
            AddComponentToList(CIEnumTableIntegrator);

            CEventForwarder.GetInstance().Register((IEventReceiver)CommonConstTableIntegrator);
            CEventForwarder.GetInstance().Register((IEventReceiver)DeltaConstTableIntegrator);
            CEventForwarder.GetInstance().Register((IEventReceiver)CIEnumTableIntegrator);
        }

        public static CFactorySourceCodeIntegrator GetInstance()
        {
            return m_Instance;
        }
    }
}