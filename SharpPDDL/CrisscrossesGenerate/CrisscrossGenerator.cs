using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpPDDL.CrisscrossesGenerate;

namespace SharpPDDL
{
    internal class CrisscrossGenerator
    {
        //Cancelation Tokens
        internal CancellationTokenSource InternalCancellationTokenSrc;
        protected CancellationToken ExternalCancellation;
        protected CancellationToken CurrentCancelTokenS;

        object PossibleNewCrisscrossCreLocker;
        object CrisscrossReduceLocker;

        //Buffors between consuments-procucents
        ConcurrentQueue<Crisscross> PossibleGoalRealization;
        SortedSet<Crisscross> PossibleNewCrisscrossCre;
        List<Crisscross> PossibleToCrisscrossReduce;

        //Classes of data workining
        protected GoalChecker goalChecker;
        protected CrisscrossNewPossiblesCreator crisscrossNewPossiblesCreator;
        protected CrisscrossReducer crisscrossReducer;

        readonly Action NoNewDataCheck;
        readonly Action CrisscrossesGenerated;
        readonly CurrentMinCumulativeCostUpdate currentMinCumulativeCostUpdate;

        internal CrisscrossGenerator(DomeinPDDL Owner)
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
            PossibleGoalRealization.Enqueue(Owner.states);

            PossibleNewCrisscrossCreLocker = new object();
            CrisscrossReduceLocker = new object();

            //Init the classes with task and set communication of it
            this.goalChecker = new GoalChecker(Owner.domainGoals, CheckingGoalRealizationARE, PossibleGoalRealization, PossibleNewCrisscrossCreLocker, PossibleNewCrisscrossCre, BuildingNewCrisscrossARE);
            this.crisscrossNewPossiblesCreator = new CrisscrossNewPossiblesCreator(Owner.actions, BuildingNewCrisscrossARE, PossibleNewCrisscrossCre, PossibleNewCrisscrossCreLocker, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker);
            this.crisscrossReducer = new CrisscrossReducer(Owner.states, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker, PossibleGoalRealization, CheckingGoalRealizationARE);

            goalChecker.foundSols = Owner.foundSols;
            goalChecker.NoNewData = NoNewDataCheck;
            crisscrossReducer.NoNewData = NoNewDataCheck;
            crisscrossNewPossiblesCreator.NoNewData = NoNewDataCheck;
            //crisscrossNewPossiblesCreator.CurrentMinCumulativeCostUpdate = 
        }

        internal void Start(CancellationToken ExternalCancellationToken)
        {
            //remember External CancelationToken to reuse it after reset
            this.ExternalCancellation = ExternalCancellationToken;

            //Token used to reset CrisscrossGenerator process when it is too big
            InternalCancellationTokenSrc = new CancellationTokenSource();
            //Invoke ReStart in case of internal cancelation
            InternalCancellationTokenSrc.Token.Register(ReStart);
            CurrentCancelTokenS = CancellationTokenSource.CreateLinkedTokenSource(ExternalCancellation, InternalCancellationTokenSrc.Token).Token;

            this.PossibleGoalRealization = new ConcurrentQueue<Crisscross>();
            this.PossibleNewCrisscrossCre = new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge()); ;
            this.PossibleToCrisscrossReduce = new List<Crisscross>();

            //Get ready all Tasks of this process
            goalChecker.Start(CurrentCancelTokenS);
            crisscrossNewPossiblesCreator.Start(CurrentCancelTokenS);
            crisscrossReducer.Start(CurrentCancelTokenS);

            //Start cheching the root in goal reach
            goalChecker.CheckingGoalRealizationARE.Set();
        }

        private void ReStart()
        {
            //make sure there is no task wainting for buffor add element
            goalChecker.CheckingGoalRealizationARE.Set();
            crisscrossNewPossiblesCreator.BuildingNewCrisscrossARE.Set();
            crisscrossReducer.ReducingCrisscrossARE.Set();

            //wait for all task finish
            Task.WaitAll(new Task[] { goalChecker.CheckingGoal, crisscrossNewPossiblesCreator.BuildingNewCrisscross, crisscrossReducer.BuildingNewCrisscross }, 100 );

            Start(ExternalCancellation);
        }

        private void CheckAllGenerated()
        {
            //lock (PossibleNewCrisscrossCreLocker)
                if (PossibleNewCrisscrossCre.Any())
                    return;

            //lock(CrisscrossReduceLocker)
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

            Console.WriteLine("all states generated");
            CrisscrossesGenerated?.Invoke();
        }
    }
}