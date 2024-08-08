using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    public class Event_CParameterIntegrator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CParameterIntegerator : AIntegerator
    {
        public const string VALUE_TRUE = "true";

        private enum PARAMETER_TYPE_INDEX
        {
            PARAMETER_TYPE_NORMAL,
            PARAMETER_TYPE_MULTI_PARENT,
            PARAMETER_TYPE_MULTI_CHILD
        }

        private List<string> m_ParameterCodeList = null;

        public CParameterIntegerator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_RPIORITY_PARAMETER, new Event_CParameterIntegrator().m_MyEvent)
        {
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    ReInitProperties();
                    break;

                default:
                    break;
            }
        }

        public override void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut)
        {
            if (oDataIn != null && oDataOut != null)
            {
                CParsedPositionObject       Position            = null;
                List<CIntegrateParamObject> IntegratedParameter = null;

                if (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                {
                    Position = (oDataIn as CParsedDataObject).GetPosition();

                    switch (Position.GetUserVisibility())
                    {
                        default:
                        case CParsedDataObject.KEYWORD_PRIVATE:
                        case CParsedDataObject.KEYWORD_INTERNAL:
                            {
                                CParsedParameterObject Parameter = null;
                                Parameter = (oDataIn as CParsedDataObject).GetParameter();
                                VerifyParameterCodeExist(Parameter.GetTagCode());
                            }
                            break;

                        case CParsedDataObject.KEYWORD_PUBLIC_PRG:
                        case CParsedDataObject.KEYWORD_PUBLIC_QRG:
                            {
                                ATableRef TableRef = null;
                                CParsedParameterObject Parameter = null;

                                TableRef = (oDataIn as CParsedDataObject).GetTableRef();
                                Parameter = (oDataIn as CParsedDataObject).GetParameter();

                                if (VerifyParameterCodeExist(Parameter.GetTagCode()) == false)
                                {
                                    if ((TableRef != null) && (Parameter != null))
                                    {
                                        switch (TableRef.GetTableRefType())
                                        {
                                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_CUSTOM:
                                                IntegratedParameter = IntegrateParameterMultiple(TableRef, Parameter);
                                                break;

                                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII:
                                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                                                IntegratedParameter = IntegrateParameterNormal(TableRef, Parameter);
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
                            
                oDataOut.SetParameterList(IntegratedParameter);
            }
        }

        private List<CIntegrateParamObject> IntegrateParameterNormal(ATableRef TableRef, CParsedParameterObject Parameter)
        {
            List<CIntegrateParamObject> oParameterListRet = null;

            if (TableRef != null && Parameter != null)
            {
                CIntegrateParamContainObject    ParameterContain   = null;
                CIntegrateParamObject           ParameterCover     = null;
                List<string>                    sParameterNameList = null;
                string                          sDefaultValue = null;

                oParameterListRet = new List<CIntegrateParamObject>();

                string sSizeLen = null;
                try
                {
                    // Convert size byte to size len
                    uint uiSizeBit = CBitUltilizer.ConvertSizeByteToSizeBit(Parameter.GetTagValueSize());
                    int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiSizeBit.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);
                    sSizeLen = iSizeLen.ToString();
                }
                catch { return null; }

                sParameterNameList = IntegrateParameterNameList(Parameter.GetTagCategory(), Parameter.GetTagName());

                sDefaultValue = Parameter.GetDefaultValue();
                if (true == TableRef.GetTableRefValueIsDecimal())
                {
                    sDefaultValue = CHexIntConverter.StringConvertHexToInt(sDefaultValue);
                }

                if (sParameterNameList != null)
                {
                    foreach (string element in sParameterNameList)
                    {
                        ParameterContain = IntegrateParameterMandatoryFields(
                                                PARAMETER_TYPE_INDEX.PARAMETER_TYPE_NORMAL,
                                                element,
                                                sDefaultValue,
                                                TableRef.GetTableRefValueType(),
                                                Parameter.GetProtection(),
                                                Parameter.GetTagCode(),
                                                TableRef.GetTableRefName(),
                                                sSizeLen,
                                                Parameter.GetTagUserName()
                                            );

                        if (ParameterContain != null)
                        {
                            if (IntegrateParameterOptionalFields(ref ParameterContain, TableRef, "0", Parameter.GetTagValueSize()))
                            {
                                ParameterCover = new CIntegrateParamObject();
                                if (ParameterCover != null)
                                {
                                    ParameterCover.AddSubParameter(ParameterContain);
                                    oParameterListRet.Add(ParameterCover);
                                }
                            }
                        }
                    }
                }
            }

            return oParameterListRet;
        }

        private List<CIntegrateParamObject> IntegrateParameterMultiple(ATableRef TableRef, CParsedParameterObject Parameter)
        {
            List<CIntegrateParamObject> oParameterListRet = null;

            if (TableRef != null && Parameter != null)
            {
                CIntegrateParamContainObject ParameterContain   = null;
                CIntegrateParamObject        ParameterCover     = null;
                List<string>                 sParameterNameList = null;
                uint                         uiTotalSizeBit     = 0;

                oParameterListRet = new List<CIntegrateParamObject>();

                string sDefaultValue = Parameter.GetDefaultValue();
                string sSizeLen = null;
                try
                {
                    // Convert size byte to size len
                    uiTotalSizeBit = CBitUltilizer.ConvertSizeByteToSizeBit(Parameter.GetTagValueSize());
                    int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiTotalSizeBit.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);
                    sSizeLen = iSizeLen.ToString();

                    // Padding for full sizelen of default value
                    sDefaultValue = sDefaultValue.PadLeft(iSizeLen, '0');
                }
                catch { return null; }

                sParameterNameList = IntegrateParameterNameList(Parameter.GetTagCategory(), Parameter.GetTagName());
                if ((sParameterNameList != null) && (sParameterNameList.Count > 0))
                {
                    foreach (string paramNameElement in sParameterNameList)
                    {
                        ParameterContain = IntegrateParameterMandatoryFields(
                                            PARAMETER_TYPE_INDEX.PARAMETER_TYPE_MULTI_PARENT,
                                            paramNameElement,
                                            null,
                                            null,
                                            Parameter.GetProtection(),
                                            Parameter.GetTagCode(),
                                            null,
                                            sSizeLen,
                                            Parameter.GetTagUserName()
                                        );

                        if (ParameterContain != null)
                        {
                            ParameterCover = new CIntegrateParamObject();
                            if (ParameterCover != null)
                            {
                                ParameterCover.AddSubParameter(ParameterContain);

                                List<CTableRefCustom.SUB_TABLE_TYPE> SubTableRefList = (TableRef as CTableRefCustom).GetSubTableRefList();
                                if ((SubTableRefList != null) && (SubTableRefList.Count > 0))
                                {
                                    string sBinDefaultValue = String.Join(String.Empty,
                                                                            sDefaultValue.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0'))
                                                                            );

                                    foreach (CTableRefCustom.SUB_TABLE_TYPE subTableElement in SubTableRefList)
                                    {
                                        string sSubDefaultValue = null;
                                        string sSubSizeLen = null;
                                        uint uiSubSizeBit = Convert.ToUInt32(subTableElement.m_sSubSize);

                                        if (uiSubSizeBit <= sBinDefaultValue.Length)
                                        {
                                            sSubDefaultValue = sBinDefaultValue.Substring(0, (int)uiSubSizeBit);
                                            sSubDefaultValue = Convert.ToString(Convert.ToInt32(sSubDefaultValue, 2), 16).ToUpper();
                                            sBinDefaultValue = sBinDefaultValue.Substring((int)uiSubSizeBit);

                                            uint uiSubSizeLen = CBitUltilizer.ConvertSizeBitToSizeAppSupport(subTableElement.m_sSubSize, CConfigTool.GetInstance().SIZE_BIT_SUPPORT);
                                            sSubDefaultValue = sSubDefaultValue.PadLeft((int)uiSubSizeLen, '0');
                                            sSubSizeLen = uiSubSizeLen.ToString();

                                            ParameterContain = IntegrateParameterMandatoryFields(
                                                        PARAMETER_TYPE_INDEX.PARAMETER_TYPE_MULTI_CHILD,
                                                        subTableElement.m_sSubName,
                                                        sSubDefaultValue,
                                                        subTableElement.m_sSubTableRef.GetTableRefValueType(),
                                                        null,
                                                        null,
                                                        subTableElement.m_sSubTableRef.GetTableRefName(),
                                                        sSubSizeLen,
                                                        subTableElement.m_sSubContext
                                                    );

                                            if (IntegrateParameterOptionalFields(ref ParameterContain, subTableElement.m_sSubTableRef, "0", sSubSizeLen))
                                            {
                                                ParameterCover.AddSubParameter(ParameterContain);
                                            }
                                        }
                                        else
                                        {
                                            CStatisticObject StatisticData = new CStatisticObject();
                                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "CI_ValueOptions: sub parameter size in bit");
                                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                        }
                                    }
                                }

                                oParameterListRet.Add(ParameterCover);
                            }
                        }
                    }
                }
            }

            return oParameterListRet;
        }

        private CIntegrateParamContainObject IntegrateParameterMandatoryFields(PARAMETER_TYPE_INDEX ParamType, 
                                                                           string sTagName, string sDefaultValue, string sValueType, string sProtection, 
                                                                           string sTagCode, string sTableRefName, string sSizeLen,   string sContext)
        {
            CIntegrateParamContainObject    oParameterRet       = null;
            bool                            bFlagErrorDetected  = false;
            CStatisticObject                StatisticData       = new CStatisticObject();
            
            if ((sTagName == null) || ((sTagName == "")))
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_TagName");
                bFlagErrorDetected = true;
            }

            if ((sSizeLen == null) || ((sSizeLen == "")))
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueSizeBytes");
                bFlagErrorDetected = true;
            }

            if ((sContext == null) || ((sContext == "")))
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_TagUserName");
                bFlagErrorDetected = true;
            }

            if ((ParamType == PARAMETER_TYPE_INDEX.PARAMETER_TYPE_NORMAL) ||
                (ParamType == PARAMETER_TYPE_INDEX.PARAMETER_TYPE_MULTI_CHILD))
            {
                if ((sDefaultValue == null) || ((sDefaultValue == "")))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_Master_Default_Eugene/CI_Master_Default_Bologna");
                    bFlagErrorDetected = true;
                }

                if ((sValueType == null) || ((sValueType == "")))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: value type");
                    bFlagErrorDetected = true;
                }

                if ((sTableRefName == null) || ((sTableRefName == "")))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: table ref invalid");
                    bFlagErrorDetected = true;
                }
            }

            if ((ParamType == PARAMETER_TYPE_INDEX.PARAMETER_TYPE_NORMAL) ||
                (ParamType == PARAMETER_TYPE_INDEX.PARAMETER_TYPE_MULTI_PARENT))
            {
                if ((sProtection == null) || ((sProtection == "")))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_UserVisibility");
                    bFlagErrorDetected = true;
                }

                if ((sTagCode == null) || ((sTagCode == "")))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "Tag Code");
                    bFlagErrorDetected = true;
                }
            }

            if (bFlagErrorDetected == true)
            {
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                return null;
            }

            oParameterRet = new CIntegrateParamContainObject(
                                    sTagName,
                                    sDefaultValue,
                                    sValueType,
                                    sProtection,
                                    sTagCode,
                                    sTableRefName,
                                    sSizeLen,
                                    sContext
                                );

            return oParameterRet;
        }

        private bool IntegrateParameterOptionalFields(ref CIntegrateParamContainObject Parameter, ATableRef TableRef, string sMinLen, string sMaxLen)
        {
            bool             bRet               = true;
            bool             bFlagErrorDetected = false;
            CStatisticObject StatisticData      = new CStatisticObject();

            if ((Parameter != null) && (TableRef != null))
            {
                switch (TableRef.GetTableRefType())
                {
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII:
                        {
                            if ((TableRef as CTableRefAscii).GetTableRefPaddingValue() == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: padding");
                                bFlagErrorDetected = true;
                            }

                            if (sMinLen == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: min len");
                                bFlagErrorDetected = true;
                            }

                            if (sMaxLen == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: max len");
                                bFlagErrorDetected = true;
                            }

                            if (bFlagErrorDetected == true)
                            {
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                return false;
                            }

                            Parameter.SetParamFillChar((TableRef as CTableRefAscii).GetTableRefPaddingValue());
                            Parameter.SetParamMinLen(sMinLen);
                            Parameter.SetParamMaxLen(sMaxLen);

                            if ((TableRef as CTableRefAscii).GetTableRefValueIsHidden() == true)
                            {
                                Parameter.SetParamHiddenValue(VALUE_TRUE);
                            }
                        }
                        break;

                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                        {
                            if ((TableRef as CTableRefReadableAscii).GetTableRefPaddingValue() == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: padding");
                                bFlagErrorDetected = true;
                            }

                            if (sMinLen == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: min len");
                                bFlagErrorDetected = true;
                            }

                            if (sMaxLen == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: max len");
                                bFlagErrorDetected = true;
                            }

                            if (bFlagErrorDetected == true)
                            {
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                return false;
                            }

                            Parameter.SetParamFillChar((TableRef as CTableRefReadableAscii).GetTableRefPaddingValue());
                            Parameter.SetParamMinLen(sMinLen);
                            Parameter.SetParamMaxLen(sMaxLen);

                            if ((TableRef as CTableRefReadableAscii).GetTableRefValueIsHidden() == true)
                            {
                                Parameter.SetParamHiddenValue(VALUE_TRUE);
                            }
                        }
                        break;

                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                        {
                            ATableRef WorkingTableRef = TableRef;
                            if (true == TableRef.GetTableRefValueIsDecimal())
                            {
                                WorkingTableRef = TableRef.GetDecimalInstance();
                            }

                            if ((TableRef as CTableRefRange).GetTableRefMinValue() == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: min len");
                                bFlagErrorDetected = true;
                            }

                            if ((TableRef as CTableRefRange).GetTableRefMaxValue() == null)
                            {
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: max len");
                                bFlagErrorDetected = true;
                            }

                            if (bFlagErrorDetected == true)
                            {
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                return false;
                            }

                            Parameter.SetParamMinValue((WorkingTableRef as CTableRefRange).GetTableRefMinValue());
                            Parameter.SetParamMaxValue((WorkingTableRef as CTableRefRange).GetTableRefMaxValue());
                        }
                        break;

                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                    default:
                        break;
                }
            }

            return bRet;
        }

        private bool VerifyParameterCodeExist(string ParameterCode)
        {
            bool                bRet = false;
            CStatisticObject    StatisticData = new CStatisticObject();

            if ((ParameterCode != null) && (ParameterCode.Length == CIntegrateParamObject.CODE_LENGTH))
            {
                foreach (string element in m_ParameterCodeList)
                {
                    if (ParameterCode == element)
                    {
                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "Duplicate Configuration Item");
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                        return true;
                    }
                }

                m_ParameterCodeList.Add(ParameterCode);
            }

            return bRet;
        }

        private List<string> IntegrateParameterNameList(List<string> sCategoryList, string sTagName)
        {
            List<string> sRetList = null;

            if ((sTagName != null) && (sTagName != string.Empty))
            {
                sRetList = new List<string>();
                if ((sCategoryList != null) && (sCategoryList.Count > 0))
                {
                    foreach (string element in sCategoryList)
                    {
                        string sParameterName = element + CPositionIntegrator.KEYWORD_SPLIT_LEVEL +  sTagName;
                        sRetList.Add(sParameterName);
                    }
                }
                else
                {
                    sRetList.Add(sTagName);
                }
            }

            return sRetList;
        }

        private void ReInitProperties()
        {
            m_ParameterCodeList = new List<string>();
        }
    }
}
