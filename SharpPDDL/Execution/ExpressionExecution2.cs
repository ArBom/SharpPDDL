﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ExpressionExecution<T1, T2> : ExpressionExecution
    {
        readonly T1 t1;
        readonly T2 t2;

        internal ExpressionExecution(string Name, ref T1 t1, ref T2 t2, Expression<Action<T1, T2>> action, bool WorkWithNewValues) 
            : base(Name, action, WorkWithNewValues, typeof(T1), t1.GetHashCode(), typeof(T2), t2.GetHashCode())
        {
            this.t1 = t1;
            this.t2 = t2;
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            TXIndex(t1, 1, Parameters);
            TXIndex(t2, 2, Parameters);
        }
    }
}
