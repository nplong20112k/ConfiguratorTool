using System.Collections.Generic;

namespace ConfigGenerator
{
    public abstract class ABinSubHandler
    {
        public ABinSubHandler(EVENT_TYPE[] myEvent = null)
        {
        }

        public abstract bool Initialize(List<IShareObject> oDataList);
        public abstract bool DataHandling(IShareObject oDataIn);
        public abstract bool Finalize(IShareObject oDataIn);
    }
}