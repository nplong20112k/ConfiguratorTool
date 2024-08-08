using System;
using System.IO;
using System.Linq;

namespace ConfigGenerator
{
    class COutputMergedFileProcessor : IOutputMergedFileProcessor
    {
        private const string m_sFileType = ".csv";
        private string m_sFilePath = null;

        public COutputMergedFileProcessor()
        {

        }

        public virtual string CreateNewFile(string sFilePath)
        {
            if (sFilePath == null)
            {
                return null;
            }

            string sFullFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + CConfigTool.GetInstance().sMergedFolderName + Path.DirectorySeparatorChar + sFilePath + m_sFileType; ;
            
            if (File.Exists(sFullFilePath))
            {
                try
                {
                    File.Delete(sFullFilePath);
                }
                catch
                {
                    return null;
                }
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sFullFilePath));
                Program.SetUnixFileFullPermissions(Path.GetDirectoryName(sFullFilePath));
                File.AppendAllText(sFullFilePath, null);
            }
            catch
            {
                return null;
            }

            m_sFilePath = sFullFilePath;
            Program.SetUnixFileFullPermissions(m_sFilePath);
            return sFullFilePath;
        }

        public string[] LoadingFile(string sFilePath)
        {
            string[] sContain = null;

            if (sFilePath == null)
            {
                return null;
            }

            if (File.Exists(sFilePath))
            {
                try
                {
                    sContain = File.ReadAllLines(sFilePath);
                }
                catch
                {
                    return null;
                }
            }

            return sContain;
        }

        public bool AddNewItem(string sData)
        {
            bool bRet = false;

            if ((m_sFilePath != null) && (sData != null))
            {
                try
                {
                    File.AppendAllText(m_sFilePath, sData);
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString());
                    return bRet;
                }
            }

            return true;
        }

        public bool AddNewItem(string[] sDataList)
        {
            string sFinalLine = null;
            foreach (string sField in sDataList)
            {
                if ((sField.Contains('\n') == true) ||
                    (sField.Contains(',') == true) ||
                    (sField.Contains('\"') == true))
                {
                    sFinalLine += "\"" + sField.Replace("\"", "\"\"") + "\"" + ',';
                }
                else
                {
                    sFinalLine += sField + ',';
                }
            }
            int iLengthWithoutLastComma = sFinalLine.Count() - 1;
            sFinalLine = sFinalLine.Substring(0, iLengthWithoutLastComma);
            sFinalLine += "\r\n";
            return AddNewItem(sFinalLine);
        }

        public virtual bool CloseFile()
        {
            return true;
        }

        public bool IsFileExistent(string sFilePath)
        {
            return File.Exists(sFilePath);
        }

        public bool MoveAndRenameFile(string sCurrentFileName, string sProductName)
        {
            bool bRet = false;
            if (File.Exists(sCurrentFileName))
            {
                string sDiretory = Path.GetDirectoryName(Path.GetDirectoryName(sCurrentFileName)) + Path.DirectorySeparatorChar + sProductName;
                string sFileName = sProductName + "_" + Path.GetFileName(sCurrentFileName);
                string sNewName = sDiretory + Path.DirectorySeparatorChar + sFileName;

                if (File.Exists(sNewName))
                {
                    File.Delete(sNewName);
                }
                if (!Directory.Exists(sDiretory))
                {
                    Directory.CreateDirectory(sDiretory);
                }
                Program.SetUnixFileFullPermissions(sDiretory);
                Program.SetUnixFileFullPermissions(sCurrentFileName);
                Program.SetUnixFileFullPermissions(sNewName);
                File.Copy(sCurrentFileName, sNewName);
                bRet = true;
            }
            return bRet;
        }
    }
}
