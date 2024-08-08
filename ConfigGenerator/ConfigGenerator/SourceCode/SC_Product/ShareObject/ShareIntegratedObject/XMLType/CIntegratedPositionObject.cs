using System.Collections.Generic;

namespace ConfigGenerator
{
    public struct POSTION_INTEGRATED_TYPE
    {
        public List<string> m_sPositionPath;
        public string       m_sGroup;
        public string       m_sFieldName;
        public string       m_sModelName;
        public string       m_sIndex;
    }

    public class CIntegratedPositionObject
    {
        public enum ROOT_PAGE_TYPE
        {
            ROOT_PAGE_UNDEF = 0,
            ROOT_PAGE_BASIC,
            ROOT_PAGE_EXPERT,
        }

        private ROOT_PAGE_TYPE                m_RootPage      = ROOT_PAGE_TYPE.ROOT_PAGE_UNDEF;
        private List<POSTION_INTEGRATED_TYPE> m_PositionList  = null;

        public CIntegratedPositionObject()
        { }

        public void SetRootPage(ROOT_PAGE_TYPE RootPage)
        {
            m_RootPage = RootPage;
        }

        public void SetPositionDataList(List<POSTION_INTEGRATED_TYPE> sPositionDataList)
        {
            m_PositionList = null;
            m_PositionList = new List<POSTION_INTEGRATED_TYPE>();
            m_PositionList.AddRange(sPositionDataList);
        }

        public void AddPositionData(POSTION_INTEGRATED_TYPE PositionData)
        {
            if (m_PositionList == null)
            {
                m_PositionList = new List<POSTION_INTEGRATED_TYPE>();
            }
            m_PositionList.Add(PositionData);
        }

        public ROOT_PAGE_TYPE GetRootPage()
        {
            return m_RootPage;
        }

        public List<POSTION_INTEGRATED_TYPE> GetPositionDataList()
        {
            return m_PositionList;
        }
    }
}
