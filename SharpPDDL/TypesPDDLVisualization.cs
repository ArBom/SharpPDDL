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

        protected override void CreateData() { }

        protected override string GraphTitle()
        {
            return "Types' graph";
        }

        internal override void AddCategories()
        {
            Dictionary<string, string> AttributesTypeConnect = new Dictionary<string, string>
            {
                ["Id"] = "TypeConnect",

                ["Stroke"] = "#FF00A600",
                ["StrokeDashArray"] = "2 0",
                ["DrawArrow"] = "true"
            };
            AddRecord(CategoryName, AttributesTypeConnect);

            Dictionary<string, string> AttributesInsideTypeConnect = new Dictionary<string, string>
            {
                ["Id"] = "InsideTypeConnect",
                //["StrokeDashArray"] = "2 2",
                //["TargetDecorator"] = "Arrow",
                //["BasedOn"] = "TypeConnect",
                ["Stroke"] = "Grey",
                ["Background"] = "Grey"

            };
            AddRecord(CategoryName, AttributesInsideTypeConnect);

            Dictionary<string, string> AttributesTypeNode = new Dictionary<string, string>
            {
                ["Id"] = "TypeNode",
                ["Background"] = "#FF0E70C0",
                ["Stroke"] = "#FF0E70C0",
                ["Icon"] = "CodeSchema_Class"
            };
            AddRecord(CategoryName, AttributesTypeNode);
        }

        void AddLink(TreeNode<SingleTypeOfDomein> TypesNode)
        {
            Dictionary<string, string> InsideLinkAttributes = new Dictionary<string, string>
            {
                ["Category"] = "InsideTypeConnect"
            };

            if (TypesNode.Content != null)
            {
                foreach (var t in TypesNode.Content.Values)
                {
                    if (t.Type.IsValueType)
                        continue;

                    InsideLinkAttributes["Target"] = t.Type.ToString();
                    InsideLinkAttributes["Source"] = TypesNode.Content.Type.ToString();
                    InsideLinkAttributes["Label"] = t.Name;

                    AddRecord(LinkName, InsideLinkAttributes);
                }
            }

            if (TypesNode.Root?.Content == null || TypesNode.Root == null)
                return;

            Dictionary<string, string> LinkAttributes = new Dictionary<string, string>
            {
                ["Category"] = "TypeConnect",
                ["Target"] = TypesNode.Root.Content.Type.ToString(),
                ["Source"] = TypesNode.Content.Type.ToString()
            };

            AddRecord(LinkName, LinkAttributes);
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
                ["Category"] = "TypeNode",
                ["Id"] = TypeNode.Content.Type.ToString(),
                ["Label"] = TypeNode.Content.Type.Name
            };

            AddRecord(NodeName, NodeAttributes);
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
