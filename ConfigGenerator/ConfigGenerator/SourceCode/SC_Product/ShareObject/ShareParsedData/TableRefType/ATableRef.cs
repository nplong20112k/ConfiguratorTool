using System.Collections.Generic;

namespace ConfigGenerator
{
    public abstract class ATableRef
    {
        public const string PADDING_DEFAULT              = "00";
        public const string MIN_CHAR_DEFAULT             = "00";
        public const string MAX_CHAR_DEFAULT             = "FF";
        public const string MIN_STRING_DEFAULT           = "00";
        public const string MAX_STRING_DEFAULT           = "FF"; //Used to FE
        public const string MIN_CHAR_READABLE_DEFAULT    = "20";
        public const string MAX_CHAR_READABLE_DEFAULT    = "7E";
        public const string MIN_STRING_READABLE_DEFAULT  = "20";
        public const string MAX_STRING_READABLE_DEFAULT  = "7E";
        

        public const string VALUE_TYPE_INT               = "int";
        public const string VALUE_TYPE_HEXINT            = "hexInt";
        public const string VALUE_TYPE_CHAR              = "char";
        public const string VALUE_TYPE_STRINGFILL        = "stringFilled";
        public const string VALUE_TYPE_READABLE_ASCII    = "readableAscii";
        public const string VALUE_TYPE_ENUM              = "enum";

        public enum TABLE_REF_TYPE
        {
            TABLE_REF_UNKNOW = 0,
            TABLE_REF_SELECTION,
            TABLE_REF_RANGE,
            TABLE_REF_ASCII,
            TABLE_REF_CUSTOM,
            TABLE_REF_READABLE_ASCII,
        }
        
        private TABLE_REF_TYPE      m_TableRefType              = TABLE_REF_TYPE.TABLE_REF_UNKNOW;
        private string              m_TableRefName              = null;
        private bool                m_TableRefValueIsDecimal    = false;
        private List<uint>          m_TableRefUnavailableInterfaceClassList  = null;

        public ATableRef(TABLE_REF_TYPE TableRefType)
        {
            if (TableRefType != TABLE_REF_TYPE.TABLE_REF_UNKNOW)
            {
                m_TableRefType = TableRefType;
            }
            else
            {
                // TODO
            }
        }

        public void SetTableRefName(string sValue)
        {
            m_TableRefName = sValue;
        }

        public string GetTableRefName()
        {
            return m_TableRefName;
        }

        public string GetTableRefValueType()
        {
            string sTemp; 
            switch (m_TableRefType)
            {
                default:
                case TABLE_REF_TYPE.TABLE_REF_SELECTION:
                    sTemp = VALUE_TYPE_ENUM;
                    break;

                case TABLE_REF_TYPE.TABLE_REF_RANGE:
                    if (m_TableRefValueIsDecimal == true)
                    {
                        sTemp = VALUE_TYPE_INT;
                    }
                    else
                    {
                        sTemp = VALUE_TYPE_HEXINT;
                    }
                    break;

                case TABLE_REF_TYPE.TABLE_REF_ASCII:
                    sTemp = VALUE_TYPE_STRINGFILL;
                    break;
                case TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                    sTemp = VALUE_TYPE_READABLE_ASCII; 
                    break;
            }
            return sTemp;
        }

        public void SetTableRefValueIsDecimal()
        {
            m_TableRefValueIsDecimal = true;
        }

        public bool GetTableRefValueIsDecimal()
        {
            return m_TableRefValueIsDecimal;
        }

        public TABLE_REF_TYPE GetTableRefType()
        {
            return m_TableRefType;
        }

        public void SetUnavailableInterfaceClassList(List<uint> InterfaceClassList)
        {
            m_TableRefUnavailableInterfaceClassList = InterfaceClassList;
        }

        public List<uint> GetUnavailableInterfaceClassList()
        {
            return m_TableRefUnavailableInterfaceClassList;
        }

        public abstract ATableRef GetDecimalInstance();
    }
}
