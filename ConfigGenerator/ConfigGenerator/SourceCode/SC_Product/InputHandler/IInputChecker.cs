namespace ConfigGenerator
{
    public interface IInputChecker
    {
        void Initialize();
        bool CheckingInputFile(string sFilePath);
    }
}