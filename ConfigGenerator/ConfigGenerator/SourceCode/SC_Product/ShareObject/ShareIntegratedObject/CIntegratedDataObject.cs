using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CIntegratedDataObject : AShareObject
    {
        //=======================================
        // ATTRIBUTES
        //=======================================
        // XML
        private List<AXmlTable>             m_TableRefList      = null;
        private List<CIntegrateParamObject> m_ParameterList     = null;
        private CIntegratedPositionObject   m_Position          = null;
        private CIntegratedRuleObject       m_Rules             = null;

        // Source code
        private ACommonConstTable           m_CommonConstTable  = null;
        private CStructure                  m_Structure         = null;
        private CDeltaConstTable            m_DeltaTable        = null;

        // Auto Test
        private CAutoTestParamObject m_AutoTestParam = null;
        private List<AXmlTable> m_TableRefAuTestList = null;

        //=======================================
        // INTERFACE FUNCTIONS
        //=======================================


        //=======================================
        // INTERNAL FUNCTIONS
        //=======================================
        public CIntegratedDataObject()
            : base(SHARE_OBJECT_ID.SOB_ID_INTEGRATED_DATA_OBJECT)
        {
        }

        // TABLE REF FUNCTION
        public void SetTableRefList(List<AXmlTable> TableRefList)
        {
            m_TableRefList = TableRefList;
        }

        public List<AXmlTable> GetTableRefList()
        {
            return m_TableRefList;
        }

        // PARAMETER FUNCTION
        public void SetParameterList(List<CIntegrateParamObject> ParameterList)
        {
            m_ParameterList = ParameterList;
        }

        public List<CIntegrateParamObject> GetParameterList()
        {
            return m_ParameterList;
        }

        // POSITION FUNCTION
        public void SetPosition(CIntegratedPositionObject Position)
        {
            m_Position = Position;
        }

        public CIntegratedPositionObject GetPosition()
        {
            return m_Position;
        }

        // RULE FUNCTION
        public void SetRules(CIntegratedRuleObject Rules)
        {
            m_Rules = Rules;
        }

        public CIntegratedRuleObject GetRules()
        {
            return m_Rules;
        }

        // COMMON CONST TABLE FUNCTION
        public void SetCommonConstTable(ACommonConstTable CommonConstTable)
        {
            m_CommonConstTable = CommonConstTable;
        }

        public ACommonConstTable GetCommonConstTable()
        {
            return m_CommonConstTable;
        }

        // ACCESS STRUCTURE
        public void SetStructure(CStructure Structure)
        {
            m_Structure = Structure;
        }

        public CStructure GetStructure()
        {
            return m_Structure;
        }

        //DELTA TABLE
        public void SetDeltaTable(CDeltaConstTable DeltaTable)
        {
            m_DeltaTable = DeltaTable;
        }

        public CDeltaConstTable GetDeltaTable()
        {
            return m_DeltaTable;
        }

        // Auto Test
        public void SetAutoTestParameter(CAutoTestParamObject ParamObject)
        {
            m_AutoTestParam = ParamObject;
        }

        public CAutoTestParamObject GetAutoTestParamObject()
        {
            return m_AutoTestParam;
        }

        public void SetTableRefAutoTestList(List<AXmlTable> TableRefList)
        {
            m_TableRefAuTestList = TableRefList;
        }

        public List<AXmlTable> GetTableRefAutoTestList()
        {
            return m_TableRefAuTestList;
        }
    }
}
