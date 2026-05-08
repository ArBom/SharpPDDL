using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPDDL
{
    class ActionsVisualization : DGML
    {
        readonly string DomainName;
        readonly IReadOnlyList<ActionPDDL> actions;
        List<string> Effects;
        List<string> Executions;

        readonly string AppName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

        internal ActionsVisualization(string DomainName, IReadOnlyList<ActionPDDL> actions)
        {
            this.DomainName = DomainName;
            this.actions = actions;
        }

        protected override void CreateData()
        {
            Effects = new List<string>();
            Executions = new List<string>();

            foreach (ActionPDDL actionPDDL in actions)
            {
                foreach (EffectPDDL effectPDDL in actionPDDL.Effects)
                    Effects.Add(effectPDDL.Name);

                foreach (ExpressionExecution expressionExecution in actionPDDL.Executions)
                    Executions.Add(expressionExecution.Name);
            }

            Effects = Effects.Distinct().ToList();
            Executions = Executions.Distinct().ToList();
        }

        protected override string GraphTitle()
        {
            return "Case Use Diagram";
        }

        internal override void AddCategories()
        {
            Dictionary<string, string> AttributesPossState = new Dictionary<string, string>
            {
                ["Id"] = "Action",
                ["Stroke"] = "#FF8E0C10",
                ["Background"] = "#FF8E0C10",
                ["NodeRadius"] = "60"
            };
            AddRecord(CategoryName, AttributesPossState);

            Dictionary<string, string> AttributesEffect = new Dictionary<string, string>
            {
                ["Id"] = "Effect",
                ["BasedOn"] = "Action"
            };
            AddRecord(CategoryName, AttributesEffect);

            Dictionary<string, string> AttributesTypeNode = new Dictionary<string, string>
            {
                ["Id"] = "Lib",
                ["Background"] = "#FF0E70C0",
                ["Stroke"] = "#FF0E70C0",
                //["Icon"] = "CodeSchema_Class"
            };
            AddRecord(CategoryName, AttributesTypeNode);

            Dictionary<string, string> AttributesContainsLink = new Dictionary<string, string>
            {
                ["Id"] = "Contains",
                ["CanBeDataDriven"] = "False",
                ["CanLinkedNodesBeDataDriven"] = "True",
                ["IsContainment"] = "True"
            };
            AddRecord(CategoryName, AttributesTypeNode);

            Dictionary<string, string> AttributesPrecLink = new Dictionary<string, string>
            {
                ["Id"] = "PrecoLink",
                ["CanBeDataDriven"] = "False"
            };
            AddRecord(CategoryName, AttributesPrecLink);

            Dictionary<string, string> AttributesExec = new Dictionary<string, string>
            {
                ["Id"] = "Execution",
                ["Stroke"] = "#FF8E0C10",
                ["StrokeDashArray"] = "4 4",
                ["Background"] = "#00000000",
                ["NodeRadius"] = "60"
            };
            AddRecord(CategoryName, AttributesExec);
        }

        internal override void AddLinkes()
        {
            foreach (ActionPDDL actionPDDL in actions)
            {
                Dictionary<string, string> PrecoAttributes = new Dictionary<string, string>
                {
                    ["Category"] = "PrecoLink",
                    ["Source"] = "App",
                    ["Target"] = actionPDDL.Name + "!Preco"
                };
                AddRecord(LinkName, PrecoAttributes);

                Dictionary<string, string> MActAttributes = new Dictionary<string, string>
                {
                    ["Category"] = "PrecoLink",
                    ["Source"] = actionPDDL.Name + "!Preco",
                    ["Target"] = actionPDDL.Name
                };
                AddRecord(LinkName, MActAttributes);

                Dictionary<string, string> ContAttributes = new Dictionary<string, string>
                {
                    ["Category"] = "Contains",
                    ["Source"] = "SharpPDDL",
                    ["Target"] = actionPDDL.Name,
                    ["FetchingParent"] = "SharpPDDL"
                };
                AddRecord(LinkName, ContAttributes);

                foreach (EffectPDDL effectPDDL in actionPDDL.Effects)
                {
                    Dictionary<string, string> EffeLinkAttributes = new Dictionary<string, string>
                    {
                        ["Category"] = "EffeLink",
                        ["Source"] = actionPDDL.Name,
                        ["Target"] = effectPDDL.Name + "!E",
                        ["StrokeDashArray"] = "4 4",
                        ["Label"] = "«include»"
                    };

                    if (actionPDDL.EffectsUsedAlsoAsExecution.Contains(effectPDDL.Name))
                        EffeLinkAttributes["IsExec"] = "True";
                    else
                        EffeLinkAttributes["IsExec"] = "False";

                    AddRecord(LinkName, EffeLinkAttributes);

                    Dictionary<string, string> ContEAttributes = new Dictionary<string, string>
                    {
                        ["Category"] = "Contains",
                        ["Source"] = "SharpPDDL",
                        ["Target"] = effectPDDL.Name + "!E",
                        ["FetchingParent"] = "SharpPDDL"
                    };
                    AddRecord(LinkName, ContEAttributes);
                }

                foreach (ExpressionExecution execution in actionPDDL.Executions)
                {
                    Dictionary<string, string> executionLinkAttributes = new Dictionary<string, string>
                    {
                        ["Category"] = "EffeLink",
                        ["Source"] = actionPDDL.Name,
                        ["Target"] = execution.Name + "!Ex",
                        ["StrokeDashArray"] = "4 4",
                        ["Label"] = "«realization»",
                        ["IsExec"] = "True"
                    };
                    AddRecord(LinkName, executionLinkAttributes);

                    Dictionary<string, string> ContExAttributes = new Dictionary<string, string>
                    {
                        ["Category"] = "Contains",
                        ["Source"] = "SharpPDDL",
                        ["Target"] = execution.Name + "!Ex",
                        ["FetchingParent"] = "SharpPDDL"
                    };
                    AddRecord(LinkName, ContExAttributes);
                }
            }
        }

        internal override void AddNodes()
        {
            Dictionary<string, string> AppAttributes = new Dictionary<string, string>
            {
                ["Category"] = "Actor",
                ["Id"] = "App",
                ["Shape"] = "None",
                ["Label"] = "웃\n\n" + System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                //["VerticalAlignment"] = "Top",
                ["Icon"] = @"C:\ProgramData\Microsoft\User Account Pictures\user-48.png"
            };
            AddRecord(NodeName, AppAttributes);

            Dictionary<string, string> SharpPDDLAttributes = new Dictionary<string, string>
            {
                ["Category"] = "Lib",
                ["Id"] = "SharpPDDL",
                ["Label"] = "SharpPDDL",
                ["Group"] = "Expanded"
            };
            AddRecord(NodeName, SharpPDDLAttributes);

            foreach (ActionPDDL actionPDDL in actions)
            {
                IEnumerable<string> Precos = actionPDDL.Preconditions.Select(p => p.Name);
                string PrecoList = "♦ " + String.Join("\n♦ ", Precos);

                Dictionary<string, string> ActionPrecoAttributes = new Dictionary<string, string>
                {
                    ["Category"] = "ActionPreco",
                    ["Id"] = actionPDDL.Name + "!Preco",
                    ["Label"] = PrecoList,
                    //["StrokeThickness"] = "4",
                    ["Background"] = "#FF770056"
                };
                AddRecord(NodeName, ActionPrecoAttributes);

                Dictionary<string, string> ActionAttributes = new Dictionary<string, string>
                {
                    ["Category"] = "Action",
                    ["Id"] = actionPDDL.Name,
                    ["Label"] = actionPDDL.Name
                };
                AddRecord(NodeName, ActionAttributes);
            }

            foreach (string effect in Effects)
            {
                Dictionary<string, string> EffectAttributes = new Dictionary<string, string>
                {
                    ["Category"] = "Effect",
                    ["Id"] = effect + "!E",
                    ["Label"] = effect,
                    ["BasedOn"] = "Action"
                };
                AddRecord(NodeName, EffectAttributes);
            }

            foreach (string execution in Executions)
            {
                Dictionary<string, string> ExecutionsAttributes = new Dictionary<string, string>
                {
                    ["Category"] = "Execution",
                    ["Id"] = execution + "!Ex",
                    ["Label"] = execution,
                };
                AddRecord(NodeName, ExecutionsAttributes);
            }
        }

        internal override void AddProperties()
        {
            Dictionary<string, string> ExecutionProperty = new Dictionary<string, string>
            {
                ["Id"] = "IsExec",
                ["DataType"] = "System.Boolean"
            };
            AddRecord(PropertyName, ExecutionProperty);

            Dictionary<string, string> ShapeProperty = new Dictionary<string, string>
            {
                ["Id"] = "Shape",
                ["DataType"] = "System.String"
            };
            AddRecord(PropertyName, ShapeProperty);

            Dictionary<string, string> ContainmentProperty = new Dictionary<string, string>
            {
                ["Id"] = "IsContainment",
                ["DataType"] = "System.Boolean"
            };
            AddRecord(PropertyName, ContainmentProperty);

            Dictionary<string, string> FetchingParentProperty = new Dictionary<string, string>
            {
                ["Id"] = "FetchingParent",
                ["DataType"] = "Microsoft.VisualStudio.GraphModel.GraphNodeId"
            };
            AddRecord(PropertyName, FetchingParentProperty);
        }

        protected override string GraphLayout()
        {
            return "Sugiyama";
        }

        internal override void AddStyles()
        {
            writer.WriteStartElement("Style");
            writer.WriteAttributeString("TargetType", "Link");
            writer.WriteAttributeString("GroupLabel", "IsExec");
            writer.WriteAttributeString("ValueLabel", "True");

            AddRecord("Condition", new Dictionary<string, string> { ["Expression"] = "IsExec = 'True'" });
            AddRecord("Setter", new Dictionary<string, string> { ["Property"] = "Stroke", ["Value"] = "#FFFF7600" });
            writer.WriteEndElement();
        }
    }
}
