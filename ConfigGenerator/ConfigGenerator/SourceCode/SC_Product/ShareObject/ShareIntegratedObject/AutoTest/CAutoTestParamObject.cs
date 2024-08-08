using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CAutoTestParamObject
    {
        private string m_sTypeValueAutoTest = null;
        private string m_StepRangeValue = null;
        private List<string> m_lsCategory = null;
        private string m_ValueSizeByte = null;
        private List<CIClassDefault> m_lsCIClassDefault = null;
        private string m_sTagCode = null;
        private string m_sTagName = null;
        private string m_sDefaultValue = null;

        public CAutoTestParamObject()
        {
            //To Do
        }

        public void SetTypeValueAutoTest(string sValueTypeAutoTest) { m_sTypeValueAutoTest = sValueTypeAutoTest; }
        public void SetCategoryList(List<string> lsCategory) { m_lsCategory = lsCategory; }
        public void SetStepRangeValue(string sValue) { m_StepRangeValue = sValue; }
        public void SetValueSizeByte(string sSize) { m_ValueSizeByte = sSize; }
        public void SetCIClassDefaultList(List<CIClassDefault> lsCIClassDefault) { m_lsCIClassDefault = lsCIClassDefault; }
        public void SetTagCode(string sTagCode) { m_sTagCode = sTagCode; }
        public void SetTagName(string sTagName) { m_sTagName = sTagName; }
        public void SetDefaultValue(string sDefault) { m_sDefaultValue = sDefault; }

        public string GetAutoTestParamValue() { return m_sTypeValueAutoTest; }
        public List<string> GetCategoryList() { return m_lsCategory; }
        public string GetStepRangeValue() { return m_StepRangeValue; }
        public string GetValueSizeByte() { return m_ValueSizeByte; }
        public List<CIClassDefault> GetListCICLassDefault() { return m_lsCIClassDefault; }
        public string GetTagCode() { return m_sTagCode; }
        public string GetTagName() { return m_sTagName; }
        public string GetDefaultValue() { return m_sDefaultValue; }
    }
}
