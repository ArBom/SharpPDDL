using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class ActionCostLambda : ExpressionVisitor
    {
        private readonly ConstantExpression DefaultCost;
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private readonly IReadOnlyList<(object Param, int? IndexInAction)> Args;
        private ReadOnlyCollection<ParameterExpression> OldParameters;
        internal Delegate ToRet;

        internal ActionCostLambda(IReadOnlyList<(object Param, int? IndexInAction)> Args, int InstantActionParamCount, uint defaultCost)
        {
            this.Args = Args;

            List<ParameterExpression> param = new List<ParameterExpression>();
            for (int i = 0; i != InstantActionParamCount; i++)
                param.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix + i.ToString()));
            _parameters = new ReadOnlyCollection<ParameterExpression>(param);

            this.DefaultCost = Expression.Constant(defaultCost, typeof(int));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            OldParameters = node.Parameters;
            Expression ModifBody = Visit(node.Body);
            Expression ModCostExpress = Expression.Variable(typeof(int));
            Expression zero = Expression.Constant(0, typeof(int));
            Expression CheckPos = Expression.GreaterThan(ModCostExpress, zero);
            LabelTarget retLabelTarget = Expression.Label(typeof(int), null);

            BlockExpression FBlock = Expression.Block(
                Expression.Assign(ModCostExpress, ModifBody),
                Expression.IfThenElse(CheckPos, Expression.Return(retLabelTarget, ModCostExpress), Expression.Return(retLabelTarget, DefaultCost))
                );

            LambdaExpression WholeFunc = Expression.Lambda(FBlock, _parameters);

            try
            {
                ToRet = WholeFunc.Compile();
            }
            catch
            {
                throw new Exception();
            }

            return WholeFunc;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            int OldIndex = OldParameters.IndexOf(node);
            int NewIndex = Args[OldIndex].IndexInAction.Value;
            return _parameters[NewIndex];
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsStatic)
                return node;

            throw new Exception();
        }
    }
}
