namespace ConfigGenerator
{
    public interface IInputReader
    {
        void GetInputParameter(int iIndex, CInputParameterObject oCurObject);
        void UpdateInputParameter(CInputParameterObject oCurObject);
        int GetNumberParameter();
        void GetConfigInfo(CInputInfoObject oInfoObject);
    }
}