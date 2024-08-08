using System.Collections.Generic;

namespace ConfigGenerator
{
    class CFactoryIntegrator
    {
        private List<AIntegerator> m_IntegratorList = null;
        private List<AFactoryIntegrator> m_FactoryIntegratorList = null;

        private static readonly CFactoryIntegrator m_Instance = new CFactoryIntegrator();
        public CFactoryIntegrator()
        {
            m_IntegratorList = new List<AIntegerator>();
            m_FactoryIntegratorList = new List<AFactoryIntegrator>();

            // Create Child Factories
            if (m_FactoryIntegratorList != null)
            {
                m_FactoryIntegratorList.Add(CFactoryXmlIntegrator.GetInstance()); // Factory for XML Integrator Type.
                m_FactoryIntegratorList.Add(CFactorySourceCodeIntegrator.GetInstance()); // Factory for SourceCode Integrator Type.
                m_FactoryIntegratorList.Add(CFactoryAutoTestIntegrator.GetInstance()); // Factory for SourceCode Integrator Type.
            }
        }

        public static CFactoryIntegrator GetInstance()
        {
            return m_Instance;
        }

        public List<AIntegerator> GetIntegratorList()
        {
            if ((m_IntegratorList != null) && (m_FactoryIntegratorList != null))
            {
                for (int i = 0; i < m_FactoryIntegratorList.Count; i++)
                {
                    try
                    {
                        m_IntegratorList.AddRange(m_FactoryIntegratorList[i].GetIntegratorList());
                    }
                    catch { }
                }
                return m_IntegratorList;
            }

            return null;
        }
    }
}
