using System.Collections.Generic;

namespace ConfigGenerator
{
    public interface IVerifyInterfaceValueFuncProperties : ICommonProperties
    {
        string GetVerifyInterfaceValueFuncType();
        string GetVerifyInterfaceValueFuncName();
        List<string[]> GetVerifyInterfaceValueFuncParams();
    }
}
