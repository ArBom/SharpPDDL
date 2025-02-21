using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpPDDL.CrisscrossesGenerate;
using System.Diagnostics;

namespace SharpPDDL
{
    internal class CrisscrossGenerator
    {
        //Cancelation Tokens
        internal CancellationTokenSource CrisscrossGeneratorCancellationTokenSrc;
        protected CancellationToken ExternalCancellation;
        protected CancellationToken CurrentCancelToken;

        object PossibleNewCrisscrossCreLocker;
        object CrisscrossReduceLocker;

        //Buffors between consuments-procucents
        protected ConcurrentQueue<Crisscross> PossibleGoalRealization;
        protected SortedSet<Crisscross> PossibleNewCrisscrossCre;
        protected List<Crisscross> PossibleToCrisscrossReduce;

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

        internal CrisscrossGenerator(Crisscross CurrentBuilded, DomeinPDDL Owner, Action<KeyValuePair<Crisscross, List<GoalPDDL>>> foundSols, Action<uint> currentMinCumulativeCostUpdate)
        {
            this.PossibleGoalRealization = new ConcurrentQueue<Crisscross>();
            this.PossibleNewCrisscrossCre = new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge());
            this.PossibleToCrisscrossReduce = new List<Crisscross>();

            this.NoNewDataCheck = new Action(CheckAllGenerated);

            //Creating AutoResetEvents
            AutoResetEvent CheckingGoalRealizationARE = new AutoResetEvent(false);
            AutoResetEvent BuildingNewCrisscrossARE = new AutoResetEvent(false);
            AutoResetEvent ReducingCrisscrossARE = new AutoResetEvent(false);

            //add the root of whole tree to check at the begining
            PossibleGoalRealization.Enqueue(CurrentBuilded);

            PossibleNewCrisscrossCreLocker = new object();
            CrisscrossReduceLocker = new object();

            //Init the classes with task and set communication of it
            this.goalChecker = new GoalChecker(Owner.domainGoals, CheckingGoalRealizationARE, PossibleGoalRealization, PossibleNewCrisscrossCreLocker, PossibleNewCrisscrossCre, BuildingNewCrisscrossARE);
            this.crisscrossNewPossiblesCreator = new CrisscrossNewPossiblesCreator(Owner.actions, BuildingNewCrisscrossARE, PossibleNewCrisscrossCre, PossibleNewCrisscrossCreLocker, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker);
            this.crisscrossReducer = new CrisscrossReducer(CurrentBuilded, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker, PossibleGoalRealization, CheckingGoalRealizationARE);

            goalChecker.foundSols = foundSols;
            goalChecker.NoNewData = NoNewDataCheck;
            crisscrossReducer.NoNewData = NoNewDataCheck;
            crisscrossNewPossiblesCreator.NoNewData = NoNewDataCheck;
        }

        private void Definetoken(CancellationToken ExternalCancellationToken)
        {
            this.ExternalCancellation = ExternalCancellationToken;
            this.CrisscrossGeneratorCancellationTokenSrc = new CancellationTokenSource();
            this.CurrentCancelToken = CancellationTokenSource.CreateLinkedTokenSource(ExternalCancellationToken, CrisscrossGeneratorCancellationTokenSrc.Token).Token;
        }

