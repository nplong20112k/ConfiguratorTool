namespace ConfigGenerator
{
    public class CCodeVerifyInterfaceFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_VERIFY_INTERFACE_HEADER =
        {
        };

        private static readonly string[] CODE_REPORT_VERIFY_INTERFACE_FOOTER =
        {
            "\n",
        };

        public CCodeVerifyInterfaceFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_VERIFY_INTERFACE_FILE_PROCESSOR,
                  CODE_REPORT_VERIFY_INTERFACE_HEADER,
                  CODE_REPORT_VERIFY_INTERFACE_FOOTER)
        {
        }
    }
}
