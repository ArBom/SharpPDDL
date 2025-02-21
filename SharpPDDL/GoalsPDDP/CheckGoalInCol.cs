using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    internal static class CheckGoalInCol
    {
        internal static Task CheckNewGoal(CancellationToken cancellationToken, Crisscross states, GoalPDDL Goal, Action<KeyValuePair<Crisscross, List<GoalPDDL>>> foundSols)
        {
            Task CheckItAsTask = new Task(() => CheckIt(cancellationToken, states, Goal, foundSols));
            CheckItAsTask.Start();
            return CheckItAsTask;
        }

        private static bool CheckNewGoalsReachPossibility(PossibleState possibleState, GoalPDDL possibleGoal)
        {
            foreach (var state in possibleState.ChangedThumbnailObjects)
                foreach (var goalObj in possibleGoal.GoalObjects)
                    if ((bool)goalObj.GoalPDDL.DynamicInvoke(state))
                        return true;

            return false;
        }

        private static void CheckIt(CancellationToken cancellationToken, Crisscross states, GoalPDDL Goal, Action<KeyValuePair<Crisscross, List<GoalPDDL>>> foundSols)
        {
            Crisscross TempCrisscross;
            CrisscrossRefEnum crisscrossRefEnum = new CrisscrossRefEnum(ref states);
            while (crisscrossRefEnum.MoveNext() && !cancellationToken.IsCancellationRequested)
            {
                TempCrisscross = crisscrossRefEnum.Current;

                if (!CheckNewGoalsReachPossibility(TempCrisscross.Content, Goal))
                    continue;

                if (Goal.GoalObjects.Count() == 1)
                {
                    foundSols.Invoke(new KeyValuePair<Crisscross, List<GoalPDDL>>(TempCrisscross, new List<GoalPDDL>() { Goal }));
                    continue;
                }

                bool goalObjCorrect = true;

                foreach (IGoalObject goalObject in Goal.GoalObjects)
                {
                    if (TempCrisscross.Content.ThumbnailObjects.Any(ThOb => (bool)goalObject.GoalPDDL.DynamicInvoke(ThOb)))
                        continue;
                    else
                    {
                        goalObjCorrect = false;
                        break;
                    }
                }

                if (goalObjCorrect)
                    foundSols.Invoke(new KeyValuePair<Crisscross, List<GoalPDDL>>(TempCrisscross, new List<GoalPDDL>() { Goal }));
            }
        }
    }
}
