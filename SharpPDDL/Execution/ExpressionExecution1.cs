using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ExpressionExecution<T1> : Execution
    {
        readonly T1 t1;
        Expression<Action<T1>> action;

        ExpressionExecution(string Name, ref T1 t1, Expression<Action<T1>> action, bool WorkWithNewValues) 
            : base(Name, action, WorkWithNewValues, typeof(T1), t1.GetHashCode())
        {
            this.t1 = t1;
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

            int? index = Index(t1, Parameters);

            if (!index.HasValue)
                throw new Exception();

            parameters[index.Value] = action.Parameters[0];

            LambdaExpression lambdaExpression = Expression.Lambda(action.Body, parameters);
            this.Delegate = lambdaExpression.Compile();
            return this.Delegate;
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