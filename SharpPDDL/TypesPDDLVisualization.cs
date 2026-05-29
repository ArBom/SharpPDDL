using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class TypesPDDLVisualization : DGML
    {
        TreeNode<SingleTypeOfDomein> allTypesNodes;

        internal TypesPDDLVisualization(TreeNode<SingleTypeOfDomein> allTypesNodes)
        {
            this.allTypesNodes = allTypesNodes;
        }

        protected override string MakeFilePath(string prefix)
        {
            return String.Concat(prefix, " (Class Diagram)", correctExtension);
        }

        protected override void CreateData() { }

        protected override string GraphTitle()
        {
            return "Types' graph";
        }

        internal override void AddCategories()
        {
            Dictionary<string, string> AttributesTypeConnect = new Dictionary<string, string>
            {
                [Id_Key] = "TypeConnect",
                [Stroke_Key] = "#FF00A600",
                ["StrokeDashArray"] = "2 0",
            };
            AddRecord(Category_Key, AttributesTypeConnect);

            Dictionary<string, string> AttributesInsideTypeConnect = new Dictionary<string, string>
            {
                [Id_Key] = "InsideTypeConnect",
                [Stroke_Key] = "Grey",
                [Background_Key] = "Grey"

            };
            AddRecord(Category_Key, AttributesInsideTypeConnect);

            Dictionary<string, string> AttributesTypeNode = new Dictionary<string, string>
            {
                [Id_Key] = "TypeNode",
                [Background_Key] = Class_Colour,
                [Stroke_Key] = Class_Colour,
                ["Icon"] = "CodeSchema_Class"
            };
            AddRecord(Category_Key, AttributesTypeNode);
        }

        void AddLink(TreeNode<SingleTypeOfDomein> TypesNode)
        {
            Dictionary<string, string> InsideLinkAttributes = new Dictionary<string, string>
            {
                [Category_Key] = "InsideTypeConnect"
            };

            if (TypesNode.Content != null)
            {
                foreach (var t in TypesNode.Content.Values)
                {
                    if (t.Type.IsValueType)
                        continue;

                    InsideLinkAttributes[Target_Key] = t.Type.ToString();
                    InsideLinkAttributes[Source_Key] = TypesNode.Content.Type.ToString();
                    InsideLinkAttributes[Label_Key] = t.Name;

                    AddRecord(Link_Key, InsideLinkAttributes);
                }
            }

            if (TypesNode.Root?.Content == null || TypesNode.Root == null)
                return;

            Dictionary<string, string> LinkAttributes = new Dictionary<string, string>
            {
                [Category_Key] = "TypeConnect",
                [Target_Key] = TypesNode.Root.Content.Type.ToString(),
                [Source_Key] = TypesNode.Content.Type.ToString()
            };

            AddRecord(Link_Key, LinkAttributes);
        }

        void AddLinkes(TreeNode<SingleTypeOfDomein> TypesNodes)
        {
            AddLink(TypesNodes);

            foreach (TreeNode<SingleTypeOfDomein> child in TypesNodes.Children)
                AddLinkes(child);
        }

        internal override void AddLinkes() => AddLinkes(allTypesNodes);

        void AddNode(TreeNode<SingleTypeOfDomein> TypeNode)
        {
            Dictionary<string, string> NodeAttributes = new Dictionary<string, string>
            {
                [Category_Key] = "TypeNode",
                [Id_Key] = TypeNode.Content.Type.ToString(),
                [Label_Key] = TypeNode.Content.Type.Name
            };

            AddRecord(Node_Key, NodeAttributes);
        }

        void AddNodes(TreeNode<SingleTypeOfDomein> TypesNodes)
        {
            if (TypesNodes.Content != null)
                AddNode(TypesNodes);

            foreach (TreeNode<SingleTypeOfDomein> child in TypesNodes.Children)
                AddNodes(child);
        }

        internal override void AddNodes() => AddNodes(allTypesNodes);

        internal override void AddProperties() { }

        internal override void AddStyles() { }

        protected override string GraphLayout() => "BottomToTop";
    }
}
