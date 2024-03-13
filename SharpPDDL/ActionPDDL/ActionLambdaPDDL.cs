using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    class ActionLambdaPDDL : ExpressionVisitor
    {
        private readonly IReadOnlyList<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private readonly IReadOnlyList<EffectPDDL> Effects; //efekty
        private readonly IReadOnlyList<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)
        private IReadOnlyList<ParameterExpression> _parameters;
        private readonly BinaryExpression AllConditions;

        public ActionLambdaPDDL(List<Parametr> parameters, List<PreconditionPDDL> preconditions, List<EffectPDDL> effects)
        {
#region Parameters
            this.Parameters = parameters;
            List<Expression> ParamsChecks = new List<Expression>();
            List<ParameterExpression> TempParams = new List<ParameterExpression>();
            for (int i = 0; i!= parameters.Count; i++)
            {
                var CurrentPar = Expression.Parameter(typeof(ThumbnailObject), "o" + i.ToString());
                TempParams.Add(CurrentPar);

                var ThObOryginalType = Expression.MakeMemberAccess(CurrentPar, typeof(ThumbnailObject).GetField("OriginalObjType"));
                var ConType = Expression.Constant(parameters[i].Type, typeof(Type));
                var ISCorrectType = Expression.Equal(ConType, ThObOryginalType);                                                            //TODO dziedziczenie z drzewka trza uwzglednic kiedyś
                ParamsChecks.Add(ISCorrectType);
            }       
            _parameters = TempParams.AsReadOnly();
#endregion

#region Preconditions
            this.Preconditions = preconditions;
            foreach (PreconditionPDDL preconditionPDDL in Preconditions)
            {

            }

#endregion
            this.Effects = effects;
        }

        public static Expression ForEach(List<BinaryExpression> collection, ParameterExpression loopVar, Expression loopContent)
        {
            var colEx = Expression.Constant(collection, typeof(List<BinaryExpression>));

            // Creating an expression to hold a local variable.
            ParameterExpression result = Expression.Variable(typeof(bool), "result");

            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(colEx, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);
            var PosResultAssign = Expression.Assign(result, Expression.Constant(true));
            var NegResultAssign = Expression.Assign(result, Expression.Constant(false));

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { enumeratorVar, result },
                enumeratorAssign,
                PosResultAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Block( NegResultAssign, Expression.Break(breakLabel) )
                    ),
                breakLabel)
            );

            return loop;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            throw new NotImplementedException();
        }
    }
}
