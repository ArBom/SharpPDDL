using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;

namespace SharpPDDL
{
    internal struct ImplementorUpdater
    {
        internal Action<string, object[]> SignalizeNeedAcception;
        internal WaitHandle WaitOn;
        internal byte PlanImplementor_Agrees;
    }

    public partial class DomeinPDDL
    {
        static Dictionary<string, DomeinPDDL> AllDomain;

        public readonly string Name;
        private TypesPDDL types;
        internal List<ActionPDDL> actions;
        internal DomainPlanner DomainPlanner;
        internal ImplementorUpdater ImplementorUpdate;
        internal PossibleState CurrentState;
        public ObservableCollection<object> domainObjects;
        internal ObservableCollection<GoalPDDL> domainGoals;
        internal EventWaitHandle _PlanRealizationEventWaitHandle;
        public EventWaitHandle PlanRealizationEventWaitHandle
        {
            get { return _PlanRealizationEventWaitHandle; }
            set { _PlanRealizationEventWaitHandle = value; }
        }

        public ListOfString PlanGenerated;

        internal void CheckActions(ParallelOptions parallelOptions = default)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 5, GloCla.ResMan.GetString("Sa0"), this.Name);

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

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 6, GloCla.ResMan.GetString("Sp0"), this.Name);
        }

        private void CheckExistActionName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 7, GloCla.ResMan.GetString("E2"));
                throw new Exception(GloCla.ResMan.GetString("E2"));
            }

            if (this.actions.Exists(action => action.Name == Name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 8, GloCla.ResMan.GetString("E3"), Name, this.Name);
                throw new Exception(GloCla.ResMan.GetString("E3"));
            }
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
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 9, GloCla.ResMan.GetString("E4"));
                throw new Exception(GloCla.ResMan.GetString("E4"));
            }

            if (this.domainGoals.Any())
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 10, GloCla.ResMan.GetString("E5"));
                throw new Exception(GloCla.ResMan.GetString("E5"));
            }
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

            if (String.IsNullOrEmpty(name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 1, GloCla.ResMan.GetString("E0"));
                throw new Exception(GloCla.ResMan.GetString("E0"));
            }

            if (AllDomain.ContainsKey(name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 2, GloCla.ResMan.GetString("E1"), name);
                throw new Exception(GloCla.ResMan.GetString("E1"));
            }

            AllDomain.Add(name, this);

            this.Name = name;
            this.actions = new List<ActionPDDL>();
            this.domainGoals = new ObservableCollection<GoalPDDL>();

            this.domainObjects = new ObservableCollection<object>();
            this.domainObjects.CollectionChanged += DomainObjects_CollectionChanged;
        }

        public void DefineTrace(TraceSource LibTrace)
        {
            GloCla.Tracer = LibTrace;
            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 0, GloCla.ResMan.GetString("I0"), GloCla.Tracer.Name);
        }

        public void SetExecutionOptions(Action<string, object[]> SignalizeNeedAcception, WaitHandle WaitOn, params AskToAgree[] askToAgrees)
        {
            if (askToAgrees.Any(a => (a != AskToAgree.GO_AHEAD) && (a != AskToAgree.DONT_DO_IT)))
            {
                if (SignalizeNeedAcception is null)
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 125, GloCla.ResMan.GetString("W10"));

                if (WaitOn is null)
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 3, GloCla.ResMan.GetString("W0"));
            }

            int Asks = (int)AskToAgree.GO_AHEAD;
            foreach (var A in askToAgrees)
            {
                Asks = (Asks | (int)A);
            }

            ImplementorUpdate = new ImplementorUpdater
            {
                SignalizeNeedAcception = SignalizeNeedAcception,
                WaitOn = WaitOn,
                PlanImplementor_Agrees = (byte)Asks
            };

            DomainPlanner?.PlanImplementor.UpdateIt(SignalizeNeedAcception, WaitOn, (byte)Asks);
        }

        private void DomainObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs eventType)
        {
            foreach (dynamic Obj in eventType.NewItems)
            {
                if (!(Obj.GetType().IsClass))
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Information, 4, GloCla.ResMan.GetString("I2"), Obj.GetType().ToString());
                    domainObjects.Remove(Obj);
                }
            }
        }
    }
}