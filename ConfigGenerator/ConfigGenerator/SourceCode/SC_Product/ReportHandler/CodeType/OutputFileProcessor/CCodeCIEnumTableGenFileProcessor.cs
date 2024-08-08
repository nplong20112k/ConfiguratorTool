
namespace ConfigGenerator
{ 
    class CCodeCIEnumTableGenFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_CI_ENUM_HEADER =
        {
            "#ifndef __CONFIG_ITEM_ENUM__\n",
            "#define __CONFIG_ITEM_ENUM__\n",
            "\n",
        };

        private static readonly string[] CODE_REPORT_CI_ENUM_FOOTER =
        {
            "\n",
            "#endif\n",
            "\n",
        };

        public CCodeCIEnumTableGenFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_CI_ENUM_FILE_PROCESSOR,
                  CODE_REPORT_CI_ENUM_HEADER,
                  CODE_REPORT_CI_ENUM_FOOTER)
        {
        }
    }
}
