using System.Collections.Generic;

namespace ConfigGenerator
{
    public class Event_EnumTableIntegrator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CCIEnumTableIntegrator : AIntegerator
    {

        List<CI_ENUM_INFO>[] m_lsEnumInfo = null;

        public CCIEnumTableIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_CI_ENUM_TABLE, new Event_EnumTableIntegrator().m_MyEvent)
        {
        }

        protected override void EventHandler(EVENT_TYPE Event, IShareObject oData)
        {
            switch (Event)
            {
                case EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO:
                    if ((oData != null) && (oData.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT))
                    {
                        m_lsEnumInfo = (oData as CSourceInfoObject).GetCIEnumInfo();
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
                if (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                {
                    CParsedParameterObject Parameter = (oDataIn as CParsedDataObject).GetParameter();
                    CStatisticObject StatisticData = new CStatisticObject();

                    if (m_lsEnumInfo != null)
                    {
                        int iTagNameIndex = -1;
                        int iTagCodeIndex = -1;

                        // Check CSV configuration item
                        List<CI_ENUM_INFO> lsResultList = null;
                        lsResultList = m_lsEnumInfo[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM].FindAll(x => x.m_sCITagName.Equals(Parameter.GetTagName()));
                        if (lsResultList.Count == 0)
                        {
                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_TagName: unavailable definition in Enumeration list");
                        }
                        else if (lsResultList.Count > 1)
                        {
                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "CI_TagName: duplicate definition in Enumeration list");
                        }
                        else
                        {
                            iTagNameIndex = m_lsEnumInfo[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM].IndexOf(lsResultList[0]);
                        }

                        lsResultList = m_lsEnumInfo[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM].FindAll(x => x.m_sCITagCode.Equals(Parameter.GetTagCode()));
                        if (lsResultList.Count == 0)
                        {
                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "Configuration item: unavailable definition in Enumeration list");
                        }
                        else if (lsResultList.Count > 1)
                        {
                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "Configuration item: duplicate definition in Enumeration list");
                        }
                        else
                        {
                            iTagCodeIndex = m_lsEnumInfo[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM].IndexOf(lsResultList[0]);
                        }

                        if ((iTagCodeIndex != -1) && (iTagNameIndex != -1) && (iTagCodeIndex != iTagNameIndex))
                        {
                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_TagName and Configuration item are not mapped");
                        }

                        // Check Aladdin internal item
                        lsResultList = m_lsEnumInfo[(int)CI_ENUM_INFO_LIST_ID.ALADDIN_CI_ENUM].FindAll(x => x.m_sCITagCode.Equals(Parameter.GetTagName()));
                        if (lsResultList.Count != 0)
                        {
                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "CI_TagName: duplicate definition in internal Aladdin list");
                        }

                        lsResultList = m_lsEnumInfo[(int)CI_ENUM_INFO_LIST_ID.ALADDIN_CI_ENUM].FindAll(x => x.m_sCITagCode.Equals(Parameter.GetTagCode()));
                        if (lsResultList.Count != 0)
                        {
                            StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_DUPLICATE, "Configuration item: duplicate definition in internal Aladdin list");
                        }

                        if (StatisticData.GetStatisticDataList().Count > 0)
                        {
                            CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                        }
                    }
                }
            }
        }
    }
}
