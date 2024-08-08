
namespace ConfigGenerator
{
    class CCodeCIEnumTableCopyFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_CI_ENUMERATION_HEADER =
        {
            "#ifndef __CONFIG_ITEM_ENUMERATION__\n",
            "#define __CONFIG_ITEM_ENUMERATION__\n",
            "\n",
        };

        private static readonly string[] CODE_REPORT_CI_ENUMERATION_FOOTER =
        {
            "\n",
            "#endif\n",
            "\n",
        };

        public CCodeCIEnumTableCopyFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_CI_ENUMERATION_FILE_PROCESSOR,
                  CODE_REPORT_CI_ENUMERATION_HEADER,
                  CODE_REPORT_CI_ENUMERATION_FOOTER)
        {
        }
    }
}
