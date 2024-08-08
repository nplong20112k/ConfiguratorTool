using System.Collections.Generic;

namespace ConfigGenerator
{
    class CAutoTestTableRefIntegrator : AIntegerator
    {
        private List<AXmlTable> m_TableRefSelectionList = new List<AXmlTable>();
        private List<AXmlTable> m_TableRefHexRangeList = new List<AXmlTable>();
        private List<AXmlTable> m_TableRefIntRangeList = new List<AXmlTable>();

        public CAutoTestTableRefIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_AUTOMATION_TEST_TABLE_REF)
        {

        }

        public override void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut)
        {
            if ((oDataIn != null) && (oDataOut != null))
            {
                ATableRef TableRefFromDataIn = null;

                TableRefFromDataIn = (oDataIn as CParsedDataObject).GetTableRef();

                UpdateTableRefAutotest(TableRefFromDataIn, ref oDataOut);
            }
        }

        private void UpdateMemberList(ATableRef TableRefIn, AXmlTable IntegratedTableRef)
        {
            if ((TableRefIn != null) && (IntegratedTableRef != null))
            {
                switch (TableRefIn.GetTableRefType())
                {
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                        if (TableRefIn.GetTableRefValueIsDecimal() == true)
                        {
                            m_TableRefIntRangeList.Add(IntegratedTableRef);
                        }
                        else
                        {
                            m_TableRefHexRangeList.Add(IntegratedTableRef);
                        }
                        break;
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                        {
                            m_TableRefSelectionList.Add(IntegratedTableRef);
                        }
                        break;
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII:
                        {
                            m_TableRefHexRangeList.Add(IntegratedTableRef);
                        }
                        break;
                }
            }
        }

        private void UpdateTableRefAutotest(ATableRef TableRefIn, ref CIntegratedDataObject oDataOut)
        {
            if (TableRefIn == null || oDataOut == null) return;

            AXmlTable TempTableRef = null;
            List<AXmlTable> TempTableRefList = new List<AXmlTable>();
            List<AXmlTable> TableRefListFromDataOut = null;

            TableRefListFromDataOut = oDataOut.GetTableRefList();

            if (TableRefListFromDataOut == null)
            {
                switch (TableRefIn.GetTableRefType())
                {
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                        foreach (AXmlTable element in m_TableRefSelectionList)
                        {
                            if (TableRefIn.GetTableRefName() == element.GetName())
                            {
                                TempTableRef = new CEnumTable();
                                (TempTableRef as CEnumTable).CopyContent(TableRefIn);
                                TempTableRefList.Add(TempTableRef);
                                oDataOut.SetTableRefAutoTestList(TempTableRefList);
                                break;
                            }
                        }
                        break;

                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                        if (TableRefIn.GetTableRefValueIsDecimal() == true)
                        {
                            foreach (AXmlTable element in m_TableRefIntRangeList)
                            {
                                if (TableRefIn.GetTableRefName() == element.GetName())
                                {
                                    TempTableRef = new CIntRangeTable();
                                    (TempTableRef as CIntRangeTable).CopyContent(TableRefIn);
                                    TempTableRefList.Add(TempTableRef);
                                    oDataOut.SetTableRefAutoTestList(TempTableRefList);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (AXmlTable element in m_TableRefHexRangeList)
                            {
                                if (TableRefIn.GetTableRefName() == element.GetName())
                                {
                                    TempTableRef = new CHexRangeTable();
                                    (TempTableRef as CHexRangeTable).CopyContent(TableRefIn);
                                    TempTableRefList.Add(TempTableRef);
                                    oDataOut.SetTableRefAutoTestList(TempTableRefList);
                                    break;
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            else if ((TableRefListFromDataOut != null) && (TableRefListFromDataOut.Count > 0))
            {
                oDataOut.SetTableRefAutoTestList(TableRefListFromDataOut);

                // Just get first table as Selection/Range/Ascii only contain 1 table
                // Custom contain many table but for auto test dont care
                UpdateMemberList(TableRefIn, TableRefListFromDataOut[0]);
            }
        }
    }
}
