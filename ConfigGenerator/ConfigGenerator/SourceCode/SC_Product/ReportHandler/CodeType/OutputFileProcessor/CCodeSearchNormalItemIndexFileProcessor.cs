namespace ConfigGenerator
{
    public class CCodeSearchNormalItemIndexFileProcessor : ACodeFileProcessor
    {
        private static readonly string[] CODE_REPORT_SEARCH_NORMAL_ITEM_INDEX_HEADER =
        {
        };

        private static readonly string[] CODE_REPORT_SEARCH_NORMAL_ITEM_INDEX_FOOTER =
        {
            "\n",
        };

        public CCodeSearchNormalItemIndexFileProcessor()
            : base(CODE_FILE_PROCESSOR_TYPE.CODE_NORMAL_SEARCH_ITEM_INDEX_FILE_PROCESSOR,
                    CODE_REPORT_SEARCH_NORMAL_ITEM_INDEX_HEADER,
                    CODE_REPORT_SEARCH_NORMAL_ITEM_INDEX_FOOTER)
        {
        }
    }
}
