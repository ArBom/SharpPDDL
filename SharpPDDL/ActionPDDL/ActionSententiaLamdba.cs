using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace SharpPDDL
{
    class ActionSententiaLamdba : ExpressionVisitor
    {
        public readonly Delegate InstantFunct;
        public readonly LambdaExpression WholeFunc;

        public ActionSententiaLamdba(List<SingleTypeOfDomein> allTypes, List<Parametr> parameters, List<(int, string, Expression[])> actionSententia)
        {
            List<ParameterExpression> _parameters = new List<ParameterExpression>();
            Expression CheckAllParam = null;

            //Getting the string.Replace(string,string) method
            MethodInfo ReplaceMethod = typeof(string).GetMethod("Replace", new Type[] { typeof(string), typeof(string) });

            for (int i = 0; i != parameters.Count; i++)
            {
                //creating parameters of lambda
                ParameterExpression CurrentPar = Expression.Parameter(typeof(PossibleStateThumbnailObject), GloCla.LamdbaParamPrefix + i.ToString());
                _parameters.Add(CurrentPar);

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

                //checking type of this and other parameters' part
                if (CheckAllParam is null)
                    CheckAllParam = opt;
                else
                    CheckAllParam = Expression.AndAlso(CheckAllParam, opt);
            }

            Expression[] texts = new Expression[actionSententia.Count];
            actionSententia.OrderBy(AS => AS.Item1);
            for (int i = 0; i != actionSententia.Count; i++)
            {
                int ParamNo = actionSententia[i].Item1;
                Type paramType = parameters[ParamNo].Type;
                SingleTypeOfDomein singleTypeOfDomein = allTypes.First(T => T.Type == paramType);
                Expression textC = Expression.Constant(actionSententia[i].Item2, typeof(string));

                for (int j = 0; j != actionSententia[i].Item3.Length; j++)
                {
                    MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
                    memberofLambdaListerPDDL.Visit(actionSententia[i].Item3[j]);
                    List<string>[] used = memberofLambdaListerPDDL.used;

                    if (used.Length > 1)
                    {
                        string ExceptionMess = String.Format(GloCla.ResMan.GetString("E11"), actionSententia[i].Item3[j].ToString());
                        GloCla.Tracer?.TraceEvent(TraceEventType.Error, 53, ExceptionMess);
                        throw new Exception(ExceptionMess);
                    }

                    if (used.Length == 1)
                        if (used[0].Count > 1)
                        {
                            string ExceptionMess = String.Format(GloCla.ResMan.GetString("E12"), actionSententia[i].Item3[j].ToString());
                            GloCla.Tracer?.TraceEvent(TraceEventType.Error, 54, ExceptionMess);
                            throw new Exception(ExceptionMess);
                        }

                    string Key = "{" + j + "}";

                    if (!actionSententia[i].Item2.Contains(Key))
                    {
                        GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 55, GloCla.ResMan.GetString("W7"), Key, actionSententia[i].Item2);
                        continue;
                    }

                    Expression OldTxt = Expression.Constant(Key, typeof(string));
                    var VoT = singleTypeOfDomein.CumulativeValues.Where(v => v.Name == used[0][0]).ToList();

                    if (!VoT.Any())
                    {
                        Expression OrygObjExpression = Expression.Property(_parameters[ParamNo], "OriginalObj");
                        Expression Converted = Expression.Convert(OrygObjExpression, paramType);
                        Expression PropertyOrFieldAccess = Expression.PropertyOrField(Converted, used[0][0]);
                        Expression PropertyOrFieldAccessString = Expression.Call(PropertyOrFieldAccess, PropertyOrFieldAccess.Type.GetMethod("ToString", new Type[] { }));
                        textC = Expression.Call(textC, ReplaceMethod, new Expression[] { OldTxt, PropertyOrFieldAccessString });
                    }
                    else
                    {
                        Expression[] argument = new[] { Expression.Constant(VoT[0].ValueOfIndexesKey)};

                        //Property of ThumbnailObject.this[uint key]
                        PropertyInfo TO_indekser = typeof(PossibleStateThumbnailObject).GetProperty("Item");
                        IndexExpression IndexAccessExpr = Expression.MakeIndex(_parameters[ParamNo], TO_indekser, argument);
                        Expression IndexAccessExprString = Expression.Call(IndexAccessExpr, IndexAccessExpr.Type.GetMethod("ToString", new Type[] { }));
                        textC = Expression.Call(textC, ReplaceMethod, new Expression[] { OldTxt, IndexAccessExprString });
                    }

                    texts[i] = textC;
                }

                if (!(GloCla.Tracer is null))
                {
                    string ExtraKey = "{" + (actionSententia[i].Item3.Length) + "}";
                    if (actionSententia[i].Item2.Contains(ExtraKey))
                        GloCla.Tracer.TraceEvent(TraceEventType.Warning, 56, GloCla.ResMan.GetString("W8"), ExtraKey, actionSententia[i].Item2);
                }
            }

            LabelTarget retLabelTarget = Expression.Label(typeof(string), null);
            Expression emptyString = Expression.Constant(String.Empty, typeof(string));
            MethodInfo ConcatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(String[]) });
            Expression textsExpre = Expression.NewArrayInit(typeof(string), texts);
            Expression ConcatMethodCall = Expression.Call(ConcatMethod, textsExpre);
            Expression ArgumentExceptionExpr = Expression.Throw(Expression.Constant(new ArgumentException()));
            ConditionalExpression WholeFunctBody = Expression.IfThenElse(CheckAllParam, Expression.Return(retLabelTarget, ConcatMethodCall), ArgumentExceptionExpr);
            BlockExpression FBlock = Expression.Block(WholeFunctBody, Expression.Label(retLabelTarget, emptyString));
            WholeFunc = Expression.Lambda(FBlock, _parameters);

            try
            {
                InstantFunct = WholeFunc.Compile();
            }
            catch (Exception e)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C8"), e.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 57, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }
    }
}
