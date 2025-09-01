using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ExpressionExecution<T1, T2> : ExpressionExecution
    {
        internal ExpressionExecution(string Name, ref T1 t1, ref T2 t2, Expression<Action<T1, T2>> action, bool WorkWithNewValues)
            : base(Name, action, WorkWithNewValues, new object[] { t1, t2 })
            => Expression.Empty();

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(Elements[0].Object, 1, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C21"), typeof(T1), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 84, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (TXIndex(Elements[1].Object, 2, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C22"), typeof(T2), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 85, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }
    }
}
