namespace ConfigGenerator
{
    public abstract class AXmlTable
    {
        const string c_sEnumTablePrefix = "CodeTable-";
        const string c_sHexTablePrefix  = "exeNumericRange-";
        const string c_sIntTablePrefix  = "IntegerRangeTable-";

        public enum XML_TABLE_TYPE
        {
            XML_TABLE_UNKNOW = 0,
            XML_TABLE_ENUM,
            XML_TABLE_HEX,
            XML_TABLE_INT,
        }

        private XML_TABLE_TYPE m_XmlTableType = XML_TABLE_TYPE.XML_TABLE_UNKNOW;
        private string m_sName = null;

        public AXmlTable(XML_TABLE_TYPE XmlTableType)
        {
            if (XmlTableType != XML_TABLE_TYPE.XML_TABLE_UNKNOW)
            {
                m_XmlTableType = XmlTableType;
            }
            else
            {
                // TODO
            }
        }

        public XML_TABLE_TYPE GetXmlTableType()
        {
            return m_XmlTableType;
        }

        public void SetName(string sIDX)
        {
            switch (m_XmlTableType)
            {
                case XML_TABLE_TYPE.XML_TABLE_ENUM:
                    m_sName = c_sEnumTablePrefix + sIDX;
                    break;
                case XML_TABLE_TYPE.XML_TABLE_HEX:
                    m_sName = c_sHexTablePrefix + sIDX;
                    break;
                case XML_TABLE_TYPE.XML_TABLE_INT:
                    m_sName = c_sIntTablePrefix + sIDX;
                    break;
                default:
                    break;
            }
            
        }

        public string GetName()
        {
            return m_sName;
        }

        public abstract bool CopyContent(ATableRef TableRef);
        public abstract bool CompareContent(ATableRef TableRef);
    }
}
