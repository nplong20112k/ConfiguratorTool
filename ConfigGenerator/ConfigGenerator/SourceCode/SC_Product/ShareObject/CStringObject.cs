namespace ConfigGenerator
{
    
    public class CStringObject : AShareObject
    {
        private List<string> m_ListStringObject = new List<string>();

        public CStringObject()
            : base(SHARE_OBJECT_ID.SOB_ID_STRING_OBJECT)
        {
            
        }

        public bool AddStringData(string sData)
        {
            bool bRet = false;
            if (m_ListStringObject != null)
            {
                m_ListStringObject.Add(sData);
            }
            return bRet;
        }

        public string GetStringData(int iIndex = 0)
        {
            string sRet = null;
            if (iIndex < m_ListStringObject.Count)
            {
                sRet = m_ListStringObject[iIndex];
            }
            return sRet;
        }

        public int GetNumberOfStringMember()
        {
            return m_ListStringObject.Count;
        }
    }
}
