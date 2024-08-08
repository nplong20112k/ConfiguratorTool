using System.Collections.Generic;

namespace ConfigGenerator
{
    public abstract class AXMLSubHandler
    {
        public AXMLSubHandler()
        {
        }

        public abstract bool Initialize(List<IShareObject> oDataList);
        public abstract bool DataHandling(IShareObject oDataIn);
        public abstract bool Finalize(IShareObject oDataIn);
    }
}
