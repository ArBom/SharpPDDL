using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{ 
    public class EffectLambdaPDDL : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> _parameters;
        public List<string>[] used;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {

            return node;
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            
            return node;
        }
    }
}