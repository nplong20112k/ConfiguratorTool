namespace ConfigGenerator
{
    class CCodeSpecialVerifyReadableAsciiFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_HEADER =
        {
        };

        private static readonly string[] CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_FOOTER =
        {
            "\n",
        };

        public CCodeSpecialVerifyReadableAsciiFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_SPECIAL_VERIFY_READABLE_ASCII_FILE_PROCESSOR,
                  CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_HEADER,
                  CODE_REPORT_SPECIAL_VERIFY_READABLE_ASCII_FOOTER)
        {
        }
    }
}
