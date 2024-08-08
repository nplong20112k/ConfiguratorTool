namespace ConfigGenerator
{
    public interface IFileHandler
    {
        IFileHandler GetInstance();
        bool IsFileOpen();
        bool OpenFile(string sFilePath);
        bool CloseFile();
        string ReadCell(int iColumn, int iRow);
        int GetNumberValidField();
    }
}
