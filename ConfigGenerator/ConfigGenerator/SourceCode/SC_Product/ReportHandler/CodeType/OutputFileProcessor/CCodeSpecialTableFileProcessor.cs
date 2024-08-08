namespace ConfigGenerator
{
    public class CCodeSpecialTableFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_SPECIAL_TABLE_HEADER =
        {
        };

        private static readonly string[] CODE_REPORT_SPECIAL_TABLE_FOOTER =
        {
            "\n",
        };

        public CCodeSpecialTableFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_TABLE_FILE_PROCESSOR,
                  CODE_REPORT_SPECIAL_TABLE_HEADER,
                  CODE_REPORT_SPECIAL_TABLE_FOOTER)
        {
        }
    }
}