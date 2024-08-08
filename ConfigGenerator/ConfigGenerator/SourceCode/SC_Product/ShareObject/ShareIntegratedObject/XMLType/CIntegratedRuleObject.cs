
using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    public enum RULE_TYPE
    {
        UNKNOW = 0,
        RULE,
        RULE_SET,
    }
    
    public class CIntegratedRuleContent
    {
        public RULE_TYPE                m_RuleType;
        public List<RULE_MEMBER_TYPE>   m_SenderList;
        public List<RULE_MEMBER_TYPE>   m_TargetList;
        public string                   m_sCondition;
        public string                   m_sAction;

        // These attributes for statistic only
        public string                   m_sTagCode;
        public string                   m_sTagName;

        public CIntegratedRuleContent ()
        {
        }

        public CIntegratedRuleContent (CIntegratedRuleContent IntegratedRuleContent)
        {
            m_RuleType      = IntegratedRuleContent.m_RuleType;
            m_sCondition    = IntegratedRuleContent.m_sCondition;
            m_sAction       = IntegratedRuleContent.m_sAction;

            m_SenderList    = new List<RULE_MEMBER_TYPE>(IntegratedRuleContent.m_SenderList);
            m_TargetList    = new List<RULE_MEMBER_TYPE>(IntegratedRuleContent.m_TargetList);

            m_sTagCode      = IntegratedRuleContent.m_sTagCode;
            m_sTagName      = IntegratedRuleContent.m_sTagName;
        }

        public bool CompareContent (CIntegratedRuleContent RuleContent)
        {
            bool bRet = false;

            if (m_RuleType != RuleContent.m_RuleType)
            {
                return bRet;
            }

            if (m_sCondition != RuleContent.m_sCondition)
            {
                return bRet;
            }

            if (m_sAction != RuleContent.m_sAction)
            {
                return bRet;
            }

            var NotInTheir = m_SenderList.Except(RuleContent.m_SenderList).ToList();
            var NotInMine = RuleContent.m_SenderList.Except(m_SenderList).ToList();

            if ((NotInTheir != null) && (NotInTheir.Count > 0) ||
                (NotInMine != null) && (NotInMine.Count > 0))
            {
                return bRet;
            }

            NotInTheir = m_TargetList.Except(RuleContent.m_TargetList).ToList();
            NotInMine = RuleContent.m_TargetList.Except(m_TargetList).ToList();

            if ((NotInTheir != null) && (NotInTheir.Count > 0) ||
                (NotInMine != null) && (NotInMine.Count > 0))
            {
                return bRet;
            }

            bRet = true;
            return bRet;
        }
    }

    public class CIntegratedRuleObject
    {
        private List<CIntegratedRuleContent> m_RuleList = null;

        public CIntegratedRuleObject( )
        { }
        
        public void SetRuleList(List<CIntegratedRuleContent> RuleList)
        {
            m_RuleList = new List<CIntegratedRuleContent>();
            m_RuleList.AddRange(RuleList);
        }

        public void AddRuleObject(CIntegratedRuleContent RuleObject)
        {
            if (m_RuleList == null)
            {
                m_RuleList = new List<CIntegratedRuleContent>();
            }
            m_RuleList.Add(RuleObject);
        }

        public List<CIntegratedRuleContent> GetRuleList()
        {
            return m_RuleList;
        }
    }
}
