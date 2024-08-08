using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigGenerator
{
    class CCodeReportChecker : AReportChecker
    {
        private enum OVERAL_AREAS_ID
        {
            INPUT_SETTINGS_ID = 0,
            INCLUDE_FILES_ID,
            NAMESPACE_ID,
            EXCEPTION_ENUM_ID,
            EXCEPTION_ENUM_STRUCTURE_ID,
            NORMAL_STRUCT_ID,
            SPECIAL_STRUCT_ID,
            DELTA_NORMAL_STRUCT_ID,
            DELTA_SPECIAL_STRUCT_ID,
            CM_ITEM_AND_FUNCTION_ID,
            CM_VERIFY_EXCEPTION_FUNCTION_ID,
            CM_VERIFY_RANGE_FUNCTION_ID,
            CM_VERIFY_SELECTIVE_FUNCTION_ID,
            CM_VERIFY_READABLEASCII_FUNCTION_ID,
            CM_VERIFY_INTERFACE_FUNCTION_ID,
            CM_SEARCH_NORMAL_ITEM_INDEX_FUNCTION_ID,
            CM_GET_NORMAL_TABLE_SIZE_ID,

            INTERFACE_SETTINGS_ID,
            FEATURE_SETTINGS_ID,
            OUTPUT_SETTINGS_ID,
            END_CONFIG_AREAD_ID,
        }

        private struct OVERAL_STRUCTURE_TYPE
        {
            public OVERAL_AREAS_ID m_AreaID;
            public string m_AreaDes;
        }

        private struct SKIPPABLE_FUNCTION
        {
            public string m_AreaDes;
        }
        private struct RAW_CONFIG_TYPE
        {
            public List<string>     m_lsIncludeContain;
            public List<string>     m_lsExceptionEnumContain;
            public List<string>     m_lsExceptionEnumStructContain;
            public List<string>     m_lsNamespaceContain;
            public List<string>     m_lsNormalSessionContain;
            public List<string>     m_lsSpecialSessionContain;
            public List<string>     m_lsDeltaNormalSessionContain;
            public List<string>     m_lsDeltaSpecialSessionContain;
            public List<string>     m_lsInterfaceSessionContain;
            public List<string>     m_lsFlagContain;
            public List<string>     m_lsCmAndFuncContain;
            public List<string>     m_lsCmVerifyExceptFuncContain;
            public List<string>     m_lsCmVerifyRangeFuncContain;
            public List<string>     m_lsCmVerifySelectiveFuncContain;
            public List<string>     m_lsCmVerifyInterfaceFuncContain;
            public List<string>     m_lsCmVerifyReadableAsciiFuncContain;
            public List<string>     m_lsCmSearchNormalItemIndexContain;
            public List<string>     m_lsCmNormalTableGetSizeContain;
            public string           m_sReportFolderPath;
            public string           m_sReportFileName;
        };

        private struct PARSED_CONFIG_TYPE
        {
            public bool                                 m_bValid;
            public List<string>                         m_lsIncludePart;
            public List<string>                         m_lsExceptionEnumPart;
            public List<EXCEPTION_CI_ITEM>              m_lsExceptionEnumStructPart;
            public string                               m_sDecleareNamespace;
            public string                               m_sUsingNamespace;
            public TABLE_STRUCT_HEADER_TYPE             m_NormalHeader;
            public TABLE_STRUCT_HEADER_TYPE             m_GetNormalTableSizeHeader;                                            
            public List<TABLE_STRUCT_DATA_TYPE>         m_NormalTable;
            public TABLE_STRUCT_HEADER_TYPE             m_SearchNormalItemIndexTableHeader;
            public TABLE_STRUCT_HEADER_TYPE             m_SpecialHeader;
            public List<TABLE_STRUCT_DATA_TYPE>         m_SpecialTable;
            public TABLE_STRUCT_HEADER_TYPE             m_DeltaNormalHeader;
            public List<TABLE_STRUCT_DATA_TYPE>         m_DeltaNormalTable;
            public TABLE_STRUCT_HEADER_TYPE             m_DeltaSpecialHeader;
            public List<TABLE_STRUCT_DATA_TYPE>         m_DeltaSpecialTable;
            public List<INTERFACE_CLASS>                m_InterfaceClass;
            public TABLE_STRUCT_HEADER_TYPE             m_CmAndFuncSection;
            public TABLE_STRUCT_HEADER_TYPE             m_CmVerifyExceptFuncSection;
            public TABLE_STRUCT_HEADER_TYPE             m_CmVerifyRangeFuncSection;
            public TABLE_STRUCT_HEADER_TYPE             m_CmVerifySelectiveFuncSection;
            public TABLE_STRUCT_HEADER_TYPE             m_CmVerifyInterfaceFuncSection;
            public TABLE_STRUCT_HEADER_TYPE             m_CmVerifyReadableAsciiFuncSection;
            public string                               m_sReportFolderPath;
            public string                               m_sReportFileName;
            public List<DELTA_TABLE_GROUP_NAME>         m_lsDeltaGroupTableName;
            public List<CI_ENUM_INFO>[]                 m_lsCIEnumInfo;
        }

        private struct TABLE_STRUCT_MEMBER_TYPE
        {
            public TABLE_STRUCT_MEMBER_ID   m_MemberID;
            public string                   m_MemberDes;
        }

        private struct TABLE_STRUCT_HEADER_TYPE
        {
            public string m_VarType;
            public string m_VarName;
            public string m_FuncName;
            public string m_sOutputType;
        }

        private const string REPORT_FILE_NAME           = "config_item_header.h";
        private const char   KW_SPLIT_EQUAL_CHAR        = '=';
        private const char   KW_SPLIT_COMMENT_CHAR      = '/';  
        private const char   KW_SPLIT_COLON_CHAR        = ':';
        private const char   KW_SPLIT_COMMA_CHAR        = ',';
        private const string KW_INCLUDE_FILE            = "INCLUDE_FILE";
        private const string KW_VARIABLE_TYPE           = "VARIABLE_TYPE";
        private const string KW_VARIABLE_NAME           = "VARIABLE_NAME";
        private const string KW_FUNCTION_NAME           = "FUNCTION_NAME";
                                                        
        private const string KW_FOLDER_PATH             = "OUTPUT_FOLDER_PATH";

        private const string KW_CI_ENUM_FILE_NAME       = "CI_ENUM_TEMPLATE_FILE_PATH";
        private const string KW_DECLARE_NAMESPACE       = "DECLARE_NAMESPACE";
        private const string KW_USING_NAMESPACE         = "USING_NAMESPACE";
        private const string KW_OUTPUT_TYPE             = "OUTPUT_TYPE";
        private const string KW_DELTA_TABLE_DETECT      = "DELTA TABLE STRUCTURE";
        private const string KW_STRUCT_TYPE             = "struct";
        private const string KW_TYPEDEF_STRUCT_TYPE     = "typedef struct";
        private const string KW_OUTPUT_CPP_TYPE         = "cpp";
        private const string KW_SPLIT_CHAR              = "_";
        private const string DELTA_EUGENE_NAME          = "eugene_";
        private const string DELTA_BOLOGNA_NAME         = "bologna_";
        private const string DELTA_SPECIAL_TYPE         = "special_";
        private const string DELTA_NULL_TYPE            = "null";

        private const string KW_CLASS_NAME              = "CLASS";
        public static readonly List<string> KW_INTERFACE_NAME = new List<string>() { "INTERFACE_", "IF_" };

        private ICodeFileProcessor  m_FileProcessor     = null;
        private CSourceCodeTemplate m_SourceTemplate    = null;

        private string              m_sTemplateFilePath = null;
        private string[]            m_sFileContent      = null;

        private RAW_CONFIG_TYPE     m_RawConfig;
        private PARSED_CONFIG_TYPE  m_ParsedConfig;
        private CSourceInfoObject   m_SourceCodeInfo;

        private CNamespaceFormatter m_NamespaceFormatter = null;

        private static readonly OVERAL_STRUCTURE_TYPE[] TEMPLATE_FILE_OVERAL_STRUCTURE = {
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.INPUT_SETTINGS_ID,                        m_AreaDes = "INPUT SETTINGS"                        },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.INCLUDE_FILES_ID,                         m_AreaDes = "INCLUDE FILES"                         },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.NAMESPACE_ID,                             m_AreaDes = "NAMESPACE NAME"                        },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.EXCEPTION_ENUM_ID,                        m_AreaDes = "EXCEPTION ENUM ITEMS"                  },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.EXCEPTION_ENUM_STRUCTURE_ID,              m_AreaDes = "EXCEPTION ENUM AND STRUCTURE ITEMS"    },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.NORMAL_STRUCT_ID,                         m_AreaDes = "NORMAL TABLE STRUCTURE"                },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.SPECIAL_STRUCT_ID,                        m_AreaDes = "SPECIAL TABLE STRUCTURE"               },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.DELTA_NORMAL_STRUCT_ID,                   m_AreaDes = "NORMAL DELTA TABLE STRUCTURE"          },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.DELTA_SPECIAL_STRUCT_ID,                  m_AreaDes = "SPECIAL DELTA TABLE STRUCTURE"         },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_ITEM_AND_FUNCTION_ID,                  m_AreaDes = "CM ITEM STRUCTURE AND FUNCTION"        },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_VERIFY_EXCEPTION_FUNCTION_ID,          m_AreaDes = "CM VERIFY EXCEPTION VALUE FUNCTION"    },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_VERIFY_RANGE_FUNCTION_ID,              m_AreaDes = "CM VERIFY SPECIAL RANGE VALUE FUNCTION"},
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_VERIFY_SELECTIVE_FUNCTION_ID,          m_AreaDes = "CM VERIFY SPECIAL SELECTIVE VALUE FUNCTION"},
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_VERIFY_READABLEASCII_FUNCTION_ID,      m_AreaDes = "CM VERIFY SPECIAL READABLE ASCII VALUE FUNCTION"},
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_VERIFY_INTERFACE_FUNCTION_ID,          m_AreaDes = "CM VERIFY INTERFACE VALUE FUNCTION"    },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_SEARCH_NORMAL_ITEM_INDEX_FUNCTION_ID,  m_AreaDes = "CM SEARCH NORMAL ITEM INDEX FUNCTION"  },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.CM_GET_NORMAL_TABLE_SIZE_ID,              m_AreaDes = "CM GET NORMAL TABLE SIZE"              },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID,                    m_AreaDes = "INTERFACE SETTINGS"                    },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.FEATURE_SETTINGS_ID,                      m_AreaDes = "FEATURE SETTINGS"                      },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.OUTPUT_SETTINGS_ID,                       m_AreaDes = "OUTPUT SETTINGS"                       },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.END_CONFIG_AREAD_ID,                      m_AreaDes = "END CONFIG AREAD"                      },
        };

        private static readonly SKIPPABLE_FUNCTION[] SKIPPABLE_FUNCTION_LIST = {
            new SKIPPABLE_FUNCTION() { m_AreaDes = "CM VERIFY SPECIAL RANGE VALUE FUNCTION"           },
            new SKIPPABLE_FUNCTION() { m_AreaDes = "CM VERIFY SPECIAL SELECTIVE VALUE FUNCTION"       },
            new SKIPPABLE_FUNCTION() { m_AreaDes = "CM VERIFY SPECIAL READABLE ASCII VALUE FUNCTION"  },

        };
        private static readonly TABLE_STRUCT_MEMBER_TYPE[] TABLE_STRUCT_MEMBER_LIST = {
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.CI_NAME_ID           , m_MemberDes = "CI NAME"             },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.CI_CODE_ID           , m_MemberDes = "CI CODE"             },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.MIN_VALUE_ID         , m_MemberDes = "MIN VALUE"           },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.MAX_VALUE_ID         , m_MemberDes = "MAX VALUE"           },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.INTERFACE_MASK_ID    , m_MemberDes = "INTERFACE MASK"      },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.PRODUCT_MASK_ID      , m_MemberDes = "PRODUCT MASK"        },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.DEFAULT_VALUE        , m_MemberDes = "DEFAULT VALUE"       },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.CI_ADDRESS_ID        , m_MemberDes = "CI ADDRESS"          },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.CI_DEFAULT_VALUE     , m_MemberDes = "CI DEFAULT VALUE"    },
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.CI_DEFAULT_ALT_VALUE , m_MemberDes = "CI DEFAULT VALUE ALT"},
            new TABLE_STRUCT_MEMBER_TYPE() { m_MemberID = TABLE_STRUCT_MEMBER_ID.CI_SIZE_ID           , m_MemberDes = "CI SIZE"             },
        };

        public CCodeReportChecker()
            : base(AReportChecker.REPORT_FILE_TYPE.REPORT_FILE_SOURCE_CODE)
        {
            m_FileProcessor  = CFactoryCodeFileProcessor.GetInstance().GetFileProcessor(CODE_FILE_PROCESSOR_TYPE.CODE_CHECKER);
            m_SourceTemplate = CSourceCodeTemplate.GetInstance();

            if ((m_FileProcessor == null) || (m_SourceTemplate == null))
            {
                return;
            }
        }

        public override bool CheckTemplateFileValid(string sFilePath)
        {
            bool bRet = false;
            Initialize();

            if (m_FileProcessor != null)
            {
                m_sFileContent = m_FileProcessor.LoadingFile(sFilePath);
                if (m_sFileContent != null)
                {
                    m_sTemplateFilePath = sFilePath;
                    bRet = true;
                }
            }

            return bRet;
        }

        public override bool CheckTemplateFileStructure()
        {
            bool bRet = false;
            if (m_sFileContent != null)
            {
                uint[] uiCheckResult = new uint[TEMPLATE_FILE_OVERAL_STRUCTURE.Length];
                
                foreach (OVERAL_STRUCTURE_TYPE element in TEMPLATE_FILE_OVERAL_STRUCTURE)
                {
                    int iPosition = Array.IndexOf(TEMPLATE_FILE_OVERAL_STRUCTURE, element);
                    foreach (SKIPPABLE_FUNCTION skipElement in SKIPPABLE_FUNCTION_LIST)
                    {
                        if (skipElement.m_AreaDes == element.m_AreaDes)
                        {
                            uiCheckResult[iPosition]++;
                            continue;
                        }
                    }
                    foreach (string line in m_sFileContent)
                    {
                        if ((line.Contains(element.m_AreaDes)) && (uiCheckResult[iPosition] != 1))
                        {
                            uiCheckResult[iPosition]++;
                        }
                    }
                }

                bRet = true;
                foreach (uint element in uiCheckResult)
                {
                    if (element != 1)
                    {
                        bRet = false;
                        break;
                    }
                }
            }

            return bRet;
        }

        public override bool ProcessTemplateFile()
        {
            bool bRet = false;

            m_RawConfig = PrepareReportFileInfo();
            m_ParsedConfig = ParserConfigInfo();

            if (m_ParsedConfig.m_bValid == true)
            {
                bRet = ProcessReportFileInfo(m_ParsedConfig);
            }

            if (bRet == true)
            {
                m_SourceCodeInfo = UpdateSourceCodeInfo(m_ParsedConfig);
                if (m_SourceCodeInfo != null)
                {
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO, m_SourceCodeInfo);
                }
            }

            return bRet;
        }

        public override void Initialize()
        {
            m_sTemplateFilePath = null;
            m_sFileContent      = null; 
            m_SourceCodeInfo    = null;
        }
        
        private RAW_CONFIG_TYPE PrepareReportFileInfo()
        {
            RAW_CONFIG_TYPE StructRet = new RAW_CONFIG_TYPE() {
                m_lsIncludeContain                      = new List<string>(),
                m_lsNamespaceContain                    = new List<string>(),
                m_lsExceptionEnumContain                = new List<string>(),
                m_lsExceptionEnumStructContain          = new List<string>(),
                m_lsNormalSessionContain                = new List<string>(),
                m_lsSpecialSessionContain               = new List<string>(),
                m_lsCmSearchNormalItemIndexContain      = new List<string>(),
                m_lsCmNormalTableGetSizeContain         = new List<string>(),
                m_lsDeltaNormalSessionContain           = new List<string>(),
                m_lsDeltaSpecialSessionContain          = new List<string>(), 
                m_lsInterfaceSessionContain             = new List<string>(),
                m_lsFlagContain                         = new List<string>(),
                m_lsCmAndFuncContain                    = new List<string>(),
                m_lsCmVerifyExceptFuncContain           = new List<string>(),
                m_lsCmVerifyReadableAsciiFuncContain    = new List<string>(),
                m_lsCmVerifyRangeFuncContain            = new List<string>(),
                m_lsCmVerifySelectiveFuncContain        = new List<string>(),
                m_lsCmVerifyInterfaceFuncContain        = new List<string>(),
                m_sReportFileName                       = REPORT_FILE_NAME,
            };

            if (m_sFileContent != null)
            {
                OVERAL_AREAS_ID FlagConfigAreaDetected                    = 0;
                List<string>    sTempInputSettingData                     = new List<string>();
                List<string>    sTempInterfaceSettingData                 = new List<string>();
                List<string>    sTempFeatureSettingData                   = new List<string>();
                List<string>    sTempOutputSettingData                    = new List<string>();

                // update config areas
                foreach (string element in m_sFileContent)
                {
                    if ((element != null) && (element != string.Empty))
                    {
                        // detect config area and update file path setting
                        if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.INPUT_SETTINGS_ID].m_AreaDes))
                        {
                            FlagConfigAreaDetected = OVERAL_AREAS_ID.INPUT_SETTINGS_ID;
                        }              
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID].m_AreaDes))
                        {
                            FlagConfigAreaDetected = OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.FEATURE_SETTINGS_ID].m_AreaDes))
                        {
                            FlagConfigAreaDetected = OVERAL_AREAS_ID.FEATURE_SETTINGS_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.OUTPUT_SETTINGS_ID].m_AreaDes))
                        {
                            FlagConfigAreaDetected = OVERAL_AREAS_ID.OUTPUT_SETTINGS_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.END_CONFIG_AREAD_ID].m_AreaDes))
                        {
                            break;
                        }
  
                        // update config contain
                        switch (FlagConfigAreaDetected)
                        {
                            case OVERAL_AREAS_ID.INPUT_SETTINGS_ID:
                                sTempInputSettingData.Add(element);
                                break;

                            case OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID:
                                sTempInterfaceSettingData.Add(element);
                                break;

                            case OVERAL_AREAS_ID.FEATURE_SETTINGS_ID:
                                sTempFeatureSettingData.Add(element);
                                break;

                            case OVERAL_AREAS_ID.OUTPUT_SETTINGS_ID:
                                sTempOutputSettingData.Add(element);
                                break;

                            default:
                                break;
                        }
                    }                  
                }

                // handle input setting area
                if (sTempInputSettingData.Count > 0)
                {
                    PrepareConfigInputSetting(sTempInputSettingData, ref StructRet);
                }

                // handle interface setting area
                if (sTempInterfaceSettingData.Count > 0)
                {
                    PrepareConfigInterfaceSetting(sTempInterfaceSettingData, ref StructRet);
                }

                // handle feature setting area
                if (sTempFeatureSettingData.Count > 0)
                {
                    PrepareConfigFeatureSetting(sTempFeatureSettingData, ref StructRet);
                }

                // handle output setting area
                if (sTempOutputSettingData.Count > 0)
                {
                    PrepareConfigOutputSetting(sTempOutputSettingData, ref StructRet);
                }
            }

            return StructRet;
        }
        
        private bool PrepareConfigInputSetting(List<string> lsInputData, ref RAW_CONFIG_TYPE ConfigContaintData)
        {
            bool bRet = false;
            OVERAL_AREAS_ID FlagAreasDetect = 0;

            if ((lsInputData != null) && (lsInputData.Count > 0))
            {
                foreach (string element in lsInputData)
                {
                    if ((element != null) && (element != string.Empty))
                    {
                        // detect areas config
                        if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.INCLUDE_FILES_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.INCLUDE_FILES_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.NAMESPACE_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.NAMESPACE_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.EXCEPTION_ENUM_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.EXCEPTION_ENUM_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.EXCEPTION_ENUM_STRUCTURE_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.EXCEPTION_ENUM_STRUCTURE_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.NORMAL_STRUCT_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.NORMAL_STRUCT_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.SPECIAL_STRUCT_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.SPECIAL_STRUCT_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.DELTA_NORMAL_STRUCT_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.DELTA_NORMAL_STRUCT_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.DELTA_SPECIAL_STRUCT_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.DELTA_SPECIAL_STRUCT_ID;
                        }                       
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_ITEM_AND_FUNCTION_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_ITEM_AND_FUNCTION_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_VERIFY_EXCEPTION_FUNCTION_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_VERIFY_EXCEPTION_FUNCTION_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_VERIFY_RANGE_FUNCTION_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_VERIFY_RANGE_FUNCTION_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_VERIFY_READABLEASCII_FUNCTION_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_VERIFY_READABLEASCII_FUNCTION_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_VERIFY_SELECTIVE_FUNCTION_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_VERIFY_SELECTIVE_FUNCTION_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_VERIFY_INTERFACE_FUNCTION_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_VERIFY_INTERFACE_FUNCTION_ID;
                        }

                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_SEARCH_NORMAL_ITEM_INDEX_FUNCTION_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_SEARCH_NORMAL_ITEM_INDEX_FUNCTION_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.CM_GET_NORMAL_TABLE_SIZE_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.CM_GET_NORMAL_TABLE_SIZE_ID;
                        }
                        // update areas config
                        switch (FlagAreasDetect)
                        {
                            case OVERAL_AREAS_ID.INCLUDE_FILES_ID:
                                ConfigContaintData.m_lsIncludeContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.NAMESPACE_ID:
                                ConfigContaintData.m_lsNamespaceContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.EXCEPTION_ENUM_ID:
                                ConfigContaintData.m_lsExceptionEnumContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.EXCEPTION_ENUM_STRUCTURE_ID:
                                ConfigContaintData.m_lsExceptionEnumStructContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.NORMAL_STRUCT_ID:
                                ConfigContaintData.m_lsNormalSessionContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.SPECIAL_STRUCT_ID:
                                ConfigContaintData.m_lsSpecialSessionContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.DELTA_NORMAL_STRUCT_ID:
                                ConfigContaintData.m_lsDeltaNormalSessionContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.DELTA_SPECIAL_STRUCT_ID:
                                ConfigContaintData.m_lsDeltaSpecialSessionContain.Add(element);
                                break;                        

                            case OVERAL_AREAS_ID.CM_ITEM_AND_FUNCTION_ID:
                                ConfigContaintData.m_lsCmAndFuncContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.CM_VERIFY_EXCEPTION_FUNCTION_ID:
                                ConfigContaintData.m_lsCmVerifyExceptFuncContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.CM_VERIFY_RANGE_FUNCTION_ID:
                                ConfigContaintData.m_lsCmVerifyRangeFuncContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.CM_VERIFY_READABLEASCII_FUNCTION_ID:
                                ConfigContaintData.m_lsCmVerifyReadableAsciiFuncContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.CM_VERIFY_SELECTIVE_FUNCTION_ID:
                                ConfigContaintData.m_lsCmVerifySelectiveFuncContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.CM_VERIFY_INTERFACE_FUNCTION_ID:
                                ConfigContaintData.m_lsCmVerifyInterfaceFuncContain.Add(element);
                                break;

                            case OVERAL_AREAS_ID.CM_SEARCH_NORMAL_ITEM_INDEX_FUNCTION_ID:
                                ConfigContaintData.m_lsCmSearchNormalItemIndexContain.Add(element);
                                break;
                            case OVERAL_AREAS_ID.CM_GET_NORMAL_TABLE_SIZE_ID:
                                ConfigContaintData.m_lsCmNormalTableGetSizeContain.Add(element);
                                break;
                            default:
                                break;
                        }

                        bRet = true;
                    }
                }
            }
            
            return bRet;
        }

        private bool PrepareConfigInterfaceSetting(List<string> lsInputData, ref RAW_CONFIG_TYPE ConfigContaintData)
        {
            bool bRet = false;
            OVERAL_AREAS_ID FlagAreasDetec = 0;

            if ((lsInputData != null) && (lsInputData.Count > 0))
            {
                foreach (string element in lsInputData)
                {
                    if ((element != null) && (element != string.Empty))
                    {
                        // detect areas config
                        if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID].m_AreaDes))
                        {
                            FlagAreasDetec = OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID;
                        }

                        // update areas config
                        switch (FlagAreasDetec)
                        {
                            case OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID:
                                ConfigContaintData.m_lsInterfaceSessionContain.Add(element);
                                break;

                            default:
                                break;
                        }

                        bRet = true;
                    }
                }
            }

            return bRet;
        }

        private bool PrepareConfigFeatureSetting(List<string> lsInputData, ref RAW_CONFIG_TYPE ConfigContaintData)
        {
            bool bRet = false;
            OVERAL_AREAS_ID FlagAreasDetec = 0;

            if ((lsInputData != null) && (lsInputData.Count > 0))
            {
                foreach (string element in lsInputData)
                {
                    if ((element != null) && (element != string.Empty))
                    {
                        // detect areas config
                        if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.FEATURE_SETTINGS_ID].m_AreaDes))
                        {
                            FlagAreasDetec = OVERAL_AREAS_ID.FEATURE_SETTINGS_ID;
                        }

                        // update areas config
                        switch (FlagAreasDetec)
                        {
                            case OVERAL_AREAS_ID.FEATURE_SETTINGS_ID:
                                ConfigContaintData.m_lsFlagContain.Add(element);
                                break;

                            default:
                                break;
                        }

                        bRet = true;
                    }
                }
            }

            return bRet;
        }

        private bool PrepareConfigOutputSetting(List<string> lsInputData, ref RAW_CONFIG_TYPE ConfigContaintData)
        {
            bool    bRet                = false;

            if ((lsInputData != null) && (lsInputData.Count > 0))
            {
                foreach (string element in lsInputData)
                {
                    if ((element != null) && (element != string.Empty))
                    {
                        string[] sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        if (sTemp.Length == 2)
                        {
                            switch (sTemp[0].Trim())
                            {
                                case KW_FOLDER_PATH:
                                    ConfigContaintData.m_sReportFolderPath = sTemp[1].Trim().Replace("\"", string.Empty);
                                    //Program.SystemHandleStatusInfo("[debug] before::m_sReportFolderPath = " + ConfigContaintData.m_sReportFolderPath + Environment.NewLine);
                                    if (string.IsNullOrEmpty(ConfigContaintData.m_sReportFolderPath) == false)
                                        ConfigContaintData.m_sReportFolderPath = ConfigContaintData.m_sReportFolderPath.Replace('\\', Path.DirectorySeparatorChar);
                                    //Program.SystemHandleStatusInfo("[debug] after::m_sReportFolderPath = " + ConfigContaintData.m_sReportFolderPath + Environment.NewLine);
                                    break;

                                default:
                                    break;
                            }

                            if (String.IsNullOrEmpty(ConfigContaintData.m_sReportFolderPath) == false)
                            {
                                bRet = true;
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        private PARSED_CONFIG_TYPE ParserConfigInfo()
        {
            PARSED_CONFIG_TYPE ParserContainRet = new PARSED_CONFIG_TYPE() {
                m_bValid = true,
                m_sReportFolderPath = null,
                m_sReportFileName = null,
            };

            // parser output setting
            if (m_RawConfig.m_sReportFileName != null)
            {
                // ParserContainRet.m_sReportFolderPath = Directory.GetCurrentDirectory();
                ParserContainRet.m_sReportFolderPath = System.AppDomain.CurrentDomain.BaseDirectory;

                if ((m_RawConfig.m_sReportFolderPath != null) && (m_RawConfig.m_sReportFolderPath != string.Empty))
                {
                    if (Path.IsPathRooted(m_RawConfig.m_sReportFolderPath))
                    {
                        ParserContainRet.m_sReportFolderPath = m_RawConfig.m_sReportFolderPath;
                    }
                    else
                    {
                        ParserContainRet.m_sReportFolderPath += Path.DirectorySeparatorChar + m_RawConfig.m_sReportFolderPath;
                    }
                }

                ParserContainRet.m_sReportFolderPath += m_SourceTemplate.m_sSourceFolderName;
                ParserContainRet.m_sReportFileName = m_RawConfig.m_sReportFileName;
            }

            // parser include part
            if (ParseIncludeSection(m_RawConfig.m_lsIncludeContain, ref ParserContainRet.m_lsIncludePart) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing include files information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser namespace part
            if (ParseNamespaceSection(m_RawConfig.m_lsNamespaceContain, ref ParserContainRet.m_sDecleareNamespace, ref ParserContainRet.m_sUsingNamespace) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing name space information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parse exception enum part
            if (ParseExceptionEnumItem(m_RawConfig.m_lsExceptionEnumContain, ref ParserContainRet.m_lsExceptionEnumPart) == false)
            {
                Program.SystemHandleStatusInfo("WARNING:: Missing manually handle CI items information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parse exception enum and structure part
            if (ParseExceptionEnumStructItem(m_RawConfig.m_lsExceptionEnumStructContain, ref ParserContainRet.m_lsExceptionEnumStructPart) == false)
            {
                Program.SystemHandleStatusInfo("WARNING:: Missing manually handle CI items information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser normal session part
            if (ParseTableStructSection(m_RawConfig.m_lsNormalSessionContain, ref ParserContainRet.m_NormalHeader, ref ParserContainRet.m_NormalTable) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing normal table information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser special session part
            if (ParseTableStructSection(m_RawConfig.m_lsSpecialSessionContain, ref ParserContainRet.m_SpecialHeader, ref ParserContainRet.m_SpecialTable) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing special table information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser Delta normal session part
            if (ParseTableStructSection(m_RawConfig.m_lsDeltaNormalSessionContain, ref ParserContainRet.m_DeltaNormalHeader, ref ParserContainRet.m_DeltaNormalTable) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing delta normal table information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser Delta special session part
            if (ParseTableStructSection(m_RawConfig.m_lsDeltaSpecialSessionContain, ref ParserContainRet.m_DeltaSpecialHeader, ref ParserContainRet.m_DeltaSpecialTable) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing delta special table information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser Interface session part
            if (ParseInterfaceSection(m_RawConfig.m_lsInterfaceSessionContain, ref ParserContainRet.m_InterfaceClass) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing/Ivalid interface information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parse Flags session part
            if (ParseFlagConfigSection(m_RawConfig.m_lsFlagContain, ref ParserContainRet.m_lsCIEnumInfo) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing/Invalid CIRO options information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm and function part
            if (ParseCmAndFuncSection(m_RawConfig.m_lsCmAndFuncContain, ref ParserContainRet.m_CmAndFuncSection) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM variable declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm verify exception function part
            if (ParseCmVerifyExceptionFuncSection(m_RawConfig.m_lsCmVerifyExceptFuncContain, ref ParserContainRet.m_CmVerifyExceptFuncSection) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM verify exception value normal item function declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm verify range function part
            if (ParseCmVerifyExceptionFuncSection(m_RawConfig.m_lsCmVerifyRangeFuncContain, ref ParserContainRet.m_CmVerifyRangeFuncSection) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM verify range value normal item function declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm verify readable ascii function part
            if (ParseCmVerifyExceptionFuncSection(m_RawConfig.m_lsCmVerifyReadableAsciiFuncContain, ref ParserContainRet.m_CmVerifyReadableAsciiFuncSection) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM verify readable ascii value special item function declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm verify selective function part
            if (ParseCmVerifyExceptionFuncSection(m_RawConfig.m_lsCmVerifySelectiveFuncContain, ref ParserContainRet.m_CmVerifySelectiveFuncSection) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM verify exception value special item function declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm verify interface value function part
            if (ParseCmVerifyExceptionFuncSection(m_RawConfig.m_lsCmVerifyInterfaceFuncContain, ref ParserContainRet.m_CmVerifyInterfaceFuncSection) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM verify interface function declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm search normal item index function part:
            if (ParseCmVerifyExceptionFuncSection(m_RawConfig.m_lsCmSearchNormalItemIndexContain, ref ParserContainRet.m_SearchNormalItemIndexTableHeader) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM search normal item index function declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser cm normal table get size function part:
            if (ParseCmVerifyExceptionFuncSection(m_RawConfig.m_lsCmNormalTableGetSizeContain, ref ParserContainRet.m_GetNormalTableSizeHeader) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing CM get normal table size function declaration information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parse Delta table group name
            if (ParseDeltaGroupNameSection(ParserContainRet.m_InterfaceClass, ref ParserContainRet.m_lsDeltaGroupTableName) == false)
            {
                ParserContainRet.m_bValid = false;
            }

            return ParserContainRet;
        }

        private bool ParseIncludeSection(List<string> lsData, ref List<string> lsParsedData)
        {
            bool bRet = false;

            if (lsData != null)
            {
                lsParsedData = new List<string>();

                foreach (string element in lsData)
                {
                    if (element.Contains(KW_INCLUDE_FILE))
                    {
                        string[] sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        if ((sTemp != null) && (sTemp.Length == 2) && (sTemp[0].Trim() == KW_INCLUDE_FILE))
                        {
                            string sIncludeData = sTemp[1].Trim().Replace("\"", string.Empty);
                            lsParsedData.Add(sIncludeData);
                        }
                    }
                }
                bRet = true;
            }

            return bRet;
        }

        private bool ParseNamespaceSection(List<string> lsRawData, ref string sDecleareNamespace, ref string sUsingNamespace)
        {
            bool bRet = false;

            if ((lsRawData != null) && (lsRawData.Count > 0))
            {
                foreach (string element in lsRawData)
                {
                    if (element.Contains(KW_DECLARE_NAMESPACE))
                    {
                        string[] sTemp = null;
                        sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        sDecleareNamespace = sTemp[1].Trim();
                    }

                    else if(element.Contains(KW_USING_NAMESPACE))
                    {
                        string[] sTemp = null;
                        sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        sUsingNamespace = sTemp[1].Trim();
                    }
                }

                bRet = true;
            }

            return bRet;
        }

        private bool ParseExceptionEnumItem(List<string> lsRawData, ref List<string> lsParsedData)
        {
            bool bRet = false;

            if ((lsRawData != null) && (lsRawData.Count > 0))
            {
                lsParsedData = new List<string>();

                foreach (string element in lsRawData)
                {
                    if (!element.Contains(KW_SPLIT_COMMENT_CHAR))
                    {
                        string sTemp = element.Trim();
                        lsParsedData.Add(sTemp);
                    }
                }

                bRet = true;
            }

            return bRet;
        }

        private bool ParseExceptionEnumStructItem(List<string> lsRawData, ref List<EXCEPTION_CI_ITEM> lsParsedData)
        {
            bool bRet = false;

            if ((lsRawData != null) && (lsRawData.Count > 0))
            {
                string sNameCIItem;
                string sValueSize;
                lsParsedData = new List<EXCEPTION_CI_ITEM>();
                EXCEPTION_CI_ITEM MemberData = new EXCEPTION_CI_ITEM()
                {
                    m_sNameCIItem = null,
                    m_sValuesize = null,
                };

                foreach (string element in lsRawData)
                {
                    if (element.Contains(KW_SPLIT_EQUAL_CHAR))
                    {
                        string[] sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);

                        sNameCIItem = sTemp[0].Trim();
                        sValueSize  = sTemp[1].Trim();

                        if ((sNameCIItem != null) && (sValueSize != null))
                        {
                            MemberData.m_sNameCIItem    = sNameCIItem;
                            MemberData.m_sValuesize     = sValueSize;

                            lsParsedData.Add(MemberData);
                        }
                    }
                }

                bRet = true;
            }

            return bRet;
        }


        private bool ParseTableStructSection(List<string> lsRawData, ref TABLE_STRUCT_HEADER_TYPE Header, ref List<TABLE_STRUCT_DATA_TYPE> Data)
        {
            bool bRet               = false;
            bool bDetectDeltaTable  = false;

            if ((lsRawData != null) && (lsRawData.Count > 0))
            {
                string sVariableType    = null;
                string sVariableName    = null;
                string sFunctionName    = null;
                string sOutputType      = null;
                bool bFlagHeaderDetect  = true;

                Header = new TABLE_STRUCT_HEADER_TYPE()
                {
                    m_VarType       = null,
                    m_VarName       = null,
                    m_FuncName      = null,
                    m_sOutputType   = null,
                };

                Data = new List<TABLE_STRUCT_DATA_TYPE>();
                TABLE_STRUCT_DATA_TYPE MemberData = new TABLE_STRUCT_DATA_TYPE() {
                    m_DatID     = TABLE_STRUCT_MEMBER_ID.UNKNOW_ID,
                    m_sDataType = null,
                    m_sDataName = null,
                };

                foreach (string element in lsRawData)
                {               
                    if (element.Contains(KW_DELTA_TABLE_DETECT))
                    {
                        bDetectDeltaTable = true;
                    }  

                    foreach (TABLE_STRUCT_MEMBER_TYPE element_1 in TABLE_STRUCT_MEMBER_LIST)
                    {
                        // detect table struct member sign
                        if (element.Contains(element_1.m_MemberDes) == true)
                        {
                            bFlagHeaderDetect = false;
                            MemberData.m_DatID = element_1.m_MemberID;
                        }
                    }

                    if ((element.Contains(KW_VARIABLE_TYPE)) ||
                        (element.Contains(KW_VARIABLE_NAME)) ||
                        (element.Contains(KW_FUNCTION_NAME)) ||
                        (element.Contains(KW_OUTPUT_TYPE)))
                    {
                        string[] sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        if ((sTemp != null) && (sTemp.Length == 2) && (sTemp[0] != null))
                        {
                            sTemp[1] = sTemp[1].Trim().Replace("\"", string.Empty);
                            switch (sTemp[0].Trim())
                            {
                                case KW_VARIABLE_TYPE:
                                    sVariableType = sTemp[1];
                                    break;

                                case KW_VARIABLE_NAME:
                                    sVariableName = sTemp[1];
                                    break;

                                case KW_FUNCTION_NAME:
                                    sFunctionName = sTemp[1];
                                    break;

                                case KW_OUTPUT_TYPE:
                                    sOutputType = ConvertTail(sTemp[1]);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    if (bFlagHeaderDetect == true)
                    {
                        if (bDetectDeltaTable)
                        {
                            if ((sVariableType != null) && (sVariableName != null) && (sOutputType != null))
                            {
                                Header.m_VarType        = sVariableType;
                                Header.m_VarName        = sVariableName;
                                Header.m_sOutputType    = sOutputType;

                                sVariableType   = null;
                                sVariableName   = null;
                                sOutputType     = null; 

                                bDetectDeltaTable = false;
                            }
                        }
                        else
                        {
                            if ((sFunctionName != null) && (sVariableType != null) && (sVariableName != null) && (sOutputType != null))
                            {
                                Header.m_VarType        = sVariableType;
                                Header.m_VarName        = sVariableName;
                                Header.m_FuncName       = sFunctionName;
                                Header.m_sOutputType    = sOutputType;

                                sFunctionName   = null;
                                sVariableType   = null;
                                sVariableName   = null;
                                sOutputType     = null;
                            }
                        }
                    }
                    else
                    {
                        if ((sVariableType != null) && (sVariableName != null))
                        {
                            MemberData.m_sDataType = sVariableType;
                            MemberData.m_sDataName = sVariableName;
                            Data.Add(MemberData);

                            sVariableType = null;
                            sVariableName = null;
                        }
                    }
                }

                bRet = true;
            }

            return bRet;
        }

        private bool ParseInterfaceSection(List<string> lsRawData, ref List<INTERFACE_CLASS> InterfaceClass)
        {
            bool bRet = false;

            if ((lsRawData != null) && (lsRawData.Count > 0))
            {
                string  sNameInterface      = null;
                string  sHostCommand        = null;
                string  sValueInterface     = null;
                bool    bvalueInterface     = false;
                bool    bCheckFirstClass    = true;
                uint    uiNameCLass         = 0;

                InterfaceClass = null;
                List<INTERFACE_CLASS> TempInterfaceClass = new List<INTERFACE_CLASS>();
                List<INTERFACE_CLASS_MEMBER> InterfaceMemberClass = new List<INTERFACE_CLASS_MEMBER>();

                INTERFACE_CLASS MemberClass = new INTERFACE_CLASS()
                {
                    m_uiClassNumber = 0,
                    m_lsInterfaceMember = null
                };

                INTERFACE_CLASS_MEMBER MemberInterface = new INTERFACE_CLASS_MEMBER()
                {
                    m_sInterfaceName  = null,
                    m_sCommand        = null,
                    m_bInterfaceValid = false
                };

                foreach (string element in lsRawData)
                {
                    if (element.Contains(KW_CLASS_NAME) == true)
                    {
                        string[] sTempClass = element.Split(KW_SPLIT_EQUAL_CHAR);
                        if (!sTempClass[1].Contains("discontinued"))
                        {
                            if (uiNameCLass != uint.Parse(sTempClass[1]))
                            {
                                if (bCheckFirstClass)
                                {
                                    uiNameCLass = uint.Parse(sTempClass[1]);
                                    bCheckFirstClass = false;
                                }
                                else
                                {
                                    MemberClass.m_uiClassNumber     = uiNameCLass;
                                    MemberClass.m_lsInterfaceMember = InterfaceMemberClass;
                                    TempInterfaceClass.Add(MemberClass);

                                    uiNameCLass = uint.Parse(sTempClass[1]);
                                    InterfaceMemberClass = new List<INTERFACE_CLASS_MEMBER>();
                                }
                            }
                        }
                    }    
                                  
                    if (KW_INTERFACE_NAME.Any(s=>element.Contains(s)) == true)
                    {
                        string[] sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        sTemp[1] = sTemp[1].Trim().Replace("\"", string.Empty);

                        sValueInterface = sTemp[1];
                        sTemp = sTemp[0].Split(KW_SPLIT_COLON_CHAR);

                        sNameInterface = sTemp[0].Trim();
                        sHostCommand = sTemp[1].Trim();

                        if (sValueInterface.Contains("1"))
                        {
                            bvalueInterface = true;    
                        }
                        else
                        {
                            bvalueInterface = false;
                        }

                        if ((sNameInterface != null) && (sValueInterface != null))
                        {
                            MemberInterface.m_sInterfaceName  = sNameInterface;
                            MemberInterface.m_sCommand        = sHostCommand;
                            MemberInterface.m_bInterfaceValid = bvalueInterface;
                         
                            InterfaceMemberClass.Add(MemberInterface);
                        }
                    }
                }

                MemberClass.m_uiClassNumber = uiNameCLass;
                MemberClass.m_lsInterfaceMember = InterfaceMemberClass;
                TempInterfaceClass.Add(MemberClass);

                //----Bubble sort class member----
                InterfaceClass = TempInterfaceClass.OrderBy(o => o.m_uiClassNumber).ToList();
                bRet = true;
            }

            return bRet;
        }

        private bool ParseFlagConfigSection(List<string> lsRawData, ref List<CI_ENUM_INFO>[] lsEnumList)
        {
            bool bRet = false;
            string sFilePath = null;

            if ((lsRawData != null) && (lsRawData.Count > 0))
            {
                foreach (string element in lsRawData)
                {        
                    if (element.Contains(KW_CI_ENUM_FILE_NAME))
                    {
                        string[] sTemp = null;
                        sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        sFilePath = sTemp[1].Trim().Replace("\"", string.Empty);
                        if( string.IsNullOrEmpty(sFilePath) == false )
                            sFilePath = sFilePath.Replace('\\', Path.DirectorySeparatorChar);
                    }
                }

                if (string.IsNullOrEmpty(sFilePath) == false)
                {
                    string sCIEnumAbsTemplateFilePath = null;
                    if (ParseAbsFilePathAndCheckFileExist(sFilePath, ref sCIEnumAbsTemplateFilePath) &&
                        ProcessHardContainFile(sCIEnumAbsTemplateFilePath, ref lsEnumList))
                    {
                        bRet = true;
                    }
                }
            }

            return bRet;
        }

        private bool ParseCmAndFuncSection(List<string> lsRawData, ref TABLE_STRUCT_HEADER_TYPE Header)
        {
            bool bRet = false;

            if (lsRawData != null)
            {
                Header = new TABLE_STRUCT_HEADER_TYPE()
                {
                    m_VarType  = null,
                    m_VarName  = null,
                    m_FuncName = null,
                };

                string sVariableType = null;
                string sVariableName = null;
                string sFunctionName = null;

                foreach (string element in lsRawData)
                {
                    if ((element.Contains(KW_VARIABLE_TYPE)) ||
                        (element.Contains(KW_VARIABLE_NAME)) ||
                        (element.Contains(KW_FUNCTION_NAME)))
                    {
                        string[] sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        if ((sTemp != null) && (sTemp.Length == 2) && (sTemp[0] != null))
                        {
                            sTemp[1] = sTemp[1].Trim().Replace("\"", string.Empty);
                            switch (sTemp[0].Trim())
                            {
                                case KW_VARIABLE_TYPE:
                                    sVariableType = sTemp[1];
                                    break;

                                case KW_VARIABLE_NAME:
                                    sVariableName = sTemp[1];
                                    break;

                                case KW_FUNCTION_NAME:
                                    sFunctionName = sTemp[1];
                                    break;

                                default:
                                    break;
                            }

                            if ((sFunctionName != null) && (sVariableType != null) && (sVariableName != null))
                            {
                                Header.m_VarType  = sVariableType;
                                Header.m_VarName  = sVariableName;
                                Header.m_FuncName = sFunctionName;
                                break;
                            }
                        }
                    }
                }
                bRet = true;
            }

            return bRet;
        }

        private bool ParseCmVerifyExceptionFuncSection(List<string> lsRawData, ref TABLE_STRUCT_HEADER_TYPE Header)
        {
            bool bRet = false;

            if (lsRawData != null)
            {
                Header = new TABLE_STRUCT_HEADER_TYPE()
                {
                    m_VarType  = null,
                    m_VarName  = null,
                    m_FuncName = null,
                };

                string sFunctionName = null;

                foreach (string element in lsRawData)
                {
                    if (element.Contains(KW_FUNCTION_NAME))
                    {
                        string[] sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        if ((sTemp != null) && (sTemp.Length == 2) && (sTemp[0] != null))
                        {
                            sTemp[1] = sTemp[1].Trim().Replace("\"", string.Empty);
                            switch (sTemp[0].Trim())
                            {
                                case KW_FUNCTION_NAME:
                                    sFunctionName = sTemp[1];
                                    break;

                                default:
                                    break;
                            }

                            if (sFunctionName != null)
                            {
                                Header.m_FuncName = sFunctionName;
                                break;
                            }
                        }
                    }
                }
                bRet = true;
            }

            return bRet;
        }

        private bool ParseDeltaGroupNameSection(List<INTERFACE_CLASS> lsInterfaceClass, ref List<DELTA_TABLE_GROUP_NAME> lsDeltaGroupName)
        {
            lsDeltaGroupName = new List<DELTA_TABLE_GROUP_NAME>();

            DELTA_TABLE_GROUP_NAME MemberDeltaGroupName = new DELTA_TABLE_GROUP_NAME()
            {
                m_sInterfaceName        = null,
                m_sEugeneName           = DELTA_EUGENE_NAME + DELTA_NULL_TYPE,
                m_sBolognaName          = DELTA_BOLOGNA_NAME + DELTA_NULL_TYPE,
                m_sEugeneSpecialName    = DELTA_EUGENE_NAME + DELTA_SPECIAL_TYPE + DELTA_NULL_TYPE,
                m_sBolognaSpecialName   = DELTA_BOLOGNA_NAME + DELTA_SPECIAL_TYPE + DELTA_NULL_TYPE,
            };

            // Add null delta table
            lsDeltaGroupName.Add(MemberDeltaGroupName);

            foreach (INTERFACE_CLASS elementClass in lsInterfaceClass)
            {
                foreach (INTERFACE_CLASS_MEMBER elementMember in elementClass.m_lsInterfaceMember)
                {
                    if (elementMember.m_bInterfaceValid)
                    {
                        int tempIndex = elementMember.m_sInterfaceName.IndexOf(KW_SPLIT_CHAR);
                        string sNameInterface = elementMember.m_sInterfaceName.Substring((tempIndex + 1)).ToLower();

                        MemberDeltaGroupName.m_sInterfaceName         = elementMember.m_sInterfaceName;
                        MemberDeltaGroupName.m_sEugeneName            = DELTA_EUGENE_NAME + sNameInterface;
                        MemberDeltaGroupName.m_sBolognaName           = DELTA_BOLOGNA_NAME + sNameInterface;
                        MemberDeltaGroupName.m_sEugeneSpecialName     = DELTA_EUGENE_NAME + DELTA_SPECIAL_TYPE + sNameInterface;
                        MemberDeltaGroupName.m_sBolognaSpecialName    = DELTA_BOLOGNA_NAME + DELTA_SPECIAL_TYPE + sNameInterface;

                        lsDeltaGroupName.Add(MemberDeltaGroupName);
                    }
                }
            }

            return true;
        }
        private bool ParseAbsFilePathAndCheckFileExist(string sRelPath, ref string sAbsPath)
        {
            bool bRet = true;
            if (String.IsNullOrEmpty(sRelPath) == false)
            {
                if (String.IsNullOrEmpty(Path.GetFileName(sRelPath)) == false)
                {
                    if (Path.IsPathRooted(sRelPath) == true)
                    {
                        sAbsPath = sRelPath;
                    }
                    else
                    {
                        if (Path.IsPathRooted(Path.GetDirectoryName(m_sTemplateFilePath)) == true)
                        {
                            sAbsPath = Path.GetDirectoryName(m_sTemplateFilePath) + Path.DirectorySeparatorChar + sRelPath;
                        }
                        else
                        {
                            // sResult = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetDirectoryName(m_TemplateFilePath) + Path.DirectorySeparatorChar + sFileName;
                            sAbsPath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + Path.GetDirectoryName(m_sTemplateFilePath) + Path.DirectorySeparatorChar + sRelPath;
                        }
                    }
            
                    if (File.Exists(sAbsPath) == false)
                    {
                        bRet = false;
                    }
                }
            }
            return bRet;
        }

        private bool ProcessReportFileInfo(PARSED_CONFIG_TYPE ParsedConfig)
        {
            bool bRet = false;

            if (m_SourceTemplate.m_sConfigItemHeaderTemplate != null)
            {
                string sIncludePart = null;
                string sDataTypeDef = null;
                string sDataDeclare = null;
                string sFuncDeclare = null;

                // process create file
                if (ParsedConfig.m_sReportFileName != null)
                {
                    string sReportFilePath = null;

                    if (string.IsNullOrEmpty(ParsedConfig.m_sReportFolderPath) == false)
                    {
                        sReportFilePath = ParsedConfig.m_sReportFolderPath + Path.DirectorySeparatorChar + ParsedConfig.m_sReportFileName;
                    }
                    else
                    {
                        // sReportFilePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + ParsedConfig.m_sReportFileName;
                        sReportFilePath = System.AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + ParsedConfig.m_sReportFileName;
                    }

                    if (m_FileProcessor.CreateNewFile(sReportFilePath) == false)
                    {
                        return false;
                    }
                }

                // process include session
                if ((ParsedConfig.m_lsIncludePart != null) && (ParsedConfig.m_lsIncludePart.Count > 0))
                {
                    foreach (string element in ParsedConfig.m_lsIncludePart)
                    {
                        sIncludePart += string.Format(m_SourceTemplate.m_sIncludeTemplate, element);
                    }
                    sIncludePart += "#include \"config_item_structure.h\"\n";
                }

                // name space
                m_NamespaceFormatter = new CNamespaceFormatter(ParsedConfig.m_sDecleareNamespace, ParsedConfig.m_sUsingNamespace);
                sIncludePart += "\n";
                sIncludePart += m_NamespaceFormatter.GetExternCUsingNamespace();
                sIncludePart += m_NamespaceFormatter.GetExternCDeclareNamespaceHeader();

                // process cm item and function session
                if ((ParsedConfig.m_CmAndFuncSection.m_VarType != null) &&
                    (ParsedConfig.m_CmAndFuncSection.m_VarName != null) &&
                    (ParsedConfig.m_CmAndFuncSection.m_FuncName != null) &&
                    (ParsedConfig.m_NormalTable != null) &&
                    (ParsedConfig.m_NormalHeader.m_VarType != null))
                {
                    string sParamsContent = null;
                    string sAccessNormalFuncType = GetVarTypeInConfigTable(ParsedConfig.m_NormalTable, TABLE_STRUCT_MEMBER_ID.MAX_VALUE_ID);
                    string sAccessNormalParamType = GetVarTypeInConfigTable(ParsedConfig.m_NormalTable, TABLE_STRUCT_MEMBER_ID.CI_NAME_ID);
                    string sAccessNormalStructType = string.Format(m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_5][(int)VAR_ID.TYPE], ParsedConfig.m_NormalHeader.m_VarType);

                    if ((m_SourceTemplate.m_sAccessParamList.Length == 5) &&
                        (sAccessNormalFuncType != null) &&
                        (sAccessNormalParamType != null))
                    {
                        sParamsContent += string.Format(m_SourceTemplate.m_sAccessFuncParamTemplate,
                                                        m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                                        m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        sAccessNormalFuncType,
                                                        m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                                        sAccessNormalParamType,
                                                        m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                                        m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.TYPE],
                                                        m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME],
                                                        sAccessNormalStructType,
                                                        m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_5][(int)VAR_ID.NAME]);
                    }

                    if ((m_SourceTemplate.m_sCmItemDeclareTemplate != null) &&
                        (m_SourceTemplate.m_sFuncTemplate != null) &&
                        (sParamsContent != null))
                    {
                        sDataDeclare += string.Format(m_SourceTemplate.m_sCmItemDeclareTemplate, ParsedConfig.m_CmAndFuncSection.m_VarType, ParsedConfig.m_CmAndFuncSection.m_VarName);
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate, m_SourceTemplate.GetIndent(0), sAccessNormalFuncType, ParsedConfig.m_CmAndFuncSection.m_FuncName, sParamsContent, m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process cm verify exception function session
                if ((ParsedConfig.m_CmVerifyExceptFuncSection.m_FuncName != null) &&
                    (ParsedConfig.m_NormalHeader.m_VarType != null))
                {
                    string sParamsContent = null;
                    string sNormalTableType = ParsedConfig.m_NormalHeader.m_VarType;

                    if ((m_SourceTemplate.m_sVerifyExceptParamList.Length == 2) &&
                        (sNormalTableType != null))
                    {
                        sParamsContent += string.Format(m_SourceTemplate.m_sVerifyExceptParamTemplate,
                                                        sNormalTableType,
                                                        m_SourceTemplate.m_sVerifyExceptParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        m_SourceTemplate.m_sVerifyExceptParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                        m_SourceTemplate.m_sVerifyExceptParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                    }

                    if ((m_SourceTemplate.m_sFuncTemplate != null) &&
                        (sParamsContent != null))
                    {
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate, 
                                                                                      m_SourceTemplate.GetIndent(0), 
                                                                                      m_SourceTemplate.m_sVerifyExceptFunctionType, 
                                                                                      ParsedConfig.m_CmVerifyExceptFuncSection.m_FuncName, 
                                                                                      sParamsContent, 
                                                                                      m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process cm verify range special function session
                if ((string.IsNullOrEmpty(ParsedConfig.m_CmVerifyRangeFuncSection.m_FuncName) == false) &&
                    (string.IsNullOrEmpty(ParsedConfig.m_SpecialHeader.m_VarType) == false))
                {
                    string sParamsContent = null;
                    string sSpecialTableType = ParsedConfig.m_SpecialHeader.m_VarType;

                    if ((m_SourceTemplate.m_sVerifyRangeParamList.Length == 2) &&
                        (sSpecialTableType != null))
                    {
                        sParamsContent += string.Format(m_SourceTemplate.m_sVerifyRangeParamTemplate,
                                                        sSpecialTableType,
                                                        m_SourceTemplate.m_sVerifyRangeParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        m_SourceTemplate.m_sVerifyRangeParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                        m_SourceTemplate.m_sVerifyRangeParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                    }

                    if ((m_SourceTemplate.m_sFuncTemplate != null) &&
                        (sParamsContent != null))
                    {
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                                                      m_SourceTemplate.GetIndent(0),
                                                                                      m_SourceTemplate.m_sVerifyRangeFunctionType,
                                                                                      ParsedConfig.m_CmVerifyRangeFuncSection.m_FuncName,
                                                                                      sParamsContent,
                                                                                      m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                //process cm convert hex int function session
                for (int i = 0; i < m_SourceTemplate.m_sConvertHexIntFuncName.Count(); i++)
                {
                    if (string.IsNullOrEmpty(m_SourceTemplate.m_sConvertHexIntFuncName[i]) == false)
                    {
                        string sParamsContent = string.Format(m_SourceTemplate.m_sConvertHexIntParamTemplate,
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE],
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME],
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.TYPE],
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME],
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.TYPE],
                                                              m_SourceTemplate.m_sConvertHexIntParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME]);

                        if ((m_SourceTemplate.m_sFuncTemplate != null) &&
                            (sParamsContent != null))
                        {
                            sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                                                          m_SourceTemplate.GetIndent(0),
                                                                                          m_SourceTemplate.m_sConvertHexIntFunctionType[i],
                                                                                          m_SourceTemplate.m_sConvertHexIntFuncName[i],
                                                                                          sParamsContent,
                                                                                          m_SourceTemplate.m_sFuncTerminator);
                        }
                    }
                }
                // process cm verify readable ascii special function session
                if ((string.IsNullOrEmpty(ParsedConfig.m_CmVerifyReadableAsciiFuncSection.m_FuncName) == false) &&
                  (string.IsNullOrEmpty(ParsedConfig.m_SpecialHeader.m_VarType) == false))
                {
                    string sParamsContent = null;
                    string sSpecialTableType = ParsedConfig.m_SpecialHeader.m_VarType;

                    if ((m_SourceTemplate.m_sVerifyReadableAsciiParamList.Length == 2) &&
                        (sSpecialTableType != null))
                    {
                        sParamsContent += string.Format(m_SourceTemplate.m_sVerifyReadableAsciiParamTemplate,
                                                        sSpecialTableType,
                                                        m_SourceTemplate.m_sVerifyReadableAsciiParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        m_SourceTemplate.m_sVerifyReadableAsciiParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                        m_SourceTemplate.m_sVerifyReadableAsciiParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                    }

                    if ((m_SourceTemplate.m_sFuncTemplate != null) &&
                       (sParamsContent != null))
                    {
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                                                      m_SourceTemplate.GetIndent(0),
                                                                                      m_SourceTemplate.m_sVerifyReadableAsciiFunctionType,
                                                                                      ParsedConfig.m_CmVerifyReadableAsciiFuncSection.m_FuncName,
                                                                                      sParamsContent,
                                                                                      m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process cm verify selective special function session
                if ((string.IsNullOrEmpty(ParsedConfig.m_CmVerifySelectiveFuncSection.m_FuncName) == false) &&
                    (string.IsNullOrEmpty(ParsedConfig.m_SpecialHeader.m_VarType) == false))
                {
                    string sParamsContent = null;
                    string sSpecialTableType = ParsedConfig.m_SpecialHeader.m_VarType;

                    if ((m_SourceTemplate.m_sVerifySelectiveParamList.Length == 2) &&
                        (sSpecialTableType != null))
                    {
                        sParamsContent += string.Format(m_SourceTemplate.m_sVerifySelectiveParamTemplate,
                                                        sSpecialTableType,
                                                        m_SourceTemplate.m_sVerifySelectiveParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME],
                                                        m_SourceTemplate.m_sVerifySelectiveParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE],
                                                        m_SourceTemplate.m_sVerifySelectiveParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                    }

                    if ((m_SourceTemplate.m_sFuncTemplate != null) &&
                        (sParamsContent != null))
                    {
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                                                      m_SourceTemplate.GetIndent(0),
                                                                                      m_SourceTemplate.m_sVerifySelectiveFunctionType,
                                                                                      ParsedConfig.m_CmVerifySelectiveFuncSection.m_FuncName,
                                                                                      sParamsContent,
                                                                                      m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process cm verify interface value function session
                if (string.IsNullOrEmpty(ParsedConfig.m_CmVerifyInterfaceFuncSection.m_FuncName) == false)
                {
                    string sParamsContent = null;

                    if (m_SourceTemplate.m_sVerifyInterfaceParamList.Length == 2)
                    {
                        sParamsContent += string.Format(m_SourceTemplate.m_sVerifyInterfaceParamTemplate,
                                                        m_SourceTemplate.m_sVerifyInterfaceParamList[(int)VAR_ID.TYPE],
                                                        m_SourceTemplate.m_sVerifyInterfaceParamList[(int)VAR_ID.NAME]);
                    }

                    if ((m_SourceTemplate.m_sFuncTemplate != null) &&
                        (sParamsContent != null))
                    {
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                                                      m_SourceTemplate.GetIndent(0),
                                                                                      m_SourceTemplate.m_sVerifyInterfaceFunctionType,
                                                                                      ParsedConfig.m_CmVerifyInterfaceFuncSection.m_FuncName,
                                                                                      sParamsContent,
                                                                                      m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process normal session
                if ((ParsedConfig.m_NormalHeader.m_VarType  != null) &&
                    (ParsedConfig.m_NormalHeader.m_VarName  != null) &&
                    (ParsedConfig.m_NormalHeader.m_FuncName != null) &&
                    (ParsedConfig.m_NormalTable != null))
                {
                    string sTableStructName = ParsedConfig.m_NormalHeader.m_VarType;
                    string sTableStructElements = null;
                    foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_NormalTable)
                    {
                        if ((m_SourceTemplate.m_sTableTypeElementTemplate != null) &&
                            (element.m_sDataType != null) &&
                            (element.m_sDataName != null))
                        {
                            sTableStructElements += string.Format(m_SourceTemplate.m_sTableTypeElementTemplate, element.m_sDataType, element.m_sDataName);
                        }
                    }

                    if ((m_SourceTemplate.m_sNorTableMemExtend != null) &&
                        (m_SourceTemplate.m_sNorTableMemExtend.Length > 0))
                    {
                        foreach (string[] element_1 in m_SourceTemplate.m_sNorTableMemExtend)
                        {
                            sTableStructElements += string.Format(m_SourceTemplate.m_sTableTypeElementTemplate, element_1[(int)VAR_ID.TYPE], element_1[(int)VAR_ID.NAME]);
                        }
                    }

                    if ((m_SourceTemplate.m_sTableTypeDefTemplate != null) &&
                        (m_SourceTemplate.m_sTableDeclareTemplate != null) &&
                        (m_SourceTemplate.m_sGetTableFuncTemplate != null))
                    {
                        sDataTypeDef += string.Format(m_SourceTemplate.m_sTableTypeDefTemplate, KW_STRUCT_TYPE, sTableStructName, sTableStructElements, null) + "\n";
                        sDataDeclare += string.Format(m_SourceTemplate.m_sTableDeclareTemplate, ParsedConfig.m_NormalHeader.m_VarType, ParsedConfig.m_NormalHeader.m_VarName);
                        sFuncDeclare += string.Format(m_SourceTemplate.m_sGetTableFuncTemplate, ParsedConfig.m_NormalHeader.m_VarType, ParsedConfig.m_NormalHeader.m_FuncName, null, m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process special session
                if ((ParsedConfig.m_SpecialHeader.m_VarType  != null) &&
                    (ParsedConfig.m_SpecialHeader.m_VarName  != null) &&
                    (ParsedConfig.m_SpecialHeader.m_FuncName != null) &&
                    (ParsedConfig.m_SpecialTable != null))
                {
                    string sTableStructName = ParsedConfig.m_SpecialHeader.m_VarType;
                    string sTableStructElements = null;
                    foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_SpecialTable)
                    {
                        if ((m_SourceTemplate.m_sTableTypeElementTemplate != null) &&
                            (element.m_sDataType != null) &&
                            (element.m_sDataName != null))
                        {
                            sTableStructElements += string.Format(m_SourceTemplate.m_sTableTypeElementTemplate, element.m_sDataType, element.m_sDataName);
                        }
                    }

                    if ((m_SourceTemplate.m_sTableTypeDefTemplate != null) &&
                        (m_SourceTemplate.m_sTableDeclareTemplate != null) &&
                        (m_SourceTemplate.m_sGetTableFuncTemplate != null))
                    {
                        sDataTypeDef += string.Format(m_SourceTemplate.m_sTableTypeDefTemplate, KW_STRUCT_TYPE, sTableStructName, sTableStructElements, null) + "\n";
                        sDataDeclare += string.Format(m_SourceTemplate.m_sTableDeclareTemplate, ParsedConfig.m_SpecialHeader.m_VarType, ParsedConfig.m_SpecialHeader.m_VarName) + "\n";
                        sFuncDeclare += string.Format(m_SourceTemplate.m_sGetTableFuncTemplate, ParsedConfig.m_SpecialHeader.m_VarType, ParsedConfig.m_SpecialHeader.m_FuncName, null, m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process normal delta session
                if ((ParsedConfig.m_DeltaNormalHeader.m_VarType != null) &&
                    (ParsedConfig.m_DeltaNormalHeader.m_VarName != null) &&
                    (ParsedConfig.m_DeltaNormalTable != null))
                {
                    string sTableStructType = ParsedConfig.m_DeltaNormalHeader.m_VarType;
                    string sTableStructName = ParsedConfig.m_DeltaNormalHeader.m_VarName;
                    string sTableStructElements = null;

                    foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_DeltaNormalTable)
                    {
                        if ((m_SourceTemplate.m_sTableTypeElementTemplate != null) &&
                            (element.m_sDataType != null) &&
                            (element.m_sDataName != null))
                        {
                            sTableStructElements += string.Format(m_SourceTemplate.m_sTableTypeElementTemplate, element.m_sDataType, element.m_sDataName);
                        }
                    }

                    if (m_SourceTemplate.m_sTableTypeDefTemplate != null)
                    {
                        sDataTypeDef += string.Format(  m_SourceTemplate.m_sTableTypeDefTemplate,
                                                        KW_TYPEDEF_STRUCT_TYPE,
                                                        sTableStructType, 
                                                        sTableStructElements,
                                                        sTableStructName) + "\n";
                    }
                }

                // process special delta session
                if ((ParsedConfig.m_DeltaSpecialHeader.m_VarType != null) &&
                    (ParsedConfig.m_DeltaSpecialHeader.m_VarName != null) &&
                    (ParsedConfig.m_DeltaSpecialTable != null))
                {
                    string sTableStructType = ParsedConfig.m_DeltaSpecialHeader.m_VarType;
                    string sTableStructName = ParsedConfig.m_DeltaSpecialHeader.m_VarName;
                    string sTableStructElements = null;

                    foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_DeltaSpecialTable)
                    {
                        if ((m_SourceTemplate.m_sTableTypeElementTemplate != null) &&
                            (element.m_sDataType != null) &&
                            (element.m_sDataName != null))
                        {
                            sTableStructElements += string.Format(m_SourceTemplate.m_sTableTypeElementTemplate, element.m_sDataType, element.m_sDataName);
                        }
                    }

                    if (m_SourceTemplate.m_sTableTypeDefTemplate != null)
                    {                     
                        sDataTypeDef += string.Format(  m_SourceTemplate.m_sTableTypeDefTemplate,
                                                        KW_TYPEDEF_STRUCT_TYPE,
                                                        sTableStructType,
                                                        sTableStructElements,
                                                        sTableStructName) + "\n";
                    }
                }

                // process group delta table name
                if (ParsedConfig.m_lsDeltaGroupTableName != null)
                {
                    string sTempNormalDeclare   = null;
                    string sTempSpecialDeclare  = null;

                    foreach (DELTA_TABLE_GROUP_NAME element in ParsedConfig.m_lsDeltaGroupTableName)
                    {
                        sTempNormalDeclare += string.Format(m_SourceTemplate.m_sDeltaTableDeclareTemplate,
                                                                ParsedConfig.m_DeltaNormalHeader.m_VarName,
                                                                element.m_sEugeneName);

                        sTempSpecialDeclare += string.Format(m_SourceTemplate.m_sDeltaTableDeclareTemplate,
                                                                ParsedConfig.m_DeltaSpecialHeader.m_VarName,
                                                                element.m_sEugeneSpecialName);

                    }

                    sDataDeclare += sTempNormalDeclare + "\n" + sTempSpecialDeclare;
                }

                // process normal search item index data
                if(ParsedConfig.m_SearchNormalItemIndexTableHeader.m_FuncName != null)
                {
                    string sParamsContent = null;
                    string sSearchNormalParamType = GetVarTypeInConfigTable(ParsedConfig.m_NormalTable, TABLE_STRUCT_MEMBER_ID.CI_NAME_ID);
                    if (m_SourceTemplate.m_sSearchNormalItemParamList.Length == 2)
                    {
                        sParamsContent += string.Format(m_SourceTemplate.m_sSearchNormalItemParamTemplate,
                                                        sSearchNormalParamType,
                                                        m_SourceTemplate.m_sSearchNormalItemParamList[(int)VAR_ID.NAME]);
                    }

                    if ((m_SourceTemplate.m_sFuncTemplate != null) &&
                        (sParamsContent != null))
                    {
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                                                      m_SourceTemplate.GetIndent(0),
                                                                                      m_SourceTemplate.m_sSearchNormalFunctionType,
                                                                                      ParsedConfig.m_SearchNormalItemIndexTableHeader.m_FuncName,
                                                                                      sParamsContent,
                                                                                      m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // process normal table get size
                if (ParsedConfig.m_GetNormalTableSizeHeader.m_FuncName != null)
                {
                    if (m_SourceTemplate.m_sFuncTemplate != null)
                    {
                        sFuncDeclare += m_SourceTemplate.GetIndent(1) + string.Format(m_SourceTemplate.m_sFuncTemplate,
                                                                                      m_SourceTemplate.GetIndent(0),
                                                                                      m_SourceTemplate.m_sGetNormalTableSizeFuncType,
                                                                                      ParsedConfig.m_GetNormalTableSizeHeader.m_FuncName,
                                                                                      null,
                                                                                      m_SourceTemplate.m_sFuncTerminator);
                    }
                }

                // write report contain to report file
                sDataTypeDef = m_NamespaceFormatter.FormatStringWithIndent(sDataTypeDef);
                sDataDeclare = m_NamespaceFormatter.FormatStringWithIndent(sDataDeclare);
                sFuncDeclare = m_NamespaceFormatter.FormatStringWithIndent(sFuncDeclare);

                string sExternCHeader = null;
                string sExternCFooter = null;

                if ((ParsedConfig.m_sDecleareNamespace == string.Empty) && (ParsedConfig.m_sUsingNamespace == string.Empty))
                {
                    sExternCHeader = m_NamespaceFormatter.FormatStringWithIndent(m_SourceTemplate.m_sExternCHeader);
                    sExternCFooter = m_NamespaceFormatter.FormatStringWithIndent(m_SourceTemplate.m_sExternCFooter);
                }

                string sReportContent = string.Format(  m_SourceTemplate.m_sConfigItemHeaderTemplate,
                                                    sIncludePart,
                                                    sDataTypeDef,
                                                    sExternCHeader,
                                                    sDataDeclare,
                                                    sFuncDeclare,
                                                    sExternCFooter);

                sReportContent += m_NamespaceFormatter.GetExternCDeclareNamespaceFooter();

                if (sReportContent != null)
                {
                    m_FileProcessor.AddNewItem(sReportContent);
                    m_FileProcessor.CloseFile();
                    bRet = true;
                }
            }

            return bRet;
        }

        private string GetVarTypeInConfigTable(List<TABLE_STRUCT_DATA_TYPE> ConfigTable, TABLE_STRUCT_MEMBER_ID sTableMember)
        {
            string sRet = null;

            if ((ConfigTable != null) && (ConfigTable.Count > 0) && (sTableMember != TABLE_STRUCT_MEMBER_ID.UNKNOW_ID))
            {
                foreach (TABLE_STRUCT_DATA_TYPE element in ConfigTable)
                {
                    if (element.m_DatID == sTableMember)
                    {
                        sRet = element.m_sDataType;
                    }
                }
            }

            return sRet;
        }

        private CSourceInfoObject UpdateSourceCodeInfo(PARSED_CONFIG_TYPE ParsedConfig)
        {
            CSourceInfoObject SourceCodeInfo = new CSourceInfoObject();

            // update folder path
            SourceCodeInfo.SetOutputFolder(ParsedConfig.m_sReportFolderPath);

            // update access normal function
            if ((ParsedConfig.m_CmAndFuncSection.m_FuncName != null) &&
                (ParsedConfig.m_NormalTable != null) &&
                (ParsedConfig.m_NormalHeader.m_VarType != null))
            {
                string sAccessNormalParamType1 = GetVarTypeInConfigTable(ParsedConfig.m_NormalTable, TABLE_STRUCT_MEMBER_ID.MAX_VALUE_ID);
                string sAccessNormalParamType2 = GetVarTypeInConfigTable(ParsedConfig.m_NormalTable, TABLE_STRUCT_MEMBER_ID.CI_NAME_ID);
                string sAccessNormalParamType3 = string.Format(m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_5][(int)VAR_ID.TYPE], ParsedConfig.m_NormalHeader.m_VarType);

                SourceCodeInfo.SetNormalAccessFunctType(sAccessNormalParamType1);
                SourceCodeInfo.SetNormalAccessFunctName(ParsedConfig.m_CmAndFuncSection.m_FuncName);

                if (m_SourceTemplate.m_sAccessParamList.Length == 5)
                {
                    SourceCodeInfo.AddNormalAccessFunctParams(m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.TYPE], m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddNormalAccessFunctParams(sAccessNormalParamType1, m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddNormalAccessFunctParams(sAccessNormalParamType2, m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_3][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddNormalAccessFunctParams(m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.TYPE], m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_4][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddNormalAccessFunctParams(sAccessNormalParamType3, m_SourceTemplate.m_sAccessParamList[(int)PARAMS_ID.PAR_5][(int)VAR_ID.NAME]);
                }
            }

            // update verify exception item normal function
            if ((ParsedConfig.m_CmVerifyExceptFuncSection.m_FuncName != null) &&
                (ParsedConfig.m_NormalHeader.m_VarType != null))
            {
                string sVerifyExceptParamType = m_SourceTemplate.m_sVerifyExceptFunctionType;

                SourceCodeInfo.SetNormalVerifyExceptFunctType(sVerifyExceptParamType);
                SourceCodeInfo.SetNormalVerifyExceptFunctName(ParsedConfig.m_CmVerifyExceptFuncSection.m_FuncName);

                if (m_SourceTemplate.m_sVerifyExceptParamList.Length == 2)
                {
                    SourceCodeInfo.AddNormalVerifyExceptFunctParams(ParsedConfig.m_NormalHeader.m_VarType, m_SourceTemplate.m_sVerifyExceptParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddNormalVerifyExceptFunctParams(m_SourceTemplate.m_sVerifyExceptParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE], m_SourceTemplate.m_sVerifyExceptParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                }
            }


            // update verify range item special function
            if ((ParsedConfig.m_CmVerifyRangeFuncSection.m_FuncName != null) &&
                (ParsedConfig.m_SpecialHeader.m_VarType != null))
            {
                string sVerifyRangeParamType = m_SourceTemplate.m_sVerifyRangeFunctionType;

                SourceCodeInfo.SetSpecialVerifyRangeFuncType(sVerifyRangeParamType);
                SourceCodeInfo.SetSpecialVerifyRangeFuncName(ParsedConfig.m_CmVerifyRangeFuncSection.m_FuncName);

                if (m_SourceTemplate.m_sVerifyRangeParamList.Length == 2)
                {
                    SourceCodeInfo.AddSpecialVerifyRangeFuncParams(ParsedConfig.m_SpecialHeader.m_VarType, m_SourceTemplate.m_sVerifyRangeParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddSpecialVerifyRangeFuncParams(m_SourceTemplate.m_sVerifyRangeParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE], m_SourceTemplate.m_sVerifyRangeParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                }
            }
            //
            //update verify readable ascii item special function
            if ((ParsedConfig.m_CmVerifyReadableAsciiFuncSection.m_FuncName != null) &&
               (ParsedConfig.m_SpecialHeader.m_VarType != null))
            {
                string sVerifyReadableAsciiParamType = m_SourceTemplate.m_sVerifyReadableAsciiFunctionType;

                SourceCodeInfo.SetSpecialVerifyReadableAsciiFuncType(sVerifyReadableAsciiParamType);
                SourceCodeInfo.SetSpecialVerifyReadableAsciiFuncName(ParsedConfig.m_CmVerifyReadableAsciiFuncSection.m_FuncName);

                if (m_SourceTemplate.m_sVerifyReadableAsciiParamList.Length == 2)
                {
                    SourceCodeInfo.AddSpecialVerifyReadableAsciiFuncParams(ParsedConfig.m_SpecialHeader.m_VarType, m_SourceTemplate.m_sVerifyReadableAsciiParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddSpecialVerifyReadableAsciiFuncParams(m_SourceTemplate.m_sVerifyReadableAsciiParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE], m_SourceTemplate.m_sVerifyReadableAsciiParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                }
            }

            // update verify selective item special function
            if ((ParsedConfig.m_CmVerifySelectiveFuncSection.m_FuncName != null) &&
                (ParsedConfig.m_SpecialHeader.m_VarType != null))
            {
                string sVerifySelectiveParamType = m_SourceTemplate.m_sVerifySelectiveFunctionType;

                SourceCodeInfo.SetSpecialVerifySelectiveFuncType(sVerifySelectiveParamType);
                SourceCodeInfo.SetSpecialVerifySelectiveFuncName(ParsedConfig.m_CmVerifySelectiveFuncSection.m_FuncName);

                if (m_SourceTemplate.m_sVerifySelectiveParamList.Length == 2)
                {
                    SourceCodeInfo.AddSpecialVerifySelectiveFuncParams(ParsedConfig.m_SpecialHeader.m_VarType, m_SourceTemplate.m_sVerifySelectiveParamList[(int)PARAMS_ID.PAR_1][(int)VAR_ID.NAME]);
                    SourceCodeInfo.AddSpecialVerifySelectiveFuncParams(m_SourceTemplate.m_sVerifySelectiveParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.TYPE], m_SourceTemplate.m_sVerifySelectiveParamList[(int)PARAMS_ID.PAR_2][(int)VAR_ID.NAME]);
                }
            }

            // update verify interface value function
            if (ParsedConfig.m_CmVerifyInterfaceFuncSection.m_FuncName != null)
            {
                string sVerifyInterfaceParamType = m_SourceTemplate.m_sVerifyInterfaceFunctionType;

                SourceCodeInfo.SetVerifyInterfaceValueFuncType(sVerifyInterfaceParamType);
                SourceCodeInfo.SetVerifyInterfaceValueFuncName(ParsedConfig.m_CmVerifyInterfaceFuncSection.m_FuncName);

                if (m_SourceTemplate.m_sVerifyInterfaceParamList.Length == 2)
                {
                    SourceCodeInfo.AddVerifyInterfaceValueFuncParams(m_SourceTemplate.m_sVerifyInterfaceParamList[(int)VAR_ID.TYPE], m_SourceTemplate.m_sVerifyInterfaceParamList[(int)VAR_ID.NAME]);
                }
            }

            // update search normal item index function
            if (ParsedConfig.m_SearchNormalItemIndexTableHeader.m_FuncName != null)
            {
                string sSearchNormalItemIndexType = m_SourceTemplate.m_sSearchNormalFunctionType;
                string sSearchNormalParamType = GetVarTypeInConfigTable(ParsedConfig.m_NormalTable, TABLE_STRUCT_MEMBER_ID.CI_NAME_ID);

                SourceCodeInfo.SetSearchNormalItemIndexFuncType(sSearchNormalItemIndexType);
                SourceCodeInfo.SetSearchNormalItemIndexFuncName(ParsedConfig.m_SearchNormalItemIndexTableHeader.m_FuncName);

                if (m_SourceTemplate.m_sSearchNormalItemParamList.Length == 2)
                {
                    SourceCodeInfo.AddNormalSearchNormalItemIndexFunctParams(sSearchNormalParamType, m_SourceTemplate.m_sSearchNormalItemParamList[(int)VAR_ID.NAME]);
                }
            }

            // update get normal table size function
            if (ParsedConfig.m_GetNormalTableSizeHeader.m_FuncName != null)
            {
                string sGetNormalTableSizeFuncType = m_SourceTemplate.m_sGetNormalTableSizeFuncType;

                SourceCodeInfo.SetNormalTableGetSizeFuncType(sGetNormalTableSizeFuncType);
                SourceCodeInfo.SetNormalTableGetSizeFuncName(ParsedConfig.m_GetNormalTableSizeHeader.m_FuncName);
            }

            // update normal table
            if ((ParsedConfig.m_NormalHeader.m_VarType  != null) &&
                (ParsedConfig.m_NormalHeader.m_VarName  != null) &&
                (ParsedConfig.m_NormalHeader.m_FuncName != null) &&
                (ParsedConfig.m_NormalHeader.m_sOutputType != null) &&
                (ParsedConfig.m_NormalTable != null))
            {
                SourceCodeInfo.SetNormalTableType(ParsedConfig.m_NormalHeader.m_VarType);
                SourceCodeInfo.SetNormalTableName(ParsedConfig.m_NormalHeader.m_VarName);
                SourceCodeInfo.SetNormalTableGetFunc(ParsedConfig.m_NormalHeader.m_FuncName);
                SourceCodeInfo.SetNormalTableOutputType(ParsedConfig.m_NormalHeader.m_sOutputType);

                foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_NormalTable)
                {
                    if (element.m_DatID != TABLE_STRUCT_MEMBER_ID.UNKNOW_ID)
                    {
                        SourceCodeInfo.AddNormalTalbeElementOrder(element);
                    }
                }
            }

            // update special table
            if ((ParsedConfig.m_SpecialHeader.m_VarType    != null) &&
                (ParsedConfig.m_SpecialHeader.m_VarName    != null) &&
                (ParsedConfig.m_SpecialHeader.m_FuncName   != null) &&
                (ParsedConfig.m_SpecialHeader.m_sOutputType != null) &&
                (ParsedConfig.m_CmAndFuncSection.m_VarName != null) &&
                (ParsedConfig.m_SpecialTable != null))
            {
                SourceCodeInfo.SetSpecialTableType(ParsedConfig.m_SpecialHeader.m_VarType);
                SourceCodeInfo.SetSpecialTableName(ParsedConfig.m_SpecialHeader.m_VarName);
                SourceCodeInfo.SetSpecialTableOutputType(ParsedConfig.m_SpecialHeader.m_sOutputType);
                SourceCodeInfo.SetSpecialTableGetFunc(ParsedConfig.m_SpecialHeader.m_FuncName);
                SourceCodeInfo.SetCmVariableName(ParsedConfig.m_CmAndFuncSection.m_VarName);

                foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_SpecialTable)
                {
                    if (element.m_DatID != TABLE_STRUCT_MEMBER_ID.UNKNOW_ID)
                    {
                        SourceCodeInfo.SetSpecialTalbeElementOrder(element);
                    }
                }
            }

            // update normal Delta table
            if ((ParsedConfig.m_DeltaNormalHeader.m_VarType != null)        &&
                (ParsedConfig.m_DeltaNormalHeader.m_VarName != null)        &&
                (ParsedConfig.m_DeltaNormalHeader.m_sOutputType != null)    &&
                (ParsedConfig.m_DeltaNormalTable != null))
            {
                SourceCodeInfo.SetNormalDeltaTableType(ParsedConfig.m_DeltaNormalHeader.m_VarType);
                SourceCodeInfo.SetNormalDeltaTableName(ParsedConfig.m_DeltaNormalHeader.m_VarName);
                SourceCodeInfo.SetNormalDeltaTableOutputType(ParsedConfig.m_DeltaNormalHeader.m_sOutputType);

                foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_DeltaNormalTable)
                {
                    if (element.m_DatID != TABLE_STRUCT_MEMBER_ID.UNKNOW_ID)
                    {
                        SourceCodeInfo.AddNormalDeltaTalbeElementOrder(element);
                    }
                }
            }

            // update special Delta table
            if ((ParsedConfig.m_DeltaSpecialHeader.m_VarType != null)       &&
                (ParsedConfig.m_DeltaSpecialHeader.m_VarName != null)       &&
                (ParsedConfig.m_DeltaSpecialHeader.m_sOutputType != null)   &&
                (ParsedConfig.m_DeltaSpecialTable != null))
            {
                SourceCodeInfo.SetSpecialDeltaTableType(ParsedConfig.m_DeltaSpecialHeader.m_VarType);
                SourceCodeInfo.SetSpecialDeltaTableName(ParsedConfig.m_DeltaSpecialHeader.m_VarName);
                SourceCodeInfo.SetSpecialDeltaTableOutputType(ParsedConfig.m_DeltaSpecialHeader.m_sOutputType);

                foreach (TABLE_STRUCT_DATA_TYPE element in ParsedConfig.m_DeltaSpecialTable)
                {
                    if (element.m_DatID != TABLE_STRUCT_MEMBER_ID.UNKNOW_ID)
                    {
                        SourceCodeInfo.AddSpecialDeltaTalbeElementOrder(element);
                    }
                }
            }

            // update Interface
            if (m_ParsedConfig.m_InterfaceClass != null)
            {
                SourceCodeInfo.SetInterface(m_ParsedConfig.m_InterfaceClass);
            }

            // update Exception enum items only
            SourceCodeInfo.SetExceptionEnumItem(m_ParsedConfig.m_lsExceptionEnumPart);

            // update Exception enum and struct
            SourceCodeInfo.SetExceptionEnumStructItem(m_ParsedConfig.m_lsExceptionEnumStructPart);
            
            // update code structure
            if (ParsedConfig.m_CmAndFuncSection.m_VarType != null)
            {
                SourceCodeInfo.SetStructureType(ParsedConfig.m_CmAndFuncSection.m_VarType);
            }

            // update output file name
            if (ParsedConfig.m_sReportFileName != null)
            {
                SourceCodeInfo.SetOutputFileName(ParsedConfig.m_sReportFileName);
            }

            // update Delta group name
            if (ParsedConfig.m_lsDeltaGroupTableName != null)
            {
                SourceCodeInfo.SetDeltaGroupName(ParsedConfig.m_lsDeltaGroupTableName);
            }

            // update Namespace
            SourceCodeInfo.SetDecleareNamespace(ParsedConfig.m_sDecleareNamespace);
            SourceCodeInfo.SetUsingNamespace(ParsedConfig.m_sUsingNamespace);

            // update List of CI Enum Info
            SourceCodeInfo.SetCIEnumInfo(ParsedConfig.m_lsCIEnumInfo);

            return SourceCodeInfo;
        }

        private bool ProcessHardContainFile(string sFilePath, ref List<CI_ENUM_INFO>[] lsCIEnumInfo)
        {
            // struct of a valid line in template file
            /************* normal item struct - use in source code*************/
            // CI_TAG_NAME = enum_value, /*CI_TAG_CODE*/
            // CI_TAG_NAME = enum value,

            /************* Aladdin item struct - not use in source code*************/
            // CI_TAG_NAME , /*CI_TAG_CODE*/ -- Aladdin struct

            bool bRet = false;

            // Create the info list
            lsCIEnumInfo = new List<CI_ENUM_INFO>[(int)CI_ENUM_INFO_LIST_ID.TOTAL_CI_ENUM_TYPE];
            for(int i = 0; i < lsCIEnumInfo.Length; i++)
            {
                lsCIEnumInfo[i] = new List<CI_ENUM_INFO>();
            }

            if(m_FileProcessor != null && sFilePath != string.Empty)
            {
                string[] sFileContain = m_FileProcessor.LoadingFile(sFilePath);
                if (sFileContain != null)
                {
                    string sStartCIAladdinMarking = "Aladdin internal item dont use in source code";
                    bool bAddtoAladdinList = false;
                    foreach (string element in sFileContain)
                    {
                        if(element.Contains(sStartCIAladdinMarking))
                        {
                            bAddtoAladdinList = true;
                        }
                        string sReplaceTabBySpace = element.Replace('\t', ' ');
                        string[] sTempSplitWithComma = sReplaceTabBySpace.Split(KW_SPLIT_COMMA_CHAR);
                        if(sTempSplitWithComma.Length == 2)
                        {
                            CI_ENUM_INFO stTempInfo;
                            string[] sTempSplitWithEqual = sTempSplitWithComma[0].Split(KW_SPLIT_EQUAL_CHAR);
                            stTempInfo.m_sCITagName = sTempSplitWithEqual[0].Trim(' ');
                            stTempInfo.m_sCITagCode = "";
                            Match match = System.Text.RegularExpressions.Regex.Match(sTempSplitWithComma[1], @"[/][*][A-Z0-9]{4}[*][/]");
                            if (match.Success)
                            {
                                stTempInfo.m_sCITagCode = match.Captures[0].Value.Trim('*', '/', ' ');
                            }

                            stTempInfo.m_sCIEnumValue = "";
                            if (sTempSplitWithEqual.Length == 2)
                            {
                                stTempInfo.m_sCIEnumValue = sTempSplitWithEqual[1].Trim(' ');
                            }

                            if (!bAddtoAladdinList)
                            {
                                lsCIEnumInfo[(int)CI_ENUM_INFO_LIST_ID.NORMAL_CI_ENUM].Add(stTempInfo);
                            }
                            else
                            {
                                lsCIEnumInfo[(int)CI_ENUM_INFO_LIST_ID.ALADDIN_CI_ENUM].Add(stTempInfo);
                            }
                        }
                    }
                    bRet = true;
                }
            }
            return bRet;
        }

        private string ConvertTail(string sData)
        {
            if (sData.Contains(KW_OUTPUT_CPP_TYPE) || sData.Contains(KW_OUTPUT_CPP_TYPE.ToUpper()))
            {
                return m_SourceTemplate.m_sCPPtype;
            }
            else
            {
                return m_SourceTemplate.m_sHeadertype;
            }
        }
    }
}
