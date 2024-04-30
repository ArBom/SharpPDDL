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
        abstract internal Type OriginalObjType {get; set; }
        internal override ushort[] ValuesIndeksesKeys => throw new NotImplementedException();
        internal abstract Expression<Predicate<PossibleStateThumbnailObject>> BuildGoalPDDP(List<SingleTypeOfDomein> allTypes);

        public override ValueType this[ushort key]
        {
            get { return Dict[key]; }
        }
    }

    internal class GoalThumbnailObject<TOriginalObj> : GoalThumbnailObject where TOriginalObj : class
    {
        //Use this constructor if you want effect of particular object
        internal GoalThumbnailObject(TOriginalObj originalObj, List<Expression<Predicate<TOriginalObj>>> goalExpectations, IReadOnlyList<SingleTypeOfDomein> allTypes, GoalPriority goalPriority = GoalPriority.MediumPriority) : base(typeof(TOriginalObj), allTypes, goalPriority)
        {
            this.OriginalObj = originalObj;
            this.OriginalObjType = typeof(TOriginalObj);
            this.GoalExpectations = goalExpectations;
        }

        //Use this constuctor if you want effect of any object of known type
        internal GoalThumbnailObject(Type type, List<Expression<Predicate<TOriginalObj>>> goalExpectations, IReadOnlyList<SingleTypeOfDomein> allTypes, GoalPriority goalPriority = GoalPriority.MediumPriority) : base(type, allTypes, goalPriority)
        {
            this.OriginalObj = null;
            this.OriginalObjType = typeof(TOriginalObj);
            this.GoalExpectations = goalExpectations;
        }

        readonly TOriginalObj OriginalObj;
        List<Expression<Predicate<TOriginalObj>>> GoalExpectations;

        internal override ushort[] ValuesIndeksesKeys => throw new NotImplementedException();

        internal override Type OriginalObjType { get; set; }

        internal override Expression<Predicate<PossibleStateThumbnailObject>> BuildGoalPDDP(List<SingleTypeOfDomein> allTypes)
        {
            GoalLambdaPDDL<TOriginalObj> goalLambdaPDDL;

            if (OriginalObj is null)
                goalLambdaPDDL = new GoalLambdaPDDL<TOriginalObj>(GoalExpectations, allTypes);
            else
                goalLambdaPDDL = new GoalLambdaPDDL<TOriginalObj>(GoalExpectations, OriginalObj, allTypes);

            return (Expression<Predicate<PossibleStateThumbnailObject>>)goalLambdaPDDL.ModifeidLambda;
        }
    }
}
