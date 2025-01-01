using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ExpressionExecution<T1, T2> : Execution
    {
        readonly T1 t1;
        readonly T2 t2;
        Expression<Action<T1, T2>> action;

        ExpressionExecution(string Name, ref T1 t1, ref T2 t2, Expression<Action<T1, T2>> action, bool WorkWithNewValues) 
            : base(Name, action, WorkWithNewValues, typeof(T1), t1.GetHashCode(), typeof(T2), t2.GetHashCode())
        {
            this.t1 = t1;
            this.t2 = t2;
            this.action = action;
        }

        internal Delegate CreateEffectDelegate(IReadOnlyList<Parametr> Parameters)
        {
            List<ParameterExpression> parameters = new List<ParameterExpression>();
            for (int i = 0; i != Parameters.Count; i++)
            {
                ParameterExpression ToAdd = Expression.Parameter(Parameters[i].Type);
                parameters.Add(ToAdd);
            }

            int? index1 = Index(t1, Parameters);
            if (!index1.HasValue)
                throw new Exception();
            parameters[index1.Value] = action.Parameters[0];

            int? index2 = Index(t2, Parameters);
            if (!index2.HasValue)
                throw new Exception();
            parameters[index1.Value] = action.Parameters[1];

            LambdaExpression lambdaExpression = Expression.Lambda(action.Body, parameters);
            this.Delegate = lambdaExpression.Compile();
            return this.Delegate;
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
