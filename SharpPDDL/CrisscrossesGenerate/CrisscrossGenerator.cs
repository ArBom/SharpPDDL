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

        internal void InitBuffors (IEnumerable<Crisscross> PossibleGoalRealization, IEnumerable<Crisscross> PossibleNewCrisscrossCre, IEnumerable<Crisscross> PossibleToCrisscrossReduce)
        {
            this.PossibleGoalRealization = (PossibleGoalRealization is null)? new ConcurrentQueue<Crisscross>() : new ConcurrentQueue<Crisscross>(PossibleGoalRealization);
            this.PossibleNewCrisscrossCre = (PossibleNewCrisscrossCre is null) ? new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge()) : new SortedSet<Crisscross>(PossibleNewCrisscrossCre, Crisscross.SortCumulativedTransitionCharge());
            this.PossibleToCrisscrossReduce = (PossibleToCrisscrossReduce is null) ? new List<Crisscross>() : new List<Crisscross>(PossibleToCrisscrossReduce);
        }

        internal CrisscrossGenerator(Crisscross CurrentBuilded, DomeinPDDL Owner, Action<KeyValuePair<Crisscross, List<GoalPDDL>>> foundSols, Action<uint> currentMinCumulativeCostUpdate)
        {
            InitBuffors(null, null, null);
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
                Stop().Wait(90);

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
                if(!CurrentCancelToken.IsCancellationRequested)
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

        internal Task<(Crisscross NewRoot, SortedSet<Crisscross> ChildlessCrisscrosses)> TranscribeState(Crisscross NewRoot, CancellationToken cancellationToken)
        {
            Task<(Crisscross, SortedSet<Crisscross>)> Transcribing = new Task<(Crisscross, SortedSet<Crisscross>)>(() =>
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Start, 122, GloCla.ResMan.GetString("Sa10"));

                SortedSet<Crisscross> ChildlessCrisscrosses = new SortedSet<Crisscross>();
                Crisscross NewOne = new Crisscross
                {
                    Content = new PossibleState(NewRoot.Content.ThumbnailObjects)
                };

                var cmp = Crisscross.IContentEqualityComparer;

                CrisscrossRefEnum NewOneEnum = new CrisscrossRefEnum(ref NewOne);
                CrisscrossEnum Enum = new CrisscrossEnum(NewRoot);
                Crisscross PrevAdded = NewOne;

                while (Enum.MoveNext() && !cancellationToken.IsCancellationRequested)
                {
                    Crisscross EnumCurr = Enum.Current;
                    Crisscross ToAddAt = NewOne;

                    if (cmp.Equals(Enum.CurrentConnector.Child, PrevAdded))
                        ToAddAt = PrevAdded;
                    else
                        foreach (ChainStruct CS in Enum.UsedAlternativeRoots)
                            ToAddAt = ToAddAt.Children.First(CCC => cmp.Equals(CCC.Child, CS.Chain)).Child;

                    ToAddAt.Add(EnumCurr.Content, Enum.CurrentConnector.ActionNr, Enum.CurrentConnector.ActionArgOryg, Enum.CurrentConnector.ActionCost, out PrevAdded);

                    if (EnumCurr.AlternativeRoots.Any())
                    {
                        //TODO jeśli EnumCurr już wykrył powtórzenie, to można to wykorzystać

                        while (NewOneEnum.MoveNext())
                        {
                            if (cmp.Equals(NewOneEnum.Current, EnumCurr))
                            {
                                Enum.Repeated = true;
                                Crisscross.Merge(ref NewOneEnum.Current, ref EnumCurr);
                                break;
                            }
                        }
                    }

                    if(!EnumCurr.Children.Any())
                        ChildlessCrisscrosses.Add(EnumCurr);
                }             

                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 123, GloCla.ResMan.GetString("Sp10"));
                return (NewOne, ChildlessCrisscrosses);
            });

            Transcribing.Start();

            return Transcribing;
        }
    }
}