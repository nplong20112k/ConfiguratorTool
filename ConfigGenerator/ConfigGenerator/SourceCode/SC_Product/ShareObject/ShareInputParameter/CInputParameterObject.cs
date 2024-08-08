namespace ConfigGenerator
{

    public class CInputParameterObject : AShareObject, 
    IGetInputPositionObject, IGetInputTableRefObject, IGetInputParameterObject, IGetInputRuleObject
    {
        //----------------------
        // ATTRIBUTES
        //----------------------
        private string[]    m_sInputParamFieldList = null;
        private bool        m_bInputParamEmpty = true;
        private int         m_nFieldListSize = 0;

        //----------------------
        // PUBLIC FUNCTIONS
        //----------------------
        public CInputParameterObject()
            : base(SHARE_OBJECT_ID.SOB_ID_INPUT_DATA_OBJECT)
        {
            m_nFieldListSize = CFieldID.GetInstance().GetFieldIDList().Length;
            m_sInputParamFieldList = new string[m_nFieldListSize];
            for (int i = 0; i < m_nFieldListSize; i++)
            {
                m_sInputParamFieldList[i] = "";
            }
        }

        public bool SetValueField(int iIndex, string sData)
        {
            bool bRet = false;

            if (iIndex < m_nFieldListSize)
            {
                sData = FillterEndLineChar(sData);
                m_sInputParamFieldList[iIndex] = sData;
            }
            m_bInputParamEmpty = false;

            return bRet;
        }

        public bool IsEmpty ()
        {
            return m_bInputParamEmpty;
        }

        public string GetCITagCode()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.TAG_CODE];
        }
        public string GetCISupportedModels()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.MODEL_PRODUCT];
        }
        public string GetCIAladdinCategory()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.ALADDIN_CATEGORY];
        }
        public string GetCITagName()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.TAG_NAME];
        }
        public string GetCITagUserName()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.TAG_USER_NAME];
        }
        public string GetCIValueOptions()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.VALUE_OPTIONS];
        }
        public string GetCIValueSizeByte()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.VALUE_SIZE_BYTE];
        }
        public string GetCIUserVisibility()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.USER_VISIBILITY];
        }
        public string GetCIAladdinVisibility()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.ALADDIN_VISIBILITY];
        }
        public string GetCIAladdinRule()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.ALADDIN_RULE];
        }
        public string GetCIReadOnly()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.READ_ONLY];
        }
        public string GetCIMasterDefaultEugene()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_EUGENE];
        }
        public string GetCIMasterDefaultBologna()
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_BOLOGNA];
        }

        public string GetCIClassDefault(int index)
        {
            return m_sInputParamFieldList[(int)FIELD_ID_TYPE.TOTAL_FIELD_ID + index];
        }

        public int GetNumberOfInterfaceClass()
        {
            return CFieldID.GetInstance().GetNumOfFlexibleColumn();
        }
    }
}
