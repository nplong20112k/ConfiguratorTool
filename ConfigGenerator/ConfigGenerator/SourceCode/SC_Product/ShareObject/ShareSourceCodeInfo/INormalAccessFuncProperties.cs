using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface INormalAccessFuncProperties : ICommonProperties
    {
        string GetNormalAccessFuncType();
        string GetNormalAccessFuncName();
        List<string[]> GetNormalAccessFuncParams();
    }
}
