using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    public class CTableRefValueConverter
    {
        public CTableRefValueConverter()
        {
        }

        public string ConvertTableRefValue(string sRawData)
        {
            string sRet = null;
            string[] sValueOptions = null;

            if (sRawData != null)
            {
                sValueOptions = sRawData.Split(CTableRefParser.DELIMITER_CHAR_END_OF_FIELD);

                if (sValueOptions[CTableRefParser.FIRST_LINE_IDX] != "")
                {
                    if (sValueOptions[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_SELECTION))
                    {
                        sRet = ConvertTableRefSelectionValue(sValueOptions);
                    }
                    else if (sValueOptions[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_RANGE))
                    {
                        sRet = ConvertTableRefRangeValue(sValueOptions);
                    }
                    else if (sValueOptions[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_ASCII))
                    {
                        sRet = ConvertTableRefAsciiValue(sValueOptions);
                    }
                    else if (sValueOptions[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_READABLE_ASCII))
                    {
                        sRet = ConvertTableRefReadableAsciiValue(sValueOptions);
                    }    
                    else if (sValueOptions[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_CUSTOM))
                    {
                        sRet = ConvertTableRefCustomValue(sValueOptions);
                    }
                }
            }
            return sRet;
        }

        private string ConvertTableRefSelectionValue(string[] sRawTableRef)
        {
            string sRet = null;

            if (sRawTableRef != null)
            {
                if (sRawTableRef[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_SELECTION))
                {
                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(CTableRefParser.INDICATE_START) && element.Contains(CTableRefParser.INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                string sTemp = CheckAndConvertTableRefValue(sTempValue[0/*VALUE_IDX*/]);
                                sRet += sTemp + CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE + sTempValue[1/*KEYWORD_IDX*/] + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                            }
                            else
                            {
                                sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                            }
                        }
                        else
                        {
                            sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }
                }
            }

            return sRet;
        }

        private string ConvertTableRefRangeValue(string[] sRawTableRef)
        {
            string sRet = null;

            if (sRawTableRef != null)
            {
                if (sRawTableRef[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_RANGE))
                {
                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(CTableRefParser.INDICATE_START) && element.Contains(CTableRefParser.INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                switch (sTempValue[CTableRefParser.KEYWORD_IDX])
                                {
                                    // Only convert for this kinds of KEYWORDs.
                                    case CTableRefParser.KEYWORD_STEP_MEMBER:
                                    case CTableRefParser.KEYWORD_MAX_MEMBER:
                                    case CTableRefParser.KEYWORD_MIN_MEMBER:
                                    case CTableRefParser.KEYWORD_EXCEPTION_MEMBER:
                                        {
                                            sRet += sTempValue[CTableRefParser.KEYWORD_IDX] + CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE;
                                            string[] sValueList = sTempValue[CTableRefParser.VALUE_IDX].Split(CTableRefParser.DELIMITER_CHAR_VALUE_WITH_VALUE);
                                            foreach (string element1 in sValueList)
                                            {
                                                if (element1 != null)
                                                {
                                                    try
                                                    {
                                                        string TempNumber = CheckAndConvertTableRefValue(element1);
                                                        sRet += TempNumber;
                                                        if (!element1.Equals(sValueList.Last()))
                                                        {
                                                            sRet += CTableRefParser.DELIMITER_CHAR_VALUE_WITH_VALUE;
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }
                                            sRet += CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                            }
                        }
                        else
                        {
                            sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }
                }

            }

            return sRet;
        }

        private string ConvertTableRefAsciiValue(string[] sRawTableRef, string sValueSize = "1")
        {
            string sRet = null;

            if (sRawTableRef != null)
            {
                if (sRawTableRef[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_ASCII))
                {
                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(CTableRefParser.INDICATE_START) && element.Contains(CTableRefParser.INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                switch (sTempValue[CTableRefParser.KEYWORD_IDX])
                                {
                                    // Only convert for this kinds of KEYWORDs.
                                    case CTableRefParser.KEYWORD_PADDING_MEMBER:
                                    case CTableRefParser.KEYWORD_MAX_MEMBER:
                                    case CTableRefParser.KEYWORD_MIN_MEMBER:
                                        {
                                            try
                                            {
                                                string sTemp = CheckAndConvertTableRefValue(sTempValue[CTableRefParser.VALUE_IDX]);
                                                sRet += sTempValue[CTableRefParser.KEYWORD_IDX] + CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE + sTemp + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                                            }
                                            catch { }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                            }
                        }
                        else
                        {
                            sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }
                }
            }

            return sRet;
        }

        private string ConvertTableRefReadableAsciiValue(string[] sRawTableRef, string sValueSize = "1")
        {
            string sRet = null;

            if (sRawTableRef != null)
            {
                if (sRawTableRef[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_READABLE_ASCII))
                {
                    foreach (string element in sRawTableRef)
                    {
                        if (!(element.Contains(CTableRefParser.INDICATE_START) && element.Contains(CTableRefParser.INDICATE_STOP)))
                        {
                            string[] sTempValue = element.Split(CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE);
                            if (sTempValue.Length == 2)
                            {
                                switch (sTempValue[CTableRefParser.KEYWORD_IDX])
                                {
                                    // Only convert for this kinds of KEYWORDs.
                                    case CTableRefParser.KEYWORD_PADDING_MEMBER:
                                    case CTableRefParser.KEYWORD_MAX_MEMBER:
                                    case CTableRefParser.KEYWORD_MIN_MEMBER:
                                        {
                                            try
                                            {
                                                string sTemp = CheckAndConvertTableRefValue(sTempValue[CTableRefParser.VALUE_IDX]);
                                                sRet += sTempValue[CTableRefParser.KEYWORD_IDX] + CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE + sTemp + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                                            }
                                            catch { }
                                        }
                                        break;
                                    case CTableRefParser.KEYWORD_EXCEPTION_MEMBER:
                                        {
                                            sRet += sTempValue[CTableRefParser.KEYWORD_IDX] + CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE;
                                            string[] sValueList = sTempValue[CTableRefParser.VALUE_IDX].Split(CTableRefParser.DELIMITER_CHAR_VALUE_WITH_VALUE);
                                            foreach (string element1 in sValueList)
                                            {
                                                if (element1 != null)
                                                {
                                                    try
                                                    {
                                                        string TempNumber = CheckAndConvertTableRefValue(element1);
                                                        sRet += TempNumber;
                                                        if (!element1.Equals(sValueList.Last()))
                                                        {
                                                            sRet += CTableRefParser.DELIMITER_CHAR_VALUE_WITH_VALUE;
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }
                                            sRet += CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                            }
                        }
                        else
                        {
                            sRet += element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }
                }
            }

            return sRet;
        }

        private string ConvertTableRefCustomValue(string[] sRawTableRef)
        {
            string sRet = null;
            bool bStartStopField = false;
            List<string> sFieldList = null;

            if (sRawTableRef != null)
            {
                string sTemp = null;
                sFieldList = new List<string>();

                if (sRawTableRef[CTableRefParser.FIRST_LINE_IDX].Contains(CTableRefParser.FORMAT_CUSTOM))
                {
                    sRet += sRawTableRef[CTableRefParser.FIRST_LINE_IDX] + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;

                    foreach (string RawElement in sRawTableRef)
                    {
                        if (RawElement.Contains(CTableRefParser.FIELD_START))
                        {
                            bStartStopField = true;
                            sTemp = null;
                        }
                        else if (RawElement.Contains(CTableRefParser.FIELD_STOP))
                        {
                            bStartStopField = false;
                            sTemp += RawElement + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                            sFieldList.Add(sTemp);
                        }

                        if (bStartStopField)
                        {
                            sTemp += RawElement + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }

                    foreach (string FieldElement in sFieldList)
                    {
                        if (FieldElement != null)
                        {
                            sRet += TableRefCustomFieldParser(FieldElement) + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                        }
                    }
                }
            }

            return sRet;
        }

        private string TableRefCustomFieldParser(string sField)
        {
            string  sRet = null;
            bool    bStartStopField = false;
            string  sSubFormat = null;
            string  sEndField = null;
            string  sTemp = "";

            string[] sTempSource = sField.Split(CTableRefParser.DELIMITER_CHAR_END_OF_FIELD);
            foreach (string Element in sTempSource)
            {
                if (Element.Contains(CTableRefParser.FIELD_START))
                {
                    sRet += Element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                    bStartStopField = true;
                    continue;
                }
                else if (Element.Contains(CTableRefParser.FIELD_STOP))
                {
                    sEndField = Element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                    break;
                }

                if (bStartStopField)
                {
                    if (Element.Contains(CTableRefParser.KEYWORD_SUB_SIZE_MEMBER))
                    {
                        sTemp = CheckAndConvertTableRefValue(Element.Split(CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE)[1]);
                        sRet += Element.Split(CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE)[0] + CTableRefParser.DELIMITER_CHAR_KEYWORD_WITH_VALUE + sTemp + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                    }
                    else if (Element.Contains(CTableRefParser.KEYWORD_SUB_CONTEXT_MEMBER))
                    {
                        sRet += Element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                    }
                    else
                    {
                        sSubFormat += Element + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;
                    }
                }
            }

            if (sSubFormat != "")
            {
                if (sSubFormat.Contains(CTableRefParser.FORMAT_SELECTION))
                {
                    string[] sTempList = sSubFormat.Split(CTableRefParser.DELIMITER_CHAR_END_OF_FIELD);
                    sRet += ConvertTableRefSelectionValue(sTempList);
                }
                else if (sSubFormat.Contains(CTableRefParser.FORMAT_RANGE))
                {
                    string[] sTempList = sSubFormat.Split(CTableRefParser.DELIMITER_CHAR_END_OF_FIELD);
                    sRet += ConvertTableRefRangeValue(sTempList);
                }
                else if (sSubFormat.Contains(CTableRefParser.FORMAT_ASCII))
                {
                    string[] sTempList = sSubFormat.Split(CTableRefParser.DELIMITER_CHAR_END_OF_FIELD);
                    sRet += ConvertTableRefAsciiValue(sTempList);
                }
            }

            sRet += sEndField + CTableRefParser.DELIMITER_CHAR_END_OF_FIELD;

            return sRet;
        }

        public string CheckAndConvertTableRefValue(string sValue)
        {
            string sRet;

            if (string.IsNullOrEmpty(sValue))
            {
                // Error detected
                CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: empty");
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                return null;
            }

            sValue = sValue.Replace("\r", "");
            sValue = sValue.Replace("\n", "");

            if (CHexIntConverter.DetectAndTrimHexPrefix(sValue, out sRet) == true)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(sRet, @"\A\b[0-9a-fA-F]+\b\Z"))
                {
                    // Error detected
                    CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_HEX_DATA_FORMAT, "CI_ValueOptions");
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);

                    sRet = null;
                }
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(sValue, @"\A\b[0-9]+\b\Z"))
                {
                    // Error detected
                    CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_INT_DATA_FORMAT, "CI_ValueOptions");
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);

                    if (System.Text.RegularExpressions.Regex.IsMatch(sValue, @"\A\b[0-9a-fA-F]+\b\Z"))
                    {
                        sValue = "0x" + sValue;
                    }
                    else
                    {
                        sValue = null;
                    }
                }

                sRet = CHexIntConverter.FormatToHexString(sValue);
            }

            return sRet;
        }
    }
}
