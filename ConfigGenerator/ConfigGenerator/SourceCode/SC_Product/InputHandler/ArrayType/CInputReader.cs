using System.Collections.Generic;

namespace ConfigGenerator
{

    class CSpupportedModelInputReader : AInputReader
    {
        public CSpupportedModelInputReader()
            : base(INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR)
        {
        }

        override public void GetConfigInfo (CInputInfoObject oInfoObject)
        {
            string[] RefFieldList = null;
            RefFieldList = CFieldID.GetInstance().GetFieldIDList();

            int iNumberParam = GetNumberParameter();
            int iParamIndex = GetParameterIDIndex(RefFieldList[(int)FIELD_ID_TYPE.MODEL_PRODUCT]);

            List<string> lsReturn = new List<string>();
            string[] sTemp = null;

            for (int i = 1; i <= iNumberParam; i++)
            {
                sTemp = m_FileHandler.ReadElement(iParamIndex, i).Split('\n');

                foreach (string Element in sTemp)
                {
                    if ((string.IsNullOrEmpty(Element) == false) &&
                        (lsReturn.Contains(Element) == false))
                    {
                        lsReturn.Add(Element);
                    }
                }
            }

            oInfoObject.SetModelName(lsReturn);
        }
    }

    class CMainInputReader : AInputReader
    {
        public CMainInputReader()
            : base(INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR)
        {
        }
    }

    class AInputReader : IInputReader
    {
        const int FIELD_NAME_PARAM_IDX = 0;
        const int PARAMETER_START_IDX = 0;

        protected IInputFileProcessor m_FileHandler = null;
        private INPUT_FILE_PROCESSOR_TYPE m_FileProcessorType;

        public AInputReader(INPUT_FILE_PROCESSOR_TYPE FileProcessorType)
        {
            m_FileProcessorType = FileProcessorType;
            m_FileHandler = CFactoryInputFileProcessor.GetInstance().GetInputFileProcessor(m_FileProcessorType);
        }

        public void GetInputParameter(int iIndex, CInputParameterObject oCurObject)
        {
            if (oCurObject == null)
            {
                return;
            }

            if (m_FileHandler.IsFileOpen() == true)
            {
                if (oCurObject.IsEmpty() == true)
                {
                    string[] RefFieldList = null;
                    string sTemp = null;

                    RefFieldList = CFieldID.GetInstance().GetFieldIDList();

                    int iNumberValidField = m_FileHandler.GetNumberValidField();

                    for (int i = 0; i < iNumberValidField; i++)
                    {
                        sTemp = m_FileHandler.ReadElement(i, FIELD_NAME_PARAM_IDX);
                        for (int j = 0; j < RefFieldList.Length; j++)
                        {
                            if (RefFieldList[j] == sTemp)
                            {
                                sTemp = m_FileHandler.ReadElement(i, iIndex);
                                oCurObject.SetValueField(j, sTemp);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // TODO
                }
            }
        }

        public void UpdateInputParameter (CInputParameterObject oCurObject)
        {
            if (oCurObject == null)
            {
                return;
            }

            if ((oCurObject.GetCITagCode() != string.Empty) &&
                (oCurObject.GetCITagCode() != null))
            {
                string[] RefFieldList = null;
                string sTemp = null;

                RefFieldList = CFieldID.GetInstance().GetFieldIDList();

                int iNumberParam = GetNumberParameter();
                int iParamIndex = GetParameterIDIndex(RefFieldList[(int)FIELD_ID_TYPE.TAG_CODE]);

                for (int i = FIELD_NAME_PARAM_IDX + 1; i <= iNumberParam; i++)
                {
                    sTemp = m_FileHandler.ReadElement(iParamIndex, i);
                    if (oCurObject.GetCITagCode() == sTemp)
                    {
                        int iNumberValidField = m_FileHandler.GetNumberValidField();

                        for (int j = 0; j < iNumberValidField; j++)
                        {
                            sTemp = m_FileHandler.ReadElement(j, FIELD_NAME_PARAM_IDX);
                            for (int k = 0; k < RefFieldList.Length; k++)
                            {
                                if (RefFieldList[k] == sTemp)
                                {
                                    sTemp = m_FileHandler.ReadElement(j, i);
                                    oCurObject.SetValueField(k, sTemp);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                // TODO
            }
        }

        public int GetNumberParameter ()
        {
            int iNumberParam = 0;
            string[] RefFieldList = null;

            if (m_FileHandler.IsFileOpen() == true)
            {
                RefFieldList = CFieldID.GetInstance().GetFieldIDList();

                int iParamIDIndex = GetParameterIDIndex(RefFieldList[(int)FIELD_ID_TYPE.TAG_CODE]);
                if (iParamIDIndex != -1)
                {
                    iNumberParam = m_FileHandler.CountNumberParamValid(iParamIDIndex);
                }
            }

            return iNumberParam;
        }

        virtual public void GetConfigInfo(CInputInfoObject oInfoObject)
        {
        }

        protected int GetParameterIDIndex (string sParameterID)
        {
            int iIndex = -1;

            int iNumberValidField = m_FileHandler.GetNumberValidField();
            for (int i = 0; i < iNumberValidField; i++)
            {
                string temp = m_FileHandler.ReadElement(i, FIELD_NAME_PARAM_IDX);
                if (sParameterID == temp)
                {
                    iIndex = i;
                    break;
                }
            }

            return iIndex;
        }
    }
}
