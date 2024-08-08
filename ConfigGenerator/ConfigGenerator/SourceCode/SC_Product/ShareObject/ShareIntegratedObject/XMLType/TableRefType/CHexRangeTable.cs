namespace ConfigGenerator
{
    class CHexRangeTable : AXmlTable
    {
        string m_sMinValue = null;
        string m_sMaxValue = null;

        public CHexRangeTable()
            :base(AXmlTable.XML_TABLE_TYPE.XML_TABLE_HEX)
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
                   ((TableRef as CTableRefRange).GetTableRefValueIsDecimal() == false)
               )
            {
                m_sMinValue     = (TableRef as CTableRefRange).GetTableRefMinValue();
                m_sMaxValue     = (TableRef as CTableRefRange).GetTableRefMaxValue();
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII)
            {
                m_sMinValue     = (TableRef as CTableRefAscii).GetTableRefMinValue();
                m_sMaxValue     = (TableRef as CTableRefAscii).GetTableRefMaxValue();
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII)
            {
                m_sMinValue = (TableRef as CTableRefReadableAscii).GetTableRefMinValue();
                m_sMaxValue = (TableRef as CTableRefReadableAscii).GetTableRefMaxValue();
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
                sTempMin    = (TableRef as CTableRefRange).GetTableRefMinValue();
                sTempMax    = (TableRef as CTableRefRange).GetTableRefMaxValue();
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII)
            {
                sTempMin    = (TableRef as CTableRefAscii).GetTableRefMinValue();
                sTempMax    = (TableRef as CTableRefAscii).GetTableRefMaxValue();
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII)
            {
                sTempMin = (TableRef as CTableRefReadableAscii).GetTableRefMinValue();
                sTempMax = (TableRef as CTableRefReadableAscii).GetTableRefMaxValue();
            }

            if (
                    (sTempMin   == m_sMinValue) &&
                    (sTempMax   == m_sMaxValue) 
                )
            {
                bRet = true;
            }

            return bRet;
        }
    }
}
