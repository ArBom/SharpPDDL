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
        // Parameters below
            List<BinaryExpression> ChecksParam = new List<BinaryExpression>();
            _parameters = new List<ParameterExpression>();

            for (int i = 0; i != parameters.Count; i++)
            {
                ParameterExpression CurrentPar = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix + i.ToString());
                _parameters.Add(CurrentPar);

                //Check if one need to check it
                if (parameters[i].CheckType is null)
                    continue;

                BinaryExpression VisitedCheckType = (BinaryExpression)Visit(parameters[i].CheckType);
                ChecksParam.Add(VisitedCheckType);
            }       

            BinaryExpression CheckAllParam = null;
            if (ChecksParam.Any())
            {
                CheckAllParam = ChecksParam[0];

                if (ChecksParam.Count >= 2)
                    for (int a = 1; a != ChecksParam.Count; a++)
                        CheckAllParam = Expression.AndAlso(CheckAllParam, ChecksParam[a]);
            }

        // Preconditions below
            List<Expression> ChecksPrecondition = new List<Expression>();
            List<Expression> ChecksPreconditionSimplified = new List<Expression>();
            foreach (var preconditionPDDL in preconditions)
            {
                var preconditionWithNewParam = VisitLambda(preconditionPDDL);
                ChecksPrecondition.Add(preconditionWithNewParam);

                if (preconditionPDDL.Parameters[1].Name.StartsWith(GloCla.EmptyName))
                    continue;

                ChecksPreconditionSimplified.Add(preconditionWithNewParam);

            }

            Expression CheckAllPreco;
            if (ChecksPrecondition.Any())
            {
                CheckAllPreco = ChecksPrecondition[0];
                for (int a = 1; a != ChecksPrecondition.Count; a++)
                    CheckAllPreco = Expression.AndAlso(CheckAllPreco, ChecksPrecondition[a]);
            }
            else
                CheckAllPreco = Expression.Constant(true);

            Expression CheckAllPrecoSimplified;
            if (ChecksPreconditionSimplified.Any())
            {
                CheckAllPrecoSimplified = ChecksPreconditionSimplified[0];
                for (int a = 1; a != ChecksPreconditionSimplified.Count; a++)
                    CheckAllPrecoSimplified = Expression.AndAlso(CheckAllPrecoSimplified, ChecksPreconditionSimplified[a]);
            }
            else
                CheckAllPrecoSimplified = Expression.Constant(true);

            // Effects below
            List<Expression>[] EffectsPais = new List<Expression>[parameters.Count];
            for (int i = 0; i != parameters.Count; i++)
            {
                EffectsPais[i] = new List<Expression>();
            }

            foreach (var effect in effects)
            {
                string ParamNo = effect.Parameters[0].Name.Remove(0, GloCla.LamdbaParamPrefix.Length);
                int ChangedParamNo = Int32.Parse(ParamNo);

                var result = VisitLambda(effect);
                EffectsPais[ChangedParamNo].Add(result);
            }

            NewExpression newDictionaryExpression = Expression.New(typeof(List<KeyValuePair<ushort, ValueType>>));
            Expression[] singleEffectsList = new Expression[parameters.Count];

            for (int a = 0; a!= parameters.Count; a++)
            {
                if (EffectsPais[a].Any())
                    singleEffectsList[a] = Expression.ListInit(newDictionaryExpression, EffectsPais[a]);
                else
                    singleEffectsList[a] = Expression.Constant(new List<KeyValuePair<ushort, ValueType>>());
            }

            NewExpression newListDictionaryExpression = Expression.New(typeof(List<List<KeyValuePair<ushort, ValueType>>>));
            ListInitExpression ListOfEfects = Expression.ListInit(newListDictionaryExpression, singleEffectsList);

            // Merge it all below
            ConstantExpression empty = Expression.Constant(null, typeof(List<List<KeyValuePair<ushort, ValueType>>>));
            LabelTarget retLabelTarget = Expression.Label(typeof(List<List<KeyValuePair<ushort, ValueType>>>), null);
            ConditionalExpression WholeParamBody = Expression.IfThenElse(CheckAllPreco, Expression.Return(retLabelTarget, ListOfEfects), Expression.Return(retLabelTarget, empty));
            ConditionalExpression WholeParamBodySimplified = Expression.IfThenElse(CheckAllPrecoSimplified, Expression.Return(retLabelTarget, ListOfEfects), Expression.Return(retLabelTarget, empty));

            ConditionalExpression WholeFunctBody;
            if (CheckAllParam is null)
                WholeFunctBody = WholeParamBody;
            else
                WholeFunctBody = Expression.IfThenElse(CheckAllParam, WholeParamBody, Expression.Return(retLabelTarget, empty));

            BlockExpression FBlock = Expression.Block(WholeFunctBody, Expression.Label(retLabelTarget, empty));
            BlockExpression FBlockSimplified = Expression.Block(WholeParamBodySimplified, Expression.Label(retLabelTarget, empty));

            LambdaExpression WholeFunc = Expression.Lambda(FBlock, _parameters);
            LambdaExpression WholeFuncSimplified = Expression.Lambda(FBlockSimplified, _parameters);

            try
            {
                InstantFunct = WholeFunc.Compile();
                InstantFunctSimplified = WholeFuncSimplified.Compile();
            }
            catch (Exception e)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C7"), e.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 45, ExceptionMess);
                throw new Exception(ExceptionMess);
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
