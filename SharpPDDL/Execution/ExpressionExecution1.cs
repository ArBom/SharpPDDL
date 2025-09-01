using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ExpressionExecution<T1> : ExpressionExecution
    {
        internal ExpressionExecution(string Name, ref T1 t1, Expression<Action<T1>> action, bool WorkWithNewValues)
            : base(Name, action, WorkWithNewValues, new object[1] { t1 })
            => Expression.Empty();

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(Elements[0].Object, 1, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C20"), typeof(T1), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 83, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }
    }
}