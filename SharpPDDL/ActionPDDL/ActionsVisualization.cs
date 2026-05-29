using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPDDL
{
    class ActionsVisualization : DGML
    {
        readonly string DomainName;
        readonly IReadOnlyList<ActionPDDL> actions;
        List<string> Effects_Strings;
        List<string> Executions_Strings;

        readonly string AppName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

        internal ActionsVisualization(string DomainName, IReadOnlyList<ActionPDDL> actions)
        {
            this.DomainName = DomainName;
            this.actions = actions;
        }

        protected override string MakeFilePath(string prefix)
        {
            return String.Concat(prefix, " (Case Use Diagram)", correctExtension);
        }

        protected override void CreateData()
        {
            Effects_Strings = new List<string>();
            Executions_Strings = new List<string>();

            foreach (ActionPDDL actionPDDL in actions)
            {
                var strings = actionPDDL.DataToCaseUseDiagram();

                foreach (string effectPDDL in strings.Item2)
                    Effects_Strings.Add(effectPDDL);

                foreach (string Execution in strings.Item4)
                    Executions_Strings.Add(Execution);
            }

            Effects_Strings = Effects_Strings.Distinct().ToList();
            Executions_Strings = Executions_Strings.Distinct().ToList();
        }

        protected override string GraphTitle()
        {
            return "Case Use Diagram";
        }

        internal override void AddCategories()
        {
            Dictionary<string, string> AttributesPossState = new Dictionary<string, string>
            {
                [Id_Key] = "Action",
                [Stroke_Key] = Action_Colour,
                [Background_Key] = Action_Colour,
                ["NodeRadius"] = "60"
            };
            AddRecord(Category_Key, AttributesPossState);

            Dictionary<string, string> AttributesEffect = new Dictionary<string, string>
            {
                [Id_Key] = "Effect",
                ["BasedOn"] = "Action"
            };
            AddRecord(Category_Key, AttributesEffect);

            Dictionary<string, string> AttributesTypeNode = new Dictionary<string, string>
            {
                [Id_Key] = "Lib",
                [Background_Key] = Class_Colour,
                [Stroke_Key] = Class_Colour,
            };
            AddRecord(Category_Key, AttributesTypeNode);

            Dictionary<string, string> AttributesContainsLink = new Dictionary<string, string>
            {
                [Id_Key] = "Contains",
                ["CanBeDataDriven"] = Boolean.FalseString,
                ["CanLinkedNodesBeDataDriven"] = Boolean.TrueString,
                ["IsContainment"] = Boolean.TrueString
            };
            AddRecord(Category_Key, AttributesTypeNode);

            Dictionary<string, string> AttributesPrecLink = new Dictionary<string, string>
            {
                [Id_Key] = "PrecoLink",
                ["CanBeDataDriven"] = Boolean.FalseString
            };
            AddRecord(Category_Key, AttributesPrecLink);

            Dictionary<string, string> AttributesExec = new Dictionary<string, string>
            {
                [Id_Key] = "Execution",
                [Stroke_Key] = Realization_Colour,
                ["StrokeDashArray"] = "4 4",
                [Background_Key] = "#00000000",
                ["NodeRadius"] = "60"
            };
            AddRecord(Category_Key, AttributesExec);
        }

        internal override void AddLinkes()
        {
            foreach (ActionPDDL actionPDDL in actions)
            {
                Dictionary<string, string> PrecoAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "PrecoLink",
                    [Source_Key] = "App",
                    [Target_Key] = actionPDDL.Name + "!Preco"
                };
                AddRecord(Link_Key, PrecoAttributes);

                Dictionary<string, string> MActAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "PrecoLink",
                    [Source_Key] = actionPDDL.Name + "!Preco",
                    [Target_Key] = actionPDDL.Name
                };
                AddRecord(Link_Key, MActAttributes);

                Dictionary<string, string> ContAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "Contains",
                    [Source_Key] = "SharpPDDL",
                    [Target_Key] = actionPDDL.Name,
                    ["FetchingParent"] = "SharpPDDL"
                };
                AddRecord(Link_Key, ContAttributes);

                foreach (string effectPDDLName in actionPDDL.DataToCaseUseDiagram().Item2)
                {
                    Dictionary<string, string> EffeLinkAttributes = new Dictionary<string, string>
                    {
                        [Category_Key] = "EffeLink",
                        [Source_Key] = actionPDDL.Name,
                        [Target_Key] = effectPDDLName + "!E",
                        ["StrokeDashArray"] = "4 4",
                        [Label_Key] = "«include»"
                    };

                    if (actionPDDL.DataToCaseUseDiagram().Item3.Contains(effectPDDLName))
                        EffeLinkAttributes["IsExec"] = Boolean.TrueString;
                    else
                        EffeLinkAttributes["IsExec"] = Boolean.FalseString;

                    AddRecord(Link_Key, EffeLinkAttributes);

                    Dictionary<string, string> ContEAttributes = new Dictionary<string, string>
                    {
                        [Category_Key] = "Contains",
                        [Source_Key] = "SharpPDDL",
                        [Target_Key] = effectPDDLName + "!E",
                        ["FetchingParent"] = "SharpPDDL"
                    };
                    AddRecord(Link_Key, ContEAttributes);
                }

                foreach (string executionName in actionPDDL.DataToCaseUseDiagram().Item4)
                {
                    Dictionary<string, string> executionLinkAttributes = new Dictionary<string, string>
                    {
                        [Category_Key] = "EffeLink",
                        [Source_Key] = actionPDDL.Name,
                        [Target_Key] = executionName + "!Ex",
                        ["StrokeDashArray"] = "4 4",
                        [Label_Key] = "«realization»",
                        ["IsExec"] = Boolean.TrueString
                    };
                    AddRecord(Link_Key, executionLinkAttributes);

                    Dictionary<string, string> ContExAttributes = new Dictionary<string, string>
                    {
                        [Category_Key] = "Contains",
                        [Source_Key] = "SharpPDDL",
                        [Target_Key] = executionName + "!Ex",
                        ["FetchingParent"] = "SharpPDDL"
                    };
                    AddRecord(Link_Key, ContExAttributes);
                }
            }
        }

        internal override void AddNodes()
        {
            Dictionary<string, string> AppAttributes = new Dictionary<string, string>
            {
                [Category_Key] = "Actor",
                [Id_Key] = "App",
                ["Shape"] = "None",
                [Label_Key] = "  ⚪\n ╭╈╮\n ╯┃╰\n ╱ ╲\n ҄   ҄\n" + System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                ["FontFamily"] = "Consolas",
                ["FontWeight"] = "Heavy"
            };
            AddRecord(Node_Key, AppAttributes);

            Dictionary<string, string> SharpPDDLAttributes = new Dictionary<string, string>
            {
                [Category_Key] = "Lib",
                [Id_Key] = "SharpPDDL",
                [Label_Key] = "SharpPDDL - " + DomainName,
                ["Group"] = "Expanded"
            };
            AddRecord(Node_Key, SharpPDDLAttributes);

            foreach (ActionPDDL actionPDDL in actions)
            {
                string PrecoList = "♦ " + String.Join("\n♦ ", actionPDDL.DataToCaseUseDiagram().Item1);

                Dictionary<string, string> ActionPrecoAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "ActionPreco",
                    [Id_Key] = actionPDDL.Name + "!Preco",
                    [Label_Key] = PrecoList,
                    ["NodeRadius"] = "0",
                    [Background_Key] = State_Colour
                };
                AddRecord(Node_Key, ActionPrecoAttributes);

                Dictionary<string, string> ActionAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "Action",
                    [Id_Key] = actionPDDL.Name,
                    [Label_Key] = actionPDDL.Name
                };
                AddRecord(Node_Key, ActionAttributes);
            }

            foreach (string effect in Effects_Strings)
            {
                Dictionary<string, string> EffectAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "Effect",
                    [Id_Key] = effect + "!E",
                    [Label_Key] = effect,
                    ["BasedOn"] = "Action"
                };
                AddRecord(Node_Key, EffectAttributes);
            }

            foreach (string execution in Executions_Strings)
            {
                Dictionary<string, string> ExecutionsAttributes = new Dictionary<string, string>
                {
                    [Category_Key] = "Execution",
                    [Id_Key] = execution + "!Ex",
                    [Label_Key] = execution,
                };
                AddRecord(Node_Key, ExecutionsAttributes);
            }
        }

        internal override void AddProperties()
        {
            Dictionary<string, string> ExecutionProperty = new Dictionary<string, string>
            {
                [Id_Key] = "IsExec",
                [DataType_Key] = "System.Boolean"
            };
            AddRecord(Property_Key, ExecutionProperty);

            Dictionary<string, string> ShapeProperty = new Dictionary<string, string>
            {
                [Id_Key] = "Shape",
                [DataType_Key] = "System.String"
            };
            AddRecord(Property_Key, ShapeProperty);

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

        protected override string GraphLayout()
        {
            return "LeftToRight";
        }

        internal override void AddStyles()
        {
            AddTFStyle(Link_Key, "IsExec", true, Stroke_Key, Realization_Colour);
        }
    }
}
