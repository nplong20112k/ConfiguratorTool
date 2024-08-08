using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CTableRefSelection : ATableRef
    {
        public struct VALUE_TYPE
        {
            public string sDescript;
            public string sValue;
        }

        private List<VALUE_TYPE> m_ValueList = null;
        private string m_sMinValue;
        private string m_sMaxValue;
        private bool m_bAcceptAllValue;
        private List<string> m_ExtraValueList = null;
        private bool m_bAllValuesInRange;

        public CTableRefSelection()
            : base(TABLE_REF_TYPE.TABLE_REF_SELECTION)
        {
            m_bAcceptAllValue = false;
            m_bAllValuesInRange = false;
        }
        
        public override ATableRef GetDecimalInstance()
        {
            CTableRefSelection NewInstance = (CTableRefSelection)this.MemberwiseClone();

            NewInstance.m_sMinValue = CHexIntConverter.StringConvertHexToInt(m_sMinValue);
            NewInstance.m_sMaxValue = CHexIntConverter.StringConvertHexToInt(m_sMaxValue);

            if ((m_ValueList != null) && (m_ValueList.Count > 0))
            {
                NewInstance.m_ValueList = new List<VALUE_TYPE>();

                VALUE_TYPE TempValue;
                foreach (VALUE_TYPE Element in m_ValueList)
                {
                    TempValue.sDescript = Element.sDescript;
                    TempValue.sValue = CHexIntConverter.StringConvertHexToInt(Element.sValue);
                    TempValue.sValue = TempValue.sValue.PadLeft(Element.sValue.Length, '0');
                    NewInstance.m_ValueList.Add(TempValue);
                }
            }

            return NewInstance;
        }

        public void AddTableRefValue(string sDecript, string sValue)
        {
            if ((sDecript != null) && (sValue != null))
            {
                if (m_ValueList == null)
                {
                    m_ValueList = new List<VALUE_TYPE>();
                }

                VALUE_TYPE TempValue;
                TempValue.sDescript = sDecript;
                TempValue.sValue = sValue;
                m_ValueList.Add(TempValue);
            }
        }

        public List<VALUE_TYPE> GetTableRefValueList()
        {
            return m_ValueList;
        }

        public void SetTableRefMinValue(string sValue)
        {
            m_sMinValue = sValue;
        }

        public void SetTableRefMaxValue(string sValue)
        {
            m_sMaxValue = sValue;
        }

        public string GetTableRefMinValue()
        {
            return m_sMinValue;
        }

        public string GetTableRefMaxValue()
        {
            return m_sMaxValue;
        }

        public void SetAcceptAllValueProperty(bool bValue)
        {
            m_bAcceptAllValue = bValue;
        }

        public bool GetAcceptAllValueProperty()
        {
            return m_bAcceptAllValue;
        }

        public void SetExtraValueList(List<string> valueList)
        {
            m_ExtraValueList = valueList;
        }

        public List<string> GetExtraValueList()
        {
            return m_ExtraValueList;
        }

        public void SetAllValuesInRangeProperty(bool bValue)
        {
            m_bAllValuesInRange = bValue;
        }

        public bool GetAllValuesInRangeProperty()
        {
            return m_bAllValuesInRange;
        }
    }
}
