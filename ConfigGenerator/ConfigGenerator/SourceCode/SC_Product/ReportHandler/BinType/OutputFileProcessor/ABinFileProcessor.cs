using System;
using System.IO;

namespace ConfigGenerator
{
    public abstract class ABinFileProcessor: IBinFileProcessor
    {
        private BIN_FILE_PROCESSOR_TYPE m_BinProcessorType;
        private FileStream m_FileStream     = null;
        private BinaryWriter m_BinaryWriter = null;
        private string m_sFilePath;
        public ABinFileProcessor(BIN_FILE_PROCESSOR_TYPE FileProcessorType)
        {
            m_BinProcessorType = FileProcessorType;
        }

        public BIN_FILE_PROCESSOR_TYPE GetProcessorType()
        {
            return m_BinProcessorType;
        }

        public virtual bool CreateNewFile(string sFilePath)
        {
            bool bRet = false;
            ReInitProperties();

            if (sFilePath == null)
            {
                return bRet;
            }

            string FolderName = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(FolderName))
            {
                try
                {
                    Directory.CreateDirectory(FolderName);
                }
                catch
                {
                    return bRet;
                }
            }

            if (File.Exists(sFilePath))
            {
                try
                {
                    File.Delete(sFilePath);
                }
                catch
                {
                    return bRet;
                }
            }

            try
            {
                FileStream fs = new FileStream(sFilePath, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                bw.Close();
                fs.Close();
            }
            catch
            {
                return bRet;
            }

            m_sFilePath = sFilePath;
            return true;
        }

        public bool AddNewItem(byte[] buffer,  int buffer_len)
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sFilePath) == false) && (buffer != null))
            {
                try
                {
                    if(m_FileStream == null && m_BinaryWriter == null)
                    {
                        m_FileStream = new FileStream(m_sFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
                        m_BinaryWriter = new BinaryWriter(m_FileStream);
                    }
                    m_BinaryWriter.Write(buffer, 0, buffer_len);
                }
                catch
                {
                    return bRet;
                }
            }

            return true;
        }
        
        public bool AddNewItem(byte[] buffer, int offset, int buffer_len)
        {
            bool bRet = false;

            if ((string.IsNullOrEmpty(m_sFilePath) == false) && (buffer != null))
            {
                try
                {
                    if(m_FileStream == null && m_BinaryWriter == null)
                    {
                        m_FileStream = new FileStream(m_sFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
                        m_BinaryWriter = new BinaryWriter(m_FileStream);
                    }

                    m_BinaryWriter.Seek(offset, SeekOrigin.Begin);
                    m_BinaryWriter.Write(buffer, 0, buffer_len);
                }
                catch
                {
                    return bRet;
                }
            }

            return true;
        }
        
        public virtual bool CloseFile()
        {
            bool bRet = true;

            m_BinaryWriter.Close();
            m_FileStream.Close();

            return bRet;
        }

        public string[] LoadingFile(string sFilePath)
        {
            string[] sContain = null;

            if (sFilePath == null)
            {
                return null;
            }

            if (File.Exists(sFilePath))
            {
                try
                {
                    sContain = File.ReadAllLines(sFilePath);
                }
                catch
                {
                    return null;
                }
            }

            return sContain;
        }

        private void ReInitProperties()
        {
            m_sFilePath = null;
        }
    }
}