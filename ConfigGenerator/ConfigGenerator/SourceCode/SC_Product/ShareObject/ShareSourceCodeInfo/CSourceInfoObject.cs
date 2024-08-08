using System;
using System.Collections.Generic;

namespace ConfigGenerator
{
    public enum TABLE_STRUCT_MEMBER_ID
    {
        UNKNOW_ID = 0,
        CI_NAME_ID,
        CI_CODE_ID,
        MIN_VALUE_ID,
        MAX_VALUE_ID,
        INTERFACE_MASK_ID,
        PRODUCT_MASK_ID,
        DEFAULT_VALUE,
        CI_ADDRESS_ID,
        CI_DEFAULT_VALUE,
        CI_DEFAULT_ALT_VALUE,
        CI_SIZE_ID,
    }

    public enum CI_ENUM_INFO_LIST_ID
    {
        NORMAL_CI_ENUM = 0,
        ALADDIN_CI_ENUM,
        TOTAL_CI_ENUM_TYPE
    }

    public struct TABLE_STRUCT_DATA_TYPE
    {
        public TABLE_STRUCT_MEMBER_ID m_DatID;
        public string m_sDataType;
        public string m_sDataName;
    }

    public struct EXCEPTION_CI_ITEM
    {
        public string   m_sNameCIItem;
        public string   m_sValuesize; 
    }

    public struct INTERFACE_CLASS_MEMBER
    {
        public string   m_sInterfaceName;
        public string   m_sCommand;
        public bool     m_bInterfaceValid;
    }

    public struct DELTA_TABLE_ITEM
    {
        public string m_sInterfaceName;
        public string m_sValue;
    }

    public struct INTERFACE_CLASS
    {
        public uint m_uiClassNumber;
        public List<INTERFACE_CLASS_MEMBER> m_lsInterfaceMember;
    }

    public struct DELTA_TABLE_GROUP_NAME
    {
        public string m_sInterfaceName;
        public string m_sEugeneName;
        public string m_sBolognaName;
        public string m_sEugeneSpecialName;
        public string m_sBolognaSpecialName;
    }

    public struct CI_ENUM_INFO
    {
        public string m_sCIEnumValue;
        public string m_sCITagName;
        public string m_sCITagCode;
    }

    public struct SUPPORT_INTERFACE
    {
        public string m_sName;
        public string m_sValue;
        public string m_sConfigBlock;
        public string m_sUserBlock;
        public string m_sMask;
    }

    public class CSourceInfoObject : AShareObject, INormalAccessFuncProperties, INormalTableProperties, ISpecialTableProperties, ICodeStructure, IInterfaceProperties, INormalVerifyExceptFuncProperties, ISpecialVerifyRangeFuncProperties, ISpecialVerifySelectiveFuncProperties, IVerifyInterfaceValueFuncProperties, ISpecialVerifyReadableAsciiFuncProperties
    {
        // common
        private string m_sOutputFolder      = null;
        private string m_sOutputFileName    = null;

        // normal access function
        private string m_sNormalAccessFuncType  = null;
        private string m_sNormalAccessFuncName  = null; 
        private List<string[]> m_lsNormalAccessFuncParams = null;

        // normal search index function
        private string m_sSearchNormalItemIndexFuncName = null;
        private string m_sSearchNormalItemIndexFuncType = null;
        private List<string[]> m_lsSearchNormalItemIndexFuncParams = null;

        // normal verify exception function
        private string m_sNormalVerifyExceptFuncType  = null;
        private string m_sNormalVerifyExceptFuncName  = null; 
        private List<string[]> m_lsNormalVerifyExceptFuncParams = null;

        // special verify range function
        private string m_sSpecialVerifyRangeFuncType = null;
        private string m_sSpecialVerifyRangeFuncName = null;
        private List<string[]> m_lsSpecialVerifyRangeFuncParams = null;

