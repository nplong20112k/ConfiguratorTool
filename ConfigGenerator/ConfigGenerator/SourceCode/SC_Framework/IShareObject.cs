namespace ConfigGenerator
{
    public enum SHARE_OBJECT_ID
    {
        SOB_ID_UNKNOW = 0,
        SOB_ID_STRING_OBJECT,     // DESCRIPTION
        SOB_ID_INPUT_DATA_OBJECT, // PARAMETER FROM INPUT FILE
        SOB_ID_PARSED_DATA_OBJECT,
        SOB_ID_INTEGRATED_DATA_OBJECT,
        SOB_ID_STATISTIC_DATA_OBJECT,
        SOB_ID_SOURCE_CONFIG_INFO_OBJECT,
        SOB_ID_XML_CONFIG_INFO_OBJECT,
        SOB_ID_INPUT_INFO_OBJECT,
    }

    public interface IShareObject
    {
        SHARE_OBJECT_ID GetObjectID();
    }

    public abstract class AShareObject: IShareObject
    {
        private SHARE_OBJECT_ID m_ObjectID = SHARE_OBJECT_ID.SOB_ID_UNKNOW;

        public AShareObject (SHARE_OBJECT_ID ObjectID)
        {
            m_ObjectID = ObjectID;
        }

        public SHARE_OBJECT_ID GetObjectID()
        {
            return m_ObjectID;
        }

        public string FillterEndLineChar(string sDataIn)
        {
            string sRet = null;

            if ((sDataIn != null) && (sDataIn != string.Empty))
            {
                sRet = sDataIn.Replace("\r\n", "\n");
                sRet = sRet.Replace("\r", "\n");
            }

            return sRet;
        }
    }
}
