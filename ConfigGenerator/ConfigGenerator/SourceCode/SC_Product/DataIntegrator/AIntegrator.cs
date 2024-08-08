namespace ConfigGenerator
{
    public enum INTEGRATOR_PRIORITY_ID
    {
        INTEGRATOR_PRIORITY_DEFAULT = 0,

        // FOR REPORT_TYPE_XML
        INTEGRATOR_PRIORITY_TABLE_REF,
        INTEGRATOR_RPIORITY_PARAMETER,
        INTEGRATOR_PRIORITY_POSITION,
        INTEGRATOR_PRIORITY_RULE,

        // FOR REPORT_TYPE_SOURCE_CODE
        INTEGRATOR_PRIORITY_COMMON_CONST_TABLE = 64,
        INTEGRATOR_PRIORITY_DELTA_CONST_TABLE,
        INTEGRATOR_PRIORITY_STRUCTURE,
        INTEGRATOR_PRIORITY_ACESS_FUNCTION,
        INTEGRATOR_PRIORITY_CI_ENUM_TABLE,

        // FOR REPORT_TYPE_SOURCE_CODE
        INTEGRATOR_PRIORITY_AUTOMATION_TEST_PARAMETER = 128,
        INTEGRATOR_PRIORITY_AUTOMATION_TEST_TABLE_REF,
    }

    public abstract class AIntegerator : AEventReceiver
    {
        private INTEGRATOR_PRIORITY_ID m_Priority = INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_DEFAULT;

        public AIntegerator(INTEGRATOR_PRIORITY_ID Priority, EVENT_TYPE[] myEvent = null)
                : base(myEvent)
        {
            m_Priority = Priority;
        }

        public INTEGRATOR_PRIORITY_ID GetPriority()
        {
            return m_Priority;
        }

        public abstract void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut);

        public virtual void FinalizeIntegratingProcess(ref CIntegratedDataObject oDataOut) {; }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData) {; }
    }
}
