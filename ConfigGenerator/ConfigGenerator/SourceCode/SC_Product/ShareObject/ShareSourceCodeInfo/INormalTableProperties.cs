using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface INormalTableProperties : ICommonProperties
    {
        string GetNormalTableType();
        string GetNormalTableName();
        string GetNormalTableGetFunc();
        string GetNormalTableSizeFuncName();
        string GetNormalTableSizeFuncType();
        List<TABLE_STRUCT_DATA_TYPE> GetNormalTableElementOrder();
    }
}
