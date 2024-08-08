using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    public static class CMaxMinUpdater
    {
        private static List<Int32> ConvertListValueStringToNumber(List<CTableRefSelection.VALUE_TYPE> ValueList)
        {
            List<Int32> iListRet = null;

            if ((ValueList != null) && (ValueList.Count > 0))
            {
                iListRet = new List<int>();
                foreach (CTableRefSelection.VALUE_TYPE ElementValue in ValueList)
                {
                    try
                    {
                        iListRet.Add(Int32.Parse(ElementValue.sValue, System.Globalization.NumberStyles.HexNumber));
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return iListRet;
        }

        private static List<string> ProcessExceptionValue(ATableRef TableRefSelection)
        {
            List<string> sListRet = null;
            Int32 iMinValue = 0;
            Int32 iMaxValue = 0;
            List<Int32> iListValue = null;

            if ((TableRefSelection != null) && (TableRefSelection.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION))
            {
                try
                {
                    iMinValue = Int32.Parse((TableRefSelection as CTableRefSelection).GetTableRefMinValue(), System.Globalization.NumberStyles.HexNumber);
                    iMaxValue = Int32.Parse((TableRefSelection as CTableRefSelection).GetTableRefMaxValue(), System.Globalization.NumberStyles.HexNumber);
                    iListValue = ConvertListValueStringToNumber((TableRefSelection as CTableRefSelection).GetTableRefValueList());
                }
                catch { }

                if ((iMaxValue > 0) && (iListValue != null) && (iListValue.Count > 0))
                {
                    for (Int32 i = iMinValue; i < iMaxValue; i++)
                    {
                        if (iListValue.Contains(i) == false)
                        {
                            if (sListRet == null)
                            {
                                sListRet = new List<string>();
                            }
                            sListRet.Add(i.ToString());
                        }
                    }
                }
            }

            return sListRet;
        }

        private static List<string> UpdateValueList(ref string sMinValue, ref string sMaxValue, List<string> ExtraValueList, List<int> iListValue)
        {
            List<string> sExceptionValueList;
            int iMinValue = int.Parse(sMinValue, System.Globalization.NumberStyles.HexNumber);
            int iMaxValue = int.Parse(sMaxValue, System.Globalization.NumberStyles.HexNumber);
            foreach (string sValue in ExtraValueList)
            {
                int iValue = int.Parse(sValue, System.Globalization.NumberStyles.HexNumber);
                if (iListValue.Contains(iValue) == false)
                {
                    iListValue.Add(iValue);
                }
                if (iValue < iMinValue)
                {
                    iMinValue = iValue;
                }
                else if (iValue > iMaxValue)
                {
                    iMaxValue = iValue;
                }
            }
            sMinValue = iMinValue.ToString("X");
            sMaxValue = iMaxValue.ToString("X");
            List<string> sList = null;
            for (int i = iMinValue; i < iMaxValue; i++)
            {
                if (iListValue.Contains(i) == false)
                {
                    if (sList == null)
                    {
                        sList = new List<string>();
                    }
                    sList.Add(i.ToString());
                }
            }
            sExceptionValueList = sList;
            return sExceptionValueList;
        }

        public static void GetRealMaxMinExceptionListForRange (ATableRef TableRef, ref string sMaxValue, ref string sMinValue, ref List<string> sExceptionValueList)
        {
            sMinValue = null;
            sMaxValue = null;
            sExceptionValueList = null;

            List<string> ExtraValueList = (TableRef as CTableRefRange).GetExtraValueList();
            if (ExtraValueList == null)
            {
                if ((TableRef as CTableRefRange).GetAcceptAllValueProperty() == true)
                {
                    int iMinValue = 0;
                    int iMaxValue = 0xFF;
                    sMinValue = iMinValue.ToString("X");
                    sMaxValue = iMaxValue.ToString("X");
                }
                else
                {
                    sMinValue = (TableRef as CTableRefRange).GetTableRefMinValue();
                    sMaxValue = (TableRef as CTableRefRange).GetTableRefMaxValue();
                }
            }
            else
            {
                sMinValue = (TableRef as CTableRefRange).GetTableRefMinValue();
                sMaxValue = (TableRef as CTableRefRange).GetTableRefMaxValue();

                int iMinValue = int.Parse(sMinValue, System.Globalization.NumberStyles.HexNumber);
                int iMaxValue = int.Parse(sMaxValue, System.Globalization.NumberStyles.HexNumber);
                List<int> iListValue = Enumerable.Range(iMinValue, iMaxValue).ToList();

                sExceptionValueList = UpdateValueList(ref sMinValue, ref sMaxValue, ExtraValueList, iListValue);
            }
        }

        public static void GetRealMaxMinExceptionListForSelection(ATableRef TableRef, ref string sMaxValue, ref string sMinValue, ref List<string> sExceptionValueList)
        {
            List<string> ExtraValueList = (TableRef as CTableRefSelection).GetExtraValueList();
            if (ExtraValueList == null)
            {
                sMinValue = (TableRef as CTableRefSelection).GetTableRefMinValue();
                sMaxValue = (TableRef as CTableRefSelection).GetTableRefMaxValue();

                if ((TableRef as CTableRefSelection).GetAcceptAllValueProperty() == true)
                {
                    int iMinValue = 0;
                    int iMaxValue = 0xFF;
                    sMinValue = iMinValue.ToString("X");
                    sMaxValue = iMaxValue.ToString("X");
                }
                else if ((TableRef as CTableRefSelection).GetAllValuesInRangeProperty() == false)
                {
                    sExceptionValueList = CMaxMinUpdater.ProcessExceptionValue(TableRef);
                }
            }
            else
            {
                sMinValue = (TableRef as CTableRefSelection).GetTableRefMinValue();
                sMaxValue = (TableRef as CTableRefSelection).GetTableRefMaxValue();
                List<int> iListValue = CMaxMinUpdater.ConvertListValueStringToNumber((TableRef as CTableRefSelection).GetTableRefValueList());
                sExceptionValueList = UpdateValueList(ref sMinValue, ref sMaxValue, ExtraValueList, iListValue);
            }
        }
    }
}
