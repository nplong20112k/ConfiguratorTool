namespace ConfigGenerator
{
    class CCodeConvertHexIntProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_CONVERT_HEX_INT_HEADER =
        {
        };
        private static readonly string[] CODE_REPORT_CONVERT_HEX_INT_FOOTER =
        {
            "\n",
        };

        public CCodeConvertHexIntProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_CONVERT_HEX_INT_FILE_PROCESSOR,
                    CODE_REPORT_CONVERT_HEX_INT_HEADER,
                    CODE_REPORT_CONVERT_HEX_INT_FOOTER)
        {

        }
    }
}