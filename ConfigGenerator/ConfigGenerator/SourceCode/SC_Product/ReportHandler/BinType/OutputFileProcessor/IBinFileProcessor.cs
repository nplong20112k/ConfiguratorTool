namespace ConfigGenerator
{
    public enum BIN_FILE_PROCESSOR_TYPE
    {
        BIN_NORMAL_CONSTANT,
        BIN_SPECIAL_CONSTANT,
        BIN_NORMAL_DELTA_CONSTANT,
        BIN_SPECIAL_DELTA_CONSTANT,
        BIN_SUPPORT_INTERFACE_CONSTANT,
        BIN_NORMAL_EXCEPTION_VALUE,
        BIN_SPECIAL_READABLE_ASCII_VALUE,
        BIN_SPECIAL_SELECTIVE_VALUE,
        BIN_CHECKER,
    }

    public interface IBinFileProcessor
    {
        BIN_FILE_PROCESSOR_TYPE GetProcessorType();
        bool CreateNewFile(string sFilePath);
        bool AddNewItem(byte[] buffer, int buffer_len);
        bool AddNewItem(byte[] buffer, int offset, int buffer_len);
        bool CloseFile();
        string[] LoadingFile(string sFilePath);
    }
}
