using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class EffectExecution : Execution
    {
        readonly EffectPDDL SourceEffectPDDL;

        internal EffectExecution(EffectPDDL SourceEffectPDDL) : base(SourceEffectPDDL.Name)
        {
            this.SourceEffectPDDL = SourceEffectPDDL;
        }


        internal override Delegate CreateEffectDelegate(IReadOnlyList<Parametr> Parameters)
        {
            List<ParameterExpression> parameters = new List<ParameterExpression>();

            /*for (int i = 0; i != Parameters.Count; i++)
            {
                if(SourceEffectPDDL.AllParamsOfAct1ClassPos == i)
                {
                    parameters.Add(SourceEffectPDDL.)
                    parameters.Add(effectPDDL1.Destination.Parameters[0]);
                }
                else
                {
                    ParameterExpression ToAdd = Expression.Parameter(Parameters[i].Type);
                    parameters.Add(ToAdd);
                }
            }
            Expression Body = Expression.Assign(effectPDDL1.Destination, SourceEffectPDDL.SourceFunc);
            LambdaExpression Whole = Expression.Lambda(Body, this.Name, parameters);

            return Whole.Compile();*/

            return null;
        }
    }
}
