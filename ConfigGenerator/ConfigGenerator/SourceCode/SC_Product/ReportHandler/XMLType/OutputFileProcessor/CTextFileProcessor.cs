using System.IO;
using System.Text.RegularExpressions;

namespace ConfigGenerator
{
    class CTextFileProcess
    {
        private string  m_CurrentPathFile   = null;
        private string  m_FileContent       = null;
        private bool    m_bIsFileLoaded     = false;
        
        public CTextFileProcess()
        {
        }

        public bool Open(string sFilePath)
        {
            bool bRet = false;
            ReInitProperties();
            
            if (File.Exists(sFilePath))
            {
                m_FileContent = File.ReadAllText(sFilePath);

                m_CurrentPathFile = sFilePath;
                m_bIsFileLoaded = true;
                bRet = true;
            }

            return bRet;
        }

        public bool Close()
        {
            bool bRet = false;

            if (m_bIsFileLoaded == true)
            {
                m_CurrentPathFile = null;
                m_bIsFileLoaded = false;
                bRet = true;
            }

            return bRet;
        }

        public bool Save()
        {
            bool bRet = false;

            if (m_bIsFileLoaded == true)
            {
                if ((m_CurrentPathFile != null) && (m_FileContent != null))
                {
                    try
                    {
                        File.WriteAllText(m_CurrentPathFile, m_FileContent);
                        bRet = true;
                    }
                    catch { }
                }
            }

            return bRet;
        }

        public int FindAndReplace(string sOldString, string sNewString)
        {
            int iRet = 0;

            if (m_bIsFileLoaded == true)
            {
                if ((m_FileContent != null) && (sOldString != null) && (sNewString != null))
                {
                    iRet = Regex.Matches(m_FileContent, sOldString).Count;
                    if (iRet > 0)
                    {
                        m_FileContent = m_FileContent.Replace(sOldString, sNewString);
                        if (!Save())
                        {
                            iRet = 0;
                        }
                    }
                }
            }

            return iRet;
        }

        private void ReInitProperties()
        {
            m_CurrentPathFile   = null;
            m_FileContent       = null;
            m_bIsFileLoaded     = false;
        }
    }
}
