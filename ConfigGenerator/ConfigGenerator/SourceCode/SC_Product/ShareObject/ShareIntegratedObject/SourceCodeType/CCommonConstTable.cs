using System.Collections.Generic;

namespace ConfigGenerator
{
    public enum COMMON_CONST_TABLE_TYPE
    {
        NORMAL_TABLE = 0,
        SPECIAL_TABLE,
    }

    public enum VALUE_LIST_TYPE
    {
        ACCEPTED_TYPE = 0,
        EXCEPTED_TYPE,
        TEXT_NON_EXCEPTED_TYPE,
        TEXT_EXCEPTED_TYPE
    }

    public abstract class ACommonConstTable
    {
        string                  m_sTagName;
        string                  m_sTagCode;
        string                  m_sInterface;
        string                  m_sProduct;
        string                  m_sMinValue;
        string                  m_sMaxValue;
        bool                    m_bIsDecimalValue;
        COMMON_CONST_TABLE_TYPE m_Type;

        public ACommonConstTable(
                    COMMON_CONST_TABLE_TYPE Type,
                    string sTagName,
                    string sTagCode,
                    string sInterface,
                    string sProduct,
                    string sMinValue,
                    string sMaxValue,
                    bool   bIsDecimalValue)
        {
            m_Type                  = Type;
            m_sTagName              = sTagName;
            m_sTagCode              = sTagCode;
            m_sInterface            = sInterface;
            m_sProduct              = sProduct;
            m_sMinValue             = sMinValue;
            m_sMaxValue             = sMaxValue;
            m_bIsDecimalValue       = bIsDecimalValue;
        }

        public COMMON_CONST_TABLE_TYPE GetComConstTableType ()
        {
            return m_Type;
        }

        public string GetTagName()
        {
            return m_sTagName;
        }

        public string GetTagCode()
        {
            return m_sTagCode;
        }

        public string GetInterfaceMask()
        {
            return m_sInterface;
        }

        public string GetProductMask()
        {
            return m_sProduct;
        }

        public string GetMaxValue()
        {
            return m_sMaxValue;
        }

        public string GetMinValue()
        {
            return m_sMinValue;
        }
        public bool CheckSpecialValueIsDecimal()
        {
            return m_bIsDecimalValue;
        }
    }

    public class CNormalConstItem : ACommonConstTable
    {
        private string m_sDefault;
        private List<string> m_sExceptionValue;

        public CNormalConstItem(
                    COMMON_CONST_TABLE_TYPE Type,
                    string sTagName,
                    string sTagCode,
                    string sInterface,
                    string sProduct,
                    string sMin,
                    string sMax,
                    bool   bDecimalValue,
                    string sDefault,
                    List<string> sExceptionValue)
            : base(COMMON_CONST_TABLE_TYPE.NORMAL_TABLE,
                    sTagName,
                    sTagCode,
                    sInterface,
                    sProduct,
                    sMin,
                    sMax,
                    bDecimalValue)
        {
            m_sDefault = sDefault;
            m_sExceptionValue = sExceptionValue;
        }

        public string GetDefaultValue()
        {
            return m_sDefault;
        }

        public List<string> GetExceptionValueList()
        {
            return m_sExceptionValue;
        }
    }

    public class CSpecialConstItem : ACommonConstTable
    {
        string m_sStruture;
        string m_sDefault;
        string m_sAltDefault;
        string m_sTagValueSize;

        string m_sDefaultString;
        string m_sAltDefaultString;

        List<string> m_sValueList;
        VALUE_LIST_TYPE m_eValueListType;

        public CSpecialConstItem(
                    COMMON_CONST_TABLE_TYPE Type,
                    string sTagName,
                    string sTagCode,
                    string sInterface,
                    string sProduct,
                    string sDefault,
                    string sAltDefault,
                    string sStructure,
                    string sValueSize,
                    string sDefaultString,
                    string sAltDefautlString,
                    string sMinValue,
                    string sMaxValue,
                    bool   bIsDecimalValue,
                    List<string> lsValue,
                    VALUE_LIST_TYPE eValueListType)
            : base(COMMON_CONST_TABLE_TYPE.SPECIAL_TABLE,
                    sTagName,
                    sTagCode,
                    sInterface,
                    sProduct,
                    sMinValue,
                    sMaxValue,
                    bIsDecimalValue)
        {
            m_sStruture                 = sStructure;
            m_sDefault                  = sDefault;
            m_sAltDefault               = sAltDefault;
            m_sTagValueSize             = sValueSize;
            m_sDefaultString            = sDefaultString;
            m_sAltDefaultString         = sAltDefautlString;
            m_sValueList                = lsValue;
            m_eValueListType            = eValueListType;
        }

        public string GetStructure()
        {
            return m_sStruture;
        }

        public string GetDefaultValue()
        {
            return m_sDefault;
        }

        public string GetAltDefaultValue()
        {
            return m_sAltDefault;
        }

        public string GetDefaultString()
        {
            return m_sDefaultString;
        }

        public string GetAltDefaultString()
        {
            return m_sAltDefaultString;
        }

        public string GetTagValueSize()
        {
            return m_sTagValueSize;
        }

        public List<string> GetValueList()
        {
            return m_sValueList;
        }

        public VALUE_LIST_TYPE GetValueListType()
        {
            return m_eValueListType;
        }
    }
}