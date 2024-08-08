namespace ConfigGenerator
{
    public interface IOutputMergedFileProcessor
    {
        string CreateNewFile(string sFilePath);
        bool AddNewItem(string sData);
        bool AddNewItem(string[] sDataList);
        bool CloseFile();
        bool IsFileExistent(string sFilePath);

        bool MoveAndRenameFile(string sCurrentFileName, string sProductName);
    }
}
