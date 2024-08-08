using System.Collections.Generic;
using System.Numerics;

namespace ConfigGenerator
{
    public class Event_CommonTableIntegrator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CCommonConstTableIntegrator : AIntegerator
    {
        private const string INTERFACE_MASK = "IF_ALL";
        private const string INTERFACE_NULL_MASK = "IF_ALL ^ IF_ALL";
        private const string PRODUCTION_MASK = "ALL_PRODUCTS";

        private List<INTERFACE_CLASS> m_InterfaceClass = null;

        private CStructureIntegerator m_StructureIntegrator;

        private List<string> m_lsExceptionEnumOnly = null;

        public CCommonConstTableIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_COMMON_CONST_TABLE, new Event_CommonTableIntegrator().m_MyEvent)
        {
            m_StructureIntegrator = new CStructureIntegerator();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                    {                   
                        m_InterfaceClass = (oData as CSourceInfoObject).GetInterfaceClass();
                        m_lsExceptionEnumOnly = (oData as CSourceInfoObject).GetExceptionEnumItem();
                    }
                    break;

                default:
                    break;
            }
            m_StructureIntegrator.EventHandler(Event, oData);
        }

        public override void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut)
        {
            if ((oDataIn != null) && (oDataOut != null))
            {
                if (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                {
                    ACommonConstTable CommonConstTable = null;
                    CStructure oStructureData = null;
                    CParsedParameterObject Parameter = (oDataIn as CParsedDataObject).GetParameter();

                    if (m_lsExceptionEnumOnly.Exists(x => x.Equals(Parameter.GetTagName())) == false)
                    {
                        ATableRef TableRef = (oDataIn as CParsedDataObject).GetTableRef();

                        if ((Parameter != null) &&
                            (TableRef != null))
                        {
                            if (Parameter.GetTagValueSize() == null)
                            {
                                // Error detected.
                                CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueSizeBytes");
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                            }
                            else
                            {
                                if (int.Parse(Parameter.GetTagValueSize()) == 1)
                                {
                                    CommonConstTable = IntegrateNormalItem(Parameter, TableRef);
                                }
                                else
                                {
                                    CommonConstTable = IntegrateSpecialItem(Parameter, TableRef);
                                }

                                // Integrate Structure Data
                                string sMaxValue = CommonConstTable.GetMaxValue();
                                oStructureData   = m_StructureIntegrator.IntegratingProcess(Parameter, sMaxValue, TableRef.GetTableRefType());
                            }
                        }
                    }
                    oDataOut.SetCommonConstTable(CommonConstTable);
                    oDataOut.SetStructure(oStructureData);
                }
            }
        }

        private ACommonConstTable IntegrateNormalItem(CParsedParameterObject Parameter, ATableRef TableRef /*, CInterfaceList Interface*/)
        {
            string sMaxValue = null;
            string sMinValue = null;
            List<string> sExceptionValueList = null;
            string sOutInterface = null;
            bool bIsDecimalValue = false;

            bIsDecimalValue = TableRef.GetTableRefValueIsDecimal();
            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE)
            {
                CMaxMinUpdater.GetRealMaxMinExceptionListForRange(TableRef, ref sMaxValue, ref sMinValue, ref sExceptionValueList);
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION)
            {
                CMaxMinUpdater.GetRealMaxMinExceptionListForSelection(TableRef, ref sMaxValue, ref sMinValue, ref sExceptionValueList);
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII)
            {
                sMinValue = (TableRef as CTableRefAscii).GetTableRefMinValue();
                sMaxValue = (TableRef as CTableRefAscii).GetTableRefMaxValue();
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_CUSTOM)
            {
                sMinValue = (TableRef as CTableRefCustom).GetTableRefMinValue();
                sMaxValue = (TableRef as CTableRefCustom).GetTableRefMaxValue();
            }

            sOutInterface = CreateInterfaceMask(m_InterfaceClass, TableRef.GetUnavailableInterfaceClassList());

            return CreateNormalConstlObject ( COMMON_CONST_TABLE_TYPE.NORMAL_TABLE,
                                              Parameter.GetTagName(),
                                              Parameter.GetTagCode(),
                                              sOutInterface,
                                              PRODUCTION_MASK,
                                              sMinValue,
                                              sMaxValue,
                                              bIsDecimalValue,
                                              Parameter.GetDefaultValue(),
                                              sExceptionValueList);
        }

        private ACommonConstTable IntegrateSpecialItem(CParsedParameterObject Parameter, ATableRef TableRef /*, CInterfaceList Interface*/)
        {
            string sOutInterface = null;
            string sTempValue;
            string sDefaultValue = "";
            string sAltDefaultValue = "";
            string sDefaultString = null;
            string sAltDefaultString = null;
            string sMinValue = null;
            string sMaxValue = null;
            bool bIsDecimalValue = false;
            List<string> sValueList = null;
            VALUE_LIST_TYPE eValueListType = VALUE_LIST_TYPE.ACCEPTED_TYPE;

            //Calculate interface
            sOutInterface = CreateInterfaceMask(m_InterfaceClass, TableRef.GetUnavailableInterfaceClassList());

            // Convert size byte to size len: sizeLen = sizeByte*2
            int iSizeLen = int.Parse(Parameter.GetTagValueSize()) * 2;
            bIsDecimalValue = TableRef.GetTableRefValueIsDecimal();

            // Padding for full sizelen of default value
            if (Parameter.GetDefaultValue() == null)
            {
                sDefaultValue = null;
            }
            else
            {
                sTempValue = CCodeFormater.PaddingValue(Parameter.GetDefaultValue(), iSizeLen, TableRef);
                sDefaultValue = CCodeFormater.FormatValue(sTempValue);
                if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII || TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII)
                {
                    sDefaultString = FormatValueInAscii(sTempValue);
                }
            }

            if (Parameter.GetAltDefaultValue() == null)
            {
                sAltDefaultValue = null;
            }
            else
            {
                sTempValue = PaddingValue(Parameter.GetAltDefaultValue(), iSizeLen, TableRef);
                sAltDefaultValue = FormatValue(sTempValue);
                if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII || TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII)
                {
                    sAltDefaultString = FormatValueInAscii(sTempValue);
                }
            }

            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE)
            {
                List<string> ExtraValueList = (TableRef as CTableRefRange).GetExtraValueList();
                if (null == ExtraValueList)
                {
                    if ((TableRef as CTableRefRange).GetAcceptAllValueProperty() == true)
                    {
                        int iNumOfChar = int.Parse(Parameter.GetTagValueSize());
                        sMaxValue = "";
                        for (int i = 0; i < iNumOfChar; i++)
                        {
                            sMaxValue += "FF";
                        }
                        sMinValue = "0";
                    }
                    else
                    {
                        sMinValue = (TableRef as CTableRefRange).GetTableRefMinValue();
                        sMaxValue = (TableRef as CTableRefRange).GetTableRefMaxValue();
                    }
                }
                else
                {
                    BigInteger biMinValue = BigInteger.Parse(sMinValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
                    BigInteger biMaxValue = BigInteger.Parse(sMaxValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
                    foreach (string sValue in ExtraValueList)
                    {
                        BigInteger biTemp = BigInteger.Parse(sValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber);
                        if (biTemp < biMinValue)
                        {
                            biMinValue = biTemp;
                        }
                        else if (biTemp > biMaxValue)
                        {
                            biMaxValue = biTemp;
                        }
                    }
                    sMinValue = biMinValue.ToString();
                    sMaxValue = biMaxValue.ToString();
                }
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION)
            {
                if ((TableRef as CTableRefSelection).GetAcceptAllValueProperty() == false)
                {
                    List<CTableRefSelection.VALUE_TYPE> lsValue = (TableRef as CTableRefSelection).GetTableRefValueList();
                    foreach (CTableRefSelection.VALUE_TYPE element in lsValue)
                    {
                        if (sValueList == null)
                        {
                            sValueList = new List<string>();
                        }
                        sValueList.Add(element.sValue);
                    }

                    List<string> ExtraValueList = (TableRef as CTableRefSelection).GetExtraValueList();
                    if (null != ExtraValueList)
                    {
                        foreach (string sValue in ExtraValueList)
                        {
                            sValueList.Add(sValue);
                        }
                    }
                }
            }
            else if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII)
            {
                List<string> lsValue = (TableRef as CTableRefReadableAscii).GetTableRefValueList();
                if(lsValue != null)
                {
                    foreach (string element in lsValue)
                    {
                        if (sValueList == null)
                        {
                            sValueList = new List<string>();
                            eValueListType = VALUE_LIST_TYPE.TEXT_EXCEPTED_TYPE;
                        }
                        sValueList.Add(element);
                    }
                }
                else
                {
                    eValueListType = VALUE_LIST_TYPE.TEXT_NON_EXCEPTED_TYPE;
                }
            }

            return CreateSpecialConstlObject(COMMON_CONST_TABLE_TYPE.SPECIAL_TABLE,
                                                Parameter.GetTagName(),
                                                Parameter.GetTagCode(),
                                                sOutInterface,
                                                PRODUCTION_MASK,
                                                sDefaultValue,
                                                sAltDefaultValue,
                                                Parameter.GetTagName().ToLower(),
                                                Parameter.GetTagValueSize(),
                                                sDefaultString,
                                                sAltDefaultString,
                                                sMinValue,
                                                sMaxValue,
                                                bIsDecimalValue,
                                                sValueList,
                                                eValueListType);
        }

        private ACommonConstTable IntegrateCustomItem(CParsedParameterObject Parameter, ATableRef TableRef /*, CInterfaceList Interface*/)
        {
            /*
            List<CTableRefCustom.SUB_TABLE_TYPE> SubTableRefList = (TableRef as CTableRefCustom).GetSubTableRefList();
            if ((SubTableRefList != null) && (SubTableRefList.Count > 0))
            {
                foreach (CTableRefCustom.SUB_TABLE_TYPE element in SubTableRefList)
                {
                    string sSubDefaultValue = null;
                    string sSubSizeLen = null;
                    try
                    {
                        int iSubSize = int.Parse(element.m_sSubSize);
                        if (iSubSize <= sDefaultValue.Length)
                        {
                            sSubDefaultValue = sDefaultValue.Substring(0, iSubSize);
                            sDefaultValue = sDefaultValue.Substring(iSubSize);
                            sSubSizeLen = iSubSize.ToString();
                        }
                    }
                    catch { return null; }

                    ParameterContain = IntegrateParameterMandatoryFields(
                                PARAMETER_TYPE_INDEX.PARAMETER_TYPE_MULTI_CHILD,
                                element.m_sSubName,
                                sSubDefaultValue,
                                element.m_sSubTableRef.GetTableRefValueType(),
                                null,
                                null,
                                element.m_sSubTableRef.GetTableRefName(),
                                sSubSizeLen,
                                element.m_sSubContext
                            );

                    if (IntegrateParameterOptionalFields(ref ParameterContain, element.m_sSubTableRef, "0", sSubSizeLen))
                    {
                        oParameterRet.AddSubParameter(ParameterContain);
                    }
                }
            }
            */

            return null;
        }

        private ACommonConstTable CreateNormalConstlObject(COMMON_CONST_TABLE_TYPE TableType, string sTagName, string sTagCode, string sInterfaceMask, string sProductMask, string sMinValue, string sMaxValue, bool bIsDecimalValue, string sDefaultValue, List<string> sExceptionValueList)
        {
            bool bFlagErrorDetected = false;
            bool bFlagLogAdded = false;
            CStatisticObject StatisticData = new CStatisticObject();

            // Detect error
            if (string.IsNullOrEmpty(sTagName) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_TagName");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (string.IsNullOrEmpty(sTagCode) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "Tag Code");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (sTagCode.Length != 4)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "Tag Code");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (sInterfaceMask == INTERFACE_NULL_MASK)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "CI_ValueOptions: All interfaces are excluded");
                bFlagLogAdded = true;
            }

            if (string.IsNullOrEmpty(sMinValue) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: min value");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (string.IsNullOrEmpty(sMaxValue) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: max value");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (sMaxValue == "0")
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "CI_ValueOptions: max value");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (string.IsNullOrEmpty(sDefaultValue) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_Master_Default_Eugene/CI_Master_Default_Bologna");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (true == bFlagLogAdded)
            {
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                if (true == bFlagErrorDetected)
                {
                    return null;
                }
            }

            CNormalConstItem CommonConstItem = new CNormalConstItem(TableType,
                                                                    sTagName,
                                                                    sTagCode,
                                                                    sInterfaceMask,
                                                                    sProductMask,
                                                                    sMinValue,
                                                                    sMaxValue,
                                                                    bIsDecimalValue,
                                                                    sDefaultValue,
                                                                    sExceptionValueList);
            return CommonConstItem;
        }

        private ACommonConstTable CreateSpecialConstlObject(COMMON_CONST_TABLE_TYPE TableType, string sTagName, string sTagCode, string sInterfaceMask, string sProductMask, string sDefaultValue, string sAltDefaultValue, string sStructure, string sValueSize, string sDefaultString, string sAltDefaultString, string sMinValue, string sMaxValue, bool bIsDecimalValue, List<string> sValueList, VALUE_LIST_TYPE eValueListType)
        {
            bool bFlagErrorDetected = false;
            bool bFlagLogAdded = false;
            CStatisticObject StatisticData = new CStatisticObject();

            // Detect error
            if (string.IsNullOrEmpty(sTagName) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_TagName");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (string.IsNullOrEmpty(sTagCode) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "Tag Code");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (sTagCode.Length != 4) 
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "Tag Code");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (sInterfaceMask == INTERFACE_NULL_MASK)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "CI_ValueOptions: All interfaces are excluded");
                bFlagLogAdded = true;
            }

            if (string.IsNullOrEmpty(sDefaultValue) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_Master_Default_Eugene/CI_Master_Default_Bologna");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (string.IsNullOrEmpty(sAltDefaultValue) == true)
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_Master_Default_Eugene/CI_Master_Default_Bologna");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (bIsDecimalValue && (Int32.Parse(sValueSize) > 16))
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "CI_ValueOptions: Value size larger 8 bytes for int value");
                bFlagErrorDetected = true;
                bFlagLogAdded = true;
            }

            if (true == bFlagLogAdded)
            {
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                if (true == bFlagErrorDetected)
                {
                    return null;
                }
            }

            CSpecialConstItem CommonConstItem = new CSpecialConstItem(TableType,
                                                                        sTagName,
                                                                        sTagCode,
                                                                        sInterfaceMask,
                                                                        sProductMask,
                                                                        sDefaultValue,
                                                                        sAltDefaultValue,
                                                                        sStructure,
                                                                        sValueSize,
                                                                        sDefaultString,
                                                                        sAltDefaultString,
                                                                        sMinValue,
                                                                        sMaxValue,
                                                                        bIsDecimalValue,
                                                                        sValueList,
                                                                        eValueListType);
            return CommonConstItem;
        }

        private string PaddingValue (string sValue, int iValueSize, ATableRef TableRef)
        {
            string sRet;

            if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII || TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII)
            {
                string sPadding;
                if (TableRef.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII)
                {
                    sPadding = (TableRef as CTableRefAscii).GetTableRefPaddingValue();
                }
                else
                {
                    sPadding = (TableRef as CTableRefReadableAscii).GetTableRefPaddingValue();
                }

                sRet = sValue;
                while (sRet.Length < iValueSize)
                {
                    sRet += sPadding;
                }
                sRet = sRet.Substring(0, iValueSize);
            }
            else
            {
                sRet = sValue.PadLeft(iValueSize, '0');
            }

            return sRet;
        }

        private string FormatValue (string sValue)
        {
            string sSubString;
            string sRet = "";

            for (int i = 0; i < sValue.Length; i += 2)
            {
                sSubString = sValue.Substring(i, 2);
                sRet += "\\x" + sSubString;
            }

            return sRet;
        }

        private string FormatValueInAscii (string sValue)
        {
            string sSubString;
            string sRet = "";
            char cChar;

            for (int i = 0; i < sValue.Length; i += 2)
            {
                sSubString = sValue.Substring(i, 2);
                cChar = (char)int.Parse(sSubString, System.Globalization.NumberStyles.HexNumber);
                if ((cChar >= ' ') && (cChar <= '~'))
                {
                    sRet += cChar;
                }
                else
                {
                    sRet += ' ';
                }
            }

            return sRet;
        }

        private string CreateInterfaceMask(List<INTERFACE_CLASS> InterfaceClass, List<uint> IgnoredClass)
        {
            string sOutInterfaceOr = null;
            string sOutInterfaceXor = null;
            string sHeaderInterfaceXor = INTERFACE_MASK + " ^ (";
            string sOutInterface = null;
            uint uiCount = 0;

            for (int i = 0; i < InterfaceClass.Count; i++)
            {
                if ((IgnoredClass != null) &&
                    (IgnoredClass.Contains(InterfaceClass[i].m_uiClassNumber)))
                {
                    uiCount++;
                    foreach (INTERFACE_CLASS_MEMBER element in InterfaceClass[i].m_lsInterfaceMember)
                    {
                        if (element.m_bInterfaceValid == true)
                        {
                            sOutInterfaceXor += " | " + element.m_sInterfaceName;
                        }
                    }
                }
                else
                {
                    foreach (INTERFACE_CLASS_MEMBER element in InterfaceClass[i].m_lsInterfaceMember)
                    {
                        if (element.m_bInterfaceValid == true)
                        {
                            sOutInterfaceOr += " | " + element.m_sInterfaceName;
                        }
                        else
                        {
                            sOutInterfaceXor += " | " + element.m_sInterfaceName;
                        }
                    }
                }
            }

            if (uiCount == 0)
            {
                return INTERFACE_MASK;
            }
            else if ((uiCount == InterfaceClass.Count) || (string.IsNullOrEmpty(sOutInterfaceOr)))
            {
                return INTERFACE_NULL_MASK;
            }
            else
            {
                sOutInterfaceOr = sOutInterfaceOr.Substring(3);
                sOutInterfaceXor = sHeaderInterfaceXor + sOutInterfaceXor.Substring(3) + ")";

                sOutInterface = (sOutInterfaceOr.Length < sOutInterfaceXor.Length) ? sOutInterfaceOr : sOutInterfaceXor;
                return sOutInterface;
            }
        }
    }
}