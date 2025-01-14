﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;

namespace SharpPDDL
{
    public partial class DomeinPDDL
    {
        static Dictionary<string, DomeinPDDL> AllDomain;

        public readonly string Name;
        private TypesPDDL types;
        internal List<ActionPDDL> actions;
        internal DomainPlanner domainPlanner;
        internal CrisscrossGenerator crisscrossGenerator;
        internal Crisscross states;
        public ObservableCollection<object> domainObjects;
        internal ObservableCollection<GoalPDDL> domainGoals;
        internal FoundSols foundSols;

        public ListOfString PlanGenerated;

        internal void CheckActions()
        {
            this.types = new TypesPDDL();
            foreach (ActionPDDL act in actions)
            {
                types.CompleteTypes(act.TakeSingleTypes());
            }

            types.CreateTypesTree();

            foreach (ActionPDDL act in actions)
            {
                act.BuildAction(types.allTypes);
            }
        }

        private void CheckExistActionName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.actions.Exists(action => action.Name == Name))
                throw new Exception(); //juz istnieje efekt o takiej nazwie
        }

        /// <summary>
        /// Method adds <see cref="ActionPDDL">Action</see> to domein
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// DomeinPDDL domeinPDDL = ...;<br/>
        /// ActionPDDL actionPDDL = new ActionPDDL("Action name");<br/>
        /// ⋮<br/>
        /// domeinPDDL.AddAction(actionPDDL);<br/>
        /// </code>
        /// </para></example>
        /// </summary>
        /// <param name="newAction"><see cref="ActionPDDL"/> to add</param>
        public void AddAction(ActionPDDL newAction)
        {
            CheckExistActionName(newAction.Name);
            this.actions.Add(newAction);
        }

        private void CheckExistGoalName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.domainGoals.Any())
                throw new Exception();
        }

        public void AddGoal(GoalPDDL newGoal)
        {
            CheckExistGoalName(newGoal.Name);
            this.domainGoals.Add(newGoal);
        }

        /// <summary>
        /// Class containing "universal" definitions of an aspect of the problem. These assumptions do not change over time, and regardless of the specific situation we are trying to solve.
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// DomeinPDDL domeinPDDL = new DomeinPDDL("domein name");<br/>
        /// domeinPDDL.AddAction(...);<br/>
        /// domeinPDDL.domainObjects.Add(...);<br/>
        /// domeinPDDL.AddGoal(...);<br/>
        /// domeinPDDL.Start();<br/>
        /// </code>
        /// </para></example>
        /// </summary>
        /// <param name="name">Name of Domein</param>
        public DomeinPDDL (string name)
        {
            if (AllDomain is null)
                AllDomain = new Dictionary<string, DomeinPDDL>();

            if (name is null)
                throw new Exception();

            if (name == "")
                throw new Exception();

            if (AllDomain.ContainsKey(name))
                throw new Exception();

            AllDomain.Add(name, this);

            this.Name = name;
            this.actions = new List<ActionPDDL>();
            this.domainGoals = new ObservableCollection<GoalPDDL>();

            this.domainObjects = new ObservableCollection<object>();
            this.domainObjects.CollectionChanged += DomainObjects_CollectionChanged;

            this.foundSols += this.GenList;
        }

        public void DefineTrace(TraceSwitch LibTraceLevel)
        {
            ExtensionMethods.traceLevel = LibTraceLevel;

            Trace.WriteLineIf(ExtensionMethods.traceLevel.TraceVerbose, ExtensionMethods.TracePrefix + "Tracing working.");
        }

        private void DomainObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs eventType)
        {
            foreach (dynamic Obj in eventType.NewItems)
            {
                if (!(Obj.GetType().IsClass))
                    throw new Exception();
            }
        }
    }
}