        //special verify readable ascii function
        private string m_sSpecialVerifyReadableAsciiFuncType = null;
        private string m_sSpecialVerifyReadableAsciiFuncName = null;
        private List<string[]> m_lsSpecialVerifyReadableAsciiFuncParams = null;

        // special verify selective function
        private string m_sSpecialVerifySelectiveFuncType = null;
        private string m_sSpecialVerifySelectiveFuncName = null;
        private List<string[]> m_lsSpecialVerifySelectiveFuncParams = null;

        // verify interface value function
        private string m_sVerifyInterfaceValueFuncType = null;
        private string m_sVerifyInterfaceValueFuncName = null;
        private List<string[]> m_lsVerifyInterfaceValueFuncParams = null;

        // normal table
        private string m_sNormalTableType       = null;
        private string m_sNormalTableName       = null;
        private string m_sNormalTableGetFunc    = null;
        private string m_sNormalTableOutputType = null;
        private string m_sNormalTableGetSizeFuncName = null;
        private string m_sNormalTableGetSizeType = null;
        private List<TABLE_STRUCT_DATA_TYPE> m_NormalTableElementOrder = null;

        // special table
        private string m_sSpecialTableType          = null;
        private string m_sSpecialTableName          = null;
        private string m_sSpecialTableGetFunc       = null;
        private string m_sSpecialTableOutputType    = null;
        private string m_sCmVariableName            = null;
        private List<TABLE_STRUCT_DATA_TYPE> m_SpecialTableElementOrder = null;

        // group delta table name
        private List<DELTA_TABLE_GROUP_NAME> m_lsDeltaGroupName  = null;
             
        // normal delta table    
        private string m_sNormalDeltaTableType          = null;
        private string m_sNormalDeltaTableName          = null;
        private string m_sNormalDeltaTableOutputType    = null;
        private List<TABLE_STRUCT_DATA_TYPE> m_NormalDeltaTableElementOrder = null;

        // special delta table
        private string m_sSpecialDeltaTableType = null;
        private string m_sSpecialDeltaTableName = null;
        private string m_sSpecialDeltaTableOutputType = null;
        private List<TABLE_STRUCT_DATA_TYPE> m_SpecialDeltaTableElementOrder = null;

        // code structure
        private string m_sStructureType = null;

        // interface
        private List<INTERFACE_CLASS> m_InterfaceClass = new List<INTERFACE_CLASS>();

        // Exception only Enum items
        private List<string>            m_lsExceptionEnumItem;

        // Exception Enum and Struct items
        private List<EXCEPTION_CI_ITEM> m_lsExceptionEnumStructItem;

        // Namespace
        private string m_sDecleareNamespace = null;
        private string m_sUsingNamespace    = null;

        // EnumInfo
        private List<CI_ENUM_INFO>[] m_lsCIEnumInfo;
        private List<SUPPORT_INTERFACE> m_lsSupportInterfaceInfo;

        public CSourceInfoObject()
            : base(SHARE_OBJECT_ID.SOB_ID_SOURCE_CONFIG_INFO_OBJECT)
        {
        }

        // set output folder
        public void SetOutputFolder(string sData)
        {
            m_sOutputFolder = sData;
        }

        // set output Filename
        public void SetOutputFileName(string sData)
        {
            m_sOutputFileName = sData;
        }

        // set normal access function properties
        public void SetNormalAccessFunctType (string sData)
        {
            m_sNormalAccessFuncType = sData;
        } 

        public void SetNormalAccessFunctName (string sData)
        {
            m_sNormalAccessFuncName = sData;
        }

        public void AddNormalAccessFunctParams(string sParamType, string sParamName)
        {
            if (m_lsNormalAccessFuncParams == null)
            {
                m_lsNormalAccessFuncParams = new List<string[]>();
            }
            m_lsNormalAccessFuncParams.Add(new string[] { sParamType, sParamName});
        }
        public List<CI_ENUM_INFO>[] GetCIEnumInfo()
        {
            return m_lsCIEnumInfo;
        }

