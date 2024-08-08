using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CParsedParameterObject
    {
        private string m_sCode;
        private string m_sName;
        private string m_sUserName;
        private List<string> m_lsCategory;
        private string m_sSize;
        private string m_sDefaultValue;
        private string m_sAltDefaultValue;
        private string m_sProtection;
        private List<string> m_lsCIClassDefaultValue;

        public CParsedParameterObject (
                string          sCode,
                string          sName,
                string          sUserName,
                List<string>    lsCategory,
                string          sSize,
                string          sDefaultValue,
                string          sAltDefaultValue,
                string          sProtection)    
        {
            m_sName             = sName;
            m_sUserName         = sUserName;
            m_sDefaultValue     = sDefaultValue;
            m_sAltDefaultValue  = sAltDefaultValue;
            m_sProtection       = sProtection;
            m_sSize             = sSize;
            m_sCode             = sCode;
            m_lsCategory        = lsCategory;
        }

        public CParsedParameterObject(
                string          sCode,
                string          sName,
                string          sUserName,
                List<string>    lsCategory,
                string          sSize,
                string          sDefaultValue,
                string          sAltDefaultValue,
                string          sProtection,
                List<string>    lsCIclassDefaultValue)
        {
            m_sName             = sName;
            m_sUserName         = sUserName;
            m_sDefaultValue     = sDefaultValue;
            m_sAltDefaultValue  = sAltDefaultValue;
            m_sProtection       = sProtection;
            m_sSize             = sSize;
            m_sCode             = sCode;
            m_lsCategory        = lsCategory;

            /*
            ----Interative between "index" and "class"----
                + index 0 <=> class 2
                + index 1 <=> class 3
                + index 2 <=> class 4
                + index 3 <=> class 5
                + index 4 <=> class 6
                + index 5 <=> class 8
                + index 6 <=> class 9
                + index 7 <=> class 10
                + index 8 <=> class 11
                + index 9 <=> class 12                 
            */
            m_lsCIClassDefaultValue = lsCIclassDefaultValue;
        }
      
        public string GetTagCode ()
        {
            return m_sCode;
        }

        public string GetTagName()
        {
            return m_sName;
        }

        public string GetTagUserName()
        {
            return m_sUserName;
        }

        public List<string> GetTagCategory()
        {
            return m_lsCategory;
        }

        public string GetTagValueSize()
        {
            return m_sSize;
        }

        public string GetDefaultValue()
        {
            return m_sDefaultValue;
        }

        public string GetAltDefaultValue()
        {
            return m_sAltDefaultValue;
        }

        public string GetProtection()
        {
            return m_sProtection;
        }

        public string GetDefaultValueClass(int index)
        {
            return m_lsCIClassDefaultValue[index]; 
        }

        public List<string> GetDefaultValueClass()
        {
            return m_lsCIClassDefaultValue;
        }
    }
}