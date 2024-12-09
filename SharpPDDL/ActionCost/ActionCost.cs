using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class ActionCost
    {
        protected LambdaExpression CostExpression;
        protected List<(object Param, int? IndexInAction)> Args;
        internal readonly uint defaultCost;
        internal Delegate CostExpressionFunc;

        private void BuildStatic(int ParamCount)
        {
            ConstantExpression ToRetC = Expression.Constant(defaultCost);
            LabelTarget labelTarget = Expression.Label(typeof(uint));
            //LabelExpression retLabelTarget = Expression.Label(labelTarget, ToRetC);
            GotoExpression expression = Expression.Return(labelTarget, ToRetC, typeof(uint));
            
            //var breake = Expression.Break(labelTarget, result);
            BlockExpression Block = Expression.Block(expression, Expression.Label(labelTarget, ToRetC));
            //BlockExpression Block = Expression.Block(Expression.Label(labelTarget, ToRetC));

            List<ParameterExpression> _parameters = new List<ParameterExpression>();
            for (int i = 0; i != ParamCount; i++)
                _parameters.Add(Expression.Parameter(typeof(ParameterExpression), "_" )); //ExtensionMethods.LamdbaParamPrefix + i.ToString()

            LambdaExpression Lambda = Expression.Lambda(Block, _parameters);
            bool a = Lambda.CanReduce;

            CostExpressionFunc = Lambda.Compile();
        }

        internal ActionCost(uint DefaultCost)
        {
            this.defaultCost = DefaultCost;
        }

        internal void TagInUse(IReadOnlyList<Parametr> Parameters)
        {
            if (CostExpression is null)
                return;

            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            _ = memberofLambdaListerPDDL.Visit(CostExpression);

            for (int CArg = 0; CArg != Args.Count; CArg++)
            {
                for (int index = 0; index != Parameters.Count; index++)
                {
                    if (Parameters[index].HashCode != Args[CArg].Param.GetHashCode())
                        continue;

                    if (Args[CArg].Param.Equals(Parameters[index].Oryginal))
                    {
                        Args[CArg] = (Args[CArg].Param, index);

                        foreach (string MemberName in memberofLambdaListerPDDL.used[CArg])
                        {
                            Parameters[index].values.First(v => v.Name == MemberName).IsInUse_ActionCostIn = true;
                        }

                        break;
                    }
                }

                if (Args[CArg].IndexInAction.HasValue)
                    continue;

                throw new Exception();
            }
        }

        internal void BuildActionCost(List<SingleTypeOfDomein> allTypes, int InstantActionParamCount)
        {
            if (CostExpression is null)
            {
                BuildStatic(InstantActionParamCount);
                return;
            }

            ActionCostLambda actionCostLambda = new ActionCostLambda(allTypes, Args, InstantActionParamCount, defaultCost);
            actionCostLambda.Visit(CostExpression);

            CostExpressionFunc = actionCostLambda.ToRet;
        }
    }

    internal class ActionCost<T1> : ActionCost 
        where T1 : class
    { 
        public ActionCost(ref T1 In1, Expression<Func<T1, int>> CostExpression, uint DefaultCost) : base (DefaultCost)
        {
            this.CostExpression = CostExpression;

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
        public ActionCost(ref T1 In1, ref T2 In2, Expression<Func<T1, T2, int>> CostExpression, uint DefaultCost) : base(DefaultCost)
        {
            this.CostExpression = CostExpression;

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
        public ActionCost(ref T1 In1, ref T2 In2, ref T3 In3, Expression<Func<T1, T2, T3, int>> CostExpression, uint DefaultCost) : base(DefaultCost)
        {
            this.CostExpression = CostExpression;

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
        public ActionCost(ref T1 In1, ref T2 In2, ref T3 In3, ref T4 In4, Expression<Func<T1, T2, T3, T4, int>> CostExpression, uint DefaultCost) : base(DefaultCost)
        {
            this.CostExpression = CostExpression;

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