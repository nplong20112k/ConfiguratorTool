using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ConfigGenerator
{
    class CSpupportedModelInputChecker : AInputChecker
    {
        public CSpupportedModelInputChecker()
            : base(INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR)
        {
        }
    }

    class CMainInputChecker: AInputChecker
    {
        public CMainInputChecker()
            : base (INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR)
        {
        }

        protected override bool CheckFileStructure()
        {
            // Check fixed part of main input file
            bool bResult = false;
            bool bFixedCheck = base.CheckFileStructure();

            if (bFixedCheck == true)
            {
                // Check flexible part
                List<string> lsFlexibleColumn = new List<string>();
                int iNumberValidField = m_FileHandler.GetNumberValidField();
                for (int i = 0; i < iNumberValidField; i++)
                {
                    string temp = m_FileHandler.ReadElement(i, 0/*FIELD_NAME_PARAM_IDX*/);
                    // Check if field name has correct format
                    if (IsMatchFormat(temp) == true)
                    {
                        lsFlexibleColumn.Add(temp);
                    }
                }

                string[] RefFieldList = CFieldID.GetInstance().GetFieldIDList(INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR);
                int nFixedColumn = RefFieldList.Length;
                int nFlexibleColumn = lsFlexibleColumn.Count;

                if ((nFixedColumn + nFlexibleColumn) == iNumberValidField)
                {
                    // Sort flexible column
                    var comparer = new CustomComparer();
                    lsFlexibleColumn.Sort(comparer);
                    // Update list of flexible column for other usage
                    bResult = CFieldID.GetInstance().AddFlexibleColumn(lsFlexibleColumn);
                }
            }

            return bResult;
        }

        private bool IsMatchFormat(string sName)
        {
            bool bRet = false;
            Regex expression = new Regex(@"^CI_Class(\d+)_Default$");
            bRet = expression.IsMatch(sName);
            return bRet;
        }
    }

    class AInputChecker : IInputChecker
    {
        protected IInputFileProcessor       m_FileHandler = null;
        private INPUT_FILE_PROCESSOR_TYPE   m_FileProcessorType;
        private bool                        m_bFileSelected = false;

        public AInputChecker(INPUT_FILE_PROCESSOR_TYPE FileProcessorType)
        {
            m_FileProcessorType = FileProcessorType;
            m_FileHandler = CFactoryInputFileProcessor.GetInstance().GetInputFileProcessor(m_FileProcessorType);
        }

        public void Initialize ()
        {
            m_bFileSelected = false;
        }

        public bool CheckingInputFile(string sFilePath)
        {
            bool bRet = false;

            if (m_FileHandler != null)
            {
                if (m_bFileSelected == false)
                {
                    if (m_FileHandler.OpenFile(sFilePath) == true)
                    {
                        bRet = CheckFileStructure();
                        if (bRet == true)
                        {
                            m_bFileSelected = true;
                        }
                    }
                }
            }

            return bRet;
        }

        virtual protected bool CheckFileStructure()
        {
            bool bRet = false;

            string[] RefFieldList = null;
            int[] tempResult = null;

            if (m_FileHandler.IsFileOpen() == true)
            {
                RefFieldList = CFieldID.GetInstance().GetFieldIDList(m_FileProcessorType);

                tempResult = new int[RefFieldList.Length];
                for (int i = 0; i < RefFieldList.Length; i++)
                {
                    tempResult[i] = 0;
                }

                int iNumberValidField = m_FileHandler.GetNumberValidField();
                for (int i = 0; i < iNumberValidField; i++)
                {
                    string temp = m_FileHandler.ReadElement(i, 0/*FIELD_NAME_PARAM_IDX*/);
                    for (int j = 0; j < RefFieldList.Length; j++)
                    {
                        if (RefFieldList[j] == temp)
                        {
                            tempResult[j]++;
                        }
                    }
                }

                bRet = true;
                for (int i = 0; i < RefFieldList.Length; i++)
                {
                    switch (tempResult[i])
                    {
                        case 0:
                            // TODO Add Log missing collumn
                            bRet = false;
                            break;

                        case 1:
                            break;

                        default:
                            // TODO Add Log duplicate collumn
                            bRet = false;
                            break;
                    }
                }
            }

            if (bRet == false)
            {
                m_FileHandler.CloseFile();
            }

            return bRet;
        }
    }

    class CustomComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var regex = new Regex(@"^CI_Class(\d+)_Default$");
            var xRegexResult = regex.Match(x);
            var yRegexResult = regex.Match(y);

            if (xRegexResult.Success && yRegexResult.Success)
            {
                return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
            }

            return x.CompareTo(y);
        }
    }
}
