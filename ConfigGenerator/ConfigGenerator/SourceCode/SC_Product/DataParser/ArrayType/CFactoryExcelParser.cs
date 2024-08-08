namespace ConfigGenerator
{
    public class CFactoryExcelParser : AFactoryParser
    {
        readonly static CFactoryExcelParser m_Instance = new CFactoryExcelParser();
        public CFactoryExcelParser()
        {
            // Create parser components
            AParser TableRefParser = new CTableRefParser();
            AParser ParameterParser = new CParameterParser();
            AParser RuleParser = new CRuleParser();
            AParser PositionParser = new CPositionParser();

            // Add to list components
            AddParser(TableRefParser);
            AddParser(ParameterParser);
            AddParser(RuleParser);
            AddParser(PositionParser);

            CEventForwarder.GetInstance().Register((IEventReceiver)TableRefParser);
        }

        public static CFactoryExcelParser GetInstance()
        {
            return m_Instance;
        }
    }
}
