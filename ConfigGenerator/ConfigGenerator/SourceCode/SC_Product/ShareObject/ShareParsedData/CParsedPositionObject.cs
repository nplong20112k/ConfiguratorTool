using System.Collections.Generic;

namespace ConfigGenerator
{
    public struct POSITION_PARSED_TYPE
    {
        public List<string> m_sPositionPath;
        public string       m_sGroup;
        public string       m_sCategories;
        public string       m_sIndex;
    }

    public class CParsedPositionObject
    {
        private string                      m_sUserVisibility    = null;
        private string                      m_sAladdinVisibility = null;
        private List<POSITION_PARSED_TYPE>  m_PositionList       = null;
        private List<string>                m_ModelNameList      = null;
        
        public CParsedPositionObject()
        { }

        public void SetUserVisibility (string sUserVisibility)
        {
            m_sUserVisibility = sUserVisibility;
        }

        public void SetAladdinVisibility(string sAladdinVisibility)
        {
            m_sAladdinVisibility = sAladdinVisibility;
        }

        public void SetPositionDataList(List<POSITION_PARSED_TYPE> sPositionDataList)
        {
            m_PositionList = null;
            m_PositionList = new List<POSITION_PARSED_TYPE>();
            m_PositionList.AddRange(sPositionDataList);
        }

        public void AddPossitionData(POSITION_PARSED_TYPE sPositionData)
        {
            if (m_PositionList == null)
            {
                m_PositionList = new List<POSITION_PARSED_TYPE>();
            }
            m_PositionList.Add(sPositionData);
        }

        public void SetModelNameList(List<string> ModelNameList)
        {
            m_ModelNameList = null;
            m_ModelNameList = new List<string>();
            m_ModelNameList.AddRange(ModelNameList);
        }

        public string GetUserVisibility()
        {
            return m_sUserVisibility;
        }

        public string GetAladdinVisibility()
        {
            return m_sAladdinVisibility;
        }

        public List<POSITION_PARSED_TYPE> GetPositionPathList()
        {
            return m_PositionList;
        }

        public List<string> GetModelNameList()
        {
            return m_ModelNameList;
        }
    }
}
