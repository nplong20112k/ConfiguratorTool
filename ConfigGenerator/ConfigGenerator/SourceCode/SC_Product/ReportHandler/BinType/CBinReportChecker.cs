using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ConfigGenerator
{
    class CBinReportChecker : AReportChecker
    {
        private enum OVERAL_AREAS_ID
        {
            INPUT_SETTINGS_ID = 0,
            EXCEPTION_ENUM_ID,
            EXCEPTION_ENUM_STRUCTURE_ID,
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

        private struct RAW_CONFIG_TYPE
        {
            public List<string>     m_lsExceptionEnumContain;
            public List<string>     m_lsInterfaceSessionContain;
            public List<string>     m_lsFlagContain;
            public string           m_sReportFolderPath;
        };

        private struct PARSED_CONFIG_TYPE
        {
            public bool                                 m_bValid;
            public List<string>                         m_lsExceptionEnumPart;
            public List<CI_ENUM_INFO>[]                 m_lsCIEnumInfo;
            public List<SUPPORT_INTERFACE>              m_lsSupportInterfaceInfo;
            public string                               m_sReportFolderPath;
            public List<INTERFACE_CLASS>                m_InterfaceClass;
        }

        private const char   KW_SPLIT_EQUAL_CHAR                = '=';
        private const char   KW_SPLIT_COMMENT_CHAR              = '/';  
        private const char   KW_SPLIT_COLON_CHAR                = ':';
        private const char   KW_SPLIT_COMMA_CHAR                = ',';
        private const string KW_CI_ENUM_FILE_NAME               = "CI_ENUM_TEMPLATE_FILE_PATH";
        private const string KW_SUPPORT_INTERFACE_FILE_NAME     = "SUPPORT_INTERFACE_TEMPLATE_FILE_PATH";
        private const string KW_FOLDER_PATH                     = "OUTPUT_FOLDER_PATH";
        private const string KW_CLASS_NAME                      = "CLASS";
        public static readonly List<string> KW_INTERFACE_NAME   = new List<string>() { "INTERFACE_", "IF_" };

        private IBinFileProcessor  m_FileProcessor      = null;

        private string              m_sTemplateFilePath = null;
        private string[]            m_sFileContent      = null;

        private RAW_CONFIG_TYPE     m_RawConfig;
        private PARSED_CONFIG_TYPE  m_ParsedConfig;
        private CSourceInfoObject   m_SourceCodeInfo;

        private static readonly OVERAL_STRUCTURE_TYPE[] TEMPLATE_FILE_OVERAL_STRUCTURE = {
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.INPUT_SETTINGS_ID,                        m_AreaDes = "INPUT SETTINGS"                        },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.EXCEPTION_ENUM_ID,                        m_AreaDes = "EXCEPTION ENUM ITEMS"                  },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.EXCEPTION_ENUM_STRUCTURE_ID,              m_AreaDes = "EXCEPTION ENUM AND STRUCTURE ITEMS"    },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.INTERFACE_SETTINGS_ID,                    m_AreaDes = "INTERFACE SETTINGS"                    },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.FEATURE_SETTINGS_ID,                      m_AreaDes = "FEATURE SETTINGS"                      },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.OUTPUT_SETTINGS_ID,                       m_AreaDes = "OUTPUT SETTINGS"                       },
            new OVERAL_STRUCTURE_TYPE() { m_AreaID = OVERAL_AREAS_ID.END_CONFIG_AREAD_ID,                      m_AreaDes = "END CONFIG AREAD"                      },
        };

        public CBinReportChecker()
            : base(AReportChecker.REPORT_FILE_TYPE.REPORT_FILE_SOURCE_BIN)
        {
            m_FileProcessor  = CFactoryBinFileProcessor.GetInstance().GetFileProcessor(BIN_FILE_PROCESSOR_TYPE.BIN_CHECKER);
            if (m_FileProcessor == null)
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
                m_SourceCodeInfo = UpdateSourceCodeInfo(m_ParsedConfig);
                if (m_SourceCodeInfo != null)
                {
                    CEventForwarder.GetInstance().Notify(EVENT_TYPE.EVENT_REQUEST_UPDATE_CONFIG_INFO, m_SourceCodeInfo);
                }

                bRet = true;
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
                m_lsExceptionEnumContain                = new List<string>(),
                m_lsInterfaceSessionContain             = new List<string>(),
                m_lsFlagContain                         = new List<string>(),
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
                    if (string.IsNullOrEmpty(element) == false)
                    {       
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
                        if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.EXCEPTION_ENUM_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.EXCEPTION_ENUM_ID;
                        }
                        else if (element.Contains(TEMPLATE_FILE_OVERAL_STRUCTURE[(int)OVERAL_AREAS_ID.EXCEPTION_ENUM_STRUCTURE_ID].m_AreaDes))
                        {
                            FlagAreasDetect = OVERAL_AREAS_ID.EXCEPTION_ENUM_STRUCTURE_ID;
                            break;
                        }
                        
                        // update areas config
                        switch (FlagAreasDetect)
                        {
                            case OVERAL_AREAS_ID.EXCEPTION_ENUM_ID:
                                ConfigContaintData.m_lsExceptionEnumContain.Add(element);
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
            };

            // parser output setting
            if (m_RawConfig.m_sReportFolderPath != null)
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

                ParserContainRet.m_sReportFolderPath += Path.DirectorySeparatorChar + "config_items";
            }

            // parse exception enum part
            if (ParseExceptionEnumItem(m_RawConfig.m_lsExceptionEnumContain, ref ParserContainRet.m_lsExceptionEnumPart) == false)
            {
                Program.SystemHandleStatusInfo("WARNING:: Missing manually handle CI items information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parser Interface session part
            if (ParseInterfaceSection(m_RawConfig.m_lsInterfaceSessionContain, ref ParserContainRet.m_InterfaceClass) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing/Ivalid interface information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            // parse Flags session part
            if (ParseFlagConfigSection(m_RawConfig.m_lsFlagContain, ref ParserContainRet.m_lsCIEnumInfo, ref ParserContainRet.m_lsSupportInterfaceInfo) == false)
            {
                Program.SystemHandleStatusInfo("ERROR:: Missing/Invalid CIRO options information !!!\r\n");
                ParserContainRet.m_bValid = false;
            }

            return ParserContainRet;
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

        private bool ParseFlagConfigSection(List<string> lsRawData, ref List<CI_ENUM_INFO>[] lsEnumList, ref List<SUPPORT_INTERFACE> lsSupportInterfaceList)
        {
            bool bRet = false;
            string sFilePath = null;
            string sJsonFilePath = null;

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

                    else if(element.Contains(KW_SUPPORT_INTERFACE_FILE_NAME))
                    {
                        string[] sTemp = null;
                        sTemp = element.Split(KW_SPLIT_EQUAL_CHAR);
                        sJsonFilePath = sTemp[1].Trim().Replace("\"", string.Empty);
                        if( string.IsNullOrEmpty(sJsonFilePath) == false )
                            sJsonFilePath = sJsonFilePath.Replace('\\', Path.DirectorySeparatorChar);

                        // parser support interface template json file path - if file exist handle to parser info
                        if (string.IsNullOrEmpty(sJsonFilePath) == false)
                        {
                            string sSupportInterfaceAbsTemplateFilePath = null;
                            if (ParseAbsFilePathAndCheckFileExist(sJsonFilePath, ref sSupportInterfaceAbsTemplateFilePath) &&
                                ProcessHardContainJsonFile(sSupportInterfaceAbsTemplateFilePath, ref lsSupportInterfaceList))
                            {
                                bRet = true;
                            }
                            else
                            {
                                return false;
                            }
                        }
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

        private CSourceInfoObject UpdateSourceCodeInfo(PARSED_CONFIG_TYPE ParsedConfig)
        {
            CSourceInfoObject SourceCodeInfo = new CSourceInfoObject();

            // update folder path
            SourceCodeInfo.SetOutputFolder(ParsedConfig.m_sReportFolderPath);

            // update Exception enum items only
            SourceCodeInfo.SetExceptionEnumItem(m_ParsedConfig.m_lsExceptionEnumPart);

            // update Interface
            if (m_ParsedConfig.m_InterfaceClass != null)
            {
                SourceCodeInfo.SetInterface(m_ParsedConfig.m_InterfaceClass);
            }

            // update List of CI Enum Info
            SourceCodeInfo.SetCIEnumInfo(ParsedConfig.m_lsCIEnumInfo);

            // update List of Support Interface Info
            SourceCodeInfo.SetSupportInterfaceInfo(ParsedConfig.m_lsSupportInterfaceInfo);

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

        private bool ProcessHardContainJsonFile(string sFilePath, ref List<SUPPORT_INTERFACE> lsSupportInterfaceInfo)
        {
            // "interface":
            // {
            //     "name":"IBM_17",
            //     "value":"0x04",
            //     "config block":"0x04",
            //     "user block":"0x81",
            //     "mask":"0x0000000000000100"
            // }

            bool bRet = false;

            // Create the info list
            lsSupportInterfaceInfo = new List<SUPPORT_INTERFACE>();

            if((m_FileProcessor != null) && (string.IsNullOrEmpty(sFilePath) == false))
            {
                string[] sFileContain = m_FileProcessor.LoadingFile(sFilePath);
                if (sFileContain != null)
                {
                    SUPPORT_INTERFACE stTempSupportInterface = new SUPPORT_INTERFACE();
                    string sJsonData = "";
                    foreach (string element in sFileContain)
                    {
                        sJsonData += element;
                    }

                    var jsonObject = JObject.Parse(sJsonData);
                    var Itf = jsonObject["interface"].Children();

                    foreach (var subItf in Itf)
                    {
                        stTempSupportInterface.m_sName = "";
                        stTempSupportInterface.m_sName = subItf["name"].ToString().Replace("0x", "");

                        stTempSupportInterface.m_sValue = "";
                        stTempSupportInterface.m_sValue = subItf["value"].ToString().Replace("0x", "");
                        
                        stTempSupportInterface.m_sConfigBlock = "";
                        stTempSupportInterface.m_sConfigBlock = subItf["config block"].ToString().Replace("0x", "");
                        
                        stTempSupportInterface.m_sUserBlock = "";
                        stTempSupportInterface.m_sUserBlock = subItf["user block"].ToString().Replace("0x", "");
                        
                        stTempSupportInterface.m_sMask = "";
                        stTempSupportInterface.m_sMask = subItf["mask"].ToString().Replace("0x", "");

                        lsSupportInterfaceInfo.Add(stTempSupportInterface);
                    }

                    bRet = true;
                }
            }
            return bRet;
        }  
    }
}