        public void SetCIEnumInfo(List<CI_ENUM_INFO>[] lsInfo)
        {
            m_lsCIEnumInfo = lsInfo;
        }
        
        public List<SUPPORT_INTERFACE> GetSupportInterfaceInfo()
        {
            return m_lsSupportInterfaceInfo;
        }

        public void SetSupportInterfaceInfo(List<SUPPORT_INTERFACE> lsInfo)
        {
            m_lsSupportInterfaceInfo = lsInfo;
        }

        // set search normal item function properties
        public void SetSearchNormalItemIndexFuncName(string sData)
        {
            m_sSearchNormalItemIndexFuncName = sData;
        }

        public void SetSearchNormalItemIndexFuncType(string sData)
        {
            m_sSearchNormalItemIndexFuncType = sData;
        }

        public void AddNormalSearchNormalItemIndexFunctParams(string sParamType, string sParamName)
        {
            if (m_lsSearchNormalItemIndexFuncParams == null)
            {
                m_lsSearchNormalItemIndexFuncParams = new List<string[]>();
            }
            m_lsSearchNormalItemIndexFuncParams.Add(new string[] { sParamType, sParamName });
        }

        // set normal verify exception function properties
        public void SetNormalVerifyExceptFunctType (string sData)
        {
            m_sNormalVerifyExceptFuncType = sData;
        } 

        public void SetNormalVerifyExceptFunctName (string sData)
        {
            m_sNormalVerifyExceptFuncName = sData;
        }

        public void AddNormalVerifyExceptFunctParams(string sParamType, string sParamName)
        {
            if (m_lsNormalVerifyExceptFuncParams == null)
            {
                m_lsNormalVerifyExceptFuncParams = new List<string[]>();
            }
            m_lsNormalVerifyExceptFuncParams.Add(new string[] { sParamType, sParamName});
        }

        // set verify range value function properties
        public void SetSpecialVerifyRangeFuncType(string sData)
        {
            m_sSpecialVerifyRangeFuncType = sData;
        }

        public void SetSpecialVerifyRangeFuncName(string sData)
        {
            m_sSpecialVerifyRangeFuncName = sData;
        }

        public void AddSpecialVerifyRangeFuncParams(string sParamType, string sParamName)
        {
            if (m_lsSpecialVerifyRangeFuncParams == null)
            {
                m_lsSpecialVerifyRangeFuncParams = new List<string[]>();
            }
            m_lsSpecialVerifyRangeFuncParams.Add(new string[] { sParamType, sParamName });
        }

        //set verify readable ascii value function properties
        public void SetSpecialVerifyReadableAsciiFuncType(string sData)
        {
            m_sSpecialVerifyReadableAsciiFuncType = sData;
        }

        public void SetSpecialVerifyReadableAsciiFuncName(string sData)
        {
            m_sSpecialVerifyReadableAsciiFuncName = sData;
        }

        public void AddSpecialVerifyReadableAsciiFuncParams(string sParamType, string sParamName)
        {
            if (m_lsSpecialVerifyReadableAsciiFuncParams == null)
            {
                m_lsSpecialVerifyReadableAsciiFuncParams = new List<string[]>();
            }
            m_lsSpecialVerifyReadableAsciiFuncParams.Add(new string[] { sParamType, sParamName });
        }

        // set verify selective value function properties
        public void SetSpecialVerifySelectiveFuncType(string sData)
        {
            m_sSpecialVerifySelectiveFuncType = sData;
        }

        public void SetSpecialVerifySelectiveFuncName(string sData)
        {
            m_sSpecialVerifySelectiveFuncName = sData;
        }

        public void AddSpecialVerifySelectiveFuncParams(string sParamType, string sParamName)
        {
            if (m_lsSpecialVerifySelectiveFuncParams == null)
            {
                m_lsSpecialVerifySelectiveFuncParams = new List<string[]>();
            }
            m_lsSpecialVerifySelectiveFuncParams.Add(new string[] { sParamType, sParamName });
        }

