using System.Collections.Generic;

namespace ConfigGenerator
{
    public struct RULE_MEMBER_TYPE
    {
        public string   m_sName;
        public string   m_sValue;
    }

    public class CParsedRuleContent
    {
        public List<RULE_MEMBER_TYPE>   m_SenderList      ;
        public List<RULE_MEMBER_TYPE>   m_TargetList      ;
        public string                   m_sCondition      ;
        public string                   m_sAction         ;
    }

    public class CParsedRuleObject
    {
        private List<CParsedRuleContent> m_RuleList = null;

        public CParsedRuleObject()
        { }

        public void SetRuleList(List<CParsedRuleContent> RuleList)
        {
            m_RuleList = new List<CParsedRuleContent>();
            m_RuleList.AddRange(RuleList);
        }

        public void AddRuleObject(CParsedRuleContent RuleObject)
        {
            if (m_RuleList == null)
            {
                m_RuleList = new List<CParsedRuleContent>();
            }
            m_RuleList.Add(RuleObject);
        }

        public List<CParsedRuleContent> GetRuleList()
        {
            return m_RuleList;
        }
    }
}
