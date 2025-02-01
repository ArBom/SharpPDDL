using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal ActionCost(uint DefaultCost)
        {
            this.defaultCost = DefaultCost;
        }

        internal void CompleteActinParams(IList<Parametr> Parameters)
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

                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E7"), Args[CArg].ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 41, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }

        internal void BuildActionCost(List<SingleTypeOfDomein> allTypes, int InstantActionParamCount)
        {
            if (CostExpression is null)
            {
                Expression<Func<int>> f = () => (int)defaultCost;
                CostExpression = f;
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