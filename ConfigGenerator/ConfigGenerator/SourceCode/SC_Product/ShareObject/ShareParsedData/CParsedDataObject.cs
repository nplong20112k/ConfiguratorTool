using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CParsedDataObject : AShareObject
    {
        public const string KEYWORD_BASIC       = "BASIC";
        public const string KEYWORD_EXPERT      = "EXPERT";
        public const string KEYWORD_EXPERT_DEV  = "EXPERT_DEV";

        public const string KEYWORD_PRIVATE     = "PRIVATE";
        public const string KEYWORD_INTERNAL    = "INTERNAL";
        public const string KEYWORD_PUBLIC_PRG  = "PUBLIC_PRG";
        public const string KEYWORD_PUBLIC_QRG  = "PUBLIC_QRG_PRG";

        public static readonly List<string> KEYWORD_INTERFACE_NA = new List<string>() {"N/A", "n.a."};

        //=======================================
        // ATTRIBUTES
        //=======================================
        private ATableRef              m_TableRef  = null;
        private CParsedParameterObject m_Parameter = null;
        private CParsedPositionObject  m_Position  = null;
        private CParsedRuleObject      m_Rule      = null;

        //=======================================
        // INTERFACE FUNCTIONS
        //=======================================


        //=======================================
        // INTERNAL FUNCTIONS
        //=======================================
        public CParsedDataObject ()
            : base(SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
        {
        }

        // TABLE REF FUNCTION
        public void SetTalbeRef(ATableRef TableRef)
        {
            m_TableRef = TableRef;
        }
        public ATableRef GetTableRef()
        {
            return m_TableRef;
        }

        // PARAMETER FUNCTION
        public void SetParameter(CParsedParameterObject Parameter)
        {
            m_Parameter = Parameter;
        }
        public CParsedParameterObject GetParameter()
        {
            return m_Parameter;
        }

        // POSITION FUNCTION
        public void SetPosition(CParsedPositionObject Position)
        {
            m_Position = Position;
        }
        public CParsedPositionObject GetPosition()
        {
            return m_Position;
        }

        // RULE FUNCTION
        public void SetRule(CParsedRuleObject Rule)
        {
            m_Rule = Rule;
        }
        public CParsedRuleObject GetRule()
        {
            return m_Rule;
        }

    }
    
}
