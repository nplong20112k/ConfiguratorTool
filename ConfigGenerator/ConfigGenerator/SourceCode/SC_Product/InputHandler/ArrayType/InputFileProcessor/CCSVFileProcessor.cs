using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace ConfigGenerator
{
    public class CMainInputFileProcessor : ACSVFileProcessor
    {
        public CMainInputFileProcessor()
            : base (INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR)
        {

        }
    }

    public class CSupportedModelInputFileProcessor : ACSVFileProcessor
    {
        public CSupportedModelInputFileProcessor()
            : base (INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR)
        {

        }
    }

    public class CCommonInputFileProcessor: ACSVFileProcessor
    {
        public CCommonInputFileProcessor()
            : base (INPUT_FILE_PROCESSOR_TYPE.COMMON_INPUT_FILE_PROCESSOR)
        {

        }
    }

    public abstract class ACSVFileProcessor : IInputFileProcessor
    {
        public int      FIRST_ROW           = 0;
        const string    CSV_DELIMITER       = ",";
        const int       NUMBER_HEADER_LINE  = 1;

        private INPUT_FILE_PROCESSOR_TYPE           m_InputProcessorType = 0;
        private TextFieldParser                     m_CSVParser = null;
        private bool                                m_bIsFileOpen = false;
        private int                                 m_iNumberOfLine = 0;
        private string[]                            m_sHeaderLine;
        private List<string[]>                      m_sLineList =  new List<string[]>(); 

        public ACSVFileProcessor(INPUT_FILE_PROCESSOR_TYPE FileProcessorType)
        {
            m_InputProcessorType = FileProcessorType;
        }

        ~ACSVFileProcessor()
        {
            if (m_CSVParser != null)
            {
                //m_CSVParser.Dispose();
                m_CSVParser.Close();
            }
        }

        public INPUT_FILE_PROCESSOR_TYPE GetProcessorType()
        {
            return m_InputProcessorType;
        }

        public bool OpenFile(string sFilePath)
        {
            bool bRet = false;
            ReInitProperties();

            if (File.Exists(sFilePath) == false)
            {
                return bRet;
            }

            if (Path.GetExtension(sFilePath) != ".csv")
            {
                return bRet;
            }

            if (sFilePath != null)
            {
                m_CSVParser = new TextFieldParser(sFilePath);

                if (m_CSVParser != null)
                {
                    try
                    {
                        m_CSVParser.SetDelimiters(CSV_DELIMITER);

                        m_sHeaderLine = m_CSVParser.ReadFields();

                        string[] sTempLine;
                        int iLineLen;

                        while (!m_CSVParser.EndOfData)
                        {
                            sTempLine = m_CSVParser.ReadFields();
                            iLineLen = sTempLine.Length - 1;
                            // Convert <CR><LF> to <LF> only
                            sTempLine[iLineLen] = sTempLine[iLineLen].Replace("\r\n","\n");

                            m_sLineList.Add(sTempLine);
                            m_iNumberOfLine++;
                        }

                        m_bIsFileOpen = true;
                        bRet = true;
                        //m_CSVParser.Dispose();
                        m_CSVParser.Close();
                        m_CSVParser = null;
                    }
                    catch { }
                }
            }

            return bRet;
        }

        public bool CloseFile()
        {
            bool bRet = false;

            if (m_bIsFileOpen == true)
            {
                if (m_CSVParser != null)
                {
                    try
                    {
                        //m_CSVParser.Dispose();
                        m_CSVParser.Close();
                        m_CSVParser = null;

                        m_bIsFileOpen = false;
                        bRet = true;
                    }
                    catch { }
                }
                else
                {
                    m_bIsFileOpen = false;
                }

            }

            return bRet;
        }

        public bool IsFileOpen()
        {
            return m_bIsFileOpen;
        }

        public string ReadElement(int iFieldIdx, int iParamIdx)
        {
            int iLineIdx = 1;

            if (iParamIdx == FIRST_ROW)
            {
                return m_sHeaderLine[iFieldIdx];
            }
            else
            {
                foreach (string[] Line in m_sLineList)
                {
                    if (iLineIdx == iParamIdx)
                    {
                        return Line[iFieldIdx];
                    }
                    iLineIdx++;
                }
            }

            return null;
        }

        public int GetNumberTotalField(int iParamIdx)
        {
            if (iParamIdx == FIRST_ROW)
            {
                return m_sHeaderLine.Length;
            }
            else
            {
                if (iParamIdx <= m_sLineList.Count)
                {
                    return m_sLineList[iParamIdx - 1].Length;
                }
                else
                {
                    return 0;
                }
            }
        }
 
        public int GetNumberValidField()
        {
            return m_sHeaderLine.Length;
        }

        public int GetNumberParamValid()
        {
            return m_iNumberOfLine;
        }

        public int CountNumberParamValid(int ColumnRef)
        {
            return m_iNumberOfLine;
        }

        private void ReInitProperties()
        { 
            m_sHeaderLine   = null;
            m_bIsFileOpen   = false;
            m_sLineList     = new List<string[]>();
            m_iNumberOfLine = 0;
        }
    }
}