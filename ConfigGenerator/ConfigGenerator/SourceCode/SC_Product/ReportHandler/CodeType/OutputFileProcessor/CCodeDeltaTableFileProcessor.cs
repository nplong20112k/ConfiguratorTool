namespace ConfigGenerator
{
    public class CCodeDeltaTableFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_NORMAL_DELTA_TABLE_HEADER =
        {
            "#include \"ConfigItemHeader.h\"\n",
            "\n"
        };
        private static readonly string[] CODE_REPORT_NORMAL_DELTA_TABLE_FOOTER =
        {

        };

        public CCodeDeltaTableFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_DELTA_TABLE_FILE_PROCESSOR,
                    CODE_REPORT_NORMAL_DELTA_TABLE_HEADER,
                    CODE_REPORT_NORMAL_DELTA_TABLE_FOOTER)
        {
        }
    }
}

