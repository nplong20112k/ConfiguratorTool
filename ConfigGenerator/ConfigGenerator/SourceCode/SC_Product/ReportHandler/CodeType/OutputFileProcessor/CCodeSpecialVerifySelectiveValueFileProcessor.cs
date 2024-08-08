namespace ConfigGenerator
{
    class CCodeSpecialVerifySelectiveValueFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_SPECIAL_VERIFY_SELECTION_VALUE_HEADER =
{
        };

        private static readonly string[] CODE_REPORT_SPECIAL_VERIFY_SELECTION_VALUE_FOOTER =
        {
            "\n",
        };

        public CCodeSpecialVerifySelectiveValueFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_VERIFY_SELECTION_VALUE_FILE_PROCESSOR,
                  CODE_REPORT_SPECIAL_VERIFY_SELECTION_VALUE_HEADER,
                  CODE_REPORT_SPECIAL_VERIFY_SELECTION_VALUE_FOOTER)
        {
        }
    }
}
