using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    public class Event_CRuleIntegrator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CRuleIntegrator : AIntegerator
    {
        private class PARAM_CODE_TYPE
        {
            public List<string> m_sParamFullName;
            public string       m_sParamCode;
            public string       m_sParamName;
        }

        private List<PARAM_CODE_TYPE>           m_ParamTrackingList  = null;
        private List<CIntegratedRuleContent>    m_IntegratedRuleList = null;

        public CRuleIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_RULE, new Event_CRuleIntegrator().m_MyEvent)
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
            if (oDataIn != null)
            {
                if (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                {
                    // Tracking parameter info
                    PARAM_CODE_TYPE ParamTrackingInfo = GetIntegratedParamInfo(oDataOut);
                    if (ParamTrackingInfo != null)
                    {
                        m_ParamTrackingList.Add(ParamTrackingInfo);
                    }

                    // Tracking integrated rule
                    CParsedRuleObject ParsedRuleObject = (oDataIn as CParsedDataObject).GetRule();
                    List<CIntegratedRuleContent> IntegratedRuleList = PrepareIntegratedRuleObject(ParsedRuleObject, ParamTrackingInfo);
                    if ((IntegratedRuleList != null) && (IntegratedRuleList.Count > 0))
                    {
                        foreach (CIntegratedRuleContent NewRuleElement in IntegratedRuleList)
                        {
                            bool bFound = false;
                            foreach (CIntegratedRuleContent OldRuleElement in m_IntegratedRuleList)
                            {
                                bFound = OldRuleElement.CompareContent(NewRuleElement);
                                if (bFound == true)
                                {
                                    break;
                                }
                            }
                            if (bFound == false)
                            {
                                m_IntegratedRuleList.Add(NewRuleElement);
                            }
                        }
                    }
                }
            }
        }
        
        public override void FinalizeIntegratingProcess (ref CIntegratedDataObject oDataOut)
        {
            if (oDataOut != null)
            {
                CIntegratedRuleObject RuleObject = CompleteIntegratedTargetRuleObject(m_IntegratedRuleList, m_ParamTrackingList);
                oDataOut.SetRules(RuleObject);
            }
        }

        private void ReInitProperties()
        {
            m_ParamTrackingList     = new List<PARAM_CODE_TYPE>();
            m_IntegratedRuleList    = new List<CIntegratedRuleContent>();
        }

        private PARAM_CODE_TYPE GetIntegratedParamInfo(CIntegratedDataObject oDataObject)
        {
            PARAM_CODE_TYPE ParameterRet = null;

            if (oDataObject != null)
            {
                List<CIntegrateParamObject> TempParamObjectList = oDataObject.GetParameterList();

                if (TempParamObjectList != null)
                {
                    PARAM_CODE_TYPE ParameterTracking = new PARAM_CODE_TYPE();

                    foreach (CIntegrateParamObject ParamObjectElement in TempParamObjectList)
                    {
                        if (ParamObjectElement != null)
                        {
                            CIntegrateParamContainObject MainParamContent = ParamObjectElement.GetParameterList()[0];
                            if (MainParamContent != null)
                            {
                                if (ParameterTracking.m_sParamFullName == null)
                                {
                                    ParameterTracking.m_sParamFullName = new List<string>();
                                }

                                ParameterTracking.m_sParamFullName.Add(MainParamContent.GetParamName());
                                ParameterTracking.m_sParamCode = MainParamContent.GetParamCode();

                                string[] sTemp = MainParamContent.GetParamName().Split(CPositionIntegrator.KEYWORD_SPLIT_LEVEL.ToCharArray());
                                ParameterTracking.m_sParamName = sTemp[sTemp.Length - 1];
                            }
                        }
                    }
                    ParameterRet = ParameterTracking;
                }
            }

            return ParameterRet;
        }

        private List<CIntegratedRuleContent> PrepareIntegratedRuleObject(CParsedRuleObject ParsedRuleObject, PARAM_CODE_TYPE ParameterInfo)
        {
            List<CIntegratedRuleContent>    RuleListRet         = null;
            PARAM_CODE_TYPE                 ProcessParameterInfo;

            if ((ParameterInfo != null) && (ParameterInfo.m_sParamCode != null))
            {
                ProcessParameterInfo = ParameterInfo;

                // First integrated sender for rule
                if (ParsedRuleObject != null)
                {
                    List<CParsedRuleContent> ParsedRuleList = ParsedRuleObject.GetRuleList();
                    if ((ParsedRuleList != null) && (ProcessParameterInfo.m_sParamCode != null))
                    {
                        RuleListRet = new List<CIntegratedRuleContent>();
                        foreach (CParsedRuleContent ParsedRuleElement in ParsedRuleList)
                        {
                            CIntegratedRuleContent IntegratedRule = IntegrateRuleVerifyContent(ParsedRuleElement);
                            if (IntegratedRule != null)
                            {
                                IntegratedRule.m_sTagCode = ParameterInfo.m_sParamCode;
                                IntegratedRule.m_sTagName = ParameterInfo.m_sParamName;

                                List<RULE_MEMBER_TYPE> TempSenderList = new List<RULE_MEMBER_TYPE>();
                                foreach (RULE_MEMBER_TYPE SenderElelement in IntegratedRule.m_SenderList)
                                {
                                    RULE_MEMBER_TYPE IntegratedSender = new RULE_MEMBER_TYPE()
                                    {
                                        m_sName = SenderElelement.m_sName,
                                        m_sValue = SenderElelement.m_sValue,
                                    };
                                    if (IntegratedSender.m_sName == null)
                                    {
                                        IntegratedSender.m_sName = IntegratedRule.m_sTagCode;
                                    }
                                    TempSenderList.Add(IntegratedSender);
                                }
                                IntegratedRule.m_SenderList = TempSenderList;

                                // Check sender instance exist in target list
                                List<string> InvalidTargetList = new List<string>();
                                foreach (RULE_MEMBER_TYPE SenderElement in IntegratedRule.m_SenderList)
                                {
                                    foreach (RULE_MEMBER_TYPE TargetElement in IntegratedRule.m_TargetList)
                                    {
                                        if (TargetElement.m_sName == SenderElement.m_sName)
                                        {
                                            InvalidTargetList.Add(SenderElement.m_sName);
                                            break;
                                        }
                                    }
                                }

                                if (InvalidTargetList.Count > 0)
                                {
                                    CStatisticObject StatisticData = new CStatisticObject(IntegratedRule.m_sTagCode, IntegratedRule.m_sTagName);

                                    foreach (string SenderElement in InvalidTargetList)
                                    {
                                        string sStatistic = "CI_AladdinRule: sender ";
                                        sStatistic = sStatistic + SenderElement + " exist in target list";
                                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, sStatistic);
                                    }

                                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                                }
                                else
                                {
                                    RuleListRet.Add(IntegratedRule);
                                }
                            }
                        }
                    }
                }
            }
            
            return RuleListRet;
        }

        private CIntegratedRuleObject CompleteIntegratedTargetRuleObject(List<CIntegratedRuleContent> IntegratedRuleList, List<PARAM_CODE_TYPE>  ParamTrackingList)
        {
            CIntegratedRuleObject RuleObjectRet = null;

            if ((IntegratedRuleList != null) && (ParamTrackingList != null))
            {
                RuleObjectRet = new CIntegratedRuleObject();

                List<CIntegratedRuleContent> TempProcessList = new List<CIntegratedRuleContent>();

                foreach (CIntegratedRuleContent RuleElement in IntegratedRuleList)
                {
                    CIntegratedRuleContent IntegratedRule = IntegrateTargetList(RuleElement, ParamTrackingList);
                    List<CIntegratedRuleContent> FinalRuleList = CreateFinalRuleFromSenderInstance(IntegratedRule, ParamTrackingList);

                    if ((FinalRuleList != null) && (FinalRuleList.Count > 0))
                    {
                        TempProcessList.AddRange(FinalRuleList);
                    }
                }

                IntegratedRuleList = TempProcessList;
                RuleObjectRet.SetRuleList(IntegratedRuleList);
            }

            return RuleObjectRet;
        }

        private bool ReplaceTagNameToTagCode(ref List<RULE_MEMBER_TYPE> MemberList, PARAM_CODE_TYPE ParamTracking)
        {
            bool bRet = false;

            if ((MemberList != null) && (ParamTracking.m_sParamCode != null))
            {
                List<RULE_MEMBER_TYPE> MemberListRet = new List<RULE_MEMBER_TYPE>();

                foreach (RULE_MEMBER_TYPE MemberElement in MemberList)
                {
                    RULE_MEMBER_TYPE IntegratedSender = new RULE_MEMBER_TYPE()
                    {
                        m_sName = MemberElement.m_sName,
                        m_sValue = MemberElement.m_sValue,
                    };

                    string sTemp = MemberElement.m_sName;

                    if (sTemp == ParamTracking.m_sParamCode)
                    {
                        foreach (string sParamTrackingNameElement in ParamTracking.m_sParamFullName)
                        {
                            IntegratedSender.m_sName = sParamTrackingNameElement;
                            MemberListRet.Add(IntegratedSender);
                        }

                        bRet = true;
                    }
                    else
                    {
                        if ('*' == sTemp.Last())
                        {
                            sTemp = sTemp.TrimEnd('*');
                        }

                        foreach (string sParamTrackingNameElement in ParamTracking.m_sParamFullName)
                        {
                            if (sParamTrackingNameElement.Contains(sTemp))
                            {
                                IntegratedSender.m_sName = MemberElement.m_sName;
                                bRet = true;
                                break;
                            }
                        }

                        // Always add back sender
                        MemberListRet.Add(IntegratedSender);
                    }
                }

                MemberList = MemberListRet;
            }

            return bRet;
        }

        private CIntegratedRuleContent IntegrateRuleVerifyContent(CParsedRuleContent ParsedRuleObject)
        {
            CIntegratedRuleContent  RuleRet             = null;
            bool                    bFlagErrorDetected  = false;
            CStatisticObject        StatisticData       = new CStatisticObject();

            if (ParsedRuleObject != null)
            {
                if ((ParsedRuleObject.m_sCondition == null) && (ParsedRuleObject.m_sCondition == string.Empty))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinRule: missing condition member");
                    bFlagErrorDetected = true;
                }

                if ((ParsedRuleObject.m_sAction == null) && (ParsedRuleObject.m_sAction == string.Empty))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinRule: missing action member");
                    bFlagErrorDetected = true;
                }

                if ((ParsedRuleObject.m_TargetList == null) && (ParsedRuleObject.m_TargetList.Count == 0))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinRule: missing target member");
                    bFlagErrorDetected = true;
                }

                if (ParsedRuleObject.m_TargetList.GroupBy(n => n).Any(e => e.Count() > 1))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "CI_AladdinRule: duplicate target member");
                    bFlagErrorDetected = true;
                }

                if ((ParsedRuleObject.m_SenderList == null) && (ParsedRuleObject.m_SenderList.Count == 0))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_AladdinRule: missing sender member");
                    bFlagErrorDetected = true;
                }

                if (ParsedRuleObject.m_SenderList.GroupBy(n => n).Any(e => e.Count() > 1))
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "CI_AladdinRule: duplicate sender member");
                    bFlagErrorDetected = true;
                }

                if ("multiEqualTo" == ParsedRuleObject.m_sCondition)
                {
                    if (ParsedRuleObject.m_TargetList.GroupBy(n => n.m_sName).Any(e => e.Count() > 1))
                    {
                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "CI_AladdinRule: duplicate target member in multiEqual rule");
                        bFlagErrorDetected = true;
                    }

                    if (ParsedRuleObject.m_SenderList.GroupBy(n => n.m_sName).Any(e => e.Count() > 1))
                    {
                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "CI_AladdinRule: duplicate sender member in multiEqual rule");
                        bFlagErrorDetected = true;
                    }
                }

                if (bFlagErrorDetected == true)
                {
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                    return null;
                }

                RuleRet = new CIntegratedRuleContent() {
                    m_RuleType      = RULE_TYPE.UNKNOW,
                    m_sAction       = ParsedRuleObject.m_sAction,
                    m_sCondition    = ParsedRuleObject.m_sCondition,
                    m_SenderList    = ParsedRuleObject.m_SenderList,
                    m_TargetList    = ParsedRuleObject.m_TargetList,
                };
            }

            return RuleRet;
        }

        private CIntegratedRuleContent IntegrateTargetList (CIntegratedRuleContent IntegratedRule, List<PARAM_CODE_TYPE> ParamTrackingList)
        {
            int iTargetCnt;

            CIntegratedRuleContent WorkingRule = new CIntegratedRuleContent(IntegratedRule);

            // Integrated rule type
            WorkingRule.m_RuleType = RULE_TYPE.RULE;
            if (WorkingRule.m_sAction == "SetValue")
            {
                WorkingRule.m_RuleType = RULE_TYPE.RULE_SET;
            }

            iTargetCnt = WorkingRule.m_TargetList.Count;

            foreach (PARAM_CODE_TYPE ParamTrackingElement in ParamTrackingList)
            {
                // Integrated target
                if (ReplaceTagNameToTagCode(ref WorkingRule.m_TargetList, ParamTrackingElement) == true)
                {
                    iTargetCnt--;
                }
            }

            if (iTargetCnt > 0)
            {
                CStatisticObject StatisticData = new CStatisticObject(WorkingRule.m_sTagCode, WorkingRule.m_sTagName);

                foreach (RULE_MEMBER_TYPE TargetElement in IntegratedRule.m_TargetList)
                {
                    if (WorkingRule.m_TargetList.Contains(TargetElement) == true)
                    {
                        // Remove missing target
                        WorkingRule.m_TargetList.Remove(TargetElement);

                        string sStatistic = "CI_AladdinRule: target ";
                        sStatistic = sStatistic + TargetElement.m_sName + " doesnt exist";
                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, sStatistic);
                    }
                }

                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);

                // Still return rule in case missing target as this does not affect aladdin
                return WorkingRule;
            }
            else
            {
                return WorkingRule;
            }
        }

        private List<CIntegratedRuleContent> CreateFinalRuleFromSenderInstance (CIntegratedRuleContent IntegratedRule, List<PARAM_CODE_TYPE> ParamTrackingList)
        {

            // Create working list
            List<CIntegratedRuleContent> WorkingRuleList = new List<CIntegratedRuleContent>();
            List<string> MissingSenderList = new List<string>();

            // Loop all rules
            WorkingRuleList.Add(IntegratedRule);
            foreach (RULE_MEMBER_TYPE SenderElement in IntegratedRule.m_SenderList)
            {
                List<CIntegratedRuleContent> TempRuleList = new List<CIntegratedRuleContent>();
                foreach (CIntegratedRuleContent WorkingRule in WorkingRuleList)
                {
                    bool bResetLoop = false;
                    foreach (PARAM_CODE_TYPE ParamTrackingElement in ParamTrackingList)
                    {
                        if (SenderElement.m_sName == ParamTrackingElement.m_sParamCode)
                        {
                            foreach (string ParaFullName in ParamTrackingElement.m_sParamFullName)
                            {
                                CIntegratedRuleContent TempRule = new CIntegratedRuleContent(WorkingRule);
                                RULE_MEMBER_TYPE NewSender = new RULE_MEMBER_TYPE()
                                {
                                    m_sName = SenderElement.m_sName,
                                    m_sValue = SenderElement.m_sValue,
                                };

                                TempRule.m_SenderList.Remove(SenderElement);

                                NewSender.m_sName = ParaFullName;
                                TempRule.m_SenderList.Add(NewSender);

                                TempRuleList.Add(TempRule);
                            }
                            bResetLoop = true;
                            break;
                        }
                    }
                    if (bResetLoop == false)
                    {
                        if (MissingSenderList.Contains(SenderElement.m_sName) == false)
                        {
                            MissingSenderList.Add(SenderElement.m_sName);
                        }
                    }
                }
                WorkingRuleList = TempRuleList;
            }

            if (MissingSenderList.Count > 0)
            {
                CStatisticObject StatisticData = new CStatisticObject(IntegratedRule.m_sTagCode, IntegratedRule.m_sTagName);

                foreach (string SenderElement in MissingSenderList)
                {
                    string sStatistic = "CI_AladdinRule: sender ";
                    sStatistic = sStatistic + SenderElement + " doesnt exist";
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, sStatistic);
                }

                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);

                return null;
            }
            else
            {
                return WorkingRuleList;
            }
        }
    }
}
