using System.Collections.Generic;

namespace ConfigGenerator
{
    public class Event_CTableRefIntegrator
    {
        public EVENT_TYPE[] m_MyEvent = new EVENT_TYPE[]
        {
            EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO,
        };
    }

    class CTableRefIntegrator: AIntegerator
    {
        public enum VERIFY_STATUS
        {
            VERIFY_UNKNOW = 0,
            VERIFY_NEW_TABLE,
            VERIFY_TABLE_EXIST,
            VERIFY_TABLE_ERROR,
        };
        
        private List<AXmlTable>        m_TableRefSelectionList     = null;
        private List<AXmlTable>        m_TableRefHexRangeList      = null;
        private List<AXmlTable>        m_TableRefIntRangeList      = null;

        public CTableRefIntegrator()
            : base(INTEGRATOR_PRIORITY_ID.INTEGRATOR_PRIORITY_TABLE_REF, new Event_CTableRefIntegrator().m_MyEvent)
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
            ATableRef TableRefFromDataIn = null;
            List<AXmlTable> XmlTableList = null;

            if (oDataIn != null && oDataOut != null)
            {
                if (oDataIn.GetObjectID() == SHARE_OBJECT_ID.SOB_ID_PARSED_DATA_OBJECT)
                {
                    TableRefFromDataIn = (oDataIn as CParsedDataObject).GetTableRef();
                    if (TableRefFromDataIn != null)
                    {
                        XmlTableList = new List<AXmlTable>();

                        switch (TableRefFromDataIn.GetTableRefType())
                        {
                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII:
                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                                {
                                    AXmlTable TempXmlTable = null;
                                    TempXmlTable = IntegrateStandardType(TableRefFromDataIn);

                                    if (TempXmlTable != null)
                                    {
                                        XmlTableList.Add(TempXmlTable);
                                    }
                                }
                                break;

                            case ATableRef.TABLE_REF_TYPE.TABLE_REF_CUSTOM:
                                {
                                    List<CTableRefCustom.SUB_TABLE_TYPE> TempSubTableRefList = (TableRefFromDataIn as CTableRefCustom).GetSubTableRefList();

                                    if ((TempSubTableRefList != null) && (TempSubTableRefList.Count > 0))
                                    {
                                        for (int i = 0; i < TempSubTableRefList.Count; i++)
                                        {
                                            AXmlTable TempXmlTable = null;
                                            CTableRefCustom.SUB_TABLE_TYPE TempSubTable = new CTableRefCustom.SUB_TABLE_TYPE();
                                            TempSubTable.m_sSubContext = TempSubTableRefList[i].m_sSubContext;
                                            TempSubTable.m_sSubName = TempSubTableRefList[i].m_sSubName;
                                            TempSubTable.m_sSubSize = TempSubTableRefList[i].m_sSubSize;

                                            TempXmlTable = IntegrateStandardType(TempSubTableRefList[i].m_sSubTableRef);

                                            if (TempXmlTable != null)
                                            {
                                                XmlTableList.Add(TempXmlTable);
                                            }
                                        }
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                    }
                    
                    if ((XmlTableList != null) && (XmlTableList.Count > 0))
                    {
                        oDataOut.SetTableRefList(XmlTableList);
                    }
                    LogIntegrateProcess(TableRefFromDataIn, XmlTableList);
                }
            }
            else
            {
                // TODO:
            }
        }

        private void ReInitProperties()
        {
            m_TableRefSelectionList = new List<AXmlTable>();
            m_TableRefHexRangeList  = new List<AXmlTable>();
            m_TableRefIntRangeList  = new List<AXmlTable>();
        }

        private AXmlTable IntegrateStandardType(ATableRef TableRefIn)
        {
            List<AXmlTable>     TempList        = null;
            AXmlTable           XmlTableRet     = null;
            bool                bFlagExist      = false;

            if (TableRefIn != null)
            {
                switch (TableRefIn.GetTableRefType())
                {
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                        {
                            XmlTableRet = new CEnumTable();
                            TempList = m_TableRefSelectionList;
                        }break;

                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII:
                        {
                            XmlTableRet = new CHexRangeTable();
                            TempList = m_TableRefHexRangeList;
                        }break;
    
                    case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                        {
                            if (TableRefIn.GetTableRefValueIsDecimal() == true)
                            {
                                XmlTableRet = new CIntRangeTable();
                                TempList = m_TableRefIntRangeList;
                            }
                            else
                            {
                                XmlTableRet = new CHexRangeTable();
                                TempList = m_TableRefHexRangeList;
                            }
                        }break;

                    default:
                        break;
                }

                if ((TempList != null) && (XmlTableRet != null))
                {
                    ATableRef WorkingTableRef = TableRefIn;
                    if (TableRefIn.GetTableRefValueIsDecimal() == true)
                    {
                        WorkingTableRef = TableRefIn.GetDecimalInstance();
                    }
                
                    if (TempList.Count > 0)
                    {
                        foreach (AXmlTable element in TempList)
                        {
                            if (element.CompareContent(WorkingTableRef))
                            {
                                TableRefIn.SetTableRefName(element.GetName());
                                bFlagExist = true;
                                break;
                            }
                        }
                    }

                    if (bFlagExist == false)
                    {
                        if (XmlTableRet.CopyContent(WorkingTableRef))
                        {
                            XmlTableRet.SetName(TempList.Count.ToString());
                            TableRefIn.SetTableRefName(XmlTableRet.GetName());

                            TempList.Add(XmlTableRet);
                            return XmlTableRet;
                        }
                    }
                }
            }

            return null;
        }

        private void LogIntegrateProcess(ATableRef TableRefOut, List<AXmlTable> XmlTableList)
        {
            bool bFlagErrorDetected = false;
            CStatisticObject StatisticData = new CStatisticObject();

            if (TableRefOut != null)
            {
                if ((XmlTableList != null) && (XmlTableList.Count > 0))
                {
                    CLogger.GetInstance().Log("New !");
                    for (int i = 0; i < XmlTableList.Count; i++)
                    {
                        CLogger.GetInstance().Log(XmlTableList[i].GetName());
                    }
                }
                else
                {
                    switch (TableRefOut.GetTableRefType())
                    {
                        case ATableRef.TABLE_REF_TYPE.TABLE_REF_ASCII:
                        case ATableRef.TABLE_REF_TYPE.TABLE_REF_READABLE_ASCII:
                        case ATableRef.TABLE_REF_TYPE.TABLE_REF_RANGE:
                        case ATableRef.TABLE_REF_TYPE.TABLE_REF_SELECTION:
                            if (TableRefOut.GetTableRefName() != null)
                            {
                                CLogger.GetInstance().Log("Exist !");
                                CLogger.GetInstance().Log(TableRefOut.GetTableRefName());
                            }
                            else
                            {
                                CLogger.GetInstance().Log("Error: Integrate Error !");
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: table ref invalid");
                                bFlagErrorDetected = true;
                            }
                            break;

                        case ATableRef.TABLE_REF_TYPE.TABLE_REF_CUSTOM:
                            List<CTableRefCustom.SUB_TABLE_TYPE> TempSubTableRefList = (TableRefOut as CTableRefCustom).GetSubTableRefList();

                            if (
                                    (TempSubTableRefList != null) &&
                                    (TempSubTableRefList.Count > 0)
                               )
                            {
                                for (int i = 0; i < TempSubTableRefList.Count; i++)
                                {
                                    if (TempSubTableRefList[i].m_sSubTableRef.GetTableRefName() != null)
                                    {
                                        CLogger.GetInstance().Log("Exist !");
                                        CLogger.GetInstance().Log(TempSubTableRefList[i].m_sSubTableRef.GetTableRefName());
                                    }
                                    else
                                    {
                                        CLogger.GetInstance().Log("Error: Integrate Error !");
                                        StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: sub-table ref invalid");
                                        bFlagErrorDetected = true;
                                    }
                                }
                            }
                            else
                            {
                                CLogger.GetInstance().Log("Error: Integrate Error !");
                                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueOptions: table ref invalid");
                                bFlagErrorDetected = true;
                            }
                            break;

                        default:
                            CLogger.GetInstance().Log("Error: Unknow !");
                            break;
                    }
                }
            }
            else
            {
                CLogger.GetInstance().Log("Error: Table Empty !");
            }

            if (bFlagErrorDetected == true)
            {
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
            }
        }
    }
}
