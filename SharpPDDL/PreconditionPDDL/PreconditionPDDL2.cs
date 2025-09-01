using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1c, T1p, T2c, T2p> : PreconditionPDDL
        where T1p : class
        where T2p : class
        where T1c : class, T1p 
        where T2c : class, T2p
    {
        internal PreconditionPDDL(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func)
            : base(Name, func, new object[2] { obj1, obj2 })
            => Expression.Empty();

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(Elements[0].Object, 1, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C32"), typeof(T1c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 112, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (TXIndex(Elements[1].Object, 2, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C33"), typeof(T2c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 113, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }
    }
}
