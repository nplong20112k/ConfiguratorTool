namespace ConfigGenerator
{
    public class CStructure
    {
        private string m_sTagName;
        private string m_sTagSizeInBit;
        private uint[] m_uiIndex = new uint[8];
        private uint[] m_uiLocation = new uint[8];
        private uint[] m_uiBitLength = new uint[8];


        public CStructure(string sTagName, string sTagSizeInBit)
        {
            m_sTagName = sTagName;
            m_sTagSizeInBit = sTagSizeInBit;
        }

        public CStructure(string sTagName, string sTagSizeInBit, uint uiIndex, uint uiLocation, uint uiBitLength)
        {
            m_sTagName = sTagName;
            m_sTagSizeInBit = sTagSizeInBit;
            m_uiIndex[(uiBitLength - 1)] = uiIndex;
            m_uiLocation[(uiBitLength - 1)] = uiLocation;
            m_uiBitLength[(uiBitLength - 1)] = uiBitLength;
        }

        public string GetTagName()
        {
            return m_sTagName;
        }

        public string GetTagSizeInBit()
        {
            return m_sTagSizeInBit;
        }

        public uint GetIndex(uint uiBitLength)
        {
            return m_uiIndex[(uiBitLength - 1)];
        }

        public uint GetLocation(uint uiBitLength)
        {
            return m_uiLocation[(uiBitLength - 1)];
        }

        public uint GetBitLength(uint uiBitLength)
        {
            return m_uiBitLength[(uiBitLength - 1)];
        }
    }
}