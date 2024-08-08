namespace ConfigGenerator
{
    class CAccessFunctionIntegrator : AIntegerator
    {

        public CAccessFunctionIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_ACESS_FUNCTION)
        {

        }

        public override void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut)
        {
        }
    }
}