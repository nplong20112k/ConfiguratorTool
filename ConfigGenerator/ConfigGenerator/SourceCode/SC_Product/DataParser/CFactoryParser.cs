using System.Collections.Generic;

namespace ConfigGenerator
{
    public class CFactoryParser
    {
        private List<AParser> m_ParserList = null;

        private List<AFactoryParser> m_FactoryParserList = null;

        readonly static CFactoryParser m_Instance = new CFactoryParser();
        public CFactoryParser()
        {
            m_ParserList = new List<AParser>();
            m_FactoryParserList = new List<AFactoryParser>();

            // Create Child Factories
            if (m_FactoryParserList != null)
            {
                m_FactoryParserList.Add(CFactoryExcelParser.GetInstance()); // Factory for Excel parser
            }
        }

        public static CFactoryParser GetInstance()
        {
            return m_Instance;
        }

        public List<AParser> GetLComponentsList()
        {
            if (m_FactoryParserList != null)
            {
                for (int i = 0; i < m_FactoryParserList.Count; i++)
                {
                    try
                    {
                        m_ParserList.AddRange(m_FactoryParserList[i].GetLComponentsList());
                    }
                    catch {}
                }
                return m_ParserList;
            }
            return null;
        }
    }
}
