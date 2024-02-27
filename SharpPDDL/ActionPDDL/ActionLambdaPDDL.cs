using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    class ActionLambdaPDDL : ExpressionVisitor
    {
        private readonly List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private readonly List<EffectPDDL> Effects; //efekty
        private readonly List<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)
        private ReadOnlyCollection<ParameterExpression> _parameters;

        public ActionLambdaPDDL(List<Parametr> parameters, List<PreconditionPDDL> preconditions, List<EffectPDDL> effects)
        {
            this.Parameters = parameters;
            List<ParameterExpression> TempParams = new List<ParameterExpression>();
            for (int i = 0; i!= parameters.Count; i++)
            {
                TempParams.Add(Expression.Parameter(typeof(ThumbnailObject), "P" + i.ToString()));
            }       
            _parameters = TempParams.AsReadOnly();

            this.Preconditions = preconditions;
            this.Effects = effects;
        }

        /*protected Expression CheckingParamsType()
        {
            Expression CheckType = Expression.TypeIs()
            Expression loop = Expression.Loop(

                )
        }*/

        protected override Expression VisitLambda<T>(Expression<T> node = null)
        {
            throw new NotImplementedException();
        }
    }
}
