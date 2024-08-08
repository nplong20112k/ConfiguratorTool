namespace ConfigGenerator
{
    public class CCodeSpecialDeltaTableFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_SPECIAL_DELTA_TABLE_HEADER =
        {
        };
        private static readonly string[] CODE_REPORT_SPECIAL_DELTA_TABLE_FOOTER =
        {
            "\n",
        };

        public CCodeSpecialDeltaTableFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_DELTA_TABLE_FILE_PROCESSOR,
                    CODE_REPORT_SPECIAL_DELTA_TABLE_HEADER,
                    CODE_REPORT_SPECIAL_DELTA_TABLE_FOOTER)
        {
        }
    }
}

