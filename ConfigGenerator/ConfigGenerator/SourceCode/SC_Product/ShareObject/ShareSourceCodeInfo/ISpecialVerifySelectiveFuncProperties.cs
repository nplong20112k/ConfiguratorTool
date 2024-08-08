using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface ISpecialVerifySelectiveFuncProperties : ICommonProperties
    {
        string GetSpecialVerifySelectiveFuncType();
        string GetSpecialVerifySelectiveFuncName();
        List<string[]> GetSpecialVerifySelectiveFuncParams();
    }
}
