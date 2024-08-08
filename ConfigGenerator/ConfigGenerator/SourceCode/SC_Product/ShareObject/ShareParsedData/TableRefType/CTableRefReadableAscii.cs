using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CTableRefReadableAscii : ATableRef
    {
        private List<string> m_lsValueList = null;
        private string m_sPaddingValue = null;
        private string m_sMinValue = null;
        private string m_sMaxValue = null;
        private bool m_bTableRefValueIsHidden = false;

        public CTableRefReadableAscii()
            : base(TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII)
        {
        }

        public override ATableRef GetDecimalInstance()
        {
            return this;
        }

        public void AddValueExceptionReadableAscii(string sValue)
        {
            if (sValue != null)
            {
                if (m_lsValueList == null)
                {
                    m_lsValueList = new List<string>();
                }

                m_lsValueList.Add(sValue);
            }
        }

        public List<string> GetTableRefValueList()
        {
            return m_lsValueList;
        }

        public void SetTableRefPaddingValue(string sValue)
        {
            m_sPaddingValue = sValue;
        }

        public void SetTableRefMinValue(string sValue)
        {
            m_sMinValue = sValue;
        }

        public void SetTableRefMaxValue(string sValue)
        {
            m_sMaxValue = sValue;
        }

        public string GetTableRefPaddingValue()
        {
            return m_sPaddingValue;
        }

        public string GetTableRefMinValue()
        {
            return m_sMinValue;
        }

        public string GetTableRefMaxValue()
        {
            return m_sMaxValue;
        }

        public void SetTableRefValueIsHidden(bool bValue)
        {
            m_bTableRefValueIsHidden = bValue;
        }

        public bool GetTableRefValueIsHidden()
        {
            return m_bTableRefValueIsHidden;
        }
    }
}