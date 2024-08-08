using System.Collections.Generic;

namespace ConfigGenerator
{
    class CPositionIntegrator : AIntegerator
    {

        public  const int       MODEL_IDX           = 1;
        public  const string    KEYWORD_SPLIT_LEVEL = ".";

        private const string    CONFIG_PAGE         = "Configuration";
        private const string    TEST_PAGE           = "Test Page";

        public CPositionIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_POSITION)
        {

        }

        public override void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut)
        {
            if (oDataIn != null && oDataOut != null)
            {
                CIntegratedPositionObject oPosition = null;

                if (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                {
                    CParsedPositionObject ParsedPosition = (oDataIn as CParsedDataObject).GetPosition();
                    List<string> IntegratedParamNameList = GetIntegratedParamInfo(oDataOut);

                    if ((ParsedPosition != null) && (IntegratedParamNameList != null))
                    {
                        CIntegratedPositionObject.ROOT_PAGE_TYPE RootPageName = CIntegratedPositionObject.ROOT_PAGE_TYPE.ROOT_PAGE_UNDEF;
                        switch (ParsedPosition.GetAladdinVisibility())
                        {
                            case CParsedDataObject.KEYWORD_BASIC:
                                RootPageName = CIntegratedPositionObject.ROOT_PAGE_TYPE.ROOT_PAGE_BASIC;
                                break;

                            case CParsedDataObject.KEYWORD_EXPERT:
                            case CParsedDataObject.KEYWORD_EXPERT_DEV:
                                RootPageName = CIntegratedPositionObject.ROOT_PAGE_TYPE.ROOT_PAGE_EXPERT;
                                break;

                            default:
                                break;
                        }

                        oPosition = IntegratePosition ( RootPageName, 
                                                        ParsedPosition.GetPositionPathList(), 
                                                        ParsedPosition.GetModelNameList(), 
                                                        IntegratedParamNameList,
                                                        ParsedPosition.GetUserVisibility());
                    }
                }

                (oDataOut as CIntegratedDataObject).SetPosition(oPosition);
            }
        }

        private List<string> GetIntegratedParamInfo(CIntegratedDataObject oDataObject)
        {
            List<string> ParamNameListRet = null;

            if (oDataObject != null)
            {
                List<CIntegrateParamObject> TempParamObjectList = oDataObject.GetParameterList();

                if (TempParamObjectList != null)
                {
                    ParamNameListRet = new List<string>();

                    foreach (CIntegrateParamObject ParamObjectElement in TempParamObjectList)
                    {
                        if (ParamObjectElement != null)
                        {
                            CIntegrateParamContainObject MainParamContent = ParamObjectElement.GetParameterList()[0];
                            if (MainParamContent != null)
                            {
                                ParamNameListRet.Add(MainParamContent.GetParamName());
                            }
                        }
                    }
                }
            }

            return ParamNameListRet;
        }
        
        private CIntegratedPositionObject IntegratePosition (   CIntegratedPositionObject.ROOT_PAGE_TYPE RootPageName,
                                                                List<POSITION_PARSED_TYPE> ParsedPositionPathList,
                                                                List<string> ModelNameList,
                                                                List<string> ParamNameList,
                                                                string sUserVisibility)
        {
            bool bFlagErrorDetected = false;
            bool bFlagWarningDetected = false;

            CStatisticObject StatisticData = new CStatisticObject();

            switch (sUserVisibility)
            {
                case CParsedDataObject.KEYWORD_PRIVATE:
                case CParsedDataObject.KEYWORD_INTERNAL:
                    return null;

                case CParsedDataObject.KEYWORD_PUBLIC_PRG:
                case CParsedDataObject.KEYWORD_PUBLIC_QRG:
                    {
                        switch (RootPageName)
                        {
                            case CIntegratedPositionObject.ROOT_PAGE_TYPE.ROOT_PAGE_UNDEF:
                                {
                                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinVisibility");
                                    bFlagErrorDetected = true;
                                }
                                break;

                            default:
                                {
                                    if ((ParsedPositionPathList == null) || (ParsedPositionPathList.Count == 0))
                                    {
                                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinMenuPathsAndCategories");
                                        bFlagErrorDetected = true;
                                    }
                                    else
                                    {
                                        List<string> lsTest = new List<string>();

                                        foreach (POSITION_PARSED_TYPE element in ParsedPositionPathList)
                                        {
                                            if (string.IsNullOrEmpty(element.m_sCategories))
                                            {
                                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinMenuPathsAndCategories: missing category");
                                                bFlagErrorDetected = true;
                                            }
                                            else if ((element.m_sPositionPath == null) || (element.m_sPositionPath.Count == 0))
                                            {
                                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinMenuPathsAndCategories: missing Aladdin page position");
                                                bFlagErrorDetected = true;
                                            }
                                            else
                                            {
                                                string sTemp = string.Join(".", element.m_sPositionPath);
                                                sTemp += element.m_sGroup;
                                                if (lsTest.Contains(sTemp))
                                                {
                                                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "CI_AladdinMenuPathsAndCategories: duplicate Aladdin page position");
                                                    bFlagWarningDetected = true;
                                                }
                                                else
                                                {
                                                    lsTest.Add(sTemp);
                                                    if (sTemp.Contains(CParserKeyword.GetInstance().KEYWORD_MODEL_NAME))
                                                    {
                                                        if ((ModelNameList == null) || (ModelNameList.Count == 0))
                                                        {
                                                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_SupportedModels");
                                                            bFlagWarningDetected = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;

                default:
                    {
                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_UserVisibility");
                        bFlagErrorDetected = true;
                    }
                    break;
            }

            if ((bFlagWarningDetected == true) || (bFlagErrorDetected == true))
            {
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                if (bFlagErrorDetected == true)
                {
                    return null;
                }
            }

            CIntegratedPositionObject oPositionObject = new CIntegratedPositionObject();
            if (oPositionObject != null)
            {
                oPositionObject.SetRootPage(RootPageName);

                List<POSTION_INTEGRATED_TYPE> TempPositionPathList = new List<POSTION_INTEGRATED_TYPE>();
                foreach (POSITION_PARSED_TYPE element in ParsedPositionPathList)
                {
                    POSTION_INTEGRATED_TYPE oPositionData = new POSTION_INTEGRATED_TYPE()
                    {
                        m_sPositionPath = new List<string>(),
                        m_sGroup        = null,
                        m_sFieldName    = null,
                        m_sModelName    = null,
                        m_sIndex        = null,
                    };

                    oPositionData.m_sPositionPath.Add(CONFIG_PAGE); // hard page for all postion path !!

                    if ((element.m_sPositionPath != null) && (element.m_sPositionPath.Count > 0))
                    {
                        oPositionData.m_sPositionPath.AddRange(element.m_sPositionPath);
                        oPositionData.m_sGroup = element.m_sGroup;
                        oPositionData.m_sIndex = element.m_sIndex;
                    }

                    if ((element.m_sCategories != null) && (element.m_sCategories != string.Empty))
                    {
                        foreach (string ParamNameElement in ParamNameList)
                        {
                            if (ParamNameElement.Contains(element.m_sCategories) == true)
                            {
                                oPositionData.m_sFieldName = ParamNameElement;
                                break;
                            }
                        }
                    }
                    
                    TempPositionPathList.Add(oPositionData);
                }

                List<POSTION_INTEGRATED_TYPE> PositionPathList = IntegrateModelName(ModelNameList, TempPositionPathList);
                oPositionObject.SetPositionDataList(PositionPathList);
            }

            return oPositionObject;
        }

        private List<POSTION_INTEGRATED_TYPE> IntegrateModelName(List<string> ModelNameList, List<POSTION_INTEGRATED_TYPE> PositionPathList)
        {
            List<POSTION_INTEGRATED_TYPE> ListRet = null;

            if ((ModelNameList == null) || (ModelNameList.Count == 0))
            {
                return PositionPathList;
            }

            if ((PositionPathList != null) && (PositionPathList.Count > 0))
            {
                ListRet = new List<POSTION_INTEGRATED_TYPE>();

                // Run for looping position list ! 
                foreach (POSTION_INTEGRATED_TYPE PositionElement in PositionPathList)
                {
                    if ((PositionElement.m_sPositionPath != null) && (PositionElement.m_sPositionPath.Count > 0))
                    {
                        if (PositionElement.m_sPositionPath[MODEL_IDX] == CParserKeyword.GetInstance().KEYWORD_MODEL_NAME)
                        {
                            // Dupplicate position with model name valid !
                            foreach (string sModelNameElement in ModelNameList)
                            {
                                if ((sModelNameElement != null) && (sModelNameElement != string.Empty))
                                {
                                    POSTION_INTEGRATED_TYPE TempPosition = new POSTION_INTEGRATED_TYPE() { m_sPositionPath = new List<string>() };

                                    TempPosition.m_sPositionPath.AddRange(PositionElement.m_sPositionPath);
                                    TempPosition.m_sPositionPath[MODEL_IDX] = sModelNameElement;
                                    TempPosition.m_sGroup = PositionElement.m_sGroup;
                                    TempPosition.m_sFieldName = PositionElement.m_sFieldName;
                                    TempPosition.m_sModelName = sModelNameElement;
                                    TempPosition.m_sIndex = PositionElement.m_sIndex;

                                    ListRet.Add(TempPosition);
                                }
                            }
                        }
                        else
                        {
                            ListRet.Add(PositionElement);
                        }
                    }
                }
            }

            return ListRet;
        }
        
    }
}
