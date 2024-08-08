using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CDeltaConstTable
    {           
        private string m_sCIItemName                        = null;
        private string m_sSizeValue                         = null;
        private List<DELTA_TABLE_ITEM> m_lsDeltaTableGroup  = null;

        public CDeltaConstTable(string sCIItemName, string sSizeValue, List<DELTA_TABLE_ITEM> lsDeltaTableGroup)
        {
            m_sCIItemName           = sCIItemName;
            m_sSizeValue            = sSizeValue;
            m_lsDeltaTableGroup     = lsDeltaTableGroup;
        }
        
        public string GetCIItemName()
        {
            return m_sCIItemName;
        }        
        
        public string GetSizeValue()
        {
            return m_sSizeValue;
        }     

        public List<DELTA_TABLE_ITEM> GetDeltaTableGroup()
        {
            return m_lsDeltaTableGroup;
        }
    }
}