namespace ConfigGenerator
{
    public class CCodeNormalDeltaTableFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_NORMAL_DELTA_TABLE_HEADER =
        {
        };
        private static readonly string[] CODE_REPORT_NORMAL_DELTA_TABLE_FOOTER =
        {
            "\n",
        };

        public CCodeNormalDeltaTableFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_DELTA_TABLE_FILE_PROCESSOR,
                    CODE_REPORT_NORMAL_DELTA_TABLE_HEADER,
                    CODE_REPORT_NORMAL_DELTA_TABLE_FOOTER)
        {
        }
    }
}

