using System;
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

        internal override void CompleteClassPos(IReadOnlyList<Parametr> listOfParams)
        {
            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash2Class)
                    continue;

                if (t2.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct1ClassPos = index;
                    break;
                }
            }

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash1Class)
                    continue;

                if (t1.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct2ClassPos = index;
                    return;
                }
            }
        }
    }
}
