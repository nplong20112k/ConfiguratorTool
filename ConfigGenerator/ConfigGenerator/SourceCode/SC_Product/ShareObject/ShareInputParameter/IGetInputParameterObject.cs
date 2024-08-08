namespace ConfigGenerator
{
    interface IGetInputParameterObject
    {
        string GetCITagCode();
        string GetCITagName();
        string GetCITagUserName();
        string GetCIMasterDefaultEugene();
        string GetCIMasterDefaultBologna();
        string GetCIValueSizeByte();
        string GetCIAladdinVisibility();
        string GetCIAladdinCategory();

        string GetCIClassDefault(int index);
        int GetNumberOfInterfaceClass();
    }
}
