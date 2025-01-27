using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class ActionCostLambda : ExpressionVisitor
    {
        private readonly ConstantExpression DefaultCost;
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private readonly IReadOnlyList<(object Param, int? IndexInAction)> Args;
        private readonly ReadOnlyCollection<SingleTypeOfDomein> allTypes;
        private ReadOnlyCollection<ParameterExpression> OldParameters;
        internal Delegate ToRet;

        internal ActionCostLambda(List<SingleTypeOfDomein> allTypes, IReadOnlyList<(object Param, int? IndexInAction)> Args, int InstantActionParamCount, uint defaultCost)
        {
            this.allTypes = new ReadOnlyCollection<SingleTypeOfDomein>(allTypes);
            this.Args = Args;

            List<ParameterExpression> param = new List<ParameterExpression>();
            for (int i = 0; i != InstantActionParamCount; i++)
                param.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), GloCla.LamdbaParamPrefix + i.ToString()));
            _parameters = new ReadOnlyCollection<ParameterExpression>(param);

            this.DefaultCost = Expression.Constant(defaultCost, typeof(uint));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            OldParameters = node.Parameters;

            //Modified function with PossibleStateThumbnailObject as a Parameters
            Expression ModifBody = Visit(node.Body);

            //Variable for modified function result
            ParameterExpression ModCostExpress = Expression.Variable(node.ReturnType, "PosResult");
            Expression conv = Expression.Convert(ModCostExpress, typeof(uint));

            //Const. 0 to check is result positive
            Expression zero = Expression.Constant(0, typeof(int));

            //Check is result positive
            Expression CheckPos = Expression.GreaterThan(ModCostExpress, zero);

            //label to return
            LabelTarget retLabelTarget = Expression.Label(typeof(uint), null);

            BlockExpression FBlock = Expression.Block(
                new ParameterExpression[] { ModCostExpress },
                Expression.Assign(ModCostExpress, ModifBody),
                Expression.IfThenElse(CheckPos, 
                    Expression.Return(retLabelTarget, conv), 
                    Expression.Return(retLabelTarget, DefaultCost)),
                Expression.Label(retLabelTarget, DefaultCost)
                );

            LambdaExpression WholeFunc = Expression.Lambda(FBlock, _parameters);

            try
            {
                ToRet = WholeFunc.Compile();
            }
            catch (Exception e)
            {
                throw new Exception();
            }

            return WholeFunc;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant)
                return node;

            //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example string("Parameter") 
            string memberExpressionName = node.Expression.ToString();

            ParameterExpression OldPar = OldParameters.First(p => p.Name == memberExpressionName);
            int OldIndex = OldParameters.IndexOf(OldPar);
            int NewIndex = Args[OldIndex].IndexInAction.Value;
            ParameterExpression newParam = _parameters[NewIndex];

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            //intersect
            SingleTypeOfDomein ParameterModel = allTypes.Where(t => t.Type == node.Expression.Type).First();

            if (ParameterModel is null)
            {
                throw new Exception();
            }

            var ValueOfIndexes = ParameterModel.CumulativeValues.Where(v => v.Name == MemberName).First();
            ushort ValueOfIndexesKey = ValueOfIndexes.ValueOfIndexesKey;
            Expression[] argument = new[] { Expression.Constant(ValueOfIndexesKey) };

            //Property of ThumbnailObject.this[uint key]
            PropertyInfo TO_indekser = typeof(PossibleStateThumbnailObject).GetProperty("Item");

            //To expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
            IndexExpression IndexAccessExpr;

            //Some kind of micro-optimalization :)
            //If the value of variable is change in athers possible state take the value 
            if (ValueOfIndexes.IsInUse_EffectOut)
            {
                IndexAccessExpr = Expression.MakeIndex(newParam, TO_indekser, argument);
            }
            //In the other case take the value from precursor
            else
            {
                PropertyInfo PrecursorPropertyInfo = typeof(PossibleStateThumbnailObject).GetProperty("Precursor");
                Expression PrecursorAccessExpression = Expression.MakeMemberAccess(newParam, PrecursorPropertyInfo);
                IndexAccessExpr = Expression.MakeIndex(PrecursorAccessExpression, TO_indekser, argument);
            }

            //Convert above expression from ValueType to particular type of frontal value
            return Expression.Convert(IndexAccessExpr, node.Type);
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
            {
                List<Expression> Arguments = new List<Expression>();
                for (int i = 0; i != node.Arguments.Count; i++)
                    Arguments.Add(Visit(node.Arguments[i]));

                MethodCallExpression Nnode = node.Update(node.Object, new ReadOnlyCollection<Expression>(Arguments));

                return Nnode;          
            }

            throw new Exception();
        }
    }
}
