namespace ConfigGenerator
{
    public enum STATISTIC_TYPE
    {
        STATISTIC_ITEM_UNKNOW = 0,

        STATISTIC_ITEM_OK,
        STATISTIC_MISSING_DATA,
        STATISTIC_INCORRECT_DATA_FORMAT,
        STATISTIC_INCORRECT_HEX_DATA_FORMAT,
        STATISTIC_INCORRECT_INT_DATA_FORMAT,
        STATISTIC_DUPLICATE,
    };

    public struct STATISTIC_DATA_TYPE
    {
        public STATISTIC_TYPE   m_StatisticType;
        public string           m_sCustomString;
    }

    public class CStatisticObject : AShareObject
    {
        private List<STATISTIC_DATA_TYPE>   m_StatisticDataList = null;
        private string                      m_sTagCode = null;
        private string                      m_sTagName = null;

        public CStatisticObject()
            : base(SHARE_OBJECT_ID.SOB_ID_STATISTIC_DATA_OBJECT)
        {
            m_StatisticDataList = new List<STATISTIC_DATA_TYPE>();
        }

        public CStatisticObject(string sTagCode, string sTagName)
            : base(SHARE_OBJECT_ID.SOB_ID_STATISTIC_DATA_OBJECT)
        {
            m_StatisticDataList = new List<STATISTIC_DATA_TYPE>();
            m_sTagCode = sTagCode;
            m_sTagName = sTagName;
        }

        public CStatisticObject(STATISTIC_TYPE eType, string sCustomString)
            : base(SHARE_OBJECT_ID.SOB_ID_STATISTIC_DATA_OBJECT)
        {
            m_StatisticDataList = new List<STATISTIC_DATA_TYPE>();
            AddStatistic(eType, sCustomString);
        }

        public void AddStatistic(STATISTIC_TYPE eType, string sCustomString)
        {
            if (m_StatisticDataList != null)
            {
                STATISTIC_DATA_TYPE TempData = new STATISTIC_DATA_TYPE();

                TempData.m_StatisticType = eType;
                TempData.m_sCustomString = sCustomString;

                m_StatisticDataList.Add(TempData);
            }
        }

        public List<STATISTIC_DATA_TYPE> GetStatisticDataList()
        {
            return m_StatisticDataList;
        }

        public string GetTagCode ()
        {
            return m_sTagCode;
        }

        public string GetTagName()
        {
            return m_sTagName;
        }
    }
}
