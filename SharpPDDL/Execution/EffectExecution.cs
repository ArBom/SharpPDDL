using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal static class EffectExecution
    {
        internal static Delegate CreateEffectDelegate<T1c, T1p> (this EffectPDDL1<T1c, T1p> effectPDDL1, IReadOnlyList<Parametr> Parameters)
        where T1p : class
        where T1c : class, T1p
        {
            List<ParameterExpression> parameters = new List<ParameterExpression>();
            /*{
                Expression.Parameter(typeof(WaitHandle), "Signal"),
                Expression.Parameter(typeof(WaitHandle), "WaitFor")
            };*/

            for (int i = 0; i != Parameters.Count; i++)
            {
                if(effectPDDL1.AllParamsOfAct1ClassPos == i)
                {
                    parameters.Add(effectPDDL1.Destination.Parameters[0]);
                }
                else
                {
                    ParameterExpression ToAdd = Expression.Parameter(Parameters[i].Type);
                    parameters.Add(ToAdd);
                }
            }
            Expression Body = Expression.Assign(effectPDDL1.Destination, Expression.Constant(effectPDDL1.newValue));
            LambdaExpression Whole = Expression.Lambda(Body, parameters);

            return Whole.Compile();
        }
    }
}
