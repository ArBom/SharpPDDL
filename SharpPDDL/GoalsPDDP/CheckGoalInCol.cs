using System.Collections.Generic;
using System.Linq;

namespace SharpPDDL
{
    internal static class CheckGoalInCol
    {
        internal static List<GoalPDDL> CheckNewGoalsReach(Crisscross updatedOb, ICollection<GoalPDDL> goalPDDLs)
        {
            List<GoalPDDL> RealizatedList = new List<GoalPDDL>();

            foreach (GoalPDDL Goal in goalPDDLs)
                if (CheckGoalAtCrisscross(Goal, updatedOb))
                    RealizatedList.Add(Goal);

            return RealizatedList;
        }

        internal static bool CheckGoalAtCrisscross(GoalPDDL Goal, Crisscross updatedOb)
        {
            if (!CheckNewGoalsReachPossibility(updatedOb.Content, Goal))
                return false;

            if (Goal.GoalObjects.Count() == 1)
                return true;

            foreach (IGoalObject goalObject in Goal.GoalObjects)
            {
                lock (updatedOb.Content.ThumbnailObjects)
                    if (updatedOb.Content.ThumbnailObjects.Any(ThOb => (bool)goalObject.GoalPDDL.DynamicInvoke(ThOb)))
                        continue;
                    else
                        return false;
            }

            return true;
        }

        private static bool CheckNewGoalsReachPossibility(PossibleState possibleState, GoalPDDL possibleGoal)
        {
            lock (possibleState.ChangedThumbnailObjects)
                foreach (var state in possibleState.ChangedThumbnailObjects)
                    foreach (var goalObj in possibleGoal.GoalObjects)
                        if ((bool)goalObj.GoalPDDL.DynamicInvoke(state))
                            return true;

            return false;
        }
    }
}
