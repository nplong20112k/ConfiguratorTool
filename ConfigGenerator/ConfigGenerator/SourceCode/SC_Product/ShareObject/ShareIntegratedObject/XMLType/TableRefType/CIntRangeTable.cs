namespace ConfigGenerator
{
    class CIntRangeTable : AXmlTable
    {
        string m_sMinValue = null;
        string m_sMaxValue = null;

        public CIntRangeTable()
            : base(AXmlTable.XML_TABLE_TYPE.XML_TABLE_INT)
        {
        }

        public string GetTableRefMinValue()
        {
            return m_sMinValue;
        }

        public string GetTableRefMaxValue()
        {
            return m_sMaxValue;
        }

        public override bool CopyContent(ATableRef TableRef)
        {
            bool bRet = false;

            if (
                   (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE) &&
                   ((TableRef as CTableRefRange).GetTableRefValueIsDecimal() == true)
               )
            {
                m_sMinValue = (TableRef as CTableRefRange).GetTableRefMinValue();
                m_sMaxValue = (TableRef as CTableRefRange).GetTableRefMaxValue();
                bRet = true;
            }

            if ((m_sMinValue != null) && (m_sMaxValue != null))
            {
                bRet = true;
            }

            return bRet;
        }

        public override bool CompareContent(ATableRef TableRef)
        {
            bool bRet = false;
            string sTempMin = "";
            string sTempMax = "";

            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE)
            {
                sTempMin = (TableRef as CTableRefRange).GetTableRefMinValue();
                sTempMax = (TableRef as CTableRefRange).GetTableRefMaxValue();
            }

            if (
                    (sTempMin == m_sMinValue) &&
                    (sTempMax == m_sMaxValue) 
                )
            {
                bRet = true;
            }

            return bRet;
        }
    }

}
