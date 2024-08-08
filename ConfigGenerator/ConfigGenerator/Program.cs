
using System.Runtime.InteropServices;

namespace ConfigGenerator
{
    internal class Program
    {

        static private bool bCloseAnyway = false;
        static private bool bDebugInfo = false;
        static void Main(string[] args)
        {            
            bCloseAnyway = false;
            bDebugInfo = false;

            if (args.Length == 0)
            {
                // Disable below code in debug
                SystemHelp();
                return;
            }
            else
            {
                foreach (string argElement in args)
                {
                    if (argElement == "+t")
                    {
                        bCloseAnyway = true;
                    }
                    else if (argElement == "+d")
                    {
                        bDebugInfo = true;
                    }
                    else if (argElement == "+h")
                    {
                        SystemHelp();
                        return;
                    }
                }
            }
            
            MainConsole mainConsole = new MainConsole(Environment.OSVersion.ToString());
            if (mainConsole != null)
            {
                mainConsole.ConsoleRunning(args, bCloseAnyway);
            }            
        }
        
        public static void SystemHandleStatusInfo(string sData)
        {
            if (sData != null)
            {
                if (MainConsole._MainConsole != null)
                {
                    MainConsole._MainConsole.UpdateStatusSystemInfo(sData);
                }
            }
        }

        public static void SystemDisplayDebugInfo(string sData)
        {
            if (true == bDebugInfo)
            {
                SystemHandleStatusInfo(sData);
            }
        }

        public static void SystemResetStateInput()
        {
            
        }

        public static void SystemEndingProgram(bool bExitOrReset)
        {
            // string sTempFolderPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + CConfigTool.GetInstance().sMergedFolderName;
            // string sTempFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + CConfigTool.GetInstance().sMergedFolderName;
            string sTempFolderPath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + CConfigTool.GetInstance().sMergedFolderName;

            if (Directory.Exists(sTempFolderPath) == true)
            {
                Program.SetUnixFileFullPermissions(sTempFolderPath);
                Directory.Delete(sTempFolderPath, true);
            }

            if (MainConsole._MainConsole != null)
            {
                if ((bExitOrReset == false) || (bCloseAnyway == true))
                {
                    MainConsole._MainConsole.Exit();                    
                    Environment.Exit(0);
                }
            }
        }

        public static void SystemHelp()
        {
            Console.WriteLine("\n");
            Console.WriteLine(" usage ConfigGenerator_<version>.exe [+h] [+t] [+a] [+s] [+b] [+ab] {+c} {ParameterFilePath.csv}\n");
            Console.WriteLine(" +h      show this help message and exit\n");
            Console.WriteLine(" +t      close console regardless result\n");
            Console.WriteLine(" +a      only generate Aladdin package\n");
            Console.WriteLine(" +s      only generate source code files\n");
            Console.WriteLine(" +b      only generate bin files\n");
            Console.WriteLine(" +ab     generate aladdin package and bin files\n");
            Console.WriteLine(" +as     generate aladdin package and source code files\n");
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static void SetUnixFileFullPermissions(string sFullFilePath)
        {
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!isWindows)
            {
                //string sFullFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "test_permission_file" + Path.DirectorySeparatorChar;
                Mono.Unix.UnixFileInfo unixFileInfo = new Mono.Unix.UnixFileInfo(sFullFilePath);

                if (unixFileInfo.Exists)
                {
                    unixFileInfo.FileAccessPermissions = Mono.Unix.FileAccessPermissions.UserReadWriteExecute |
                                                         Mono.Unix.FileAccessPermissions.GroupReadWriteExecute |
                                                         Mono.Unix.FileAccessPermissions.OtherReadWriteExecute;
                    
                }
            }
        }

    }
}