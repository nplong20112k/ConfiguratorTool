using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface ISpecialVerifyReadableAsciiFuncProperties : ICommonProperties
    {
        string GetSpecialVerifyReadableAsciiFuncType();
        string GetSpecialVerifyReadableAsciiFuncName();
        List<string[]> GetSpecialVerifyReadableAsciiFuncParams();
    }
}
