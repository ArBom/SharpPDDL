using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    class GoalChecker
    {
        internal readonly ObservableCollection<GoalPDDL> domainGoals;
        internal Task CheckingGoal { get; private set; }
        internal bool IsWaiting = false;
        internal Action<KeyValuePair<Crisscross, List<GoalPDDL>>> foundSols;
        internal Action NoNewData;

        protected UInt32 CurrentMinCumulativeCost;
        internal Action<uint> CurrentMinCumulativeCostUpdate;

        internal AutoResetEvent CheckingGoalRealizationARE;
        ConcurrentQueue<Crisscross> PossibleGoalRealization;

        object PossibleNewCrisscrossCreLocker;
        SortedSet<Crisscross> PossibleNewCrisscrossCre;
        AutoResetEvent BuildingNewCrisscrossARE;

        internal GoalChecker(ObservableCollection<GoalPDDL> domainGoals, AutoResetEvent CheckingGoalRealizationARE, ConcurrentQueue<Crisscross> PossibleGoalRealization, object PossibleNewCrisscrossCreLocker, SortedSet<Crisscross> PossibleNewCrisscrossCre, AutoResetEvent BuildingNewCrisscrossARE)
        {
            this.domainGoals = domainGoals;

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
            CurrentMinCumulativeCost = 0;

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
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 68, GloCla.ResMan.GetString("Sa9"), Task.CurrentId);

            while (!token.IsCancellationRequested)
            {
                CheckingGoalRealizationARE.WaitOne();
                IsWaiting = false;

                while (!PossibleGoalRealization.IsEmpty)
                {
                    if (!PossibleGoalRealization.TryDequeue(out Crisscross possibleStatesCrisscross))
                        continue;

                    List<GoalPDDL> GoalsReach = CheckNewGoalsReach(possibleStatesCrisscross);

                    if (GoalsReach.Any())
                    {
                        KeyValuePair<Crisscross, List<GoalPDDL>> ToRet = new KeyValuePair<Crisscross, List<GoalPDDL>>(possibleStatesCrisscross, GoalsReach);
                        this.foundSols?.Invoke(ToRet);
                        CurrentMinCumulativeCost = possibleStatesCrisscross.CumulativedTransitionCharge;
                    }
                    else if (possibleStatesCrisscross.CumulativedTransitionCharge > CurrentMinCumulativeCost)
                    {
                        CurrentMinCumulativeCost = possibleStatesCrisscross.CumulativedTransitionCharge;
                        CurrentMinCumulativeCostUpdate?.Invoke(CurrentMinCumulativeCost);
                    }

                    if (!possibleStatesCrisscross.Children.Any())
                        lock (PossibleNewCrisscrossCreLocker)
                        {
                            PossibleNewCrisscrossCre.Add(possibleStatesCrisscross);
                            BuildingNewCrisscrossARE.Set();
                        }
                }
                NoNewData.BeginInvoke(null, null);
                IsWaiting = true;
            }

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 69, GloCla.ResMan.GetString("Sp9"), Task.CurrentId);
        }
    }
}
