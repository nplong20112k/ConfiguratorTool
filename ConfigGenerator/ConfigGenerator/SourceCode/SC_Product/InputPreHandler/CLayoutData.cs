namespace ConfigGenerator
{
    public class CLayoutData
    {
        public CLayoutData(string[] sString, int iCount)
        {
            m_sFirstLine = sString;
            m_iCountLine = iCount;
        }

        public string[] m_sFirstLine { get; set; }
        public int m_iCountLine { get; set; }
    }
}
