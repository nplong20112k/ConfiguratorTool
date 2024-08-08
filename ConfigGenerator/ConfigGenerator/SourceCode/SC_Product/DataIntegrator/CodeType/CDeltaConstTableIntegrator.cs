using System.Collections.Generic;
using System.Numerics;

namespace ConfigGenerator
{
    public class Event_DeltaTableIntegrator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
                EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CDeltaConstTableIntegrator : AIntegerator
    {
        private enum CHECK_VALUE_RESULT
        {
            CHECK_VALUE_OK = 0,
            OUT_OF_RANGE,
            EXCEPTION,
        }

        private List<INTERFACE_CLASS> m_lsInterfaceClass    = null;

        private List<string> m_lsExceptionEnumOnly = null;

        public CDeltaConstTableIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_DELTA_CONST_TABLE, new Event_DeltaTableIntegrator().m_MyEvent)
        {

        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                    {
                        m_lsInterfaceClass = (oData as CSourceInfoObject).GetInterfaceClass();
                        m_lsExceptionEnumOnly = (oData as CSourceInfoObject).GetExceptionEnumItem();
                    }
                    break;

                default:
                    break;
            }
        }

        public override void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut)
        {
            if ((oDataIn != null) && (oDataOut != null))
            {
                if (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                {
                    CDeltaConstTable DeltaTableObj = null;

                    CParsedParameterObject Parameter = (oDataIn as CParsedDataObject).GetParameter();
                    if (m_lsExceptionEnumOnly.Exists(x => x.Equals(Parameter.GetTagName())) == false)
                    {
                        ATableRef TableRef = (oDataIn as CParsedDataObject).GetTableRef();
                        List<DELTA_TABLE_ITEM> DeltaTableGroup = null;

                        if ((Parameter != null) &&
                            (TableRef != null))
                        {
                            DeltaTableGroup = CreateDeltaGroup(m_lsInterfaceClass, Parameter, TableRef);
                            DeltaTableObj = new CDeltaConstTable(Parameter.GetTagName(), Parameter.GetTagValueSize(), DeltaTableGroup);
                        }
                    }

                    oDataOut.SetDeltaTable(DeltaTableObj);
                }
            }
        }

        public List<DELTA_TABLE_ITEM> CreateDeltaGroup(List<INTERFACE_CLASS> lsInterfaceClass, CParsedParameterObject Parameter, ATableRef TableRef)
        {
            List<DELTA_TABLE_ITEM> DeltaTableGroup  = new List<DELTA_TABLE_ITEM>();
            DELTA_TABLE_ITEM MemberItem             = new DELTA_TABLE_ITEM();
            List<string>    sValueDefaultClass      = new List<string>();
            string          sValueSize              = Parameter.GetTagValueSize();

            for (int i = 0; i < m_lsInterfaceClass.Count; i++)
            {
                sValueDefaultClass.Add(Parameter.GetDefaultValueClass(i));
            }

            for (int i = 0; i < lsInterfaceClass.Count; i++)
            {
                if (sValueDefaultClass[i] != null)
                {
                    if (!CParsedDataObject.KEYWORD_INTERFACE_NA.Contains(sValueDefaultClass[i].Trim()))
                    {
                        foreach (INTERFACE_CLASS_MEMBER elementMember in lsInterfaceClass[i].m_lsInterfaceMember)
                        {
                            MemberItem.m_sInterfaceName = elementMember.m_sInterfaceName;

                            if (elementMember.m_bInterfaceValid == true)
                            {
                                CHECK_VALUE_RESULT eResult = IsDeltaValueValid(sValueDefaultClass[i], TableRef);
                                if (eResult == CHECK_VALUE_RESULT.CHECK_VALUE_OK)
                                {
                                    if (sValueSize == "1")
                                    {
                                        MemberItem.m_sValue = sValueDefaultClass[i];
                                    }
                                    else
                                    {
                                        int iSizeLen = int.Parse(sValueSize) * 2;
                                        string sTempValue = CCodeFormater.PaddingValue(sValueDefaultClass[i], iSizeLen, TableRef);
                                        MemberItem.m_sValue = CCodeFormater.FormatValue(sTempValue);
                                    }

                                    DeltaTableGroup.Add(MemberItem);
                                }
                                else
                                {
                                    // Error detected
                                    string sStatistic = "CI_Class{0}_Default: Delta value is ";
                                    sStatistic = string.Format(sStatistic, lsInterfaceClass[i].m_uiClassNumber.ToString());
                                    if (eResult == CHECK_VALUE_RESULT.OUT_OF_RANGE)
                                    {
                                        sStatistic += "out of range";
                                    }
                                    else
                                    {
                                        sStatistic += "exception value";
                                    }

                                    CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                }
                            }
                        }
                    }
                }
            }

            return DeltaTableGroup;
        }

        private CHECK_VALUE_RESULT IsDeltaValueValid(string sValue, ATableRef TableRef)
        {
            CHECK_VALUE_RESULT bRet = CHECK_VALUE_RESULT.OUT_OF_RANGE;

            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION)
            {
                string sMaxValue = null;
                string sMinValue = null;
                List<string> sExceptionValueList = null;

                CMaxMinUpdater.GetRealMaxMinExceptionListForSelection(TableRef, ref sMaxValue, ref sMinValue, ref sExceptionValueList);

                if (IsValueInRange(sValue, sMinValue, sMaxValue) == true)
                {
                    bRet = CHECK_VALUE_RESULT.EXCEPTION;
                    BigInteger biCompareValue = BigInteger.Parse(sValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
                    List<CTableRefSelection.VALUE_TYPE> sListValue = (TableRef as CTableRefSelection).GetTableRefValueList();
                    if (sListValue.Count > 0)
                    {
                        foreach (CTableRefSelection.VALUE_TYPE ElementValue in sListValue)
                        {
                            BigInteger biValueInList = BigInteger.Parse(ElementValue.sValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
                            if (biValueInList == biCompareValue)
                            {
                                bRet = CHECK_VALUE_RESULT.CHECK_VALUE_OK;
                                break;
                            }
                        }
                    }

                    if (CHECK_VALUE_RESULT.EXCEPTION == bRet)
                    {
                        if ((null != sExceptionValueList) && (0 < sExceptionValueList.Count))
                        {
                            foreach (string ElementValue in sExceptionValueList)
                            {
                                BigInteger biValueInList = BigInteger.Parse(ElementValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
                                if (biValueInList == biCompareValue)
                                {
                                    bRet = CHECK_VALUE_RESULT.CHECK_VALUE_OK;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            bRet = CHECK_VALUE_RESULT.CHECK_VALUE_OK;
                        }
                    }
                }
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE)
            {
                string sMaxValue = null;
                string sMinValue = null;
                List<string> sExceptionValueList = null;

                CMaxMinUpdater.GetRealMaxMinExceptionListForRange(TableRef, ref sMaxValue, ref sMinValue, ref sExceptionValueList);

                if (IsValueInRange(sValue, sMinValue, sMaxValue) == true)
                {
                    bRet = CHECK_VALUE_RESULT.CHECK_VALUE_OK;
                }
            }
            else
            {
                bRet = CHECK_VALUE_RESULT.CHECK_VALUE_OK;
            }
            return bRet;
        }

        private bool IsValueInRange(string sCompareValue, string sMinValue, string sMaxValue)
        {
            bool bRet = false;
            BigInteger biCompareValue = BigInteger.Parse(sCompareValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
            BigInteger biMinValue = BigInteger.Parse(sMinValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
            BigInteger biMaxValue = BigInteger.Parse(sMaxValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
            if ((biMinValue <= biCompareValue) && (biCompareValue <= biMaxValue))
            {
                bRet = true;
            }
            return bRet;
        }
    }
}
