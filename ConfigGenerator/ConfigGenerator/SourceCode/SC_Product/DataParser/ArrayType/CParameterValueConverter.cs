namespace ConfigGenerator
{
    public class CParameterValueConverter
    {
        public string CheckAndConvertParameterValue(string sName, string sValue)
        {
            string sRet;

            if (string.IsNullOrEmpty(sValue))
            {
                // Error detected
                CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "Default Values Eugene/Bologna or Interface group");
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
                    CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_HEX_DATA_FORMAT, "Default Values Eugene/Bologna or Interface group");
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);

                    sRet = null;
                }
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(sValue, @"\A\b[0-9]+\b\Z"))
                {
                    // Error detected
                    CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_INT_DATA_FORMAT, "Default Values Eugene/Bologna or Interface group");
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