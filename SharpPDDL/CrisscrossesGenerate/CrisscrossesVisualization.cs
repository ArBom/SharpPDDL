using System.Collections.Generic;
using System.Linq;
using System;

namespace SharpPDDL
{
    internal class CrisscrossesVisualization : DGML
    {
        readonly DomainPDDL Owner;
        readonly Crisscross root;
        List<string> FoundedGoalCrisscrosses;
        Dictionary<string, Crisscross> states;
        List<CrisscrossChildrenCon> CurrentRealization;

        internal CrisscrossesVisualization(DomainPDDL Owner, Crisscross root, List<string> FoundedGoalCrisscrosses, List<CrisscrossChildrenCon> CurrentRealization)
        {
            this.Owner = Owner;
            this.root = root;
            this.FoundedGoalCrisscrosses = FoundedGoalCrisscrosses;
            this.CurrentRealization = CurrentRealization;
        }
        
        protected override string MakeFilePath(string prefix)
        {
            string ToRet = String.Format(ResVis.GetString("CriFileName"), prefix, correctExtension);
            return ToRet;
        }

        protected override void CreateData()
        {
            states = new Dictionary<string, Crisscross>();

            foreach (var t in Owner.DomainPlanner.CurrentBuilder.crisscrossReducer._IndexedStates)
                states[t.Key] = t.Value;
        }

        protected override string GraphLayout() => ResVis.GetString("CriGraphLayout");

        protected override string GraphTitle()
        {
            return ResVis.GetString("CriGraphTitle");
        }

        internal override void AddCategories()
        {
            Dictionary<string, string> AttributesComment = new Dictionary<string, string>
            {
                [Id_Key] = "Comment",
                [Background_Key] = "LightGoldenrodYellow"
            };
            AddRecord(Category_Key, AttributesComment);

            Dictionary<string, string> AttributesPossState = new Dictionary<string, string>
            {
                [Id_Key] = "PossState",
                [Stroke_Key] = State_Colour,
                [Background_Key] = State_Colour
            };
            AddRecord(Category_Key, AttributesPossState);

            Dictionary<string, string> AttributesInitState = new Dictionary<string, string>
            {
                [Id_Key] = "InitState",
                ["BasedOn"] = "PossState",
            };
            AddRecord(Category_Key, AttributesInitState);

            Dictionary<string, string> AttributesFinalState = new Dictionary<string, string>
            {
                [Id_Key] = "FinalState",
                ["BasedOn"] = "PossState",
            };
            AddRecord(Category_Key, AttributesFinalState);

            Dictionary<string, string> AttributesConnector = new Dictionary<string, string>
            {
                [Id_Key] = "Connector",
                [Stroke_Key] = Action_Colour
            };
            AddRecord(Category_Key, AttributesConnector);
        }

        internal override void AddLinkes()
        {
            Dictionary<string, string> Attributes = new Dictionary<string, string>
            {
                [Category_Key] = "Connector"
            };

            foreach (var curr in states)
            {
                foreach (var l in curr.Value.Children)
                {
                    if (CurrentRealization.Any(CR => CR.Equals(l)))
                        Attributes["IsInRealize"] = Boolean.TrueString;
                    else
                        Attributes["IsInRealize"] = Boolean.FalseString;

                    Attributes[Source_Key] = curr.Key;
                    Attributes[Target_Key] = l.Child.Content.CheckSum;
                    Attributes[Label_Key] = l.ActionNr.ToString();
                    Attributes["ActionName"] = Owner.actions[l.ActionNr].Name;
                    Attributes["ActionCost"] = l.ActionCost.ToString();

                    string Sententia = (string)Owner.actions[l.ActionNr].InstantActionSententia?.DynamicInvoke(l.ActionArgThOb);
                    Attributes["Sententia"] = Sententia;

                    AddRecord(Link_Key, Attributes);
                }
            }
        }

        internal override void AddNodes()
        {
            string ActionLegendLabel = ResVis.GetString("CriActListHead") + "\n";
            for (int i = 0; i != Owner.actions.Count; i++)
            {
                ActionLegendLabel = ActionLegendLabel + "\n" + String.Format(ResVis.GetString("CriActListRecord"), i, Owner.actions[i].Name);
            }

            Dictionary<string, string> ActionLegendAttributes = new Dictionary<string, string>
            {
                [Id_Key] = "ActionLegend",
                [Category_Key] = "Comment",
                [Label_Key] = ActionLegendLabel
            };
            AddRecord(Node_Key, ActionLegendAttributes);

            Dictionary<string, string> Attributes = new Dictionary<string, string>();

            foreach (var curr in states)
            {
                if (CurrentRealization.Any(CR => CR.Child.Content.CheckSum == curr.Key))
                    Attributes["IsInRealize"] = Boolean.TrueString;
                else
                    Attributes["IsInRealize"] = Boolean.FalseString;

                Attributes[Label_Key] = curr.Key;

                if (curr.Key == root.Content.CheckSum)
                {
                    Attributes[Label_Key] = String.Format(ResVis.GetString("CriStateStart"), Attributes[Label_Key]);
                    Attributes["IsInRealize"] = Boolean.TrueString;
                }

                if (FoundedGoalCrisscrosses.Contains(curr.Key))
                    Attributes[Label_Key] = String.Format(ResVis.GetString("CriStateFinish"), Attributes[Label_Key]);

                Attributes[Category_Key] = "PossState";

                Attributes[Id_Key] = curr.Key;
                Attributes["CumulativeCost"] = curr.Value.CumulativedTransitionCharge.ToString();

                AddRecord(Node_Key, Attributes);
            }
        }

        internal override void AddProperties()
        {
            Dictionary<string, string> AttributesIsInRealize = new Dictionary<string, string>
            {
                [Id_Key] = "IsInRealize",
                [Label_Key] = ResVis.GetString("CriInRealizeLabelKey"),
                [DataType_Key] = "System.Boolean"
            };
            AddRecord(Property_Key, AttributesIsInRealize);

            Dictionary<string, string> AttributesCumulativeCost = new Dictionary<string, string>
            {
                [Id_Key] = "CumulativeCost",
                [Label_Key] = ResVis.GetString("CriCumCostLabelKey"),
                [DataType_Key] = "System.UInt32"
            };
            AddRecord(Property_Key, AttributesCumulativeCost);

            Dictionary<string, string> AttributesActionName = new Dictionary<string, string>
            {
                [Id_Key] = "ActionName",
                [Label_Key] = ResVis.GetString("CriActNaLabelKey"),
                [DataType_Key] = "System.String"
            };
            AddRecord(Property_Key, AttributesActionName);

            Dictionary<string, string> AttributesActionCost = new Dictionary<string, string>
            {
                [Id_Key] = "ActionCost",
                [Label_Key] = ResVis.GetString("CriCostLabelKey"),
                [DataType_Key] = "System.UInt32"
            };
            AddRecord(Property_Key, AttributesActionCost);

            Dictionary<string, string> AttributesSententia = new Dictionary<string, string>
            {
                [Id_Key] = "Sententia",
                [Label_Key] = ResVis.GetString("CriActSenLabelKey"),
                [DataType_Key] = "System.String"
            };
            AddRecord(Property_Key, AttributesSententia);
        }

        internal override void AddStyles()
        {
            AddTFStyle(Link_Key, "IsInRealize", true, Stroke_Key, Realization_Colour);
            AddTFStyle(Node_Key, "IsInRealize", true, Stroke_Key, Realization_Colour);
            AddTFStyle(Link_Key, "IsInRealize", false, Stroke_Key, Action_Colour);
        }
    }
}
