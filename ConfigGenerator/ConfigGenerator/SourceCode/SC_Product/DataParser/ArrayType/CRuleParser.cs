using System.Collections.Generic;

namespace ConfigGenerator
{
    class CRuleParser : AParser
    {
        private enum MEMBER_TYPE
        {
            UNKNOW = 0,
            SENDER,
            TARGET,
        };

        private const int    KEYWORD_IDX           = 0;
        private const int    CONTENT_IDX           = 1;
                                                  
        private const char   KEYWORD_SPLIT_LINE    = '\n';
        private const char   KEYWORD_SPLIT_CONTENT = '=';
        private const char   KEYWORD_SPLIT_MEMBER  = ',';
        private const char   KEYWORD_SPLIT_VALUE   = ',';
        private const string KEYWORD_SPLIT_RAW_MEM = "AND";
                                                  
        private const string KEYWORD_VALUE_SENDER  = "ValueSender=";
        private const string KEYWORD_CONDITION     = "Condition=";
                                                  
        private const string KEYWORD_TARGET        = "Target=";
        private const string KEYWORD_VALUE_TARGET  = "ValueTarget=";
        private const string KEYWORD_ACTION        = "Action=";

        private const string KEYWORD_RULE_START    = "[rule";
        private const string KEYWORD_RULE_STOP     = "[/rule";

        public override bool ParserProcessing(IShareObject oDataObject, ref CParsedDataObject oShareObject)
        {
            bool bRet = false;

            CParsedRuleObject RuleObject = null;
            IGetInputRuleObject InputObject = (IGetInputRuleObject)oDataObject;
            if (InputObject != null)
            {
                RuleObject = ParseRule(InputObject.GetCIAladdinRule());
                if (RuleObject != null)
                {
                    oShareObject.SetRule(RuleObject);
                    bRet = true;
                }
            }
            
            return bRet;
        }

        public CParsedRuleObject ParseRule(string sValue)
        {
            CParsedRuleObject oRetObject = null;

            if ((sValue != null) && (sValue != string.Empty))
            {
                oRetObject = new CParsedRuleObject();

                List<string> sRawRuleList = ParseRawRuleList(sValue);
                foreach (string sRawRuleElement in sRawRuleList)
                {
                    CParsedRuleContent TempParsedRule = ParseRuleContent(sRawRuleElement);
                    if (TempParsedRule != null)
                    {
                        oRetObject.AddRuleObject(TempParsedRule);
                    }
                }
            }

            return oRetObject;
        }

        private List<string> ParseRawRuleList(string sValue)
        {
            List<string> sRawRuleListRet = null;

            if ((sValue != null) && (sValue != string.Empty))
            {
                sRawRuleListRet = new List<string>();

                string[]    sLineList       = sValue.Split(KEYWORD_SPLIT_LINE);
                string      sResultParsed   = null;
                bool        bStartStopField = false;

                foreach (string lineElement in sLineList)
                {
                    if (lineElement.Contains(KEYWORD_RULE_START))
                    {
                        bStartStopField = true;
                        sResultParsed = null;
                    }
                    else if (lineElement.Contains(KEYWORD_RULE_STOP))
                    {
                        bStartStopField = false;
                        if ((sResultParsed != null) && (sResultParsed != string.Empty))
                        {
                            sRawRuleListRet.Add(sResultParsed);
                        }
                    }

                    if (bStartStopField == true)
                    {
                        sResultParsed += lineElement + KEYWORD_SPLIT_LINE;
                    }
                }
            }

            return sRawRuleListRet;
        }

