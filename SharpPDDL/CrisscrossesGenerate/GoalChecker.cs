using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL.CrisscrossesGenerate
{
    class GoalChecker
    {
        ObservableCollection<GoalPDDL> domainGoals;
        internal Task CheckingGoal;
        internal FoundSols foundSols;

        internal AutoResetEvent CheckingGoalRealizationARE;
        ConcurrentQueue<Crisscross> PossibleGoalRealization;

        object PossibleNewCrisscrossCreLocker;
        List<Crisscross> PossibleNewCrisscrossCre;
        AutoResetEvent BuildingNewCrisscrossARE;

        internal GoalChecker(ObservableCollection<GoalPDDL> domainGoals, FoundSols foundSols, AutoResetEvent CheckingGoalRealizationARE, ConcurrentQueue<Crisscross> PossibleGoalRealization, object PossibleNewCrisscrossCreLocker, List<Crisscross> PossibleNewCrisscrossCre, AutoResetEvent BuildingNewCrisscrossARE)
        {
            this.domainGoals = domainGoals;
            this.foundSols = foundSols;

            this.CheckingGoalRealizationARE = CheckingGoalRealizationARE;
            this.PossibleGoalRealization = PossibleGoalRealization;

            this.PossibleNewCrisscrossCreLocker = PossibleNewCrisscrossCreLocker;
            this.PossibleNewCrisscrossCre = PossibleNewCrisscrossCre;
            this.BuildingNewCrisscrossARE = BuildingNewCrisscrossARE;
        }

        internal void Start(CancellationToken cancellationToken)
        {
            CheckingGoal = new Task(() => CheckGoalProces(cancellationToken));
            CheckingGoal.Start();
        }

        private bool CheckNewGoalsReachPossibility(PossibleState possibleState, GoalPDDL possibleGoal)
        {
            foreach (var state in possibleState.ChangedThumbnailObjects)
                foreach (var goalObj in possibleGoal.GoalObjects)
                    if ((bool)goalObj.GoalPDDL.DynamicInvoke(state))
                        return true;

            return false;
        }

        private List<GoalPDDL> CheckNewGoalsReach(Crisscross updatedOb)
        {
            List<GoalPDDL> RealizatedList = new List<GoalPDDL>();

            foreach (GoalPDDL Goal in domainGoals)
            {
                if (!CheckNewGoalsReachPossibility(updatedOb.Content, Goal))
                    continue;

                if (Goal.GoalObjects.Count() == 1)
                {
                    RealizatedList.Add(Goal);
                    continue;
                }

                bool goalObjCorrect = true;

                foreach (IGoalObject goalObject in Goal.GoalObjects)
                {
                    if (updatedOb.Content.ThumbnailObjects.Any(ThOb => (bool)goalObject.GoalPDDL.DynamicInvoke(ThOb)))
                        continue;
                    else
                    {
                        goalObjCorrect = false;
                        break;
                    }
                }

                if (goalObjCorrect)
                    RealizatedList.Add(Goal);
            }

            return RealizatedList;
        }

        private void CheckGoalProces(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                CheckingGoalRealizationARE.WaitOne();

                while (!PossibleGoalRealization.IsEmpty)
                {
                    if (!PossibleGoalRealization.TryDequeue(out Crisscross possibleStatesCrisscross))
                        continue;

                    List<GoalPDDL> GoalsReach = CheckNewGoalsReach(possibleStatesCrisscross);

                    if (GoalsReach.Count != 0)
                    {
                        KeyValuePair<Crisscross, List<GoalPDDL>> ToRet = new KeyValuePair<Crisscross, List<GoalPDDL>>(possibleStatesCrisscross, GoalsReach);
                        this.foundSols?.Invoke(ToRet);
                    }

                    if (possibleStatesCrisscross.Children.Count == 0)
                    {
                        lock (PossibleNewCrisscrossCreLocker)
                            PossibleNewCrisscrossCre.Add(possibleStatesCrisscross);
                        BuildingNewCrisscrossARE.Set();
                    }
                }
            }
        }
    }
}
