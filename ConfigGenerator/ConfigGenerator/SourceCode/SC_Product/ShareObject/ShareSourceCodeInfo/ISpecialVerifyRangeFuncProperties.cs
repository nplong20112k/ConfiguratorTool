using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface ISpecialVerifyRangeFuncProperties : ICommonProperties
    {
        string GetSpecialVerifyRangeFuncType();
        string GetSpecialVerifyRangeFuncName();
        List<string[]> GetSpecialVerifyRangeFuncParams();
    }
}