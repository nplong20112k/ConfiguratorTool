using System.Collections.Generic;

namespace ConfigGenerator
{
    public abstract class AFactoryIntegrator
    {
        protected List<AIntegerator> m_IntegratorList = null;

        public AFactoryIntegrator()
        {
            m_IntegratorList = new List<AIntegerator>();
        }

        public List<AIntegerator> GetIntegratorList()
        {
            return m_IntegratorList;
        }

        protected void AddComponentToList(AIntegerator Component)
        {
            if (m_IntegratorList.Count == 0)
            {
                this.m_IntegratorList.Add(Component);
            }
            else
            {
                for (int i = 0; i < m_IntegratorList.Count; i++)
                {
                    if (Component.GetPriority() < m_IntegratorList[i].GetPriority())
                    {
                        m_IntegratorList.Insert(i, Component);
                        break;
                    }
                    else if (i == (m_IntegratorList.Count - 1))
                    {
                        m_IntegratorList.Add(Component);
                        break;
                    }
                }
            }
        }
    }
}
