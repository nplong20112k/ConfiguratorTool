using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CIntegrateParamContainObject
    {
        // Required members
        private string m_sName          = null;
        private string m_sDefaultValue  = null;
        private string m_sTypeValue     = null;
        private string m_sProtection    = null;
        private string m_sCode          = null;
        private string m_sTableRefName  = null;
        private string m_sSizeLen       = null;
        private string m_sUserName      = null;
        
        // Optional member
        private string m_sMinValue      = null;
        private string m_sMaxValue      = null;
        private string m_sMinLen        = null;
        private string m_sMaxLen        = null;
        private string m_sFillChar      = null;
        private string m_sHiddenValue   = null;

        public CIntegrateParamContainObject(
                string sName,
                string sDefaultValue,
                string sTypeValue,
                string sProtection,
                string sCode,
                string sTableRefName,
                string sSizeLen,
                string sUserName
            )
        {
            m_sName         = sName;
            m_sDefaultValue = sDefaultValue;
            m_sTypeValue    = sTypeValue;
            m_sProtection   = sProtection;
            m_sCode         = sCode;
            m_sTableRefName = sTableRefName;
            m_sSizeLen      = sSizeLen;
            m_sUserName     = sUserName;
        }
        
        public void SetParamFillChar(string sFillChar)  { m_sFillChar = sFillChar; }
        public void SetParamMinValue(string sMinValue)  { m_sMinValue = sMinValue; }
        public void SetParamMaxValue(string sMaxValue)  { m_sMaxValue = sMaxValue; }
        public void SetParamMinLen(string sMinLen)      { m_sMinLen   = sMinLen;   }
        public void SetParamMaxLen(string sMaxLen)      { m_sMaxLen   = sMaxLen;   }
        public void SetParamHiddenValue(string sValue)  { m_sHiddenValue = sValue; }
        
        public string GetParamName()         { return m_sName;         }
        public string GetParamDefaultValue() { return m_sDefaultValue; }
        public string GetParamTypeValue()    { return m_sTypeValue;    }
        public string GetParamProtection()   { return m_sProtection;   }
        public string GetParamCode()         { return m_sCode;         }
        public string GetParamTableRefName() { return m_sTableRefName; }
        public string GetParamSizeLen()      { return m_sSizeLen;      }
        public string GetParamUserName()     { return m_sUserName;     }

        public string GetParamMinValue()     { return m_sMinValue;     }
        public string GetParamMaxValue()     { return m_sMaxValue;     }
        public string GetParamMinLen()       { return m_sMinLen;       }
        public string GetParamMaxLen()       { return m_sMaxLen;       }
        public string GetParamFillChar()     { return m_sFillChar;     }
        public string GetParamHiddenValue()  { return m_sHiddenValue;  }

    }

    public class CIntegrateParamObject
    {
        public const int CODE_LENGTH = 4;

        private List<CIntegrateParamContainObject> m_ParameterList = null;

        public CIntegrateParamObject()
        {
            if (m_ParameterList == null)
            {
                m_ParameterList = new List<CIntegrateParamContainObject>();
            }
        }

        public void AddSubParameter(CIntegrateParamContainObject SubParameter)
        {
            m_ParameterList.Add(SubParameter);
        }

        public List<CIntegrateParamContainObject> GetParameterList()
        {
            return m_ParameterList;
        }
    }
}
