using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CInputInfoObject : AShareObject, INumberItemsProperties, IModelNameProperties
    {
        public CInputInfoObject()
            : base(SHARE_OBJECT_ID.SOB_ID_INPUT_INFO_OBJECT)
        {
        }

        private List<string>    m_lsModelName = null;
        private string          m_sNumber = null;

        public string GetNumberItems()
        {
            return m_sNumber;
        }

        public void SetNumberItems(string sNumber)
        {
            if (string.IsNullOrEmpty(sNumber) == false)
            {
                m_sNumber = sNumber;
            }
        }

        public List<string> GetModelName()
        {
            return m_lsModelName;
        }

        public void SetModelName(List<string> lsModelName)
        {
            if ((lsModelName != null) && (lsModelName.Count > 0))
            {
                m_lsModelName = lsModelName;
            }
        }
    }
}