        // set verify interface value function properties
        public void SetVerifyInterfaceValueFuncType(string sData)
        {
            m_sVerifyInterfaceValueFuncType = sData;
        }

        public void SetVerifyInterfaceValueFuncName(string sData)
        {
            m_sVerifyInterfaceValueFuncName = sData;
        }

        public void AddVerifyInterfaceValueFuncParams(string sParamType, string sParamName)
        {
            if (m_lsVerifyInterfaceValueFuncParams == null)
            {
                m_lsVerifyInterfaceValueFuncParams = new List<string[]>();
            }
            m_lsVerifyInterfaceValueFuncParams.Add(new string[] { sParamType, sParamName });
        }

        // set normal table properties
        public void SetNormalTableType(string sData)
        {
            m_sNormalTableType = sData;
        }

        public void SetNormalTableOutputType(string sData)
        {
            m_sNormalTableOutputType = sData;
        }

        public void SetNormalTableName(string sData)
        {
            m_sNormalTableName = sData;
        }

        public void SetNormalTableGetFunc(string sData)
        {
            m_sNormalTableGetFunc = sData;
        }

        public void AddNormalTalbeElementOrder(TABLE_STRUCT_DATA_TYPE lData)
        {
            if (m_NormalTableElementOrder == null)
            {
                m_NormalTableElementOrder = new List<TABLE_STRUCT_DATA_TYPE>();
            }
            m_NormalTableElementOrder.Add(lData);
        }

        public void SetNormalTableGetSizeFuncName(string sData)
        {
            m_sNormalTableGetSizeFuncName = sData;
        }

        public void SetNormalTableGetSizeFuncType(string sData)
        {
            m_sNormalTableGetSizeType = sData;
        }

        // set special table properties
        public void SetSpecialTableType(string sData)
        {
            m_sSpecialTableType = sData;
        }

        public void SetSpecialTableOutputType(string sData)
        {
            m_sSpecialTableOutputType = sData;
        }

        public void SetSpecialTableName(string sData)
        {
            m_sSpecialTableName = sData;
        }

        public void SetSpecialTableGetFunc(string sData)
        {
            m_sSpecialTableGetFunc = sData;
        }

        public void SetCmVariableName(string sData)
        {
            m_sCmVariableName = sData;
        }

        public void SetSpecialTalbeElementOrder(TABLE_STRUCT_DATA_TYPE lData)
        {
            if (m_SpecialTableElementOrder == null)
            {
                m_SpecialTableElementOrder = new List<TABLE_STRUCT_DATA_TYPE>();
            }
            m_SpecialTableElementOrder.Add(lData);
        }

        // set normal Delta table properties
        public void AddNormalDeltaTalbeElementOrder(TABLE_STRUCT_DATA_TYPE lData)
        {
            if (m_NormalDeltaTableElementOrder == null)
            {
                m_NormalDeltaTableElementOrder = new List<TABLE_STRUCT_DATA_TYPE>();
            }
            m_NormalDeltaTableElementOrder.Add(lData);
        }

        public void SetNormalDeltaTableType(string sData)
        {
            m_sNormalDeltaTableType = sData;
        }

        public void SetNormalDeltaTableName(string sData)
        {
            m_sNormalDeltaTableName = sData;
        }

        public void SetNormalDeltaTableOutputType(string sData)
        {
            m_sNormalDeltaTableOutputType = sData;
        }

        // set special table properties
        public void AddSpecialDeltaTalbeElementOrder(TABLE_STRUCT_DATA_TYPE lData)
        {
            if (m_SpecialDeltaTableElementOrder == null)
            {
                m_SpecialDeltaTableElementOrder = new List<TABLE_STRUCT_DATA_TYPE>();
            }
            m_SpecialDeltaTableElementOrder.Add(lData);
        }

        public void SetSpecialDeltaTableType(string sData)
        {
            m_sSpecialDeltaTableType = sData;
        }

        public void SetSpecialDeltaTableName(string sData)
        {
            m_sSpecialDeltaTableName = sData;
        }

