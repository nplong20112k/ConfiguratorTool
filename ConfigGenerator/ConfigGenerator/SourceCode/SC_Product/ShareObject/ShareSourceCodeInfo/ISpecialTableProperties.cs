using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface ISpecialTableProperties : ICommonProperties
    {
        string GetSpecialTableType();
        string GetSpecialTableName();
        string GetSpecialTableGetFunc();
        string GetCmVariableName();
        List<TABLE_STRUCT_DATA_TYPE> GetSpecialTableElementOrder();
    }
}
