using System.Collections.Generic;

namespace ConfigGenerator
{
    public abstract class AFactoryParser
    {
        private List<AParser> m_ParserList = null;

        public AFactoryParser()
        {
            m_ParserList = new List<AParser>();
        }

        public bool AddParser(AParser oParser)
        {
            bool bRet = false;
            if (oParser != null)
            {
                m_ParserList.Add(oParser);
                bRet = true;
            }

            return bRet;
        }

        public List<AParser> GetLComponentsList()
        {
            return m_ParserList;
        }
    }
}
