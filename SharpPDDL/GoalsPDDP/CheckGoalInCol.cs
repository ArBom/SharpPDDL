using System;
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

        internal static bool CheckConnectors (ICollection<ConcatenatedCondition> conCond, List<IEnumerable<ThumbnailObject>> O)
        {
            List<List<ThumbnailObject>> O2 = new List<List<ThumbnailObject>>();
            int[] CurrPos = new int[O.Count()];
            int[] MaxPos = new int[CurrPos.Length];

            for (int i = 0; i != CurrPos.Length; i++)
            {
                O2.Add(O[i].ToList());
                MaxPos[i] = O2[i].Count() - 1;
            }

            bool IncCurrPos(int i)
            {
                if (i < 0)
                    return false;

                if (CurrPos[i] == MaxPos[i])
                    return IncCurrPos(i - 1);

                CurrPos[i]++;

                if (i+1 < CurrPos.Length)
                    Array.Clear(CurrPos, i + 1, CurrPos.Length - (i+1));

                return true;
            }

            int Bigger = CurrPos.Length;
            ThumbnailObject[] ToCheck = new ThumbnailObject[2];
            do
            {
                bool AllOk = true;

                foreach (var cC in conCond)
                {
                    Bigger = cC.BiggerThumbnail;
                    ToCheck[0] = O2[cC.Thumbnail1][CurrPos[cC.Thumbnail1]];
                    ToCheck[1] = O2[cC.Thumbnail2][CurrPos[cC.Thumbnail2]];
                    if(!(bool)cC.GoalPDDL.DynamicInvoke(ToCheck))
                    {
                        AllOk = false;
                        break;
                    }
                }

                if (AllOk)
                    return true;
            }
            while (IncCurrPos(Bigger));

            return false;
        }

        internal static bool CheckGoalAtCrisscross(GoalPDDL Goal, Crisscross updatedOb)
        {
            if (!CheckNewGoalsReachPossibility(updatedOb.Content, Goal))
                return false;

            if (Goal.GoalObjects.Count() == 1)
                return true;

            bool AnyConnectors = Goal.concatenatedConditions.Any();
            List<IEnumerable<ThumbnailObject>> O = new List<IEnumerable<ThumbnailObject>>();

            foreach (IGoalObject goalObject in Goal.GoalObjects)
            {
                lock (updatedOb.Content.ThumbnailObjects)
                    if (updatedOb.Content.ThumbnailObjects.Any(ThOb => (bool)goalObject.GoalPDDL.DynamicInvoke(ThOb)))
                    {
                        if (AnyConnectors)
                            O.Add(updatedOb.Content.ThumbnailObjects.Where(ThOb => (bool)goalObject.GoalPDDL.DynamicInvoke(ThOb)));

                        continue;
                    }
                    else
                        return false;
            }

            if (AnyConnectors)
                return CheckConnectors(Goal.concatenatedConditions, O);

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
