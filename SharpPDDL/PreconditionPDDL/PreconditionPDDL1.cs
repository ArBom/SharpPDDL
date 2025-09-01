using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1c, T1p> : PreconditionPDDL 
        where T1p : class
        where T1c : class, T1p
    {
        internal PreconditionPDDL(string Name, ref T1c obj1, Expression<Predicate<T1p>> func)
            : base(Name, func, new object[1] { obj1 })
            => Expression.Empty();

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(Elements[0].Object, 1, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C31"), typeof(T1c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 111, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }
    }
}