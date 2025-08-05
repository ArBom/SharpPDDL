using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal interface IGoalObject
    {
        object OriginalObj { get; set; }
        Type OriginalObjType { get; }
        Delegate GoalPDDL { get; }
        LambdaExpression BuildGoalPDDP(DomeinPDDL GoalOwner);
        DomeinPDDL NewPDDLdomain { get; }
        bool MigrateIntheEnd { get; }
    }

    internal class GoalObject<T> : IGoalObject where T : class
    {
        private T _OriginalObj;
        public object OriginalObj
        {
            get { return _OriginalObj; }
            set
            {
                if (_OriginalObj is null)
                    _OriginalObj = (T)value;
            }
        }

        private readonly Type _OriginalObjType;
        public Type OriginalObjType => _OriginalObjType;

        private Delegate _GoalPDDL = null;
        public Delegate GoalPDDL => _GoalPDDL;

        private bool _MigrateIntheEnd;
        public bool MigrateIntheEnd => _MigrateIntheEnd;

        private readonly DomeinPDDL _newPDDLdomain;
        public DomeinPDDL NewPDDLdomain => _newPDDLdomain;

        private readonly bool MigrateAccordingtoConstructor;
        private readonly ICollection<Expression<Predicate<T>>> Expectations;

        internal GoalObject(T originalObj, Type originalObjType, DomeinPDDL newPDDLdomain, ICollection<Expression<Predicate<T>>> excetptions, bool MigrateIt)
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

            _MigrateIntheEnd = MigrateAccordingtoConstructor ? !GoalOwner.Equals(NewPDDLdomain) : false;

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