using System.Collections.Generic;
using System.Xml;

namespace ConfigGenerator
{
    public struct REF_NODE_TYPE
    {
        public List<string> RefPathNameList;
        public XmlNode      Node;
    }

    public interface IXMLFileProcess
    {
        bool                XMLCreateFile(string sFilePath, XmlNode ReportContentNode);
        bool                XMLLoadingFile(string sFilePath, bool bReadOnly = true);

        XmlNode             XMLGetRootNode();
        XmlNode             XMLCreateNode(string NodeName, List<string[]> sAttribute = null);
        XmlNode             XMLImportNode(XmlNode Node);
        List<REF_NODE_TYPE> XMLSearchNodes(string NodeName, REF_NODE_TYPE ParentRefNode, List<string[]> sAttribute = null);

        bool                XMLAddNode(XmlNode NodeIn, REF_NODE_TYPE ParentRefNode, bool bFlagAtFirst = false);
        bool                XMLInsertNode(XmlNode NodeIn, REF_NODE_TYPE ParentRefNode, REF_NODE_TYPE SiblingRefNode, bool bFlagAfter = true);
        bool                XMLReplaceNode(XmlNode NewNode, XmlNode OldNode);
        bool                XMLRemoveNode(XmlNode OldNode);
        bool                XMLChangeAttributes(XmlNode TargetNode, List<string[]> sAttributes);
    }
}
