namespace ConfigGenerator
{
    class CCodeSpecialVerifyRangeProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_VERIFY_RANGE_HEADER =
        {
        };
        private static readonly string[] CODE_REPORT_VERIFY_RANGE_FOOTER =
        {
            "\n",
        };

        public CCodeSpecialVerifyRangeProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_VERIFY_RANGE_FILE_PROCESSOR,
                    CODE_REPORT_VERIFY_RANGE_HEADER,
                    CODE_REPORT_VERIFY_RANGE_FOOTER)
        {

        }
    }
}