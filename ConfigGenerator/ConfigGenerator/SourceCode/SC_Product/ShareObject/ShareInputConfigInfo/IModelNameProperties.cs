using System.Collections.Generic;

namespace ConfigGenerator
{
    interface IModelNameProperties
    {
        List<string> GetModelName();
        void SetModelName(List<string> lsModelName);
    }
}
