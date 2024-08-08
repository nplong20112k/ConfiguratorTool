using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ConfigGenerator
{
    public class Event_TableRefParser
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }
    class CTableRefParser : AParser
    {
        public const int FIRST_LINE_IDX                         = 0;
        public const int KEYWORD_IDX                            = 0;
        public const int VALUE_IDX                              = 1;
        public const int MINIMUM_SIZE_SUPPORT_IN_BIT            = 4;

        public const string FORMAT_SELECTION                    = "[format=selection]";
        public const string FORMAT_RANGE                        = "[format=range]";
        public const string FORMAT_ASCII                        = "[format=ascii]";
        public const string FORMAT_CUSTOM                       = "[format=custom]";
        public const string FORMAT_READABLE_ASCII               = "[format=readableascii]";
        public const string FORMAT_CUSTOM_OPTIMIZE              = "[selectionValues";

        public const string KEYWORD_MIN_MEMBER                  = "min";
        public const string KEYWORD_MAX_MEMBER                  = "max";
        public const string KEYWORD_STEP_MEMBER                 = "step";
        public const string KEYWORD_EXCEPTION_MEMBER            = "except";
        public const string KEYWORD_PADDING_MEMBER              = "padding";
        public const string KEYWORD_VALUE_SEND                  = "valueSent";
        public const string KEYWORD_SUB_SIZE_MEMBER             = "size";
        public const string KEYWORD_SUB_CONTEXT_MEMBER          = "display";
        public const string KEYWORD_ALL_VALUES_ACCEPTED         = "AllValuesAccepted";
        public const string KEYWORD_EXTRA_VALUES_ACCEPTED       = "ExtraValuesAccepted";
        public const string KEYWORD_ALL_VALUES_IN_RANGE         = "AllValuesInRange";
        public const string KEYWORD_VALUES                      = "values";
        public const string KEYWORD_ALADDIN_HIDDEN_VALUE        = "AladdinHiddenValue";
        public const string KEYWORD_NOT_AVAILABLE_FOR           = "NotAvailableFor";
        public const string KEYWORD_ONLY_AVAILABLE_FOR          = "OnlyAvailableFor";
        public const string KEYWORD_CLASSES_MEMBER              = "classes";
        public const string KEYWORD_EXTRA_OPTION                = "ExtraOption";
        public const string KEYWORD_PRG_VALUE_INCLUDED          = "PRG_ValueIncluded";

        public const string FIELD_START                         = "[field";
        public const string FIELD_STOP                          = "[/field";
        public const string SELECTION_OPTIMIZE_START            = "[selectionValues";
        public const string SELECTION_OPTIMIZE_STOP             = "[/selectionValues";

        public const char DELIMITER_CHAR_END_OF_FIELD           = '\n';
        public const char DELIMITER_CHAR_KEYWORD_WITH_VALUE     = '=';
        public const char DELIMITER_CHAR_INDICATOR_WITH_VALUE   = ' ';
        public const char DELIMITER_CHAR_VALUE_WITH_VALUE       = ',';

        public const string INDICATE_START                      = "[";
        public const string INDICATE_STOP                       = "]";

        public const string VALUE_TYPE_INT                      = "int";
        public const string VALUE_TYPE_HEXINT                   = "hex";
        public const string VALUE_TRUE                          = "true";
        public const string VALUE_FALSE                         = "false";
        public const string VALUE_SELECTION                     = "selection";

        private CTableRefValueConverter m_TableRefValueConverter = null;
        private CTableRefOptionalFieldParser m_TableRefOptionalFieldParser = null;
        private List<uint> m_AvailableClassList = null;

        public CTableRefParser()
            : base(new Event_TableRefParser().m_MyEvent)
        {
            m_TableRefValueConverter = new CTableRefValueConverter();
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                    {
                        List<INTERFACE_CLASS> m_InterfaceClass = (oData as CSourceInfoObject).GetInterfaceClass();
                        if (m_InterfaceClass.Count != 0)
                        {
                            m_AvailableClassList = new List<uint>(m_InterfaceClass.Count);
                            foreach (INTERFACE_CLASS element in m_InterfaceClass)
                            {
                                m_AvailableClassList.Add(element.m_uiClassNumber);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }
        //========================================
        // INTERFACE FUNCTIONS
        //========================================
        public override bool ParserProcessing(IShareObject oDataObject, ref CParsedDataObject oShareObject)
        {
            bool bRet = true;
            string[] sValueOptions = null;
            uint uiValueSize = 0;

            string sTemp = null;
            ATableRef TableRef = null;

            bool bFlagErrorDetected = false;
            CStatisticObject StatisticData = new CStatisticObject();

            if (oDataObject != null)
            {
                // Detect and process table custom optimize
                sTemp = DetectAndProcessCustomeOptimize(oDataObject);

                // Parse Extended property include ExtraOption, PRG_ValueIncluded
                m_TableRefOptionalFieldParser = new CTableRefOptionalFieldParser();
                sTemp = ParseOptionalField(sTemp, ref m_TableRefOptionalFieldParser);

                // Convert value before parse
                sTemp = m_TableRefValueConverter.ConvertTableRefValue(sTemp);

                // Verify value size byte before parse
                IGetInputTableRefObject InputObject = (IGetInputTableRefObject)oDataObject;
                uiValueSize = CBitUltilizer.ConvertSizeByteToSizeBit(InputObject.GetCIValueSizeByte());

                if (uiValueSize == 0)
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueSizeBytes");
                    bFlagErrorDetected = true;
                }

                if ((sTemp != null) && (uiValueSize != 0))
                {
                    sValueOptions = sTemp.Split(DELIMITER_CHAR_END_OF_FIELD);

                    if (sValueOptions[FIRST_LINE_IDX] != "")
                    {
                        if (sValueOptions[FIRST_LINE_IDX].Contains(FORMAT_SELECTION))
                        {
                            TableRef = TableRefSelectionParser(sValueOptions, uiValueSize);
                        }
                        else if (sValueOptions[FIRST_LINE_IDX].Contains(FORMAT_RANGE))
                        {
                            TableRef = TableRefRangeParser(sValueOptions, uiValueSize);
                        }
                        else if (sValueOptions[FIRST_LINE_IDX].Contains(FORMAT_ASCII))
                        {
                            TableRef = TableRefAsciiParser(sValueOptions, uiValueSize);
                        }
                        else if (sValueOptions[FIRST_LINE_IDX].Contains(FORMAT_READABLE_ASCII))
                        {
                            TableRef = TableRefReadableAsciiParser(sValueOptions, uiValueSize);
                        }    
                        else if (sValueOptions[FIRST_LINE_IDX].Contains(FORMAT_CUSTOM))
                        {
                            TableRef = TableRefCustomParser(sValueOptions, uiValueSize);
                        }

                        if (m_TableRefOptionalFieldParser.GetClassList() != null)
                        {
                            TableRef.SetUnavailableInterfaceClassList(m_TableRefOptionalFieldParser.GetClassList());
                        }
                    }
                }
                else
                {
                    if (sTemp == null)
                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: empty");
                    bFlagErrorDetected = true;
                }
            }
            oShareObject.SetTalbeRef(TableRef);

            if (bFlagErrorDetected == true)
            {
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
            }

            return bRet;
        }

        //========================================
        // INTERNAL FUNCTIONS
        //========================================
        private ATableRef TableRefSelectionParser(string[] sRawTableRef, uint uiValueSize)
        {
            CTableRefSelection TableRef = null;

            if (sRawTableRef != null)
            {
                if (sRawTableRef[FIRST_LINE_IDX].Contains(FORMAT_SELECTION))
                {
                    TableRef = new CTableRefSelection();
                    List<CTableRefSelection.VALUE_TYPE> TempValueList = new List<CTableRefSelection.VALUE_TYPE>();

                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(INDICATE_START) && element.Contains(INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                if ((sTempValue[KEYWORD_IDX] != "") && (sTempValue[VALUE_IDX] != ""))
                                {
                                    CTableRefSelection.VALUE_TYPE TempValue = new CTableRefSelection.VALUE_TYPE { sDescript = null, sValue = null };

                                    TempValue.sValue = sTempValue[0/*VALUE_IDX*/];
                                    TempValue.sDescript = sTempValue[1/*KEYWORD_IDX*/];
                                    TempValueList.Add(TempValue);
                                }
                            }
                        }
                    }

                    if (m_TableRefOptionalFieldParser.GetValueIsDecimalOption() == true)
                    {
                        TableRef.SetTableRefValueIsDecimal();
                    }

                    int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiValueSize.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);
                    byte[] byteArray = new byte[iSizeLen/2+1];

                    int i = 0;
                    for (i = 0; i < (iSizeLen/2); i++)
                    {
                        byteArray.SetValue((byte)0xFF, i);
                    }
                    byteArray.SetValue((byte)0x7F, i);

                    BigInteger biMinValue = new BigInteger(byteArray);
                    BigInteger biMaxValue = 0;

                    foreach (CTableRefSelection.VALUE_TYPE element in TempValueList)
                    {
                        if (element.sValue.Length <= iSizeLen)
                        {
                            string sValue = element.sValue.PadLeft(iSizeLen, '0');
                            TableRef.AddTableRefValue(element.sDescript, sValue);

                            // Check min and max
                            biMinValue = BigInteger.Min(biMinValue, BigInteger.Parse(element.sValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber));
                            biMaxValue = BigInteger.Max(biMaxValue, BigInteger.Parse(element.sValue.Insert(0, "0"), System.Globalization.NumberStyles.HexNumber));
                        }
                        else
                        {
                            // error detected !
                            return null;
                        }
                    }

                    string sMinValue = biMinValue.ToString("x");
                    if (biMinValue != 0)
                    {
                        sMinValue = sMinValue.TrimStart('0');
                    }
                    TableRef.SetTableRefMinValue(sMinValue);

                    string sMaxValue = biMaxValue.ToString("x");
                    if (biMaxValue != 0)
                    {
                        sMaxValue = sMaxValue.TrimStart('0');
                    }
                    TableRef.SetTableRefMaxValue(sMaxValue);

                    List<string> lsStatistic = new List<string>();
                    if (m_TableRefOptionalFieldParser.IsValueExtraOptionValid() == false)
                    {
                        lsStatistic.Add("Redundant extra option: some extra options cannot coexist.");
                    }
                    else
                    {
                        bool bAllValuesAcceptedOption = m_TableRefOptionalFieldParser.GetAllValuesAcceptedOption();
                        bool bAllValuesInRangeOption = m_TableRefOptionalFieldParser.GetAllValuesInRangeOption();
                        List<string> lsExtraValue = m_TableRefOptionalFieldParser.GetExtraValueList();
                        bool bIsExtraValueExist = (lsExtraValue != null);

                        if (bAllValuesAcceptedOption == true)
                        {
                            TableRef.SetAcceptAllValueProperty(true);
                        }

                        if (bAllValuesInRangeOption == true)
                        {
                            TableRef.SetAllValuesInRangeProperty(true);
                        }

                        if (bIsExtraValueExist == true)
                        {
                            TableRef.SetExtraValueList(lsExtraValue);
                        }
                    }

                    if (m_TableRefOptionalFieldParser.GetPRGValueIncludedOption() == CTableRefOptionalFieldParser.PRG_PROPERTY.ENUM_SELECTION)
                    {
                        lsStatistic.Add("Invalid value: PRG_ValueIncluded.");
                    }

                    if (m_TableRefOptionalFieldParser.GetAladdinHiddenValueOption() == true)
                    {
                        lsStatistic.Add("Redundant field: ExtraOption_AladdinHiddenValue.");
                    }
                    if (lsStatistic.Count > 0)
                    {
                        foreach (string sElement in lsStatistic)
                        {
                            CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sElement);
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                        }
                    }
                }
            }

            return (ATableRef)TableRef;
        }

        private ATableRef TableRefRangeParser(string[] sRawTableRef, uint uiValueSize)
        {
            ATableRef TableRef = null;
            bool bFlagConvert = false;
            string[] sValueExceptionList = null;

            if (sRawTableRef != null)
            {
                TableRef = new CTableRefRange();

                if (sRawTableRef[FIRST_LINE_IDX].Contains(FORMAT_RANGE))
                {
                    string MinMember = null;
                    string MaxMember = null;

                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(INDICATE_START) && element.Contains(INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                switch (sTempValue[KEYWORD_IDX])
                                {
                                    case KEYWORD_MIN_MEMBER:
                                        MinMember = sTempValue[VALUE_IDX];
                                        break;

                                    case KEYWORD_MAX_MEMBER:
                                        MaxMember = sTempValue[VALUE_IDX];
                                        break;

                                    case KEYWORD_STEP_MEMBER:
                                        ((CTableRefRange)TableRef).SetTableRefStepValue(sTempValue[VALUE_IDX]);
                                        break;

                                    case KEYWORD_EXCEPTION_MEMBER:
                                        bFlagConvert = true;
                                        sValueExceptionList = sTempValue[VALUE_IDX].Split(DELIMITER_CHAR_VALUE_WITH_VALUE);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    if (m_TableRefOptionalFieldParser.GetValueIsDecimalOption() == true)
                    {
                        TableRef.SetTableRefValueIsDecimal();
                    }

                    int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiValueSize.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);

                    if ((MinMember != null) && (MaxMember != null) &&
                        (MinMember.Length <= iSizeLen) &&
                        (MaxMember.Length <= iSizeLen) )
                    {
                        ((CTableRefRange)TableRef).SetTableRefMinValue(MinMember);
                        ((CTableRefRange)TableRef).SetTableRefMaxValue(MaxMember);
                    }
                    else
                    {
                        // error detected;
                        return null;
                    }

                    if (bFlagConvert)
                    {
                        TableRef = TableRefConvertRangeToSelection((CTableRefRange)TableRef, sValueExceptionList, uiValueSize);
                    }

                    List<string> lsStatistic = new List<string>();
                    if (m_TableRefOptionalFieldParser.IsValueExtraOptionValid() == false)
                    {
                        lsStatistic.Add("Redundant extra option: some extra options cannot coexist.");
                    }
                    else
                    {
                        bool bAllValuesAcceptedOption = m_TableRefOptionalFieldParser.GetAllValuesAcceptedOption();
                        bool bAllValuesInRangeOption = m_TableRefOptionalFieldParser.GetAllValuesInRangeOption();
                        List<string> lsExtraValue = m_TableRefOptionalFieldParser.GetExtraValueList();
                        bool bIsExtraValueExist = (lsExtraValue != null);

                        if (bFlagConvert)
                        {
                            if (bAllValuesAcceptedOption == true)
                            {
                                ((CTableRefSelection)TableRef).SetAcceptAllValueProperty(true);
                            }

                            if (bAllValuesInRangeOption == true)
                            {
                                ((CTableRefSelection)TableRef).SetAllValuesInRangeProperty(true);
                            }

                            if (bIsExtraValueExist == true)
                            {
                                ((CTableRefSelection)TableRef).SetExtraValueList(lsExtraValue);
                            }
                        }
                        else
                        {
                            if (bAllValuesAcceptedOption == true)
                            {
                                ((CTableRefRange)TableRef).SetAcceptAllValueProperty(true);
                            }

                            if (bAllValuesInRangeOption == true)
                            {
                                lsStatistic.Add("Redundant field: ExtraOption_AllValuesInRange.");
                            }

                            if (bIsExtraValueExist == true)
                            {
                                ((CTableRefRange)TableRef).SetExtraValueList(lsExtraValue);
                            }
                        }
                    }

                    if (m_TableRefOptionalFieldParser.GetPRGValueIncludedOption() == CTableRefOptionalFieldParser.PRG_PROPERTY.ENUM_TRUE)
                    {
                        lsStatistic.Add("Invalid value: PRG_ValueIncluded.");
                    }

                    if (m_TableRefOptionalFieldParser.GetAladdinHiddenValueOption() == true)
                    {
                        lsStatistic.Add("Redundant field: ExtraOption_AladdinHiddenValue.");
                    }

                    if (lsStatistic.Count > 0)
                    {
                        foreach (string sElement in lsStatistic)
                        {
                            CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sElement);
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                        }
                    }
                }

            }
            return (ATableRef)TableRef;
        }

        private ATableRef TableRefConvertRangeToSelection(CTableRefRange TableRefInput, string[] sValueExceptionList, uint uiValueSize)
        {
            CTableRefSelection TableRefOut = null;
            int iMinValue = 0;
            int iMaxValue = 0;
            int iStep = 0;

            try
            {
                iMinValue = int.Parse(TableRefInput.GetTableRefMinValue(), System.Globalization.NumberStyles.HexNumber);
                iMaxValue = int.Parse(TableRefInput.GetTableRefMaxValue(), System.Globalization.NumberStyles.HexNumber);
                iStep     = int.Parse(TableRefInput.GetTableRefStepValue(), System.Globalization.NumberStyles.HexNumber);
            }
            catch { }

            List<int> iValueExceptionList = new List<int>();
            foreach (string element in sValueExceptionList)
            {
                if (element != null)
                {
                    try
                    {
                        iValueExceptionList.Add(int.Parse(element, System.Globalization.NumberStyles.HexNumber));
                    }
                    catch { }
                }
            }

            if ((iMinValue < iMaxValue) && (iValueExceptionList.Count > 0))
            {
                TableRefOut = new CTableRefSelection();
                int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiValueSize.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);
                for (int i = iMinValue; i <= iMaxValue;)
                {
                    foreach (int element in iValueExceptionList)
                    {
                        if (element == i)
                        {
                            goto JumpTo;
                        }
                    }
                    TableRefOut.AddTableRefValue(i.ToString(), i.ToString("X").PadLeft(iSizeLen, '0'));

                JumpTo:;
                    i += iStep;
                }

                TableRefOut.SetTableRefMinValue(TableRefInput.GetTableRefMinValue());
                TableRefOut.SetTableRefMaxValue(TableRefInput.GetTableRefMaxValue());

                return TableRefOut;
            }

            return TableRefInput;
        }

        private ATableRef TableRefAsciiParser(string[] sRawTableRef, uint uiValueSize)
        {
            CTableRefAscii TableRef = null;

            if ((sRawTableRef != null))
            {
                TableRef = new CTableRefAscii();

                TableRef.SetTableRefMinValue(ATableRef.MIN_STRING_DEFAULT);
                TableRef.SetTableRefMaxValue(ATableRef.MAX_STRING_DEFAULT);
                TableRef.SetTableRefPaddingValue(ATableRef.PADDING_DEFAULT);

                if (uiValueSize == 8/*8Bit*/)
                {
                    TableRef.SetTableRefMinValue(ATableRef.MIN_CHAR_DEFAULT);
                    TableRef.SetTableRefMaxValue(ATableRef.MAX_CHAR_DEFAULT);
                }

                if (sRawTableRef[FIRST_LINE_IDX].Contains(FORMAT_ASCII))
                {
                    string MinMember = null;
                    string MaxMember = null;

                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(INDICATE_START) && element.Contains(INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                switch (sTempValue[KEYWORD_IDX])
                                {
                                    case KEYWORD_PADDING_MEMBER:
                                        // Padding must be 1 byte
                                        if (sTempValue[VALUE_IDX].Length == 1)
                                        {
                                            sTempValue[VALUE_IDX] += sTempValue[VALUE_IDX];
                                        }

                                        ((CTableRefAscii)TableRef).SetTableRefPaddingValue(sTempValue[VALUE_IDX]);
                                        break;

                                    case KEYWORD_MIN_MEMBER:
                                        MinMember = sTempValue[VALUE_IDX];
                                        break;

                                    case KEYWORD_MAX_MEMBER:
                                        MaxMember = sTempValue[VALUE_IDX];
                                        break;

                                    case KEYWORD_STEP_MEMBER:
                                        ((CTableRefAscii)TableRef).SetTableRefStepValue(sTempValue[VALUE_IDX]);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    if ((MinMember != null) && (MaxMember != null))
                    {
                        int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiValueSize.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);

                        if ((MinMember.Length <= iSizeLen) &&
                            (MaxMember.Length <= iSizeLen) )
                        {
                            ((CTableRefAscii)TableRef).SetTableRefMinValue(MinMember);
                            ((CTableRefAscii)TableRef).SetTableRefMaxValue(MaxMember);
                        }
                        else
                        {
                            // error detected;
                            return null;
                        }
                    }

                    if (m_TableRefOptionalFieldParser.GetAladdinHiddenValueOption() == true)
                    {
                        TableRef.SetTableRefValueIsHidden(true);
                    }

                    if (m_TableRefOptionalFieldParser.GetValueIsDecimalOption() == true)
                    {
                        TableRef.SetTableRefValueIsDecimal();
                    }

                    if (m_TableRefOptionalFieldParser.GetPRGValueIncludedOption() == CTableRefOptionalFieldParser.PRG_PROPERTY.ENUM_TRUE)
                    {
                        string sStatistic = "Invalid value: PRG_ValueIncluded.";
                        CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                    }
                }

            }
            return (ATableRef)TableRef;
        }

        private ATableRef TableRefReadableAsciiParser(string[] sRawTableRef, uint uiValueSize)
        {
            CTableRefReadableAscii TableRef = null;
            string[] sValueExceptionList = null;

            if ((sRawTableRef != null))
            {
                TableRef = new CTableRefReadableAscii();

                TableRef.SetTableRefMinValue(ATableRef.MIN_STRING_READABLE_DEFAULT);
                TableRef.SetTableRefMaxValue(ATableRef.MAX_STRING_READABLE_DEFAULT);
                TableRef.SetTableRefPaddingValue(ATableRef.PADDING_DEFAULT);

                if (uiValueSize == 8/*8Bit*/)
                {
                    TableRef.SetTableRefMinValue(ATableRef.MIN_CHAR_READABLE_DEFAULT);
                    TableRef.SetTableRefMaxValue(ATableRef.MAX_CHAR_READABLE_DEFAULT);
                }

                if (sRawTableRef[FIRST_LINE_IDX].Contains(FORMAT_READABLE_ASCII))
                {
                    string MinMember = null;
                    string MaxMember = null;

                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(INDICATE_START) && element.Contains(INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                switch (sTempValue[KEYWORD_IDX])
                                {
                                    case KEYWORD_PADDING_MEMBER:
                                        // Padding must be 1 byte
                                        if (sTempValue[VALUE_IDX].Length == 1)
                                        {
                                            sTempValue[VALUE_IDX] += sTempValue[VALUE_IDX];
                                        }

                                        ((CTableRefReadableAscii)TableRef).SetTableRefPaddingValue(sTempValue[VALUE_IDX]);
                                        break;

                                    case KEYWORD_MIN_MEMBER:
                                        MinMember = sTempValue[VALUE_IDX];
                                        break;

                                    case KEYWORD_MAX_MEMBER:
                                        MaxMember = sTempValue[VALUE_IDX];
                                        break;

                                    case KEYWORD_EXCEPTION_MEMBER:
                                        sValueExceptionList = sTempValue[VALUE_IDX].Split(DELIMITER_CHAR_VALUE_WITH_VALUE);
                                        TableRef = TableRefParseReadableAscii((CTableRefReadableAscii)TableRef, sValueExceptionList, uiValueSize);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    if ((MinMember != null) && (MaxMember != null))
                    {
                        int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiValueSize.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);

                        if ((MinMember.Length <= iSizeLen) &&
                            (MaxMember.Length <= iSizeLen))
                        {
                            ((CTableRefReadableAscii)TableRef).SetTableRefMinValue(MinMember);
                            ((CTableRefReadableAscii)TableRef).SetTableRefMaxValue(MaxMember);
                        }
                        else
                        {
                            // error detected;
                            return null;
                        }
                    }       

                    if (m_TableRefOptionalFieldParser.GetAladdinHiddenValueOption() == true)
                    {
                        TableRef.SetTableRefValueIsHidden(true);
                    }

                    if (m_TableRefOptionalFieldParser.GetPRGValueIncludedOption() == CTableRefOptionalFieldParser.PRG_PROPERTY.ENUM_TRUE)
                    {
                        string sStatistic = "Invalid value: PRG_ValueIncluded.";
                        CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                    }
                }

            }
            return (ATableRef)TableRef;
        }

        private CTableRefReadableAscii TableRefParseReadableAscii(CTableRefReadableAscii TableRefInput, string[] sValueExceptionList, uint uiValueSize)
        {
            int iMinValue = 0;
            int iMaxValue = 0;

            try
            {
                iMinValue = int.Parse(TableRefInput.GetTableRefMinValue(), System.Globalization.NumberStyles.HexNumber);
                iMaxValue = int.Parse(TableRefInput.GetTableRefMaxValue(), System.Globalization.NumberStyles.HexNumber);
            }
            catch { }

            List<int> iValueExceptionList = new List<int>();
            foreach (string element in sValueExceptionList)
            {
                if (element != null)
                {
                    try
                    {
                        iValueExceptionList.Add(int.Parse(element, System.Globalization.NumberStyles.HexNumber));
                    }
                    catch { }
                }
            }

            if ((iMinValue < iMaxValue) && (iValueExceptionList.Count > 0))
            {
                int iSizeLen = (int)CBitUltilizer.ConvertSizeBitToSizeAppSupport(uiValueSize.ToString(), CConfigTool.GetInstance().SIZE_BIT_SUPPORT);
                for (int i = iMinValue; i <= iMaxValue; i += 1)
                {
                    bool bCheck = iValueExceptionList.Contains(i);
                    if (bCheck)
                    {
                        TableRefInput.AddValueExceptionReadableAscii(i.ToString("X")/*.PadLeft(iSizeLen, '0')*/);
                    }
                }

                TableRefInput.SetTableRefMinValue(TableRefInput.GetTableRefMinValue());
                TableRefInput.SetTableRefMaxValue(TableRefInput.GetTableRefMaxValue());

                return TableRefInput;
            }
            return TableRefInput;
        }

        private ATableRef TableRefCustomParser(string[] sRawTableRef, uint uiValueSize)
        {
            CTableRefCustom TableRef = null;

            bool bStartStopField = false;
            List<string> sFieldList = null;

            if ((sRawTableRef != null) && (uiValueSize != 0))
            {
                TableRef = new CTableRefCustom();
                sFieldList = new List<string>();
                string sTemp = null;
                string sMinValue = null;
                string sMaxValue = null;

                uint uiValueSizeInByte = uiValueSize / 8;
                for (int i = 0; i < uiValueSizeInByte; i++)
                {
                    sMinValue += ATableRef.MIN_CHAR_DEFAULT;
                    sMaxValue += ATableRef.MAX_CHAR_DEFAULT;
                }

                TableRef.SetTableRefMinValue(sMinValue);
                TableRef.SetTableRefMaxValue(sMaxValue);

                if (sRawTableRef[FIRST_LINE_IDX].Contains(FORMAT_CUSTOM))
                {
                    foreach (string RawElement in sRawTableRef)
                    {
                        if (RawElement.Contains(FIELD_START))
                        {
                            bStartStopField = true;
                            sTemp = null;
                        }
                        else if (RawElement.Contains(FIELD_STOP))
                        {
                            bStartStopField = false;
                            sTemp += RawElement + DELIMITER_CHAR_END_OF_FIELD;
                            sFieldList.Add(sTemp);
                        }

                        if (bStartStopField)
                        {
                            sTemp += RawElement + DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }

                    foreach (string FieldElement in sFieldList)
                    {
                        string sSubName = null;
                        string sSubSize = null;
                        string sSubContext = null;
                        ATableRef sSubTableRef = null;

                        if (FieldElement != null)
                        {
                            sSubTableRef = TableRefCustomFieldParser(FieldElement, ref sSubName, ref sSubSize, ref sSubContext);
                            if ((sSubName != null) && (sSubSize != null) && (sSubContext != null) && (sSubTableRef != null))
                            {
                                TableRef.AddSubTableRef(sSubName, sSubSize, sSubContext, sSubTableRef);
                            }
                            else
                            {
                                // TODO
                            }
                        }
                    }
                    if (m_TableRefOptionalFieldParser.GetValueIsDecimalOption() == true)
                    {
                        TableRef.SetTableRefValueIsDecimal();
                    }
                }
            }
            return (ATableRef)TableRef;
        }

        private ATableRef TableRefCustomFieldParser(string sField, ref string sSubName, ref string sSubSize, ref string sSubContext)
        {
            ATableRef sSubTableRef = null;
            bool bStartStopField = false;
            string sSubFormat = null;
            uint uiSubSize = 0;

            string[] sTempSource = sField.Split(DELIMITER_CHAR_END_OF_FIELD);
            foreach (string Element in sTempSource)
            {
                if (Element.Contains(FIELD_START))
                {
                    sSubName = Element.Split(DELIMITER_CHAR_INDICATOR_WITH_VALUE)[1];
                    sSubName = sSubName.Trim(']');
                    bStartStopField = true;
                    continue;
                }
                else if (Element.Contains(FIELD_STOP))
                {
                    break;
                }

                if (bStartStopField)
                {
                    if (Element.Contains(KEYWORD_SUB_SIZE_MEMBER))
                    {
                        sSubSize = Element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE)[1];
                        try
                        {
                            uiSubSize = uint.Parse(CHexIntConverter.StringConvertHexToInt(sSubSize));
                            if ((uiSubSize % MINIMUM_SIZE_SUPPORT_IN_BIT) != 0)
                            {
                                string sStatistic = "Size of single parameter is invalid: size is not multiple of " + 
                                                    MINIMUM_SIZE_SUPPORT_IN_BIT.ToString() +
                                                    " bits";
                                CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                            }
                        }
                        catch { }
                    }
                    else if (Element.Contains(KEYWORD_SUB_CONTEXT_MEMBER))
                    {
                        sSubContext = Element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE)[1];
                    }
                    else
                    {
                        sSubFormat += Element + DELIMITER_CHAR_END_OF_FIELD;
                    }
                }
            }
            
            if ((sSubFormat != "") && (uiSubSize != 0))
            {
                if (sSubFormat.Contains(FORMAT_SELECTION))
                {
                    string[] sTempList = sSubFormat.Split(DELIMITER_CHAR_END_OF_FIELD);
                    sSubTableRef = TableRefSelectionParser(sTempList, uiSubSize);
                }
                else if (sSubFormat.Contains(FORMAT_RANGE))
                {
                    string[] sTempList = sSubFormat.Split(DELIMITER_CHAR_END_OF_FIELD);
                    sSubTableRef = TableRefRangeParser(sTempList, uiSubSize);
                }
                else if (sSubFormat.Contains(FORMAT_ASCII))
                {
                    string[] sTempList = sSubFormat.Split(DELIMITER_CHAR_END_OF_FIELD);
                    sSubTableRef = TableRefAsciiParser(sTempList, uiSubSize);
                }
            }

            return sSubTableRef;
        }

        private string DetectAndProcessCustomeOptimize(IShareObject oDataObject)
        {
            string sRet = null;
            string sValue = (oDataObject as IGetInputTableRefObject).GetCIValueOptions();
            if ((sValue != null) && (sValue != string.Empty))
            {
                List<List<string>> sRefContentList = new List<List<string>>();
                List<string> lsRefContent = null;
                List<string> lsInput = sValue.Split(DELIMITER_CHAR_END_OF_FIELD).ToList();
                List<string> lsWorkingInput = sValue.Split(DELIMITER_CHAR_END_OF_FIELD).ToList();

                if (lsInput.First().Contains(FORMAT_CUSTOM_OPTIMIZE) == false)
                {
                    return sValue;
                }

                // update all common selection table to ref list
                bool bStartStopField = false;

                foreach (string RawElement in lsInput)
                {
                    if ((RawElement != null) && (RawElement != string.Empty))
                    {
                        if (RawElement.Contains(SELECTION_OPTIMIZE_START))
                        {
                            lsRefContent = new List<string>();
                            bStartStopField = true;
                        }
                        else if (RawElement.Contains(SELECTION_OPTIMIZE_STOP))
                        {
                            bStartStopField = false;
                            sRefContentList.Add(lsRefContent);

                            // Already copy the reference selection => Remove from intput
                            lsWorkingInput.RemoveRange(0, lsRefContent.Count + 1);
                        }
                        else if (RawElement.Contains(FORMAT_CUSTOM))
                        {
                            break;
                        }

                        if (bStartStopField)
                        {
                            lsRefContent.Add(RawElement);
                        }
                    }
                }

                lsInput.Clear();
                lsInput.AddRange(lsWorkingInput);

                // replace common table to reference point in raw data
                foreach (string LineElement in lsInput)
                {
                    if ((LineElement != null) && (LineElement != string.Empty))
                    {
                        if (LineElement.Contains(SELECTION_OPTIMIZE_START) == true)
                        {
                            foreach (List<string> RefContentElement in sRefContentList)
                            {
                                if ((RefContentElement != null) && (RefContentElement.Count > 0))
                                {
                                    // check if ref point match with common ref table
                                    if (LineElement == RefContentElement.First())
                                    {
                                        // Replace new content
                                        lsWorkingInput.InsertRange(lsWorkingInput.IndexOf(LineElement), RefContentElement);

                                        // Remove current mark
                                        lsWorkingInput.Remove(LineElement);

                                        // Remove marking of replacement
                                        lsWorkingInput.Remove(RefContentElement.First());
                                        lsWorkingInput.Remove(RefContentElement.Last());
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if ((lsWorkingInput != null) && (lsWorkingInput.Count > 0))
                {
                    foreach (string LineElement in lsWorkingInput)
                    {
                        if ((LineElement != null) && (LineElement != string.Empty))
                        {
                            sRet += LineElement + DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }
                }
            }

            return sRet;
        }

        private string ParseOptionalField(string sRawData, ref CTableRefOptionalFieldParser OptionalParser)
        {
            string sReturn = null;

            if (sRawData != null)
            {
                bool bExtraValuesAccepted = false;
                bool bNotAvailableFor = false;
                bool bOnlyAvailableFor = false;
                string[] sValueOptions = sRawData.Split(DELIMITER_CHAR_END_OF_FIELD);
                foreach (string element in sValueOptions)
                {
                    if (element.Contains(KEYWORD_EXTRA_OPTION))
                    {
                        string[] sExtraOption = element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                        if (sExtraOption.Length == 2)
                        {
                            switch (sExtraOption[VALUE_IDX].Trim(INDICATE_STOP.ToCharArray()))
                            {
                                case KEYWORD_ALL_VALUES_ACCEPTED:
                                    {
                                        OptionalParser.SetAllValuesAcceptedOption(true);
                                    }
                                    break;

                                case KEYWORD_EXTRA_VALUES_ACCEPTED:
                                    {
                                        bExtraValuesAccepted = true;
                                    }
                                    break;

                                case KEYWORD_ALL_VALUES_IN_RANGE:
                                    {
                                        OptionalParser.SetAllValuesInRangeOption(true);
                                    }
                                    break;

                                case KEYWORD_ALADDIN_HIDDEN_VALUE:
                                    {
                                        OptionalParser.SetAladdinHiddenValueOption(true);
                                    }
                                    break;

                                case KEYWORD_NOT_AVAILABLE_FOR:
                                    {
                                        bNotAvailableFor = true;
                                    }
                                    break;

                                case KEYWORD_ONLY_AVAILABLE_FOR:
                                    {
                                        bOnlyAvailableFor = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else if (element.Contains(KEYWORD_CLASSES_MEMBER))
                    {
                        if ((bNotAvailableFor == true) && (bOnlyAvailableFor == true))
                        {
                            string sStatistic = "Invalid logic: NotAvailableFor and OnlyAvailableFor cannot coexist.";
                            CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                        }
                        else
                        {
                            if ((bNotAvailableFor == true) || (bOnlyAvailableFor == true))
                            {
                                string[] sTempValue = element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                                if ((sTempValue.Length == 2) && (sTempValue[VALUE_IDX] != null))
                                {
                                    string[] sClassValues = sTempValue[VALUE_IDX].Split(DELIMITER_CHAR_VALUE_WITH_VALUE);
                                    List<uint> AvailableInterfaceList = new List<uint>();

                                    foreach (string sClass in sClassValues)
                                    {
                                        if (System.Text.RegularExpressions.Regex.IsMatch(sClass, @"\A\b[0-9]+\b\Z"))
                                        {
                                            uint uiClass = uint.Parse(sClass.Trim());
                                            if (m_AvailableClassList.Contains(uiClass) == true)
                                            {
                                                if (bNotAvailableFor == true)
                                                {
                                                    OptionalParser.AddClassInterface(uiClass);
                                                }
                                                else
                                                {
                                                    AvailableInterfaceList.Add(uiClass);
                                                }
                                            }
                                            else
                                            {
                                                string sStatistic = "Invalid value: incorrect interface class.";
                                                CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                            }
                                        }
                                        else
                                        {
                                            string sStatistic = "Invalid value: incorrect interface class.";
                                            CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                        }
                                    }

                                    if (AvailableInterfaceList.Count != 0)
                                    {
                                        foreach (uint uiClass in m_AvailableClassList)
                                        {
                                            if (AvailableInterfaceList.Contains(uiClass) == false)
                                            {
                                                OptionalParser.AddClassInterface(uiClass);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string sStatistic = "Incorrect syntax: missing " + KEYWORD_EXTRA_OPTION + "field.";
                                CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                            }
                        }
                    }
                    else if (element.Contains(KEYWORD_VALUES))
                    {
                        if (bExtraValuesAccepted == true)
                        {
                            string[] sTempValue = element.Split(DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if ((sTempValue.Length == 2) && (sTempValue[VALUE_IDX] != null))
                            {
                                string[] sValuesList = sTempValue[VALUE_IDX].Split(DELIMITER_CHAR_VALUE_WITH_VALUE);
                                foreach (string sValue in sValuesList)
                                {
                                    string sRet = null;
                                    if (CheckAndConvertValue(sValue, out sRet) == true)
                                    {
                                        OptionalParser.AddExtraValue(sRet);
                                    }
                                    else
                                    {
                                        string sStatistic = "Invalid value: incorrect format.";
                                        CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string sStatistic = "Incorrect syntax: missing " + KEYWORD_EXTRA_OPTION + "field.";
                            CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                        }
                    }
                    else if ((element.Contains(KEYWORD_PRG_VALUE_INCLUDED)))
                    {
                        if (element.Contains(VALUE_TRUE))
                        {
                            OptionalParser.SetPRGValueIncludedOption(CTableRefOptionalFieldParser.PRG_PROPERTY.ENUM_TRUE);
                        }
                        else if (element.Contains(VALUE_FALSE))
                        {
                            OptionalParser.SetPRGValueIncludedOption(CTableRefOptionalFieldParser.PRG_PROPERTY.ENUM_FALSE);
                        }
                        else if (element.Contains(VALUE_SELECTION))
                        {
                            OptionalParser.SetPRGValueIncludedOption(CTableRefOptionalFieldParser.PRG_PROPERTY.ENUM_SELECTION);
                        }
                    }
                    else if (element.Contains(KEYWORD_VALUE_SEND))
                    {
                        if (element.Contains(VALUE_TYPE_INT))
                        {
                            OptionalParser.SetValueIsDecimalValue(true);
                        }
                    }
                    else
                    {
                        sReturn += element + DELIMITER_CHAR_END_OF_FIELD;
                    }
                }
            }
            return sReturn;
        }

        private bool CheckAndConvertValue(string sValue, out string sResult)
        {
            bool bRet = true;
            string sRet;
            sResult = null;

            if (string.IsNullOrEmpty(sValue))
            {
                return false;
            }

            sValue = sValue.Replace("\r", "");
            sValue = sValue.Replace("\n", "");

            if (CHexIntConverter.DetectAndTrimHexPrefix(sValue, out sRet) == true)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(sRet, @"\A\b[0-9a-fA-F]+\b\Z"))
                {
                    sRet = null;
                    bRet = false;
                }
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(sValue, @"\A\b[0-9]+\b\Z"))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(sValue, @"\A\b[0-9a-fA-F]+\b\Z"))
                    {
                        sValue = "0x" + sValue;
                    }
                    else
                    {
                        sValue = null;
                        bRet = false;
                    }
                }

                sRet = CHexIntConverter.FormatToHexString(sValue);
            }
            sResult = sRet;
            return bRet;
        }
    }
}