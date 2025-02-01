using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal interface IGoalObject
    {
        object OriginalObj { get; }
        Type OriginalObjType { get; }
        Delegate GoalPDDL { get; }
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

        private Delegate _GoalPDDL = null;
        public Delegate GoalPDDL { get { return _GoalPDDL ?? throw new Exception(); } }

        private List<Expression<Predicate<T>>> Expectations;

        public GoalObject(T originalObj, Type originalObjType, DomeinPDDL newPDDLdomain, List<Expression<Predicate<T>>> excetptions)
        {
            if (excetptions is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E28"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 104, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!excetptions.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E29"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 105, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            this._OriginalObj = originalObj;
            this._OriginalObjType = originalObjType;
            this._newPDDLdomain = newPDDLdomain;
            this.Expectations = excetptions;
        }

        public LambdaExpression BuildGoalPDDP(List<SingleTypeOfDomein> allTypes)
        {
            GoalLambdaPDDL<T> goalLambdaPDDL;

            if (_OriginalObj is null)
                goalLambdaPDDL = new GoalLambdaPDDL<T>(Expectations, allTypes, _OriginalObjType);
            else
                goalLambdaPDDL = new GoalLambdaPDDL<T>(Expectations, allTypes, _OriginalObj);

            LambdaExpression ToRet = goalLambdaPDDL.ModifeidLambda;

            try
            {
                _GoalPDDL = ToRet.Compile();
            }
            catch (Exception e)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C29"), e.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 106, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            return ToRet;
        }
    }
}