using System.IO;

namespace ConfigGenerator
{
    public enum VAR_ID
    {
        TYPE = 0,
        NAME,
    }

    public enum PARAMS_ID
    {
        PAR_1 = 0,
        PAR_2,
        PAR_3,
        PAR_4,
        PAR_5,

        PARAM_TOTAL,
    }

    public class CSourceCodeTemplate
    {
        public string m_sSourceFolderName           = Path.DirectorySeparatorChar + "config_items";

        public string m_sExternCHeader              =   "#ifdef __cplusplus\n"  +
                                                        "extern \"C\"\n"        +
                                                        "{\n"                  +
                                                        "#endif\n";

        public string m_sExternCFooter              =   "#ifdef __cplusplus\n"  +
                                                        "}\n"                  +
                                                        "#endif\n";

        public string m_sCPlusPlusHeader            =   "#ifdef __cplusplus\n";

        public string m_sCPlusPlusFooter            =   "#endif\n";

        public string m_sConfigItemHeaderTemplate   =   "{0}"                   +
                                                        "\n"                    +
                                                        "{1}"                   +
                                                        "typedef enum\n"        +
                                                        "{{\n"                  +
                                                        "    SET = 1,\n"        +
                                                        "    GET = 0,\n"        +
                                                        "}} ACCESS_TYPE;\n"     +
                                                        "\n"                    +
                                                        "{2}\n"                 +
                                                        "{3}\n"                 +
                                                        "{4}\n"                 +
                                                        "{5}";

        public string m_sIncludeTemplate            =   "#include \"{0}\"\n";
                                                  
        public string m_sTableTypeDefTemplate       =   "{0} {1}\n"                             +
                                                        "{{\n"                                     +
                                                        "{2}"                                      +
                                                        "}}{3};\n";

        public string m_sTableTypeElementTemplate   = "    {0,-25} {1};\n";
                                                  
        public string m_sTableDeclareTemplate       = "    extern const struct {0} {1}[];\n";

        public string m_sDeltaTableDeclareTemplate  = "    extern const {0} {1}[];\n";

        public string m_sFuncTemplate               = "{0}{1} {2} ({3}){4}\n";

        public string[][] m_sAccessParamList        =  new string[][]{  new string[2]{ "ACCESS_TYPE", "SetFlag"     },
                                                                        new string[2]{ "{0}"        , "Value"       },
                                                                        new string[2]{ "{0}"        , "CINumber"    },
                                                                        new string[2]{ "bool"       , "*bValid"     },
                                                                        new string[2]{ "struct {0}" , "*pNormalItem"}};

        public string m_sSearchNormalItemParamTemplate = "{0} {1}";  

        public string[] m_sSearchNormalItemParamList  = new string[] { "{0}", "CINumber" };

        public string m_sSearchNormalFunctionType = "int";

        public string m_sGetNormalTableSizeFuncType = "int";

        public string m_sAccessFuncParamTemplate    = "{0} {1}, {2} {3}, {4} {5}, {6} {7}, {8} {9}";

        public string[][] m_sVerifyExceptParamList  =  new string[][]{  new string[2]{ "{0}"            , "NormalItemPtr"  },
                                                                        new string[2]{ "unsigned char"  , "Value"          }};

        public string m_sVerifyExceptParamTemplate  = "struct {0}* {1}, {2} {3}";

        public string m_sVerifyExceptFunctionType   = "bool";

        public string[][] m_sVerifyRangeParamList   = new string[][]{  new string[2]{ "{0}"             , "SpecialItemPtr"  },
                                                                       new string[2]{ "unsigned char *" , "pValue"          }};

        public string m_sVerifyRangeParamTemplate   = "struct {0}* {1}, {2} {3}";

        public string m_sVerifyRangeFunctionType    = "bool";

