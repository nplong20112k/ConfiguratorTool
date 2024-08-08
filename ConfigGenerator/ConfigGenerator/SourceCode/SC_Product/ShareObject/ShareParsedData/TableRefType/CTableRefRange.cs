using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CTableRefRange : ATableRef
    {
        private string m_sMinValue = null;
        private string m_sMaxValue = null;
        private string m_sStepValue = null;
        private List<string> m_ExtraValueList = null;
        private bool m_bAcceptAllValue;

        public CTableRefRange()
            : base(TABLE_REF_TYPE.TABLE_REF_RANGE)
        {
            m_bAcceptAllValue = false;
        }

        public override ATableRef GetDecimalInstance()
        {
            CTableRefRange NewInstance = (CTableRefRange) this.MemberwiseClone();

            NewInstance.m_sMinValue = CHexIntConverter.StringConvertHexToInt(m_sMinValue);
            NewInstance.m_sMaxValue = CHexIntConverter.StringConvertHexToInt(m_sMaxValue);
            NewInstance.m_sStepValue = CHexIntConverter.StringConvertHexToInt(m_sStepValue);

            return NewInstance;
        }

        public void SetTableRefMinValue(string sValue)
        {
            m_sMinValue = sValue;
        }
        public void SetTableRefMaxValue(string sValue)
        {
            m_sMaxValue = sValue;
        }
        public void SetTableRefStepValue(string sValue)
        {
            m_sStepValue = sValue;
        }
        
        public string GetTableRefMinValue()
        {
            return m_sMinValue;
        }
        public string GetTableRefMaxValue()
        {
            return m_sMaxValue;
        }
        public string GetTableRefStepValue()
        {
            return m_sStepValue;
        }

        public void SetExtraValueList(List<string> valueList)
        {
            m_ExtraValueList = valueList;
        }

        public List<string> GetExtraValueList()
        {
            return m_ExtraValueList;
        }

        public void SetAcceptAllValueProperty(bool bValue)
        {
            m_bAcceptAllValue = bValue;
        }

        public bool GetAcceptAllValueProperty()
        {
            return m_bAcceptAllValue;
        }
    }
}
