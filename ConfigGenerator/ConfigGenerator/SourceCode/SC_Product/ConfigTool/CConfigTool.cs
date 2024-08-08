using System.Reflection;

namespace ConfigGenerator
{
    class CConfigTool : IConfigTool
    {
        public uint     SIZE_BIT_SUPPORT    = 4;
        public string   sVersionRelease     = "";
        public string   sDateRelease        = DateTime.Now.ToString("dd MMM yyyy");
        public string   sMergedFolderName   = "MergedInputFiles" + System.IO.Path.DirectorySeparatorChar + "Temp";

        private volatile static CConfigTool m_Instance = null;
        private CConfigTool() { sVersionRelease = Assembly.GetEntryAssembly().GetName().Version.ToString(); }

        public static CConfigTool GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new CConfigTool();
            }

            return m_Instance;
        }
    }
}
