using System;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class ParametrPreconditionLambda : ExpressionVisitor
    {
        private ParameterExpression _parameter;
        private Expression FuncsExpressions = null;

        public ParametrPreconditionLambda(BinaryExpression typeChEx)
        {
            _parameter = Expression.Parameter(typeof(ThumbnailObject));

            if (!(typeChEx is null))
                FuncsExpressions = Visit(typeChEx);
        }

        internal void AddPrecondition (Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> Preco)
        {
            var f2 = Visit(Preco.Body);

            if (FuncsExpressions is null)
                FuncsExpressions = f2;
            else
                FuncsExpressions = Expression.AndAlso(FuncsExpressions, f2);
        }

        internal Predicate<ThumbnailObject> func()
        {
            Predicate<ThumbnailObject> WholePredicate;

            if (FuncsExpressions is null)
                WholePredicate = TH => true;
            else
            {
                FuncsExpressions.Reduce();
                WholePredicate = (Predicate<ThumbnailObject>)Expression.Lambda(typeof(Predicate<ThumbnailObject>), FuncsExpressions, _parameter).Compile();
            }

            return WholePredicate;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}
