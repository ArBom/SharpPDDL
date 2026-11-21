using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Diagnostics;

namespace SharpPDDL
{
    class ActionLambdaPDDL : ExpressionVisitor
    {
        private List<ParameterExpression> _parameters;
        public readonly Delegate InstantFunct;
        public readonly Delegate InstantFunctSimplified;

        public ActionLambdaPDDL(IReadOnlyList<Parametr> parameters, IReadOnlyList<Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>>> preconditions, IReadOnlyList<Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>> effects)
        {
            // Parameters
            BinaryExpression CheckAllParam = BuildNewParameters(parameters);

            // Preconditions
            RebuildPreco(preconditions, out Expression CheckAllPreco, out Expression CheckAllPrecoSimplified);

            // Effects
            ListInitExpression ListOfEfects = RebuildEffect(parameters, effects);

            // Build Lambdas
            LambdaExpression WholeFuncLambda = WholeFunc(CheckAllParam, CheckAllPreco, ListOfEfects);
            LambdaExpression WholeFuncSimplifiedLambda = FBlockSimplified(CheckAllPrecoSimplified, ListOfEfects);

            try
            {
                InstantFunct = WholeFuncLambda.Compile();
                InstantFunctSimplified = WholeFuncSimplifiedLambda.Compile();
            }
            catch (Exception e)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C7"), e.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 45, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }

        private BinaryExpression BuildNewParameters(IReadOnlyList<Parametr> OldParameters)
        {
            List<BinaryExpression> ChecksParams = new List<BinaryExpression>();
            _parameters = new List<ParameterExpression>();

            for (int i = 0; i != OldParameters.Count; i++)
            {
                ParameterExpression CurrentPar = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix + i.ToString());
                _parameters.Add(CurrentPar);

                //Check if one need to check it
                if (OldParameters[i].CheckType is null)
                    continue;

                BinaryExpression VisitedCheckType = (BinaryExpression)Visit(OldParameters[i].CheckType);
                ChecksParams.Add(VisitedCheckType);
            }

            BinaryExpression CheckAllParam = null;
            foreach (BinaryExpression CheckParam in ChecksParams)
            {
                if (CheckAllParam is null)
                    CheckAllParam = CheckParam;
                else
                    CheckAllParam = Expression.AndAlso(CheckAllParam, CheckParam);
            }
            return CheckAllParam;
        }

        private void RebuildPreco(IReadOnlyList<Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>>> preconditions, out Expression CheckAllPreco, out Expression CheckAllPrecoSimplified)
        {
            CheckAllPreco = null;
            CheckAllPrecoSimplified = null;

            foreach (var preconditionPDDL in preconditions)
            {
                Expression preconditionWithNewParam = VisitLambda(preconditionPDDL);

                if (CheckAllPreco is null)
                    CheckAllPreco = preconditionWithNewParam;
                else
                    CheckAllPreco = Expression.AndAlso(CheckAllPreco, preconditionWithNewParam);

                if (preconditionPDDL.Parameters[1].Name.StartsWith(GloCla.EmptyName))
                    continue;

                if (CheckAllPrecoSimplified is null)
                    CheckAllPrecoSimplified = preconditionWithNewParam;
                else
                    CheckAllPrecoSimplified = Expression.AndAlso(CheckAllPrecoSimplified, preconditionWithNewParam);
            }
        }

        private ListInitExpression RebuildEffect(IReadOnlyList<Parametr> parameters, IReadOnlyList<Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>> effects)
        {
            List<Expression>[] EffectsPais = new List<Expression>[parameters.Count];
            for (int i = 0; i != parameters.Count; i++)
                EffectsPais[i] = new List<Expression>();

            foreach (var effect in effects)
            {
                string ParamNo = effect.Parameters[0].Name.Remove(0, GloCla.LamdbaParamPrefix.Length);
                int ChangedParamNo = Int32.Parse(ParamNo);

                var result = VisitLambda(effect);
                EffectsPais[ChangedParamNo].Add(result);
            }

            NewExpression newDictionaryExpression = Expression.New(typeof(List<KeyValuePair<ushort, ValueType>>));
            Expression[] singleEffectsList = new Expression[parameters.Count];

            for (int a = 0; a != parameters.Count; a++)
            {
                if (EffectsPais[a].Any())
                    singleEffectsList[a] = Expression.ListInit(newDictionaryExpression, EffectsPais[a]);
                else
                    singleEffectsList[a] = Expression.Constant(new List<KeyValuePair<ushort, ValueType>>());
            }

            NewExpression newListDictionaryExpression = Expression.New(typeof(List<List<KeyValuePair<ushort, ValueType>>>));
            ListInitExpression ListOfEfects = Expression.ListInit(newListDictionaryExpression, singleEffectsList);
            return ListOfEfects;
        }

        LambdaExpression WholeFunc(BinaryExpression CheckAllParam, Expression CheckAllPreco, ListInitExpression ListOfEfects)
        {
            ConstantExpression empty = Expression.Constant(null, typeof(List<List<KeyValuePair<ushort, ValueType>>>));
            LabelTarget retLabelTarget = Expression.Label(typeof(List<List<KeyValuePair<ushort, ValueType>>>), null);
            GotoExpression returnEmpty = Expression.Return(retLabelTarget, empty);
            GotoExpression returnEList = Expression.Return(retLabelTarget, ListOfEfects);

            Expression WholeParamBody;
            if (CheckAllPreco is null)
                WholeParamBody = returnEList;
            else
                WholeParamBody = Expression.IfThenElse(CheckAllPreco, returnEList, returnEmpty);

            Expression WholeFunctBody;
            if (CheckAllParam is null)
                WholeFunctBody = WholeParamBody;
            else
                WholeFunctBody = Expression.IfThenElse(CheckAllParam, WholeParamBody, returnEmpty);

            BlockExpression FBlock = Expression.Block(WholeFunctBody, Expression.Label(retLabelTarget, empty));

            return Expression.Lambda(FBlock, _parameters);
        }

        LambdaExpression FBlockSimplified(Expression CheckAllPrecoSimplified, ListInitExpression ListOfEfects)
        {
            ConstantExpression empty = Expression.Constant(null, typeof(List<List<KeyValuePair<ushort, ValueType>>>));
            LabelTarget retLabelTarget = Expression.Label(typeof(List<List<KeyValuePair<ushort, ValueType>>>), null);
            GotoExpression returnEmpty = Expression.Return(retLabelTarget, empty);
            GotoExpression returnEList = Expression.Return(retLabelTarget, ListOfEfects);

            Expression WholeParamBodySimplified;
            if (CheckAllPrecoSimplified is null)
                WholeParamBodySimplified = returnEList;
            else
                WholeParamBodySimplified = Expression.IfThenElse(CheckAllPrecoSimplified, returnEList, returnEmpty);

            BlockExpression FBlockSimplified = Expression.Block(WholeParamBodySimplified, Expression.Label(retLabelTarget, empty));

            return Expression.Lambda(FBlockSimplified, _parameters);
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
