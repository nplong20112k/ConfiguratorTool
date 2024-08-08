using System.Text.RegularExpressions;

namespace ConfigGenerator
{
    public class CNamespaceFormatter
    {
        private const string NEW_LINE       = "\n";

        private CSourceCodeTemplate m_SourceTemplate = null;

        private string m_sDeclareNamespace  = null;
        private string m_sUsingNamespace    = null;

        public CNamespaceFormatter (string sDeclareNamespace, string sUsingNamespace)
        {
            m_sDeclareNamespace = sDeclareNamespace;
            m_sUsingNamespace   = sUsingNamespace;

            m_SourceTemplate = CSourceCodeTemplate.GetInstance();

            if (m_SourceTemplate == null)
            {
                return;
            }
        }

        public string GetDeclareNamespaceHeader ()
        {
            if ((m_sDeclareNamespace != string.Empty) && (m_sDeclareNamespace != null))
            {
                return string.Format(m_SourceTemplate.m_sDecleareNamespaceHeader, m_sDeclareNamespace);
            }
            return null;
        }

        public string GetDeclareNamespaceFooter ()
        {
            if ((m_sDeclareNamespace != string.Empty) && (m_sDeclareNamespace != null))
            {
                return m_SourceTemplate.m_sDecleareNamespaceFooter;
            }
            return null;
        }

        public string GetExternCDeclareNamespaceHeader()
        {
            string sRet = null;
            if ((m_sDeclareNamespace != string.Empty) && (m_sDeclareNamespace != null))
            {
                sRet = m_SourceTemplate.m_sCPlusPlusHeader;
                sRet += string.Format(m_SourceTemplate.m_sDecleareNamespaceHeader, m_sDeclareNamespace);
                sRet += m_SourceTemplate.m_sCPlusPlusFooter;
            }
            return sRet;
        }

        public string GetExternCDeclareNamespaceFooter()
        {
            string sRet = null;

            if ((m_sDeclareNamespace != string.Empty) && (m_sDeclareNamespace != null))
            {
                sRet = m_SourceTemplate.m_sCPlusPlusHeader;
                sRet += m_SourceTemplate.m_sDecleareNamespaceFooter;
                sRet += m_SourceTemplate.m_sCPlusPlusFooter;
            }
            return sRet;
        }

        public string FormatStringWithIndent (string sInput)
        {
            if ((m_sDeclareNamespace != string.Empty) && (m_sDeclareNamespace != null))
            {
                string[]    sTempInput = Regex.Split(sInput, "(\n)");
                string      sOutput = null;

                foreach (string element in sTempInput)
                {
                    if ((element != string.Empty) && (element != null) && (element != NEW_LINE))
                    {
                        sOutput += m_SourceTemplate.GetIndent(1);
                    }
                    sOutput += element;
                }
                return sOutput;
            }
            return sInput;
        }

        public string GetUsingNamespace ()
        {
            if ((m_sUsingNamespace != string.Empty) && (m_sUsingNamespace != null))
            {
                return string.Format(m_SourceTemplate.m_sUsingNamespace, m_sUsingNamespace);
            }
            return null;
        }

        public string GetExternCUsingNamespace()
        {
            string sRet = null;

            if ((m_sUsingNamespace != string.Empty) && (m_sUsingNamespace != null))
            {
                sRet = m_SourceTemplate.m_sCPlusPlusHeader;
                sRet += string.Format(m_SourceTemplate.m_sUsingNamespace, m_sUsingNamespace);
                sRet += m_SourceTemplate.m_sCPlusPlusFooter;
            }
            return sRet;
        }
    }
}