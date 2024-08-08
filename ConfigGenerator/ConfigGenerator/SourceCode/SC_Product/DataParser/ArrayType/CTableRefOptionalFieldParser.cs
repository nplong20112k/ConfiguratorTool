using System.Collections.Generic;

namespace ConfigGenerator
{

    class CTableRefOptionalFieldParser
    {
        public enum PRG_PROPERTY
        {
            ENUM_UNDEF = 0,
            ENUM_TRUE,
            ENUM_FALSE,
            ENUM_SELECTION,
        }

        private List<uint> m_ClassList = null;
        private bool m_bAllValuesAccepted = false;
        private bool m_bAladdinHiddenValue = false;
        private bool m_bValueIsDecimal = false;
        private PRG_PROPERTY m_ePRGValueIncluded = PRG_PROPERTY.ENUM_UNDEF;
        private List<string> m_ExtraValueList = null;
        private bool m_bAllValuesInRange = false;
        private int iNumOfValueExtraOption = 0;

        public void AddClassInterface(uint uiClassInterface)
        {
            if (m_ClassList == null)
            {
                m_ClassList = new List<uint>();
            }

            m_ClassList.Add(uiClassInterface);
        }

        public void SetAllValuesAcceptedOption(bool bValue)
        {
            m_bAllValuesAccepted = bValue;
            iNumOfValueExtraOption += 1;
        }

        public void SetAladdinHiddenValueOption(bool bValue)
        {
            m_bAladdinHiddenValue = bValue;
        }

        public void SetValueIsDecimalValue(bool bValue)
        {
            m_bValueIsDecimal = bValue;
        }

        public void SetPRGValueIncludedOption(PRG_PROPERTY eProperty)
        {
            m_ePRGValueIncluded = eProperty;
        }

        public void AddExtraValue(string uiValue)
        {
            if (m_ExtraValueList == null)
            {
                m_ExtraValueList = new List<string>();
                iNumOfValueExtraOption += 1;
            }

            m_ExtraValueList.Add(uiValue);
        }

        public void SetAllValuesInRangeOption(bool bValue)
        {
            m_bAllValuesInRange = bValue;
            iNumOfValueExtraOption += 1;
        }

        public List<uint> GetClassList()
        {
            return m_ClassList;
        }

        public bool GetAllValuesAcceptedOption()
        {
            return m_bAllValuesAccepted;
        }

        public bool GetAladdinHiddenValueOption()
        {
            return m_bAladdinHiddenValue;
        }

        public bool GetValueIsDecimalOption()
        {
            return m_bValueIsDecimal;
        }

        public PRG_PROPERTY GetPRGValueIncludedOption()
        {
            return m_ePRGValueIncluded;
        }

        public List<string> GetExtraValueList()
        {
            return m_ExtraValueList;
        }

        public bool GetAllValuesInRangeOption()
        {
            return m_bAllValuesInRange;
        }

        public bool IsValueExtraOptionValid()
        {
            if (iNumOfValueExtraOption > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
