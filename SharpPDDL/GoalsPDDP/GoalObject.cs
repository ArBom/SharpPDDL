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
        LambdaExpression BuildGoalPDDP(DomeinPDDL GoalOwner);
        DomeinPDDL newPDDLdomain { get; }
        bool MigrateIntheEnd { get; }
    }

    internal class GoalObject<T> : IGoalObject where T : class
    {
        private readonly T _OriginalObj;
        public object OriginalObj => _OriginalObj;

        private readonly Type _OriginalObjType;
        public Type OriginalObjType => _OriginalObjType;

        private readonly DomeinPDDL _newPDDLdomain;
        public DomeinPDDL newPDDLdomain => _newPDDLdomain;

        private Delegate _GoalPDDL = null;
        public Delegate GoalPDDL => _GoalPDDL;

        private bool _MigrateIntheEnd;
        public bool MigrateIntheEnd => _MigrateIntheEnd;

        private readonly bool MigrateAccordingtoConstructor;
        private List<Expression<Predicate<T>>> Expectations;

        public GoalObject(T originalObj, Type originalObjType, DomeinPDDL newPDDLdomain, List<Expression<Predicate<T>>> excetptions, bool MigrateIt = false)
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
            this.MigrateAccordingtoConstructor = MigrateIt;
        }

        public LambdaExpression BuildGoalPDDP(DomeinPDDL GoalOwner)
        {
            GoalLambdaPDDL<T> goalLambdaPDDL;

            if (_OriginalObj is null)
                goalLambdaPDDL = new GoalLambdaPDDL<T>(Expectations, GoalOwner.types.allTypes, _OriginalObjType);
            else
                goalLambdaPDDL = new GoalLambdaPDDL<T>(Expectations, GoalOwner.types.allTypes, _OriginalObj);

            _MigrateIntheEnd = MigrateAccordingtoConstructor ? !GoalOwner.Equals(newPDDLdomain) : false;

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