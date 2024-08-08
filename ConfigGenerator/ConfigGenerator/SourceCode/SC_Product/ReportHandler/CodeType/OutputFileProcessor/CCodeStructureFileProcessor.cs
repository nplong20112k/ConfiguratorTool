namespace ConfigGenerator
{
    public class CCodeStructureFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_STRUCTURE_HEADER =
        {
            "#ifndef __CONFIG_ITEM_STRUCTURE__\n",
            "#define __CONFIG_ITEM_STRUCTURE__\n",
            "\n",
        };

        private static readonly string[] CODE_REPORT_STRUCTURE_FOOTER =
        {
            "\n",
            "#endif\n",
            "\n",
        };

        public CCodeStructureFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_STRUCTURE_FILE_PROCESSOR,
                  CODE_REPORT_STRUCTURE_HEADER,
                  CODE_REPORT_STRUCTURE_FOOTER)
        {
        }
    }
}