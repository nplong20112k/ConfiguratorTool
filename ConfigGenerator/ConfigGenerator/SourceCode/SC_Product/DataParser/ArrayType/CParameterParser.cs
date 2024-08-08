using System;
using System.Collections.Generic;

namespace ConfigGenerator
{
    class CParameterParser : AParser
    {
        public const string PROTECTION_USER        = "USER";
        public const string PROTECTION_DEVELOPER   = "DEVELOPER";
                                                   
        private CParameterValueConverter m_ParameterValueConverter = null;
        private string m_sTagName;

        public CParameterParser()
        {
            m_ParameterValueConverter = new CParameterValueConverter();
        }

        public override bool ParserProcessing(IShareObject oDataObject, ref CParsedDataObject oShareObject)
        {
            bool bRet = true;
            CParsedParameterObject ParameterObject = null;
            IGetInputParameterObject InputObject = (IGetInputParameterObject)oDataObject;

            if (InputObject != null)
            {
                // Get Tag Name
                m_sTagName = InputObject.GetCITagName();

                // Format default value
                string sDefaultValue = m_ParameterValueConverter.CheckAndConvertParameterValue(InputObject.GetCITagName(), InputObject.GetCIMasterDefaultEugene());
                string sAltDefaultValue = m_ParameterValueConverter.CheckAndConvertParameterValue(InputObject.GetCITagName(), InputObject.GetCIMasterDefaultBologna());

                // format interface default value
                List<string> sDefaultInterfaceValue = new List<string>();
                int nNumOfIntClass = InputObject.GetNumberOfInterfaceClass();
                for (int i = 0; i < nNumOfIntClass; i++)
                {
                    sDefaultInterfaceValue.Add(ParseInterfaceDefaultValue(InputObject.GetCIClassDefault(i)));
                }

                // Parser visibility
                string sProtection = PROTECTION_USER;
                if (string.ReferenceEquals(InputObject.GetCIAladdinVisibility(), CParsedDataObject.KEYWORD_EXPERT_DEV))
                {
                    sProtection = PROTECTION_DEVELOPER;
                }

                // Paser categories
                List<string> sCategoryList = ParseCategories(InputObject.GetCIAladdinCategory());

                ParameterObject = new CParsedParameterObject(
                                        InputObject.GetCITagCode(),
                                        m_sTagName,
                                        InputObject.GetCITagUserName(),
                                        sCategoryList,
                                        InputObject.GetCIValueSizeByte(),
                                        sDefaultValue,
                                        sAltDefaultValue,
                                        sProtection,
                                        sDefaultInterfaceValue);

                oShareObject.SetParameter(ParameterObject);
            }

            return bRet;
        }

        private string ParseInterfaceDefaultValue(string sValue)
        {
            string sRet = null;

            if (String.IsNullOrEmpty(sValue) == true)
            {
                return sRet;
            }

            sRet = m_ParameterValueConverter.CheckAndConvertParameterValue(m_sTagName, sValue);
            return sRet;
        }

        private List<string> ParseCategories(string sValue)
        {
            List<string> sRetList = null;

            if ((sValue != null) && (sValue != string.Empty))
            {
                sRetList = new List<string>();
                
                string[] sLineList = sValue.Split(CParserKeyword.GetInstance().KEYWORD_SPLIT_LINE);
                foreach (string lineElement in sLineList)
                {
                    if ((lineElement != null) && (lineElement != string.Empty))
                    {
                        string[] sFieldList = lineElement.Split(CParserKeyword.GetInstance().KEYWORD_SPLIT_POSITION);
                        if ((sFieldList != null) && (sFieldList.Length == 2))
                        {
                            if (string.IsNullOrEmpty(sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.CATEGORY_IDX]) == false)
                            {
                                // format category
                                string sTemp = sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.CATEGORY_IDX];
                                
                                // check dupplicate category
                                bool bFlagExist = false;
                                foreach (string element in sRetList)
                                {
                                    if (element == sTemp)
                                    {
                                        bFlagExist = true;
                                    }
                                }

                                if (bFlagExist == false)
                                {
                                    sRetList.Add(sTemp);
                                }
                            }
                        }
                    }
                }
            }

            return sRetList;
        }
    }
}