        internal void Start(CancellationToken ExternalCancellationToken)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 59, GloCla.ResMan.GetString("Sa6"));

            //Cancellation Tokens
            Definetoken(ExternalCancellationToken);

            //init working class
            this.PossibleGoalRealization = new ConcurrentQueue<Crisscross>();
            this.PossibleNewCrisscrossCre = new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge());
            this.PossibleToCrisscrossReduce = new List<Crisscross>();

            //Get ready all Tasks of this process
            goalChecker.Start(CurrentCancelToken);
            crisscrossNewPossiblesCreator.Start(CurrentCancelToken);
            crisscrossReducer.Start(CurrentCancelToken);

            //Start cheching the root in goal reach
            goalChecker.CheckingGoalRealizationARE.Set();
        }

        internal void ReStart(Crisscross CurrentBuildedRoot)
        {
            if (ExternalCancellation.IsCancellationRequested)
                throw new Exception();

            if (!CurrentCancelToken.IsCancellationRequested)
                Stop().Wait();

            Definetoken(this.ExternalCancellation);

            Action<KeyValuePair<Crisscross, List<GoalPDDL>>> foundSols = goalChecker.foundSols;

            //Creating AutoResetEvents
            AutoResetEvent CheckingGoalRealizationARE = new AutoResetEvent(PossibleGoalRealization.Any());
            AutoResetEvent BuildingNewCrisscrossARE = new AutoResetEvent(PossibleNewCrisscrossCre.Any());
            AutoResetEvent ReducingCrisscrossARE = new AutoResetEvent(PossibleToCrisscrossReduce.Any());

            //Init the classes with task and set communication of it
            this.goalChecker = new GoalChecker(goalChecker.domainGoals, CheckingGoalRealizationARE, PossibleGoalRealization, PossibleNewCrisscrossCreLocker, PossibleNewCrisscrossCre, BuildingNewCrisscrossARE);
            this.crisscrossNewPossiblesCreator = new CrisscrossNewPossiblesCreator(crisscrossNewPossiblesCreator.Actions.ToList(), BuildingNewCrisscrossARE, PossibleNewCrisscrossCre, PossibleNewCrisscrossCreLocker, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker);
            this.crisscrossReducer = new CrisscrossReducer(CurrentBuildedRoot, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker, PossibleGoalRealization, CheckingGoalRealizationARE);

            goalChecker.foundSols = foundSols;
            goalChecker.NoNewData = NoNewDataCheck;
            crisscrossReducer.NoNewData = NoNewDataCheck;
            crisscrossNewPossiblesCreator.NoNewData = NoNewDataCheck;

            //Get ready all Tasks of this process
            goalChecker.Start(CurrentCancelToken);
            crisscrossNewPossiblesCreator.Start(CurrentCancelToken);
            crisscrossReducer.Start(CurrentCancelToken);
        }

        internal Task Stop()
        {
            Task Stopping = new Task(() => {
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 61, GloCla.ResMan.GetString("I5"));

                //use internal Cancellation Token
                if(!CrisscrossGeneratorCancellationTokenSrc.IsCancellationRequested)
                    CrisscrossGeneratorCancellationTokenSrc.Cancel();

                //make sure there is no task wainting for buffor add element
                goalChecker.CheckingGoalRealizationARE.Set();
                crisscrossNewPossiblesCreator.BuildingNewCrisscrossARE.Set();
                crisscrossReducer.ReducingCrisscrossARE.Set();

                //wait for all task finish
                Task.WaitAll(new Task[] { goalChecker.CheckingGoal, crisscrossNewPossiblesCreator.BuildingNewCrisscross, crisscrossReducer.BuildingNewCrisscross }, 100);

                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 60, GloCla.ResMan.GetString("Sp6"));
            });

            Stopping.Start();

            return Stopping;
        }

        private void CheckAllGenerated()
        {
            //GloCla.Tracer?.TraceEvent(TraceEventType.Information, 62, GloCla.ResMan.GetString("I6"));

            if (PossibleNewCrisscrossCre.Any())
                return;

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

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 63, GloCla.ResMan.GetString("I7"));
            CrisscrossesGenerated?.Invoke();
        }

        //internal Task TranscribeStateTask(Crisscross NewRoot, out Crisscross NewOne, out SortedSet<Crisscross> PossibleNewCrisscrossCre, CancellationToken cancellationToken)
        //{
        //    return Task.Factory.StartNew(() => TranscribeState(NewRoot, cancellationToken));
        //}

        internal Task<Tuple<Crisscross, SortedSet<Crisscross>>> TranscribeState(Crisscross NewRoot, CancellationToken cancellationToken)
        {
            Task<Tuple<Crisscross, SortedSet<Crisscross>>> Transcribing = new Task<Tuple<Crisscross, SortedSet<Crisscross>>>(() =>
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Start, 122, GloCla.ResMan.GetString("Sa10"));

                SortedSet<Crisscross> TempPossibleNewCrisscrossCre = new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge());

                Crisscross NewOne = new Crisscross
                {
                    Content = NewRoot.Content
                };

                AutoResetEvent ReducingCrisscrossARE = new AutoResetEvent(false);
                List<Crisscross> PossibleToCrisscrossReduce = new List<Crisscross>();

                void TranscribeChild(Crisscross Source, Crisscross Destination)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    foreach (CrisscrossChildrenCon SourceCCC in Source.Children)
                    {
                        if (Destination.Children.Exists(c => c.Child.Content.CheckSum == SourceCCC.Child.Content.CheckSum))
                            if (Destination.Children.Exists(c => (c.Child.Content.CheckSum == SourceCCC.Child.Content.CheckSum && c.ActionNr == SourceCCC.ActionNr && c.ActionArgOryg.ToString() == SourceCCC.ActionArgOryg.ToString())))
                                continue;

                        Destination.Add(SourceCCC.Child.Content, SourceCCC.ActionNr, SourceCCC.ActionArgOryg, SourceCCC.ActionCost, out Crisscross crisscross);

                        if (SourceCCC.Child.AlternativeRoots.Any())
                            PossibleToCrisscrossReduce.Add(crisscross);

                        if (SourceCCC.Child.Children.Any())
                            TranscribeChild(SourceCCC.Child, crisscross);
                        else
                            TempPossibleNewCrisscrossCre.Add(SourceCCC.Child);
                    };

                    ReducingCrisscrossARE.Set();
                }

                CrisscrossReducer TempCrisscrossReducer = new CrisscrossReducer(NewOne, ReducingCrisscrossARE, PossibleToCrisscrossReduce, new object(), null, new AutoResetEvent(true));
                TempCrisscrossReducer.Start(cancellationToken);
                TranscribeChild(NewRoot, NewOne);

                while (!TempCrisscrossReducer.IsWaiting)
                    Thread.Sleep(30);

                TempCrisscrossReducer.BuildingNewCrisscross.Dispose();

                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 123, GloCla.ResMan.GetString("Sp10"));

                return new Tuple<Crisscross, SortedSet<Crisscross>>(NewOne, TempPossibleNewCrisscrossCre);
            });

            Transcribing.Start();

            return Transcribing;
        }
    }
}