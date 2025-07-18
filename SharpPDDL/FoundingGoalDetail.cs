using System.Collections.Generic;

namespace SharpPDDL
{
    internal delegate void FoundSols(KeyValuePair<Crisscross, List<GoalPDDL>> foundSolutions);

    internal class FoungingGoalDetail
    {
        internal readonly GoalPDDL GoalPDDL;
        internal bool IsFoundingChippest;

        internal FoungingGoalDetail(GoalPDDL goalPDDL)
        {
            this.GoalPDDL = goalPDDL;
            IsFoundingChippest = false;
        }

        public override int GetHashCode()
        {
            return GoalPDDL.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as FoungingGoalDetail);
        }

        public bool Equals(FoungingGoalDetail obj)
        {
            if (obj != null && GoalPDDL != null && obj.GoalPDDL != null)
                return obj.GoalPDDL.Name.Equals(GoalPDDL.Name);

            return false;
        }
    }
}
