using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ExpressionExecution<T1> : Execution
    {
        readonly T1 t1;
        Expression<Action<T1>> action;

        ExpressionExecution(string Name, ref T1 t1, Expression<Action<T1>> action) : base(Name)
        {
            this.t1 = t1;
            this.action = action;
        }

        internal override Delegate CreateEffectDelegate(IReadOnlyList<Parametr> Parameters)
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
    }
}