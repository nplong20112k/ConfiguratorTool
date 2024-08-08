using System.Collections.Generic;

namespace ConfigGenerator
{
    class CEnumTable : AXmlTable
    {
        List<CTableRefSelection.VALUE_TYPE> m_ValueList = null;

        public CEnumTable()
            :base(AXmlTable.XML_TABLE_TYPE.XML_TABLE_ENUM)
        {
        }

        public List<CTableRefSelection.VALUE_TYPE> GetTableRefValueList()
        {
            return m_ValueList;
        }

        public override bool CopyContent(ATableRef TableRef)
        {
            bool bRet = false;
            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION)
            {
                List<CTableRefSelection.VALUE_TYPE> TempValueList = (TableRef as CTableRefSelection).GetTableRefValueList();
                if ((TempValueList != null) && (TempValueList.Count > 0))
                {
                    CTableRefSelection.VALUE_TYPE TempValue;
                    m_ValueList = null;
                    m_ValueList = new List<CTableRefSelection.VALUE_TYPE>();

                    for (int i = 0; i < TempValueList.Count; i++)
                    {
                        TempValue.sDescript = TempValueList[i].sDescript;
                        TempValue.sValue = TempValueList[i].sValue;
                        m_ValueList.Add(TempValue);
                    }

                    bRet = true;
                }
            }
            return bRet;
        }

        public override bool CompareContent(ATableRef TableRef)
        {
            bool bRet = false;
            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION)
            {
                List<CTableRefSelection.VALUE_TYPE> TempList = (TableRef as CTableRefSelection).GetTableRefValueList();

                if ((m_ValueList != null) && (TempList != null))
                {
                    if (m_ValueList.Count == TempList.Count)
                    {
                        bRet = true;
                        for (int i = 0; i < m_ValueList.Count; i++)
                        {
                            if ((m_ValueList[i].sDescript != TempList[i].sDescript) ||
                                (m_ValueList[i].sValue != TempList[i].sValue))
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                }
            }
            return bRet;
        }
    }
}
