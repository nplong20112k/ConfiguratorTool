using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    class CPositionParser : AParser
    {
        public const int    GROUP_IDX              = 1;
        public const int    ITEM_SUB_IDX           = 0;
        public const int    ORDER_SUB_IDX          = 1;

        private const char KEYWORD_SPLIT_GROUP     = ':';
        private const char DELIMITER_PAGE_CHAR     = '.';
        private const char ITEM_INDEX_SPLIT_CHAR   = '@';

        public override bool ParserProcessing(IShareObject oDataObject, ref CParsedDataObject oShareObject)
        {
            bool bRet = false;

            string                      sAladdinVisibility  = null;
            string                      sUserVisibility     = null;
            List<POSITION_PARSED_TYPE>  AladPagePosList     = null;
            List<string>                ModelNameList       = null;
            CParsedPositionObject       PositionObject      = null;

            IGetInputPositionObject InputObject = (IGetInputPositionObject)oDataObject;
            if (InputObject != null)
            {
                PositionObject     = new CParsedPositionObject();

                sUserVisibility = InputObject.GetCIUserVisibility();
                if ((sUserVisibility != null) && (sUserVisibility != string.Empty))
                {
                    PositionObject.SetUserVisibility(sUserVisibility);
                }

                sAladdinVisibility = InputObject.GetCIAladdinVisibility();
                if ((sAladdinVisibility != null) && (sAladdinVisibility != string.Empty))
                {
                    PositionObject.SetAladdinVisibility(sAladdinVisibility);
                }
                
                AladPagePosList = ParsePositions(InputObject.GetCIAladdinCategory());
                if (AladPagePosList != null)
                {
                    PositionObject.SetPositionDataList(AladPagePosList);
                }

                ModelNameList = ParseModelNameList(InputObject.GetCISupportedModels());
                if (ModelNameList != null)
                {
                    PositionObject.SetModelNameList(ModelNameList);
                }

                oShareObject.SetPosition(PositionObject);
            }

            return bRet;
        }

        private List<POSITION_PARSED_TYPE> ParsePositions(string sValue)
        {
            List<POSITION_PARSED_TYPE> sRetList = null;

            if (String.IsNullOrEmpty(sValue) == false)
            {
                sRetList = new List<POSITION_PARSED_TYPE>();
                
                string[] sLineList = sValue.Split(CParserKeyword.GetInstance().KEYWORD_SPLIT_LINE);
                foreach (string lineElement in sLineList)
                {
                    if (String.IsNullOrEmpty(lineElement) == false)
                    {
                        POSITION_PARSED_TYPE PositionData = new POSITION_PARSED_TYPE() {  m_sPositionPath = new List<string>(),
                                                                                          m_sGroup        = null,
                                                                                          m_sCategories   = null,
                                                                                          m_sIndex        = null };

                        string[] sFieldList;

                        if (lineElement.Contains(CParserKeyword.GetInstance().KEYWORD_SPLIT_POSITION))
                        {
                            sFieldList = lineElement.Split(CParserKeyword.GetInstance().KEYWORD_SPLIT_POSITION);
                        }
                        else
                        {
                            sFieldList = new string[((int)CParserKeyword.SUB_CATEGORY_IDX.CATEGORY_IDX + 1)];
                            sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.POSITION_IDX] = lineElement;
                            sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.CATEGORY_IDX] = lineElement.Trim(CParserKeyword.GetInstance().KEYWORD_MODEL_NAME.ToArray());
                        }

                        if ((sFieldList != null) && (sFieldList.Length > 0))
                        {
                            if (String.IsNullOrEmpty(sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.POSITION_IDX]) == false)
                            {
                                // Update index
                                string[] sSubFieldList = sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.POSITION_IDX].Split(ITEM_INDEX_SPLIT_CHAR);
                                if ((sSubFieldList.Length == 2) && (String.IsNullOrEmpty(sSubFieldList[ORDER_SUB_IDX]) == false))
                                {
                                    short sResult;
                                    if (Int16.TryParse(sSubFieldList[ORDER_SUB_IDX], out sResult) == false)
                                    {
                                        // Error detected
                                        CStatisticObject StatisticData = new CStatisticObject(STATISTIC_TYPE.STATISTIC_INCORRECT_INT_DATA_FORMAT, "Incorrect index value");
                                        CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);

                                        PositionData.m_sIndex = null;
                                    }
                                    else
                                    {
                                        PositionData.m_sIndex = sSubFieldList[ORDER_SUB_IDX];
                                    }
                                }

                                sSubFieldList = sSubFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.POSITION_IDX].Split(KEYWORD_SPLIT_GROUP);
                                if ((sSubFieldList != null) && (sSubFieldList.Length > 0))
                                {
                                    // update position path properties
                                    if (String.IsNullOrEmpty(sSubFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.POSITION_IDX]) == false)
                                    {
                                        string[] sPageList = sSubFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.POSITION_IDX].Split(DELIMITER_PAGE_CHAR);
                                        foreach (string PageElement in sPageList)
                                        {
                                            if (String.IsNullOrEmpty(PageElement) == false)
                                            {
                                                PositionData.m_sPositionPath.Add(PageElement);
                                            }

                                        }
                                    }

                                    // update group property
                                    if ((sSubFieldList.Length == 2) && (String.IsNullOrEmpty(sSubFieldList[GROUP_IDX]) == false))
                                    {
                                        PositionData.m_sGroup = sSubFieldList[GROUP_IDX];
                                    }
                                }
                            }

                            // update categories property
                            if ((sFieldList.Length == 2) && (String.IsNullOrEmpty(sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.CATEGORY_IDX]) == false))
                            {
                                PositionData.m_sCategories = sFieldList[(int)CParserKeyword.SUB_CATEGORY_IDX.CATEGORY_IDX];
                            }
                        }
                    
                        sRetList.Add(PositionData);
                    }
                }
            }

            return sRetList;
        }

        private List<string> ParseModelNameList(string sValue)
        {
            List<string> sListRet = null;

            if ((sValue != null) && (sValue != string.Empty))
            {
                sListRet = new List<string>();
                string[] sLineList = sValue.Split(CParserKeyword.GetInstance().KEYWORD_SPLIT_LINE);
                foreach (string lineElement in sLineList)
                {
                    if ((lineElement != null) && (lineElement != string.Empty))
                    {
                        sListRet.Add(lineElement);
                    }
                }
            }

            return sListRet;
        }
    }
}