        public string[][] m_sConvertHexIntParamList = new string[][] {
                                                                             new string[2] { "int", "iItem"},
                                                                             new string[2] { "unsigned int", "uiSizeItem"},
                                                                             new string[2] { "unsigned char *", "pucTmp"  },
                                                                             new string[2] { "unsigned char *", "pucTmpOut" }};
        public string m_sConvertHexIntParamTemplate = "{0} {1}, {2} {3}, {4} {5}, {6} {7}";
        public string[] m_sConvertHexIntFunctionType = new string[2] { "int", "int"};
        public string[] m_sConvertHexIntFuncName = new string[2] { "ConvertSpecialItemHextoInt", "ConvertSpecialItemInttoHex"};

        public string[][] m_sVerifyReadableAsciiParamList = new string[][]{  new string[2]{ "{0}"             , "SpecialItemPtr"  },
                                                                       new string[2]{ "unsigned char *" , "pValue"          }};

        public string m_sVerifyReadableAsciiParamTemplate = "struct {0}* {1}, {2} {3}";

        public string m_sVerifyReadableAsciiFunctionType = "bool";

        public string[][] m_sVerifySelectiveParamList   = new string[][]{  new string[2]{ "{0}"             , "SpecialItemPtr"  },
                                                                       new string[2]{ "unsigned char *" , "pValue"          }};

        public string m_sVerifySelectiveParamTemplate   = "struct {0}* {1}, {2} {3}";

        public string m_sVerifySelectiveFunctionType    = "bool";

        public string[] m_sVerifyInterfaceParamList = new string[] { "unsigned char", "ucValue" };

        public string m_sVerifyInterfaceParamTemplate = "{0} {1}";

        public string m_sVerifyInterfaceFunctionType = "bool";

        public string m_sGetTableFuncTemplate       = "    struct {0}* {1}({2}){3}\n";
                                                          
        public string m_sCmItemDeclareTemplate      = "    extern {0} {1};\n";

        public string[][] m_sNorTableMemExtend      = new string[][] { new string[2] { "unsigned char", "usIndex"     },
                                                                       new string[2] { "unsigned char", "ucLocation"  },
                                                                       new string[2] { "unsigned char", "ucBitLength" }};

        public string m_sConfigStructureExtend      = "ci_category_{0}bit";

        public string m_sCINumberType               = "CI_NUMBER_TYPE";

        public string m_sCINormalTerminate          = "CI_NORMAL_TERMINATE";

        public string m_sCIByteTypeName             = "ci_category_1byte";

        public string m_sFuncTerminator             = ";";

        public string m_sIncludeHeader              = "#include \"{0}\"\n" +
                                                      "\n";

        public string m_sCPPtype                    = ".cpp";

        public string m_sHeadertype                 = ".h";

        private string m_sIndent                     = "    ";

        public string m_sDeltaTableItem             = "{0}{{ {1,-54},{2} }},\n";

        public string m_sUsingNamespace             = "using namespace {0};\n\n";

        public string m_sDecleareNamespace          = "namespace {0}\n" +
                                                      "{{\n" +
                                                      "{1}" +
                                                      "}}\n";

        public string m_sDecleareNamespaceHeader    = "namespace {0}\n" +
                                                      "{{\n";

        public string m_sDecleareNamespaceFooter    = "}\n";

        public string m_sTypedefEnumItem            = "{0}    {1} = {2},\n";

        public string m_sTypedefHeader              = "{0}typedef enum \n{0}{{\n";

        public string m_sTypedefEnumItemWithComment = "{0}    {1} = {2}, /*{3}*/\n";

        public string m_sTypedefEnumItemWithCommentWithoutValue = "{0}    {1}, /*{2}*/\n";

        public string m_sTypedefFooter              = "{0}}} {1} ;\n";

        readonly static CSourceCodeTemplate m_Instance = new CSourceCodeTemplate();

        public static CSourceCodeTemplate GetInstance()
        {
            return m_Instance;
        }

        public string GetIndent(uint uiNumIndent)
        {
            string sRet = "";

            for (int i = 0; i < uiNumIndent; i++)
            {
                sRet += m_sIndent;
            }

            return sRet;
        }
    }
}
