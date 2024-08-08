namespace ConfigGenerator
{
    public class CCodeInterfaceTypedefFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_INTERFACE_TYPEDEF_HEADER =
        {
            "#ifndef __CONFIG_ITEM_INTERFACE_TYPEDEF__\n",
            "#define __CONFIG_ITEM_INTERFACE_TYPEDEF__\n",
            "\n",
        };

        private static readonly string[] CODE_REPORT_INTERFACE_TYPEDEF_FOOTER =
        {
            "\n",
            "#endif\n",
            "\n",
        };

        public CCodeInterfaceTypedefFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_INTERFACE_TYPEDEF_FILE_PROCESSOR,
                  CODE_REPORT_INTERFACE_TYPEDEF_HEADER,
                  CODE_REPORT_INTERFACE_TYPEDEF_FOOTER)
        {
        }
    }
}
