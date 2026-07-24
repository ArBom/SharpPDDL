using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class TypesPDDLVisualization : DGML
    {
        protected readonly TreeNode<SingleTypeOfDomein> allTypesNodes;
        protected readonly char[] ParNr = { GloCla.PointerVName[0], '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩', '⑪', '⑫', '⑬', '⑭', '⑮', '⑯', '⑰', '⑱', '⑲', '⑳'};

        internal TypesPDDLVisualization(TreeNode<SingleTypeOfDomein> allTypesNodes)
        {
            this.allTypesNodes = allTypesNodes;
        }

        protected override string MakeFilePath(string prefix)
        {
            string ToRet = String.Format(ResVis.GetString("TypFileName"), prefix, correctExtension);
            return ToRet;
        }

        protected override void CreateData() { }

        protected override string GraphTitle()
        {
            return ResVis.GetString("TypGraphTitle");
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

            Dictionary<string, string> AttributesObjTypeNode = new Dictionary<string, string>
            {
                [Id_Key] = "ObjTypeNode",
                ["BasedOn"] = "TypeNode",
                ["Icon"] = ""
            };
            AddRecord(Category_Key, AttributesObjTypeNode);

            Dictionary<string, string> AttributesValueNode = new Dictionary<string, string>
            {
                [Id_Key] = "ValueNode",
                ["Icon"] = "CodeSchema_Field"
            };
            AddRecord(Category_Key, AttributesValueNode);

            Dictionary<string, string> AttributesContainsLink = new Dictionary<string, string>
            {
                [Id_Key] = "Contains",
                ["CanBeDataDriven"] = Boolean.FalseString,
                ["CanLinkedNodesBeDataDriven"] = Boolean.TrueString,
                ["IsContainment"] = Boolean.TrueString
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
                    InsideLinkAttributes[Source_Key] = TypesNode.Content.Type.ToString() + "!" + t.ValueOfIndexesKey;
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

            foreach (Value mem in TypesNode.Content.CumulativeValues)
            {
                Dictionary<string, string> ContExAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "Contains",
                    [Source_Key] = TypesNode.Content.Type.ToString(),
                    [Target_Key] = TypesNode.Content.Type.ToString() + "!" + mem.ValueOfIndexesKey,
                    ["FetchingParent"] = TypesNode.Content.Type.ToString()
                };
                AddRecord(Link_Key, ContExAttributes);
            }
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
                [Category_Key] = TypeNode.Content.Type.FullName == "System.Object" ? "ObjTypeNode" : "TypeNode",
                [Id_Key] = TypeNode.Content.Type.ToString(),
                [Label_Key] = TypeNode.Content.Type.Name,
            };

            if (TypeNode.Content.CumulativeValues.Any())
            {
                NodeAttributes.Add("Group", "Expanded");

                foreach (Value mem in TypeNode.Content.CumulativeValues)
                {
                    Dictionary<string, string> ValueNodeAttributes = new Dictionary<string, string>
                    {
                        [Category_Key] = "ValueNode",
                        [Id_Key] = TypeNode.Content.Type.ToString() + "!" + mem.ValueOfIndexesKey,
                        [Label_Key] = mem.Name != GloCla.PointerVName ? mem.Name : ResVis.GetString("TypPointer"),
                        ["VNr"] = mem.ValueOfIndexesKey <= 20 ? ParNr[mem.ValueOfIndexesKey].ToString() : "(" + mem.ValueOfIndexesKey + ")",
                        ["VType"] = mem.Type.ToString(),
                        ["VPreIn"] = mem.IsInUse_PreconditionIn.ToString(),
                        ["VEffIn"] = mem.IsInUse_EffectIn.ToString(),
                        ["VEffOut"] = mem.IsInUse_EffectOut.ToString(),
                        ["VCostIn"] = mem.IsInUse_ActionCostIn.ToString()
                    };
                    AddRecord(Node_Key, ValueNodeAttributes);
                }
            }

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

        internal override void AddProperties()
        {
            Dictionary<string, string> AttributesVNr = new Dictionary<string, string>
            {
                [Id_Key] = "VNr",
                [Label_Key] = ResVis.GetString("TypVNr"),
                [DataType_Key] = "System.String"
            };
            AddRecord(Property_Key, AttributesVNr);

            Dictionary<string, string> AttributesVPreIn = new Dictionary<string, string>
            {
                [Id_Key] = "VPreIn",
                [Label_Key] = ResVis.GetString("TypVPreInLabelKey"),
                [DataType_Key] = "System.Boolean"
            };
            AddRecord(Property_Key, AttributesVPreIn);

            Dictionary<string, string> AttributesVEffIn = new Dictionary<string, string>
            {
                [Id_Key] = "VEffIn",
                [Label_Key] = ResVis.GetString("TypVEffInLabelKey"),
                [DataType_Key] = "System.Boolean"
            };
            AddRecord(Property_Key, AttributesVEffIn);

            Dictionary<string, string> AttributesVEffOut = new Dictionary<string, string>
            {
                [Id_Key] = "VEffOut",
                [Label_Key] = ResVis.GetString("TypVEffOutLabelKey"),
                [DataType_Key] = "System.Boolean"
            };
            AddRecord(Property_Key, AttributesVEffOut);

            Dictionary<string, string> AttributesVCostIn = new Dictionary<string, string>
            {
                [Id_Key] = "VCostIn",
                [Label_Key] = ResVis.GetString("TypVCostInLabelKey"),
                [DataType_Key] = "System.Boolean"
            };
            AddRecord(Property_Key, AttributesVCostIn);

            Dictionary<string, string> AttributesVType = new Dictionary<string, string>
            {
                [Id_Key] = "VType",
                [Label_Key] = ResVis.GetString("TypVTypeLabelKey"),
                [DataType_Key] = "System.String"
            };
            AddRecord(Property_Key, AttributesVType);

            Dictionary<string, string> ContainmentProperty = new Dictionary<string, string>
            {
                [Id_Key] = "IsContainment",
                [DataType_Key] = "System.Boolean"
            };
            AddRecord(Property_Key, ContainmentProperty);

            Dictionary<string, string> FetchingParentProperty = new Dictionary<string, string>
            {
                [Id_Key] = "FetchingParent",
                [DataType_Key] = "Microsoft.VisualStudio.GraphModel.GraphNodeId"
            };
            AddRecord(Property_Key, FetchingParentProperty);
        }

        internal override void AddStyles() { }

        protected override string GraphLayout() => ResVis.GetString("TypGraphLayout");

        protected override string GraphDirection() => ResVis.GetString("TypGraphDirection");
    }
}