        private CParsedRuleContent ParseRuleContent(string sValue)
        {
            CParsedRuleContent oRuleRet = null;

            if ((sValue != null) && (sValue != string.Empty))
            {
                CParsedRuleContent RuleObject = new CParsedRuleContent() {
                          m_SenderList        = new List<RULE_MEMBER_TYPE>(),
                          m_sCondition        = null,
                          m_TargetList        = new List<RULE_MEMBER_TYPE>(),
                          m_sAction           = null,
                };

                string[] sLineList = sValue.Split(KEYWORD_SPLIT_LINE);
                foreach (string lineElement in sLineList)
                {
                    if ((lineElement != null) && (lineElement != string.Empty))
                    {
                        // Parsed TARGET content
                        if (lineElement.Contains(KEYWORD_TARGET) == true)
                        {
                            string sTempContent = lineElement.Replace(KEYWORD_TARGET, string.Empty).Trim();
                            List<RULE_MEMBER_TYPE> tempTargetList = ParseRuleMember(MEMBER_TYPE.TARGET, sTempContent);
                            if ((tempTargetList != null) && (tempTargetList.Count > 0))
                            {
                                RuleObject.m_TargetList.AddRange(tempTargetList);
                            }
                        }
                        // Parsed VALUE_SENDER content
                        else if (lineElement.Contains(KEYWORD_VALUE_SENDER) == true)
                        {
                            string sTempContent = lineElement.Replace(KEYWORD_VALUE_SENDER, string.Empty).Trim();
                            List<RULE_MEMBER_TYPE> tempSenderList = ParseRuleMember(MEMBER_TYPE.SENDER ,sTempContent);
                            if ((tempSenderList != null) && (tempSenderList.Count > 0))
                            {
                                RuleObject.m_SenderList.AddRange(tempSenderList);
                            }
                        }
                        // Parsed CONDITION content
                        else if (lineElement.Contains(KEYWORD_CONDITION) == true)
                        {
                            RuleObject.m_sCondition = lineElement.Replace(KEYWORD_CONDITION, string.Empty).Trim();
                        }
                        // Parsed ACTION content
                        else if (lineElement.Contains(KEYWORD_ACTION) == true)
                        {
                            RuleObject.m_sAction = lineElement.Replace(KEYWORD_ACTION, string.Empty).Trim();
                        }
                    }
                }
                oRuleRet = RuleObject;
            }
            return oRuleRet;
        }
        
        private List<RULE_MEMBER_TYPE> ParseRuleMember(MEMBER_TYPE MemberType, string sValue)
        {
            List<RULE_MEMBER_TYPE> oMemListRet = null;

            if ((MemberType != MEMBER_TYPE.UNKNOW) && (sValue != null) && (sValue != string.Empty))
            {
                oMemListRet = new List<RULE_MEMBER_TYPE>();
                sValue = sValue.Replace(KEYWORD_SPLIT_RAW_MEM, KEYWORD_SPLIT_MEMBER.ToString());

                string[] sMemList = sValue.Split(KEYWORD_SPLIT_MEMBER);
                foreach (string memElement in sMemList)
                {
                    if ((memElement != null) && (memElement != string.Empty))
                    {
                        string[] sSubMemList = memElement.Split(KEYWORD_SPLIT_CONTENT);
                        if ((sSubMemList != null) && (sSubMemList.Length > 0))
                        {
                            RULE_MEMBER_TYPE tempMember = new RULE_MEMBER_TYPE();
                            if (MemberType == MEMBER_TYPE.TARGET)
                            {
                                // update name nameber
                                tempMember.m_sName = sSubMemList[KEYWORD_IDX].Trim();
                                if (sSubMemList.Length == 2)
                                {
                                    // value member detected
                                    tempMember.m_sValue = sSubMemList[CONTENT_IDX].Trim();
                                }
                            }
                            else if (MemberType == MEMBER_TYPE.SENDER)
                            {
                                if (sSubMemList.Length == 1)
                                {
                                    // only value member detected.
                                    tempMember.m_sValue = sSubMemList[KEYWORD_IDX].Trim();
                                }
                                else if (sSubMemList.Length == 2)
                                {
                                    // both name and value member detected.
                                    tempMember.m_sName = sSubMemList[KEYWORD_IDX].Trim();
                                    tempMember.m_sValue = sSubMemList[CONTENT_IDX].Trim();
                                }
                            }

                            oMemListRet.Add(tempMember);
                        }
                    }
                }
            }

            return oMemListRet;
        }
    }
}
