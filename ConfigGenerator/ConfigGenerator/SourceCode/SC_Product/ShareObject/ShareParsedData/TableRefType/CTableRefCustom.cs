using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CTableRefCustom : ATableRef
    {
        public struct SUB_TABLE_TYPE
        {
            public string m_sSubName;
            public string m_sSubSize;
            public string m_sSubContext;
            public ATableRef m_sSubTableRef;
        }

        private List<SUB_TABLE_TYPE> m_SubTableRef = null;
        private string m_sMinValue = null;
        private string m_sMaxValue = null;

        public CTableRefCustom()
            : base(TABLE_REF_TYPE.TABLE_REF_CUSTOM)
        {

        }

        public override ATableRef GetDecimalInstance()
        {
            CTableRefCustom NewInstance = (CTableRefCustom) this.MemberwiseClone();

            if ((m_SubTableRef != null) && (m_SubTableRef.Count > 0))
            {
                NewInstance.m_SubTableRef = new List<SUB_TABLE_TYPE>();

                foreach (SUB_TABLE_TYPE Element in m_SubTableRef)
                {
                    SUB_TABLE_TYPE TempSubTable = new SUB_TABLE_TYPE();
                    TempSubTable.m_sSubContext = Element.m_sSubContext;
                    TempSubTable.m_sSubName = Element.m_sSubName;
                    TempSubTable.m_sSubSize = Element.m_sSubSize;

                    if (Element.m_sSubTableRef != null)
                    {
                        switch (Element.m_sSubTableRef.GetTableRefType())
                        {
                            case TABLE_REF_TYPE.TABLE_REF_ASCII:
                                TempSubTable.m_sSubTableRef = new CTableRefAscii();
                                break;
                            case TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                                TempSubTable.m_sSubTableRef = new CTableRefReadableAscii();
                                break;
                            case TABLE_REF_TYPE.TABLE_REF_RANGE:
                                TempSubTable.m_sSubTableRef = new CTableRefRange();
                                break;

                            case TABLE_REF_TYPE.TABLE_REF_SELECTION:
                                TempSubTable.m_sSubTableRef = new CTableRefSelection();
                                break;
                            default:
                                break;
                        }
                        TempSubTable.m_sSubTableRef = Element.m_sSubTableRef.GetDecimalInstance();
                        NewInstance.m_SubTableRef.Add(TempSubTable);
                    }
                }
            }

            return this;
        }

        public void AddSubTableRef(SUB_TABLE_TYPE subTable)
        {
            AddSubTableRef(subTable.m_sSubName, subTable.m_sSubSize, subTable.m_sSubContext, subTable.m_sSubTableRef);
        }

        public void AddSubTableRef(string SubName, string SubSize, string SubContext, ATableRef SubTableRef)
        {
            if ((SubTableRef != null) && (SubName != null) && (SubSize != null) && (SubContext != null))
            {
                if (m_SubTableRef == null)
                {
                    m_SubTableRef = new List<SUB_TABLE_TYPE>();
                }

                SUB_TABLE_TYPE TempSubTableRef;

                TempSubTableRef.m_sSubName = SubName;
                TempSubTableRef.m_sSubSize = SubSize;
                TempSubTableRef.m_sSubContext = SubContext;
                TempSubTableRef.m_sSubTableRef = SubTableRef;

                m_SubTableRef.Add(TempSubTableRef);
            }
        }

        public List<SUB_TABLE_TYPE> GetSubTableRefList()
        {
            return m_SubTableRef;
        }

        public void SetTableRefMinValue(string sValue)
        {
            m_sMinValue = sValue;
        }

        public void SetTableRefMaxValue(string sValue)
        {
            m_sMaxValue = sValue;
        }

        public string GetTableRefMinValue()
        {
            return m_sMinValue;
        }

        public string GetTableRefMaxValue()
        {
            return m_sMaxValue;
        }
    }
}
