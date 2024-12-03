using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal abstract class ActionCost
    {
        protected Type CostExpressionFuncType;
        protected LambdaExpression CostExpression;
        protected List<(object Param, int? IndexInAction)> Args;
        internal Delegate CostExpressionFunc;

        internal Delegate CreateCostExpressionFunc(IReadOnlyList<Parametr> Parameters, uint defaultCost)
        {
            for (int CArg = 0; CArg != Args.Count; CArg++)
            {
                for (int index = 0; index != Parameters.Count; index++)
                {
                    if (Parameters[index].HashCode != Args[CArg].Item1.GetHashCode())
                        continue;

                    if (Args[CArg].Param.Equals(Parameters[index].Oryginal))
                    {
                        Args[CArg] = (Args[CArg].Param, index);
                        break;
                    }
                }

                if (Args[CArg].IndexInAction.HasValue)
                    continue;

                throw new Exception();
            }

            ActionCostLambda actionCostLambda = new ActionCostLambda(Args, Parameters.Count, defaultCost);

            //actionCostLambda.Visit(null);
            //TODO wrzucenie pierwotnej lambdy do pow.

            return actionCostLambda.ToRet;
        }
    }

    internal class ActionCost<T1> : ActionCost 
        where T1 : class
    { 
        public ActionCost(ref T1 In1, Expression<Func<T1, int>> CostExpression)
        {
            this.CostExpression = CostExpression;
            this.CostExpressionFuncType = CostExpression.Type;

            Args = new List<(object Param, int? IndexInAction)>
            {
                (In1, null)
            };
        }
    }

    internal class ActionCost<T1, T2> : ActionCost
         where T1 : class
         where T2 : class
    {
        public ActionCost(ref T1 In1, ref T2 In2, Expression<Func<T1, T2, int>> CostExpression)
        {
            this.CostExpression = CostExpression;
            this.CostExpressionFuncType = CostExpression.Type;

            Args = new List<(object Param, int? IndexInAction)>
            {
                (In1, null),
                (In2, null)
            };
        }
    }

    internal class ActionCost<T1, T2, T3> : ActionCost
        where T1 : class
        where T2 : class
        where T3 : class 
    {
        public ActionCost(ref T1 In1, ref T2 In2, ref T3 In3, Expression<Func<T1, T2, T3, int>> CostExpression)
        {
            this.CostExpression = CostExpression;
            this.CostExpressionFuncType = CostExpression.Type;

            Args = new List<(object Param, int? IndexInAction)>
            {
                (In1, null),
                (In2, null),
                (In3, null)
            };
        }
    }

    internal class ActionCost<T1, T2, T3, T4> : ActionCost
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        public ActionCost(ref T1 In1, ref T2 In2, ref T3 In3, ref T4 In4, Expression<Func<T1, T2, T3, T4, int>> CostExpression)
        {
            this.CostExpression = CostExpression;
            this.CostExpressionFuncType = CostExpression.Type;

            Args = new List<(object Param, int? IndexInAction)>
            {
                (In1, null),
                (In2, null),
                (In3, null),
                (In4, null)
            };
        }
    }
}
