using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ExpressionExecution<T1> : ExpressionExecution
    {
        readonly T1 t1;

        internal ExpressionExecution(string Name, ref T1 t1, Expression<Action<T1>> action, bool WorkWithNewValues) 
            : base(Name, action, WorkWithNewValues, typeof(T1), t1.GetHashCode())
        {
            this.t1 = t1;
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            TXIndex(t1, 1, Parameters);
        }
    }
}