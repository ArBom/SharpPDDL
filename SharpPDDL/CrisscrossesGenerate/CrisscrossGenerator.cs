using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SharpPDDL
{
    internal class CrisscrossGenerator
    {
        //////////////////////////////////////////////////
        //                                              //
        // CrisscrossGenerator(CurrentBuilded)          //
        //                           ⋮                  //
        //  ┌──┐                     ⋮     ┌──┐         //
        //  │crisscrossReducer       ⋮     │goalChecker //
        //  ├──┴──┐                  ▼     ├──┴──┐      //
        //  │ ⤺   │PossibleGoalRealization│ ⤺   │      //
        //  │⤹  🚦③│   (ConcurrentQueue)   │⤹  🚦①│      //
        //  │ ⤻   │⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯▶│ ⤻  │      //
        //  └─────┘                        └─────┘      //
        //     ▼                               ⋰        //
        //       ⋱             PossibleNewCrisscrossCre //
        // PossibleToCrisscrossReduce      ⋰(SortedSet) //
        //           ⋱ (SortedSet)       ⋰              //
        //             ⋱               ⋰                //
        //               ⋱           ⋰                  //
        //               ┌──┐     ▲                     //
        //               │crisscrossNewPossiblesCreator //
        //               ├──┴──┐                        //
        //               │ ⤺   │                       //
        //               │⤹  🚦②│                       //
        //               │ ⤻   │                       //
        //               └─────┘                       //
        //                                             //
        /////////////////////////////////////////////////

        //Cancelation Tokens
        internal CancellationTokenSource InternalCancellationCrisscrossGenerator;
        protected CancellationToken CancellationDomein;
        protected CancellationToken CancellationCrisscrossGenerator;
        internal bool IsCrisscrossGeneratorCancellationRequested => CancellationCrisscrossGenerator.IsCancellationRequested;

        //Lockers
        protected readonly object PossibleNewCrisscrossCreLocker;
        protected readonly object CrisscrossReduceLocker;

        //Buffors between consuments-procucents
        protected ConcurrentQueue<Crisscross> PossibleGoalRealization;
        protected ICollection<Crisscross> PossibleNewCrisscrossCre;
        protected ICollection<Crisscross> PossibleToCrisscrossReduce;

        //Classes of data workining
        protected GoalChecker goalChecker;
        protected CrisscrossNewPossiblesCreator crisscrossNewPossiblesCreator;
        protected CrisscrossReducer crisscrossReducer;

        protected readonly Action NoNewDataCheck;
        internal Action CrisscrossesGenerated;

        internal uint CheckCost()
        {
            uint ret1, ret2;

            lock(PossibleNewCrisscrossCreLocker)
            {
                ret1 = PossibleNewCrisscrossCre.Any() ? PossibleNewCrisscrossCre.First().CumulativedTransitionCharge : uint.MaxValue;
            }

            lock (CrisscrossReduceLocker)
            {
                ret2 = PossibleToCrisscrossReduce.Any() ? PossibleToCrisscrossReduce.First().CumulativedTransitionCharge : uint.MaxValue;
            }

            return Math.Min(ret1, ret2);
        }

        internal void InitBuffors (IEnumerable<Crisscross> PossibleGoalRealization, IEnumerable<Crisscross> PossibleNewCrisscrossCre, IEnumerable<Crisscross> PossibleToCrisscrossReduce, SortedList<string, Crisscross> NewIndexedStates)
        {
            this.PossibleGoalRealization = new ConcurrentQueue<Crisscross>();

            if (!(PossibleGoalRealization is null))
                foreach (Crisscross c in PossibleGoalRealization)
                    this.PossibleGoalRealization.Enqueue(c);

            this.PossibleNewCrisscrossCre = (PossibleNewCrisscrossCre is null) ? new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge()) : new SortedSet<Crisscross>(PossibleNewCrisscrossCre, Crisscross.SortCumulativedTransitionCharge());
            this.PossibleToCrisscrossReduce = (PossibleToCrisscrossReduce is null) ? new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge()) : new SortedSet<Crisscross>(PossibleToCrisscrossReduce, Crisscross.SortCumulativedTransitionCharge());

            if (!(this.crisscrossReducer is null))
            {
                this.crisscrossReducer.IndexedStates = NewIndexedStates;
                this.crisscrossReducer.PossibleToCrisscrossReduce = this.PossibleToCrisscrossReduce;
                this.crisscrossReducer.PossibleGoalRealization = this.PossibleGoalRealization;
            }

            if (!(this.goalChecker is null))
            {
                this.goalChecker.PossibleNewCrisscrossCre = this.PossibleNewCrisscrossCre;
                this.goalChecker.PossibleGoalRealization = this.PossibleGoalRealization;
            }

            if (!(this.crisscrossNewPossiblesCreator is null))
            {
                this.crisscrossNewPossiblesCreator.PossibleNewCrisscrossCre = this.PossibleNewCrisscrossCre;
                this.crisscrossNewPossiblesCreator.PossibleToCrisscrossReduce = this.PossibleToCrisscrossReduce;
            }
        }

        internal CrisscrossGenerator(Crisscross CurrentBuilded, DomeinPDDL Owner, Action<KeyValuePair<Crisscross, List<GoalPDDL>>> foundSols, Action<uint> currentMinCumulativeCostUpdate)
        {
            InitBuffors(null, null, null, null);
            this.NoNewDataCheck = new Action(CheckAllGenerated);

            //Creating AutoResetEvents
            AutoResetEvent CheckingGoalRealizationARE = new AutoResetEvent(false); //🚦①
            AutoResetEvent BuildingNewCrisscrossARE = new AutoResetEvent(false); //🚦②
            AutoResetEvent ReducingCrisscrossARE = new AutoResetEvent(false); //🚦③

            //add the root of whole tree to check at the begining
            PossibleGoalRealization.Enqueue(CurrentBuilded);

            PossibleNewCrisscrossCreLocker = new object();
            CrisscrossReduceLocker = new object();

            //Init the classes with task and set communication of it
            this.goalChecker = new GoalChecker(Owner.domainGoals, CheckingGoalRealizationARE, PossibleGoalRealization, PossibleNewCrisscrossCreLocker, PossibleNewCrisscrossCre, BuildingNewCrisscrossARE);
            this.crisscrossNewPossiblesCreator = new CrisscrossNewPossiblesCreator(Owner.actions, BuildingNewCrisscrossARE, PossibleNewCrisscrossCre, PossibleNewCrisscrossCreLocker, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker);
            this.crisscrossReducer = new CrisscrossReducer(CurrentBuilded, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker, PossibleGoalRealization, CheckingGoalRealizationARE);

            goalChecker.foundSols = foundSols;
            goalChecker.CurrentMinCumulativeCostUpdate = currentMinCumulativeCostUpdate;
            goalChecker.NoNewData = NoNewDataCheck;
            crisscrossReducer.NoNewData = NoNewDataCheck;
            crisscrossNewPossiblesCreator.NoNewData = NoNewDataCheck;
        }

        private void Definetoken(CancellationToken CancellationDomein)
        {
            if (CancellationDomein.IsCancellationRequested)
                throw new Exception();

            this.CancellationDomein = CancellationDomein;
            this.InternalCancellationCrisscrossGenerator = new CancellationTokenSource();
            this.CancellationCrisscrossGenerator = CancellationTokenSource.CreateLinkedTokenSource(CancellationDomein, InternalCancellationCrisscrossGenerator.Token).Token;
        }

        internal void Start(CancellationToken CancellationDomein)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 59, GloCla.ResMan.GetString("Sa6"));

            //Cancellation Tokens
            Definetoken(CancellationDomein);

            //Get ready all Tasks of this process
            goalChecker.Start(CancellationCrisscrossGenerator);
            crisscrossNewPossiblesCreator.Start(CancellationCrisscrossGenerator);
            crisscrossReducer.Start(CancellationCrisscrossGenerator);

            //Start cheching the root in goal reach
            goalChecker.CheckingGoalRealizationARE.Set();
        }

        internal void ReStart()
        {
            if (!CancellationCrisscrossGenerator.IsCancellationRequested)
                return;

            Definetoken(this.CancellationDomein);

            //Get ready all Tasks of this process
            goalChecker.Start(CancellationCrisscrossGenerator);
            crisscrossNewPossiblesCreator.Start(CancellationCrisscrossGenerator);
            crisscrossReducer.Start(CancellationCrisscrossGenerator);

            //Start subproceses in goal reach
            goalChecker.CheckingGoalRealizationARE.Set();
            crisscrossNewPossiblesCreator.BuildingNewCrisscrossARE.Set();
            crisscrossReducer.ReducingCrisscrossARE.Set();
        }

        internal void MakeExtraChild(Crisscross crisscross)
        {
            if (!crisscross.Children.Any())
                crisscrossNewPossiblesCreator.BuildNewStateForCrisscross(crisscross);

            for (int i = 0; i != crisscross.Children.Count; i++)
            {
                Crisscross ty = crisscross.Children[i].Child;
                crisscrossReducer.MergeCrisscross(ref ty);
            }
        }

        internal Task Stop()
        {
            Task Stopping = new Task(() =>
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 61, GloCla.ResMan.GetString("I5"));

                //use internal Cancellation Token
                if (!CancellationCrisscrossGenerator.IsCancellationRequested)
                    InternalCancellationCrisscrossGenerator.Cancel();
                else
                    return;

                //make sure there is no task wainting for buffor add element
                goalChecker.CheckingGoalRealizationARE.Set();
                crisscrossNewPossiblesCreator.BuildingNewCrisscrossARE.Set();
                crisscrossReducer.ReducingCrisscrossARE.Set();

                //wait for all task finish
                Task.WaitAll(new Task[] { goalChecker.CheckingGoal, crisscrossNewPossiblesCreator.BuildingNewCrisscross, crisscrossReducer.BuildingNewCrisscross }, 100);

                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 60, GloCla.ResMan.GetString("Sp6"));
            });

            if (Stopping is null)
            return Task.CompletedTask;

            Stopping.Start();
            return Stopping;
        }

        private void CheckAllGenerated()
        {
            //GloCla.Tracer?.TraceEvent(TraceEventType.Information, 62, GloCla.ResMan.GetString("I6"));

            lock (PossibleNewCrisscrossCreLocker)
                if (PossibleNewCrisscrossCre.Any())
                    return;

            lock (CrisscrossReduceLocker)
                if (PossibleToCrisscrossReduce.Any())
                    return;

            if (PossibleGoalRealization.Any())
                return;

            if (!goalChecker.IsWaiting)
                return;

            if (!crisscrossNewPossiblesCreator.IsWaiting)
                return;

            if (!crisscrossReducer.IsWaiting)
                return;

            CrisscrossesGenerated?.Invoke();
        }
    }
}