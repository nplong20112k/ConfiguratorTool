namespace ConfigGenerator
{
    class CStructureIntegerator
    {
        private uint[] m_uiTempIndex = null;
        private uint[] m_uiTempBit = null;

        public CStructureIntegerator()
        {
        }

        public void EventHandler(EVENT_TYPE Event, IShareObject oData)
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

        public CStructure IntegratingProcess(CParsedParameterObject Parameter, string sMaxValue, ATableRef.TABLE_REF_TYPE eTableRefType)
        {
            CStructure StructureObject;
            if (eTableRefType == ATableRef.TABLE_REF_TYPE.TABLE_REF_CUSTOM)
            {
                StructureObject = IntegrateCustomItem(Parameter);
            }
            else
            {
                StructureObject = IntegrateSingleItem(Parameter, sMaxValue);
            }
            return StructureObject;
        }

        private CStructure IntegrateSingleItem(CParsedParameterObject Parameter, string sMaxValue)
        {
            string sSizeInBit = null;

            if (int.Parse(Parameter.GetTagValueSize()) == 1)
            {
                if (sMaxValue != null)
                {
                    sSizeInBit = CBitUltilizer.GetMostSignificantSetBit(uint.Parse(sMaxValue, System.Globalization.NumberStyles.HexNumber)).ToString();
                }
            }
            else
            {
                int iSizeInbit = int.Parse(Parameter.GetTagValueSize()) * 8;
                sSizeInBit = iSizeInbit.ToString();
            }

            return CreateStructureObject(Parameter.GetTagName(), sSizeInBit);
        }

        private CStructure IntegrateCustomItem(CParsedParameterObject Parameter)
        {
            string sSizeInBit = null;
            if (int.Parse(Parameter.GetTagValueSize()) == 1)
            {
                sSizeInBit = "8";
            }
            else
            {
                uint uiSizeInBit = uint.Parse(Parameter.GetTagValueSize()) * 8;
                sSizeInBit = uiSizeInBit.ToString();
            }
            return CreateStructureObject(Parameter.GetTagName(), sSizeInBit);
        }

        private CStructure CreateStructureObject(string sTagName, string sSizeInBit)
        {
            bool bFlagErrorDetected = false;
            CStatisticObject StatisticData = new CStatisticObject();

            // Detect error
            if ((sTagName == null) || (sTagName == ""))
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_TagName");
                bFlagErrorDetected = true;
            }

            if ((sSizeInBit == null) || (sSizeInBit == ""))
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_MISSING_DATA, "CI_ValueSizeBytes");
                bFlagErrorDetected = true;
            }

            if (sSizeInBit == "0")
            {
                StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "CI_ValueOptions: max value");
                bFlagErrorDetected = true;
            }

            if (sSizeInBit != null)
            {
                uint uiSizeInBit = uint.Parse(sSizeInBit);
                uint uiLocation = 0;
                if ((uiSizeInBit > 0) && (uiSizeInBit <= 8))
                {
                    if ((m_uiTempBit[(uiSizeInBit - 1)] + uiSizeInBit) <= 8)
                    {
                        uiLocation = m_uiTempBit[(uiSizeInBit - 1)];
                        m_uiTempBit[(uiSizeInBit - 1)] = m_uiTempBit[(uiSizeInBit - 1)] + uiSizeInBit;
                    }
                    else
                    {
                        m_uiTempIndex[(uiSizeInBit - 1)]++;
                        m_uiTempBit[(uiSizeInBit - 1)] = uiSizeInBit;
                        uiLocation = 0;
                    }

                    CStructure StructureObjectWithIndex = new CStructure(sTagName.ToLower(), sSizeInBit, m_uiTempIndex[(uiSizeInBit - 1)], uiLocation, uiSizeInBit);
                    return StructureObjectWithIndex;
                }
                else if ((uiSizeInBit % 8) != 0)
                {
                    StatisticData.AddStatistic(STATISTIC_TYPE.STATISTIC_INCORRECT_DATA_FORMAT, "CI_ValueOptions: max Value");
                    bFlagErrorDetected = true;
                }
            }

            if (bFlagErrorDetected == true)
            {
                CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_RESPOND_ERROR_OCCURED, StatisticData);
                return null;
            }

            CStructure StructureObject = new CStructure(sTagName.ToLower(), sSizeInBit);

            return StructureObject;
        }

        private void ReInitProperties()
        {
            m_uiTempIndex = new uint[8];
            m_uiTempBit = new uint[8];
        }
    }
}