        public void SetSpecialDeltaTableOutputType(string sData)
        {
            m_sSpecialDeltaTableOutputType = sData;
        }

        // set structure type properties
        public void SetStructureType(string sData)
        {
            m_sStructureType = sData;
        }

        // set interface
        public void SetInterface(List<INTERFACE_CLASS> InterfaceClass)
        {
            m_InterfaceClass    = InterfaceClass;
        }

        // set exception only enum items
        public void SetExceptionEnumItem(List<string> lsExceptionEnumItem)
        {
            m_lsExceptionEnumItem = lsExceptionEnumItem;
        }

        // set exception enum and struct items
        public void SetExceptionEnumStructItem(List<EXCEPTION_CI_ITEM> lsExceptionEnumStructItem)
        {
            m_lsExceptionEnumStructItem = lsExceptionEnumStructItem;
        }

        // set Delta group name
        public void SetDeltaGroupName(List<DELTA_TABLE_GROUP_NAME> lsData)
        {
            m_lsDeltaGroupName = lsData;
        }

        // set Decleare Namespace
        public void SetDecleareNamespace(string sData)
        {
            if (sData.Length != 0)
            {
                m_sDecleareNamespace = sData;
            }
        }

        // set Using Namespace
        public void SetUsingNamespace(string sData)
        {
            if (sData.Length != 0)
            {
                m_sUsingNamespace = sData;
            }
        }

        // get output folder properties
        public string GetOutputFolder()
        {
            return m_sOutputFolder;
        }

        // get output file name
        public string GetOutputFileName()
        {
            return m_sOutputFileName;
        }

        // get normal access function properties
        public string GetNormalAccessFuncType()
        {
            return m_sNormalAccessFuncType;
        }

        public string GetNormalAccessFuncName()
        {
            return m_sNormalAccessFuncName;
        }

        public List<string[]> GetNormalAccessFuncParams()
        {
            return m_lsNormalAccessFuncParams;
        }

        // get search normal item index properties
        public string GetSearchNormalItemIndexFuncName()
        {
            return m_sSearchNormalItemIndexFuncName;
        }

        public string GetSearchNormalItemIndexFuncType()
        {
            return m_sSearchNormalItemIndexFuncType;
        }

        public List<string[]> GetSearchNormalItemIndexParamList()
        {
            return m_lsSearchNormalItemIndexFuncParams;
        }
  

        // get normal verify exception function properties
        public string GetNormalVerifyExceptFuncType()
        {
            return m_sNormalVerifyExceptFuncType;
        }

        public string GetNormalVerifyExceptFuncName()
        {
            return m_sNormalVerifyExceptFuncName;
        }

        public List<string[]> GetNormalVerifyExceptFuncParams()
        {
            return m_lsNormalVerifyExceptFuncParams;
        }

        // get special verify range function properties
        public string GetSpecialVerifyRangeFuncType()
        {
            return m_sSpecialVerifyRangeFuncType;
        }

        public string GetSpecialVerifyRangeFuncName()
        {
            return m_sSpecialVerifyRangeFuncName;
        }

        public List<string[]> GetSpecialVerifyRangeFuncParams()
        {
            return m_lsSpecialVerifyRangeFuncParams;
        }

        // get special verify readable ascii function properties
        public string GetSpecialVerifyReadableAsciiFuncType()
        {
            return m_sSpecialVerifyReadableAsciiFuncType;
        }

        public string GetSpecialVerifyReadableAsciiFuncName()
        {
            return m_sSpecialVerifyReadableAsciiFuncName;
        }

        public List<string[]> GetSpecialVerifyReadableAsciiFuncParams()
        {
            return m_lsSpecialVerifyReadableAsciiFuncParams;
        }

        // get special verify selective function properties
        public string GetSpecialVerifySelectiveFuncType()
        {
            return m_sSpecialVerifySelectiveFuncType;
        }

        public string GetSpecialVerifySelectiveFuncName()
        {
            return m_sSpecialVerifySelectiveFuncName;
        }

