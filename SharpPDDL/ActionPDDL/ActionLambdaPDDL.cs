using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SharpPDDL
{
    class ActionLambdaPDDL : ExpressionVisitor
    {
        private IReadOnlyList<ParameterExpression> _parameters;
        public readonly Delegate InstantFunct;
        public readonly LambdaExpression WholeFunc;

        public ActionLambdaPDDL(IReadOnlyList<Parametr> parameters, IReadOnlyList<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>>> preconditions, IReadOnlyList<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>> effects)
        {
        // Parameters below
            List<BinaryExpression> ChecksParam = new List<BinaryExpression>();
            List<ParameterExpression> TempParams = new List<ParameterExpression>();
            for (int i = 0; i != parameters.Count; i++)
            {
                ParameterExpression CurrentPar = Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix + i.ToString());
                TempParams.Add(CurrentPar);

                var key = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredFields.First(df => df.Name == "OriginalObjType");
                var ThObOryginalType = Expression.MakeMemberAccess(CurrentPar, key);
                var ConType = Expression.Constant(parameters[i].Type, typeof(Type));
                var ISCorrectType = Expression.Equal(ThObOryginalType, ConType);                                                            //TODO dziedziczenie z drzewka trzeba uwzglednic kiedyś
                ChecksParam.Add(ISCorrectType);
            }       
            _parameters = TempParams.AsReadOnly();

            BinaryExpression CheckAllParam = ChecksParam[0];
            for (int a = 1; a != ChecksParam.Count; a++)
                CheckAllParam = Expression.AndAlso(CheckAllParam, ChecksParam[a]);

        // Preconditions below
            List<BinaryExpression> ChecksPrecondition = new List<BinaryExpression>();
            foreach (var preconditionPDDL in preconditions)
            {
                var preconditionWithNewParam = (BinaryExpression)VisitLambda(preconditionPDDL);
                ChecksPrecondition.Add(preconditionWithNewParam);
            }

            BinaryExpression CheckAllPreco = ChecksPrecondition[0];
            for (int a = 1; a != ChecksParam.Count; a++)
                CheckAllPreco = Expression.AndAlso(CheckAllPreco, ChecksPrecondition[a]);

        // Effects below
            List<Expression>[] EffectsPais = new List<Expression>[parameters.Count];
            for (int i = 0; i != parameters.Count; i++)
            {
                EffectsPais[i] = new List<Expression>();
            }

            foreach (var effect in effects)
            {
                string ParamNo = effect.Parameters[0].Name.Remove(0, ExtensionMethods.LamdbaParamPrefix.Length);
                int ChangedParamNo = Int32.Parse(ParamNo);

                var result = VisitLambda(effect);
                EffectsPais[ChangedParamNo].Add(result);
            }

            NewExpression newDictionaryExpression = Expression.New(typeof(List<KeyValuePair<ushort, ValueType>>));
            Expression[] singleEffectsList = new Expression[parameters.Count];

            for (int a = 0; a!= parameters.Count; a++)
            {
                singleEffectsList[a] = Expression.ListInit(newDictionaryExpression, EffectsPais[a]);
            }

            var arrayOfEfects = Expression.NewArrayInit(typeof(List<KeyValuePair<ushort, ValueType>>), singleEffectsList);

        // Merge it all below
            var LackOfEffects = Expression.Constant(null, typeof(List<KeyValuePair<ushort, ValueType>>));
            var WholeParamBody = Expression.IfThenElse(CheckAllPreco, arrayOfEfects, LackOfEffects);
            var WholeFunctBody = Expression.IfThenElse(CheckAllParam, WholeParamBody, LackOfEffects);

            WholeFunc = Expression.Lambda(WholeFunctBody, _parameters);

            try
            {
                InstantFunct = WholeFunc.Compile();
            }
            catch
            {
                throw new Exception();
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Visit(node.Body);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameters.First(p => p.Name == node.Name);
        }
    }
}
