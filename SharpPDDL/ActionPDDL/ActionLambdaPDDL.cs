using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
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
                
                //checking if types equals
                var ConType = Expression.Constant(parameters[i].Type, typeof(Type));
                var keyOrygObjType = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObjType");
                var ThObOryginalType = Expression.MakeMemberAccess(CurrentPar, keyOrygObjType);
                var TypeEqals = Expression.Equal(ThObOryginalType, ConType);

                //checking if type is inherited
                var keyOrygObj = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObj");
                var OrygObj = Expression.MakeMemberAccess(CurrentPar, keyOrygObj);
                var ISCorrectType = Expression.TypeIs(OrygObj, parameters[i].Type);

                //connect upper...
                var opt = Expression.OrElse(TypeEqals, ISCorrectType);

                //...and add to list
                ChecksParam.Add(opt);
            }       
            _parameters = TempParams.AsReadOnly();

            BinaryExpression CheckAllParam = ChecksParam[0];

            if (ChecksParam.Count >= 2)
                for (int a = 1; a != ChecksParam.Count; a++)
                    CheckAllParam = Expression.AndAlso(CheckAllParam, ChecksParam[a]);

        // Preconditions below
            List<Expression> ChecksPrecondition = new List<Expression>();
            foreach (var preconditionPDDL in preconditions)
            {
                var preconditionWithNewParam = VisitLambda(preconditionPDDL);
                ChecksPrecondition.Add(preconditionWithNewParam);
            }

            Expression CheckAllPreco = ChecksPrecondition[0];
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
                if (EffectsPais[a].Count != 0)
                    singleEffectsList[a] = Expression.ListInit(newDictionaryExpression, EffectsPais[a]);
                else
                    singleEffectsList[a] = Expression.Constant(new List<KeyValuePair<ushort, ValueType>>());
            }

            NewExpression newListDictionaryExpression = Expression.New(typeof(List<List<KeyValuePair<ushort, ValueType>>>));
            ListInitExpression ListOfEfects = Expression.ListInit(newListDictionaryExpression, singleEffectsList);

            // Merge it all below
            //ConstantExpression empty = Expression.Constant(new List<List<KeyValuePair<ushort, ValueType>>>());
            ConstantExpression empty = Expression.Constant(null, typeof(List<List<KeyValuePair<ushort, ValueType>>>));
            LabelTarget retLabelTarget = Expression.Label(typeof(List<List<KeyValuePair<ushort, ValueType>>>), null);
            ConditionalExpression WholeParamBody = Expression.IfThenElse(CheckAllPreco, Expression.Return(retLabelTarget, ListOfEfects), Expression.Return(retLabelTarget, empty));
            ConditionalExpression WholeFunctBody = Expression.IfThenElse(CheckAllParam, WholeParamBody, Expression.Return(retLabelTarget, empty));
            BlockExpression FBlock = Expression.Block(WholeFunctBody, Expression.Label(retLabelTarget, empty));

            WholeFunc = Expression.Lambda(FBlock, _parameters);

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
