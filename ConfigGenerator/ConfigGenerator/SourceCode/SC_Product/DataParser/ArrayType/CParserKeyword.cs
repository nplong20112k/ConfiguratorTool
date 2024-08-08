namespace ConfigGenerator
{
    public class CParserKeyword
    {
        public string KEYWORD_MODEL_NAME    = "##MODEL##";

        public enum SUB_CATEGORY_IDX
        {
            POSITION_IDX = 0,
            CATEGORY_IDX
        };

        public char KEYWORD_SPLIT_LINE      = '\n';
        public char KEYWORD_SPLIT_POSITION  = '_';

        CParserKeyword ()
        {
        }

        readonly static CParserKeyword m_Instance = new CParserKeyword();

        public static CParserKeyword GetInstance()
        {
            return m_Instance;
        }
    }
}