        public List<string[]> GetSpecialVerifySelectiveFuncParams()
        {
            return m_lsSpecialVerifySelectiveFuncParams;
        }

        // get verify interface value function properties
        public string GetVerifyInterfaceValueFuncType()
        {
            return m_sVerifyInterfaceValueFuncType;
        }

        public string GetVerifyInterfaceValueFuncName()
        {
            return m_sVerifyInterfaceValueFuncName;
        }

        public List<string[]> GetVerifyInterfaceValueFuncParams()
        {
            return m_lsVerifyInterfaceValueFuncParams;
        }

        // get normal table properties
        public string GetNormalTableType()
        {
            return m_sNormalTableType;
        }

        public string GetNormalTableName()
        {
            return m_sNormalTableName;
        }

        public string GetNormalTableGetFunc()
        {
            return m_sNormalTableGetFunc;
        }

        public string GetNormalTableOutputType()
        {
            return m_sNormalTableOutputType;
        }

        public List<TABLE_STRUCT_DATA_TYPE> GetNormalTableElementOrder()
        {
            return m_NormalTableElementOrder;
        }

        public string GetNormalTableSizeFuncName()
        {
            return m_sNormalTableGetSizeFuncName;
        }

        public string GetNormalTableSizeFuncType()
        {
            return m_sNormalTableGetSizeType;
        }

        // get special table properties
        public string GetSpecialTableType()
        {
            return m_sSpecialTableType;
        }

        public string GetSpecialTableName()
        {
            return m_sSpecialTableName;
        }

        public string GetSpecialTableGetFunc()
        {
            return m_sSpecialTableGetFunc;
        }

        public string GetCmVariableName()
        {
            return m_sCmVariableName;
        }

        public string GetSpecialTableOutputType()
        {
            return m_sSpecialTableOutputType;
        }

        public List<TABLE_STRUCT_DATA_TYPE> GetSpecialTableElementOrder()
        {
            return m_SpecialTableElementOrder;
        }

        // get normal delta table properties
        public string GetNormalDeltaTableType()
        {
            return m_sNormalDeltaTableType;
        }

        public string GetNormalDeltaTableName()
        {
            return m_sNormalDeltaTableName;
        }

        public string GetNormalDeltaTableOutputType()
        {
            return m_sNormalDeltaTableOutputType;
        }

        public List<TABLE_STRUCT_DATA_TYPE> GetNormalDeltaTableElementOrder()
        {
            return m_NormalDeltaTableElementOrder;
        }

        // get special delta table properties
        public string GetSpecialDeltaTableType()
        {
            return m_sSpecialDeltaTableType;
        }

        public string GetSpecialDeltaTableName()
        {
            return m_sSpecialDeltaTableName;
        }

        public string GetSpecialDeltaTableOutputType()
        {
            return m_sSpecialDeltaTableOutputType;
        }

        public List<TABLE_STRUCT_DATA_TYPE> GetSpecialDeltaTableElementOrder()
        {
            return m_SpecialDeltaTableElementOrder;
        }

        // get code structure type
        public string GetCodeStructureType()
        {
            return m_sStructureType;
        }

        // get interface
        public List<INTERFACE_CLASS> GetInterfaceClass()
        {
            return m_InterfaceClass;
        }

        // get exception enum items only
        public List<String> GetExceptionEnumItem()
        {
            return m_lsExceptionEnumItem;
        }

        // get exception enum and struct items
        public List<EXCEPTION_CI_ITEM> GetExceptionEnumStructItem()
        {
            return m_lsExceptionEnumStructItem;
        }

        public List<DELTA_TABLE_GROUP_NAME> GetDeltaGroupName()
        {
            return m_lsDeltaGroupName;
        }

        // get Decleare Namespace
        public string GetDecleareNamespace()
        {
            return m_sDecleareNamespace;
        }

        // get Using Namespace
        public string GetUsingNamespace()
        {
            return m_sUsingNamespace;
        }
    }
}
