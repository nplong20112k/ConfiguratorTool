using System;
using System.Collections.Generic;

namespace ConfigGenerator
{
    public enum FIELD_ID_TYPE
    {
        TAG_CODE = 0,
        MODEL_PRODUCT,
        ALADDIN_CATEGORY,
        TAG_NAME,
        TAG_USER_NAME,
        TAG_DESCRIPTION,
        VALUE_OPTIONS,
        VALUE_SIZE_BYTE,
        INTERNAL_NOTE,
        USER_VISIBILITY,
        ALADDIN_VISIBILITY,
        ALADDIN_RULE,
        READ_ONLY,
        MASTER_DEFAULT_EUGENE,
        MASTER_DEFAULT_BOLOGNA,

        TOTAL_FIELD_ID, // ALWAYS AT LAST!
    }

    public class CFieldID
    {
        private List<string>[]          m_sFieldIDPerFileList   = null;
        private string[]                m_sFieldIDList          = null;
        private int                     m_nNumOfFlexibleColumn  = 0;

        readonly static CFieldID m_Instance = new CFieldID();
        public static CFieldID GetInstance()
        {
            return m_Instance;
        }

        public CFieldID()
        {
            m_sFieldIDList = new string[(int)FIELD_ID_TYPE.TOTAL_FIELD_ID];

            InitializeFieldIDList(ref m_sFieldIDList);

            m_sFieldIDPerFileList = new List<string>[(int)INPUT_FILE_PROCESSOR_TYPE.TOTAL_FILE_PROCESSOR];

            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR] = new List<string>();
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_CODE]);
            //m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.MODEL_PRODUCT]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.ALADDIN_CATEGORY]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_NAME]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_USER_NAME]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_DESCRIPTION]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.VALUE_OPTIONS]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.VALUE_SIZE_BYTE]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.INTERNAL_NOTE]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.USER_VISIBILITY]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.ALADDIN_VISIBILITY]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.ALADDIN_RULE]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.READ_ONLY]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_EUGENE]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.MAIN_INPUT_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_BOLOGNA]);

            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR] = new List<string>();
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_CODE]);
            m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add(m_sFieldIDList[(int)FIELD_ID_TYPE.MODEL_PRODUCT]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.ALADDIN_CATEGORY]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_NAME]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_USER_NAME]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.TAG_DESCRIPTION]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.VALUE_OPTIONS]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.VALUE_SIZE_BYTE]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.INTERNAL_NOTE]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.USER_VISIBILITY]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.ALADDIN_VISIBILITY]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.ALADDIN_RULE]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.READ_ONLY]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_EUGENE]);
            // m_sFieldIDPerFileList[(int)INPUT_FILE_PROCESSOR_TYPE.SUPPORTED_MODEL_FILE_PROCESSOR].Add (m_sFieldIDList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_BOLOGNA]);
        }

        private void InitializeFieldIDList(ref string[] sFieldList)
        {
            sFieldList[(int)FIELD_ID_TYPE.TAG_CODE]                 = "Configuration Item";
            sFieldList[(int)FIELD_ID_TYPE.MODEL_PRODUCT]            = "CI_SupportedModels";
            sFieldList[(int)FIELD_ID_TYPE.ALADDIN_CATEGORY]         = "CI_AladdinMenuPathsAndCategories";
            sFieldList[(int)FIELD_ID_TYPE.TAG_NAME]                 = "CI_TagName";
            sFieldList[(int)FIELD_ID_TYPE.TAG_USER_NAME]            = "CI_TagUserName";
            sFieldList[(int)FIELD_ID_TYPE.TAG_DESCRIPTION]          = "CI_TagDescription";
            sFieldList[(int)FIELD_ID_TYPE.VALUE_OPTIONS]            = "CI_ValueOptions";
            sFieldList[(int)FIELD_ID_TYPE.VALUE_SIZE_BYTE]          = "CI_ValueSizeBytes";
            sFieldList[(int)FIELD_ID_TYPE.INTERNAL_NOTE]            = "CI_InternalNotes";
            sFieldList[(int)FIELD_ID_TYPE.USER_VISIBILITY]          = "CI_UserVisibility";
            sFieldList[(int)FIELD_ID_TYPE.ALADDIN_VISIBILITY]       = "CI_AladdinVisibility";
            sFieldList[(int)FIELD_ID_TYPE.ALADDIN_RULE]             = "CI_AladdinRule";
            sFieldList[(int)FIELD_ID_TYPE.READ_ONLY]                = "CI_ReadOnly";
            sFieldList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_EUGENE]    = "CI_Master_Default_Eugene";
            sFieldList[(int)FIELD_ID_TYPE.MASTER_DEFAULT_BOLOGNA]   = "CI_Master_Default_Bologna";
        }

        public string[] GetFieldIDList(INPUT_FILE_PROCESSOR_TYPE FileType)
        {
            return m_sFieldIDPerFileList[(int) FileType].ToArray();
        }
        
        public string[] GetFieldIDList()
        {
            return m_sFieldIDList;
        }

        public bool AddFlexibleColumn(List<string> list)
        {
            bool bRet = false;

            if (m_sFieldIDList == null)
            {
                m_sFieldIDList = new string[(int)FIELD_ID_TYPE.TOTAL_FIELD_ID + list.Count];
            }
            else
            {
                Array.Resize(ref m_sFieldIDList, (int)FIELD_ID_TYPE.TOTAL_FIELD_ID + list.Count);
            }

            InitializeFieldIDList(ref m_sFieldIDList);
            int nIndex = (int)FIELD_ID_TYPE.TOTAL_FIELD_ID;
            foreach (string element in list)
            {
                m_sFieldIDList[nIndex++] = element;
            }
            m_nNumOfFlexibleColumn = list.Count;
            bRet = true;
            return bRet;
        }

        public int GetNumOfFlexibleColumn()
        {
            return m_nNumOfFlexibleColumn;
        }
    }

}
