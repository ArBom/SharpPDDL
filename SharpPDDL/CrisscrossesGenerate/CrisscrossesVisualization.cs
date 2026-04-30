using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Linq;

namespace SharpPDDL
{
    internal class CrisscrossesVisualization : DGML
    {
        readonly DomainPDDL Owner;
        readonly Crisscross root;
        List<string> FoundedGoalCrisscrosses;
        Dictionary<string, Crisscross> states;

        internal CrisscrossesVisualization(DomainPDDL Owner, Crisscross root, List<string> FoundedGoalCrisscrosses)
        {
            this.Owner = Owner;
            this.root = root;
            this.FoundedGoalCrisscrosses = FoundedGoalCrisscrosses;
        }

        protected override void CreateData()
        {
            states = new Dictionary<string, Crisscross>();

            foreach (var t in Owner.DomainPlanner.CurrentBuilder.crisscrossReducer._IndexedStates)
                states[t.Key] = t.Value;
        }

        protected override string GraphTitle()
        {
            return "States of " + Owner.Name + " domain";
        }

        internal override void AddCategories()
        {
            Dictionary<string, string> AttributesComment = new Dictionary<string, string>
            {
                ["Id"] = "Comment",
                ["Background"] = "LightGoldenrodYellow"
            };
            AddRecord(CategoryName, AttributesComment);

            Dictionary<string, string> AttributesPossState = new Dictionary<string, string>
            {
                ["Id"] = "PossState",
                ["Background"] = "Purple"
            };
            AddRecord(CategoryName, AttributesPossState);

            Dictionary<string, string> AttributesInitState = new Dictionary<string, string>
            {
                ["Id"] = "InitState",
                ["BasedOn"] = "PossState",
                ["Background"] = "MediumOrchid",
                //["Icon"] = @""
            };
            AddRecord(CategoryName, AttributesInitState);

            Dictionary<string, string> AttributesFinalState = new Dictionary<string, string>
            {
                ["Id"] = "FinalState",
                ["BasedOn"] = "PossState",
                ["Background"] = "MediumPurple",
                //["Icon"] = @""
            };
            AddRecord(CategoryName, AttributesFinalState);

            Dictionary<string, string> AttributesConnector = new Dictionary<string, string>
            {
                ["Id"] = "Connector",
                ["Background"] = "Red"
            };
            AddRecord(CategoryName, AttributesConnector);
        }

        internal override void AddLinkes()
        {
            Dictionary<string, string> Attributes = new Dictionary<string, string>
            {
                ["Category"] = "Connector"
            };

            foreach (var curr in states)
            {
                foreach (var l in curr.Value.Children)
                {
                    Attributes["Source"] = curr.Key;
                    Attributes["Target"] = l.Child.Content.CheckSum;
                    Attributes["Label"] = l.ActionNr.ToString();
                    Attributes["ActionName"] = Owner.actions[l.ActionNr].Name;
                    Attributes["ActionCost"] = l.ActionCost.ToString();

                    string Sententia = (string)Owner.actions[l.ActionNr].InstantActionSententia?.DynamicInvoke(l.ActionArgThOb);
                    Attributes["Sententia"] = Sententia;

                    AddRecord(LinkName, Attributes);
                }
            }
        }

        internal override void AddNodes()
        {
            string ActionLegendLabel = "Actions List:\n";
            for (int i = 0; i != Owner.actions.Count; i++)
            {
                ActionLegendLabel = ActionLegendLabel + "\n" + i + ": " + Owner.actions[i].Name;
            }

            Dictionary<string, string> ActionLegendAttributes = new Dictionary<string, string>();
            ActionLegendAttributes["Id"] = "ActionLegend";
            ActionLegendAttributes["Category"] = "Comment";
            ActionLegendAttributes["Label"] = ActionLegendLabel;
            AddRecord(NodeName, ActionLegendAttributes);

            Dictionary<string, string> Attributes = new Dictionary<string, string>();

            foreach (var curr in states)
            {
                if(curr.Key == root.Content.CheckSum)
                    Attributes["Category"] = "InitState";
                else if (FoundedGoalCrisscrosses.Contains(curr.Key))
                    Attributes["Category"] = "FinalState";
                else
                    Attributes["Category"] = "PossState";

                Attributes["Id"] = curr.Key;
                Attributes["Label"] = curr.Key;
                Attributes["CumulativeCost"] = curr.Value.CumulativedTransitionCharge.ToString();

                AddRecord(NodeName, Attributes);
            }
        }

        internal override void AddProperties()
        {
            Dictionary<string, string> AttributesCumulativeCost = new Dictionary<string, string>
            {
                ["Id"] = "CumulativeCost",
                ["Label"] = "Cumulative Cost",
                ["DataType"] = "System.UInt32"
            };
            AddRecord(PropertyName, AttributesCumulativeCost);

            Dictionary<string, string> AttributesActionName = new Dictionary<string, string>
            {
                ["Id"] = "ActionName",
                ["Label"] = "Action Name",
                ["DataType"] = "System.String"
            };
            AddRecord(PropertyName, AttributesActionName);

            Dictionary<string, string> AttributesActionCost = new Dictionary<string, string>
            {
                ["Id"] = "ActionCost",
                ["Label"] = "Action Cost",
                ["DataType"] = "System.UInt32"
            };
            AddRecord(PropertyName, AttributesActionCost);

            Dictionary<string, string> AttributesSententia = new Dictionary<string, string>
            {
                ["Id"] = "Sententia",
                ["Label"] = "Action Sententia",
                ["DataType"] = "System.String"
            };
            AddRecord(PropertyName, AttributesSententia);
        }
    }
}
