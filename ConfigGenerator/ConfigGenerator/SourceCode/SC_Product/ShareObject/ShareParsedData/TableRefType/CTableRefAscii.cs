namespace ConfigGenerator
{
    public class CTableRefAscii : ATableRef
    {
        private string m_PaddingValue = null;
        private string m_MinValue = null;
        private string m_MaxValue = null;
        private string m_StepValue = null;
        private bool   m_TableRefValueIsHidden = false;

        public CTableRefAscii()
            : base(TABLE_REF_TYPE.TABLE_REF_ASCII)
        {

        }

        public override ATableRef GetDecimalInstance()
        {
            return this;
        }

        public void SetTableRefPaddingValue(string sValue)
        {
            m_PaddingValue = sValue;
        }

        public void SetTableRefMinValue(string sValue)
        {
            m_MinValue = sValue;
        }

        public void SetTableRefMaxValue(string sValue)
        {
            m_MaxValue = sValue;
        }

        public void SetTableRefStepValue(string sValue)
        {
            m_StepValue = sValue;
        }
        
        public string GetTableRefPaddingValue()
        {
            return m_PaddingValue;
        }

        public string GetTableRefMinValue()
        {
            return m_MinValue;
        }

        public string GetTableRefMaxValue()
        {
            return m_MaxValue;
        }

        public string GetTableRefStepValue()
        {
            return m_StepValue;
        }

        public void SetTableRefValueIsHidden(bool bValue)
        {
            m_TableRefValueIsHidden = bValue;
        }

        public bool GetTableRefValueIsHidden()
        {
            return m_TableRefValueIsHidden;
        }
    }
}
