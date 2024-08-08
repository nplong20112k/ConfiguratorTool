namespace ConfigGenerator
{
    public class CCodeNormalAccessFuncFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_ACCESS_FUNCTION_HEADER =
        {
        };
        private static readonly string[] CODE_REPORT_ACCESS_FUNCTION_FOOTER =
        {
            "\n",
        };

        public CCodeNormalAccessFuncFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_ACCESS_FUNCTION_FILE_PROCESSOR,
                    CODE_REPORT_ACCESS_FUNCTION_HEADER,
                    CODE_REPORT_ACCESS_FUNCTION_FOOTER)
        {

        }
    }
}