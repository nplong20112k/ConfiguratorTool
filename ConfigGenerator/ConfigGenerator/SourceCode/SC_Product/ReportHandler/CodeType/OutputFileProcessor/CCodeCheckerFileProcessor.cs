namespace ConfigGenerator
{
    public class CCodeCheckerFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_CHECK_FILE_HEADER =
{
            "#ifndef __CONFIG_ITEM_HEADER__\n",
            "#define __CONFIG_ITEM_HEADER__\n",
            "\n",
            "#define SUPPORT_BY_TOOL_GENERATOR\n",
            "#if defined (SUPPORT_BY_TOOL_GENERATOR)\n",
            "\n",
        };

        private static readonly string[] CODE_REPORT_CHECK_FILE_FOOTER =
        {
            "\n",
            "#endif\n",
            "\n",
            "#endif\n",
            "\n",
        };

        public CCodeCheckerFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_CHECKER,
                  CODE_REPORT_CHECK_FILE_HEADER,
                  CODE_REPORT_CHECK_FILE_FOOTER)
        {

        }
    }
}