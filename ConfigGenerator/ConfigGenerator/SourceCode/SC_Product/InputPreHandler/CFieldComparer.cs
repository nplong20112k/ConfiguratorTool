using System.Collections.Generic;
using System.Linq;

namespace ConfigGenerator
{
    public class FieldComparer : IComparer<string>
    {
        private List<string> m_sPredefinedArray = null;

        public FieldComparer(string[] sPredefinedArray)
        {
            m_sPredefinedArray = sPredefinedArray.ToList();
        }

        public int Compare(string x, string y)
        {
            return m_sPredefinedArray.IndexOf(x).CompareTo(m_sPredefinedArray.IndexOf(y));
        }
    }
}
