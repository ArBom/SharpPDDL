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

        internal override void CompleteClassPos(IReadOnlyList<Parametr> listOfParams)
        {
            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash1Class)
                    continue;

                if (t1.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct1ClassPos = index;
                    return;
                }
            }
        }
    }
}