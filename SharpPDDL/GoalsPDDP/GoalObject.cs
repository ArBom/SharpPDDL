using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal interface IGoalObject
    {
        object OriginalObj { get; }
        Type OriginalObjType { get; }
        LambdaExpression GoalPDDL { get; }
        LambdaExpression BuildGoalPDDP(List<SingleTypeOfDomein> allTypes);
        DomeinPDDL newPDDLdomain { get; }
    }

    internal class GoalObject<T> : IGoalObject where T : class
    {
        private readonly T _OriginalObj;
        public object OriginalObj { get { return _OriginalObj; } }

        private readonly Type _OriginalObjType;
        public Type OriginalObjType { get { return _OriginalObjType; } }

        private readonly DomeinPDDL _newPDDLdomain;
        public DomeinPDDL newPDDLdomain { get { return _newPDDLdomain; } }

        private LambdaExpression _GoalPDDL = null;
        public LambdaExpression GoalPDDL { get { return _GoalPDDL ?? throw new Exception(); } }

        private List<Expression<Predicate<T>>> Expectations;

        public GoalObject(T originalObj, Type originalObjType, DomeinPDDL newPDDLdomain, List<Expression<Predicate<T>>> excetptions)
        {
            if (excetptions is null)
                throw new Exception();

            if (excetptions.Count == 0)
                throw new Exception();

            this._OriginalObj = originalObj;
            this._OriginalObjType = originalObjType;
            this._newPDDLdomain = newPDDLdomain;
            this.Expectations = excetptions;
        }

        public LambdaExpression BuildGoalPDDP(List<SingleTypeOfDomein> allTypes)
        {
            //_GoalPDDL =
            throw new NotImplementedException();
        }
    }
}
