namespace ConfigGenerator
{
    class CCodeNormalVerifyExceptionProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_VERIFY_EXCEPTION_HEADER =
        {
        };
        private static readonly string[] CODE_REPORT_VERIFY_EXCEPTION_FOOTER =
        {
            "\n",
        };

        public CCodeNormalVerifyExceptionProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_VERIFY_EXCEPTION_FILE_PROCESSOR,
                    CODE_REPORT_VERIFY_EXCEPTION_HEADER,
                    CODE_REPORT_VERIFY_EXCEPTION_FOOTER)
        {

        }
    }
}
