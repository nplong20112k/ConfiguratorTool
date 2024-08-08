namespace ConfigGenerator
{
    public class CCodeNormalTableFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_NORMAL_TABLE_HEADER =
        {
        };

        private static readonly string[] CODE_REPORT_NORMAL_TABLE_FOOTER =
        {
            "\n",
        };

        public CCodeNormalTableFileProcessor()
            : base (CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_TABLE_FILE_PROCESSOR,
                    CODE_REPORT_NORMAL_TABLE_HEADER,
                    CODE_REPORT_NORMAL_TABLE_FOOTER)
        {
        }
    }
}