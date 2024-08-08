using System;
using System.IO;

namespace ConfigGenerator
{
    class MainConsole
    {
        private bool m_bFlagRunning = true;
        private string m_sInputFilePath = null;
        private string m_sLauncherName = null;

        private string m_sGenType = "GENERATE_NONE";

        public static MainConsole _MainConsole = null;

        private void ConsoleLoading()
        {
            Console.WriteLine("\n");
            Console.WriteLine(" +-+ +-+ +-+ +-+ +-+ +-+   +-+ +-+ +-+ +-+ +-+ +-+ +-+ +-+ +-+   +-+ +-+ +-+ +-+");
            Console.WriteLine("                 Enviroment " + m_sLauncherName + "\n");
            Console.WriteLine("                 Configuration Items Resource Optimizer - CIRO v" + CConfigTool.GetInstance().sVersionRelease);
            Console.WriteLine(" +-+ +-+ +-+ +-+ +-+ +-+   +-+ +-+ +-+ +-+ +-+ +-+ +-+ +-+ +-+   +-+ +-+ +-+ +-+\n");
        }

        public MainConsole(string sLauncherName = null)
        {
            m_sLauncherName = sLauncherName;
            ConsoleLoading();
            _MainConsole = this;
        }

        public void UpdateStatusSystemInfo(string sData)
        {
            if (sData != null)
            {
                Console.Write (sData);
            }
        }

        public void ConsoleRunning(string[] sInputInfoList, bool bCloseAnyway)
        {
            Factory ConfigGeneratorSytem = new Factory();

            if (ProcessInputFileInfo(sInputInfoList) == true)
            {
                GenerateRequest();
            }
            else
            {
                Console.WriteLine("\nInput file invalid !");
                if (!bCloseAnyway)
                {
                    Console.ReadLine();
                }
                m_bFlagRunning = false;
                Environment.Exit(-1);
            }
            while (m_bFlagRunning);
        }

        public void Exit()
        {
            // Thread.Sleep(1000);
            m_bFlagRunning = false;
        }

        private bool ProcessInputFileInfo(string[] sInputInfoList)
        {
            bool bRet = false;

            if ((sInputInfoList != null) && (sInputInfoList.Length >= 2))
            {
                int uiFileIndex = -1;
                for (int i = 0; i < sInputInfoList.Length; i++ )
                {
                    if (sInputInfoList[i] == "+a")
                    {
                        m_sGenType = "GENERATE_ALADDIN_PACKAGE";
                        uiFileIndex = i + 1;
                        
                    }
                    else if (sInputInfoList[i] == "+s")
                    {
                        m_sGenType = "GENERATE_SOURCE_CODE";
                        uiFileIndex = i + 1;
                        
                    }
                    else if (sInputInfoList[i] == "+b")
                    {
                        m_sGenType = "GENERATE_BIN_FILES";
                        uiFileIndex = i + 1;
                        
                    }
                    else if (sInputInfoList[i] == "+ab")
                    {
                        m_sGenType = "GENERATE_ALADDIN_PACKAGE_AND_BIN_FILES";
                        uiFileIndex = i + 1;
                        
                    }
                    else if (sInputInfoList[i] == "+as")
                    {
                        m_sGenType = "GENERATE_ALADDIN_PACKAGE_AND_SOURCE_CODE";
                        uiFileIndex = i + 1;
                        
                    }
                }

                if(m_sGenType == "GENERATE_NONE")
                {
                    Console.WriteLine("Missing option generation");
                    Program.SystemHelp();
                    return false;
                }

                if (uiFileIndex != -1)
                {
                    m_sInputFilePath = sInputInfoList[uiFileIndex];

                    if (File.Exists(m_sInputFilePath) == true)
                    {
                        m_sInputFilePath = Path.GetFullPath(m_sInputFilePath);
                        bRet = true;
                    }
                }
            }

            // Enable below code in debug
            // m_sInputFilePath = "D:\\Works\\GIT\\Tools\\ConfigGenerator_SupportedModel_Working\\Pegasus\\Product_models\\PD96\\PD96_parameter_file.xml";
            // bRet = true;

            return bRet;
        }

        private void GenerateRequest()
        {
            CStringObject tempObject = new CStringObject();
            CStringObject genTypeObject = new CStringObject();

            // Do update generate report type
            genTypeObject.AddStringData(m_sGenType);
            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_UPDATE_GENERATE_TYPE, genTypeObject);

            // Do request start generate report
            tempObject.AddStringData(m_sInputFilePath);
            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_PARSE_PARAMETER_FILE, tempObject);
        }
    }
}
