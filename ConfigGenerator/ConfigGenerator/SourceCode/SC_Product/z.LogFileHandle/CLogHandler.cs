using System;
using System.IO;

namespace ConfigGenerator
{
    public class CLogger
    {
        static readonly string[] LOG_FILENAME_FILTER =
        {
            "CEventForwarder",
            "AEventReceiver",
            //"CReportGeneratorDirector",
            //"CTableRefIntegrator",
        };

        static readonly string[] LOG_METHOD_FILTER =
        {
            //"LogIntegrateProcess",
        };


        private const string    LOG_HEADER = "Log data of last running at local time ";
        private const string    LOG_FILE_NAME = "LogFile.txt";

        private string          m_sFilePath = null;

        private bool            m_bFileExist = false;
        private bool            m_bConSoleEnable = false;

        readonly static CLogger m_Instance = new CLogger();

        public CLogger()
        {
            // m_sFilePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + LOG_FILE_NAME;
            m_sFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + LOG_FILE_NAME;
            
            LogNewSession(m_sFilePath);
            Program.SetUnixFileFullPermissions(m_sFilePath);
        }

        public static CLogger GetInstance()
        {
            return m_Instance;
        }

        public void LogEnableConsole(bool SetStatus)
        {
            m_bConSoleEnable = SetStatus;
        }

        public void LogNewSession(string sFilePath)
        {
            string sHeader = LOG_HEADER + DateTime.Now.ToString() + " \n\n";

            if (m_bConSoleEnable)
            {
                Console.Clear();
                Console.WriteLine(sHeader);
            }
            else
            {
                if (sFilePath == null)
                {
                    return;
                }

                string FolderName = Path.GetDirectoryName(sFilePath);
                if (!Directory.Exists(FolderName))
                {
                    try
                    {
                        Directory.CreateDirectory(FolderName);
                        Program.SetUnixFileFullPermissions(FolderName);
                    }
                    catch
                    {
                        return;
                    }
                }

                if (File.Exists(sFilePath))
                {
                    try
                    {
                        File.Delete(sFilePath);
                    }
                    catch
                    {
                        return;
                    }
                }

                try
                {
                    File.AppendAllText(sFilePath, sHeader);
                }
                catch
                {
                    return;
                }
            }
            m_bFileExist = true;
        }

        public void Log(string sContent,
                        [System.Runtime.CompilerServices.CallerFilePath] string sFileName = "",
                        [System.Runtime.CompilerServices.CallerMemberName] string sMethodName = "")
        {

            if (m_bConSoleEnable)
            {
                Console.WriteLine(sContent);
            }
            else
            {
                if (m_bFileExist)
                {
                    try
                    {
                        sFileName = Path.GetFileNameWithoutExtension(sFileName);

                        if (LOG_FILENAME_FILTER.Length > 0)
                        {
                            foreach (string sFile in LOG_FILENAME_FILTER)
                            {
                                if (sFileName == sFile)
                                {
                                    return;
                                }
                            }
                        }

                        if (LOG_METHOD_FILTER.Length > 0)
                        {
                            foreach (string sFile in LOG_METHOD_FILTER)
                            {
                                if (sMethodName == sFile)
                                {
                                    return;
                                }
                            }
                        }

                        string sTemp = string.Format("{0,-88}|   {1,-48}|   {2}\n", sContent, sFileName, sMethodName);
                        File.AppendAllText(m_sFilePath, sTemp);
                    }
                    catch { }
                }
            }
        }
    }
}
