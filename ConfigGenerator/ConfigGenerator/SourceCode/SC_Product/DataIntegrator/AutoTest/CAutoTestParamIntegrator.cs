using System.Collections.Generic;

namespace ConfigGenerator
{
    public class Event_AUTO_TEST_TABLE_REF_INTEGRATOR
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
           EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    public struct CIClassDefault
    {
        public uint m_uiClassNumber;
        public string sDefaultValue;
    }

    class CAutoTestParamIntegrator : AIntegerator
    {
        private List<INTERFACE_CLASS> m_lsInterfaceClass = null;

        private CAutoTestParamObject m_AutoTestParamObject = null;

        public CAutoTestParamIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_AUTOMATION_TEST_PARAMETER, new Event_AUTO_TEST_TABLE_REF_INTEGRATOR().m_MyEvent)
        {

        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                    {
                        m_lsInterfaceClass = (oData as CSourceInfoObject).GetInterfaceClass();
                    }
                    break;

                default:
                    break;
            }
        }

        public override void IntegratingProcess(ref IShareObject oDataIn, ref CIntegratedDataObject oDataOut)
        {
            if ((oDataIn != null) && (oDataOut != null))
            {
                CParsedParameterObject ParsedParameterObject = (oDataIn as CParsedDataObject).GetParameter();
                ATableRef TableRefIn = (oDataIn as CParsedDataObject).GetTableRef();
                m_AutoTestParamObject = new CAutoTestParamObject();
                m_AutoTestParamObject.SetTagCode(ParsedParameterObject.GetTagCode());
                m_AutoTestParamObject.SetTagName(ParsedParameterObject.GetTagName());
                m_AutoTestParamObject.SetDefaultValue(ParsedParameterObject.GetDefaultValue());
                m_AutoTestParamObject.SetCategoryList(ParsedParameterObject.GetTagCategory());
                m_AutoTestParamObject.SetValueSizeByte(ParsedParameterObject.GetTagValueSize());
                m_AutoTestParamObject.SetStepRangeValue(HandleStepRangeValue(TableRefIn));
                m_AutoTestParamObject.SetCIClassDefaultList(HandelCIClassDefault(ParsedParameterObject,TableRefIn));
                m_AutoTestParamObject.SetTypeValueAutoTest(HandleTableRefType(TableRefIn));
                (oDataOut as CIntegratedDataObject).SetAutoTestParameter(m_AutoTestParamObject);
            }
        }

        private string HandleTableRefType(ATableRef TableRef)
        {
            if (TableRef == null) return null;

            string sTempTableRefType = null;

            ATableRef.TABLE_REF_TYPE XmlTableType = (TableRef.GetTableRefType());
            switch (XmlTableType)
            {
                case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                    sTempTableRefType = "Enumeration";
                    break;
                case ATableRef.TABLE_REF_TYPE.TABLE_REF_CUSTOM:
                case ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII:
                    sTempTableRefType = "String";
                    break;
                case ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                    sTempTableRefType = "String";
                    break;
                case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                    sTempTableRefType = "Range";
                    break;
            }

            return sTempTableRefType;
        }

        private List<CIClassDefault> HandelCIClassDefault(CParsedParameterObject Parameter, ATableRef TableRef)
        {
            List<CIClassDefault> TempList = new List<CIClassDefault>();
            if (Parameter != null && TableRef != null)
            {
                List<string> lsClassDefaulValue = Parameter.GetDefaultValueClass();
                List<uint> lsUnavailableClass = TableRef.GetUnavailableInterfaceClassList();

                for (int i = 0; i < m_lsInterfaceClass.Count; i++)
                {
                    uint uiClassNumber = m_lsInterfaceClass[i].m_uiClassNumber;
                    if (CheckIsUnavailable(uiClassNumber, lsUnavailableClass) == false)
                    {
                        CIClassDefault TempClassDefaule = new CIClassDefault();
                        TempClassDefaule.m_uiClassNumber = m_lsInterfaceClass[i].m_uiClassNumber;
                        TempClassDefaule.sDefaultValue = lsClassDefaulValue[i];
                        TempList.Add(TempClassDefaule);
                    }
                }
            }
            return TempList;
        }

        private bool CheckIsUnavailable(uint uiNumber, List<uint> lsClassUnavailable)
        {
            bool bRet = false;
            if (lsClassUnavailable != null)
            {
                if (lsClassUnavailable.Contains(uiNumber) == true)
                {
                    bRet = true;
                }
            }
            return bRet;
        }

        private string HandleStepRangeValue(ATableRef TableRefIn)
        {
            string sTempValue = null;
            if ((TableRefIn != null) && (TableRefIn.GetTableRefType() == ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE))
            {
                sTempValue = (TableRefIn as CTableRefRange).GetTableRefStepValue();
            }
            return sTempValue;
        }
    }
}
