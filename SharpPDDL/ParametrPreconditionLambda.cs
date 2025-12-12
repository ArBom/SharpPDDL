using System;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ParametrPreconditionLambda : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private Expression FuncsExpressions;

        public ParametrPreconditionLambda(BinaryExpression typeChEx)
        {
            _parameter = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix);

            FuncsExpressions = typeChEx is null ? null : Visit(typeChEx);
        }

        internal void AddPrecondition (Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> Preco)
        {
            Expression f2 = Visit(Preco.Body);

            if (FuncsExpressions is null)
                FuncsExpressions = f2;
            else
                FuncsExpressions = Expression.AndAlso(FuncsExpressions, f2);

            FuncsExpressions = FuncsExpressions.Reduce();
        }

        internal Func<ThumbnailObject, bool> BuildFunc()
        {
            Func<ThumbnailObject, bool> Func;

            if (FuncsExpressions is null)
            {
                Func = TH => true;
            }
            else
            {
                Func = (Func<ThumbnailObject, bool>)Expression.Lambda(typeof(Func<ThumbnailObject, bool>), FuncsExpressions, _parameter).Compile();
            }

            return Func;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}
