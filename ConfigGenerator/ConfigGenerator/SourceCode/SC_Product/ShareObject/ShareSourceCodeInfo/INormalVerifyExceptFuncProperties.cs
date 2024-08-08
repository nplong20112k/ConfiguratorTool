using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface INormalVerifyExceptFuncProperties : ICommonProperties
    {
        string GetNormalVerifyExceptFuncType();
        string GetNormalVerifyExceptFuncName();
        List<string[]> GetNormalVerifyExceptFuncParams();
    }
}
