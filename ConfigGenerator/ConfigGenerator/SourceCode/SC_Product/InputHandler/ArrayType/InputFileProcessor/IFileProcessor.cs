namespace ConfigGenerator
{
    public enum INPUT_FILE_PROCESSOR_TYPE
    {
        MAIN_INPUT_FILE_PROCESSOR = 0,
        SUPPORTED_MODEL_FILE_PROCESSOR,
        COMMON_INPUT_FILE_PROCESSOR,
        TOTAL_FILE_PROCESSOR
    }

    public interface IInputFileProcessor
    {
        INPUT_FILE_PROCESSOR_TYPE GetProcessorType();

        bool OpenFile(string sFilePath);
        bool CloseFile();

        bool IsFileOpen();

        string ReadElement(int iFieldIdx, int iParamIdx);

        int GetNumberTotalField(int iParamIdx);
        int GetNumberValidField();
        int GetNumberParamValid();
        int CountNumberParamValid(int ColumnRef);
    }
}
