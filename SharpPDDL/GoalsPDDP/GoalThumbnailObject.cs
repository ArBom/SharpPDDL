using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal enum GoalPriority { Ignore = 0, LowPriority = 1, MediumPriority = 3, HighPriority = 7, TopHihtPriority = 17 };

    internal abstract class GoalThumbnailObject : ThumbnailObject
    {
        protected GoalThumbnailObject(Type originalObjType, IReadOnlyList<SingleTypeOfDomein> allTypes, GoalPriority goalPriority)
        {
            this.OriginalObjType = originalObjType;
            this.goalPriority = goalPriority;
        }

        GoalPriority goalPriority;
        Type OriginalObjType;
        internal override ushort[] ValuesIndeksesKeys => throw new NotImplementedException();
        Expression<Predicate<PossibleStateThumbnailObject>> CheckGoal;

        public override ValueType this[ushort key]
        {
            get { return Dict[key]; }
        }
    }

    internal class GoalThumbnailObject<TOriginalObj> : GoalThumbnailObject where TOriginalObj : class
    {
        internal GoalThumbnailObject(TOriginalObj originalObj, Expression<Predicate<TOriginalObj>> goalExpectation, IReadOnlyList<SingleTypeOfDomein> allTypes, GoalPriority goalPriority = GoalPriority.MediumPriority) : base(typeof(TOriginalObj), allTypes, goalPriority)
        {
            this.OriginalObj = originalObj;
            this.GoalExpectation = goalExpectation;
        }

        internal GoalThumbnailObject(Type type, Expression<Predicate<TOriginalObj>> goalExpectation, IReadOnlyList<SingleTypeOfDomein> allTypes, GoalPriority goalPriority = GoalPriority.MediumPriority) : base(type, allTypes, goalPriority)
        {
            this.OriginalObj = null;
            this.GoalExpectation = goalExpectation;
        }

        readonly TOriginalObj OriginalObj;
        Expression<Predicate<TOriginalObj>> GoalExpectation;

        internal override ushort[] ValuesIndeksesKeys => throw new NotImplementedException();
    }
}
