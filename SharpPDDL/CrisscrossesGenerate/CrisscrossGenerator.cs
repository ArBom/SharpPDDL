using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
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

        //Buffors between consuments-procucents
        ConcurrentQueue<Crisscross> PossibleGoalRealization;
        List<Crisscross> PossibleNewCrisscrossCre;
        List<Crisscross> PossibleToCrisscrossReduce;

        //Classes of data workining
        protected GoalChecker goalChecker;
        protected CrisscrossNewPossiblesCreator crisscrossNewPossiblesCreator;
        protected CrisscrossReducer crisscrossReducer;

        internal CrisscrossGenerator(DomeinPDDL Owner)
        {
            //Creating AutoResetEvents
            AutoResetEvent CheckingGoalRealizationARE = new AutoResetEvent(false);
            AutoResetEvent BuildingNewCrisscrossARE = new AutoResetEvent(false);
            AutoResetEvent ReducingCrisscrossARE = new AutoResetEvent(false);


            this.PossibleGoalRealization = new ConcurrentQueue<Crisscross>();
            this.PossibleNewCrisscrossCre = new List<Crisscross>();
            this.PossibleToCrisscrossReduce = new List<Crisscross>();

            //add the root of whole tree to check at the begining
            PossibleGoalRealization.Enqueue(Owner.states);

            object PossibleNewCrisscrossCreLocker = new object();
            object CrisscrossReduceLocker = new object();

            //Init the classes with task and set communication of it
            this.goalChecker = new GoalChecker(Owner.domainGoals, Owner.foundSols, CheckingGoalRealizationARE, PossibleGoalRealization, PossibleNewCrisscrossCreLocker, PossibleNewCrisscrossCre, BuildingNewCrisscrossARE);
            this.crisscrossNewPossiblesCreator = new CrisscrossNewPossiblesCreator(Owner.actions, BuildingNewCrisscrossARE, PossibleNewCrisscrossCre, PossibleNewCrisscrossCreLocker, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker);
            this.crisscrossReducer = new CrisscrossReducer(Owner.states, ReducingCrisscrossARE, PossibleToCrisscrossReduce, CrisscrossReduceLocker, PossibleGoalRealization, CheckingGoalRealizationARE);
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
            this.PossibleNewCrisscrossCre = new List<Crisscross>();
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
            Task.WaitAll(new Task[] { goalChecker.CheckingGoal, crisscrossNewPossiblesCreator.BuildingNewCrisscross, crisscrossReducer.BuildingNewCrisscross }, 50 );

            Start(ExternalCancellation);
        }
    }
}