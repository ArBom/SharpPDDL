using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    class ActionLambdaPDDL : ExpressionVisitor
    {
        private readonly List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private readonly List<EffectPDDL> Effects; //efekty
        private readonly List<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)

        public ActionLambdaPDDL(List<Parametr> parameters, List<PreconditionPDDL> preconditions, List<EffectPDDL> effects)
        {
            this.Parameters = parameters;
            this.Preconditions = preconditions;
            this.Effects = effects;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            throw new NotImplementedException();
        }
    }
}
