using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace SharpPDDL
{
    internal static class ActionSententiaLamdba
    {
        private static Expression CheckAllParam(IReadOnlyList<Parametr> parameters, out IReadOnlyList<ParameterExpression> _parameters)
        {
            Expression ToRet = null;

            //new parameters used in new lambda
            List<ParameterExpression> _params = new List<ParameterExpression>();

            for (int i = 0; i != parameters.Count; i++)
            {
                //creating new parameters of output lambda
                ParameterExpression CurrentPar = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix + i.ToString());
                _params.Add(CurrentPar);

                //checking if types equals
                var ConType = Expression.Constant(parameters[i].Type, typeof(Type));
                var keyOrygObjType = typeof(ThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObjType");
                var ThObOryginalType = Expression.MakeMemberAccess(CurrentPar, keyOrygObjType);
                var TypeEqals = Expression.Equal(ThObOryginalType, ConType);

                //checking if type is inherited
                var keyOrygObj = typeof(ThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObj");
                var OrygObj = Expression.MakeMemberAccess(CurrentPar, keyOrygObj);
                var ISCorrectType = Expression.TypeIs(OrygObj, parameters[i].Type);

                //connect upper...
                var opt = Expression.OrElse(TypeEqals, ISCorrectType);

                //checking type of this and other parameters' part
                if (ToRet is null)
                    ToRet = opt;
                else
                    ToRet = Expression.AndAlso(ToRet, opt);
            }

            //do not change it later
            _parameters = _params.AsReadOnly();
            return ToRet;
        }

        private static List<string>[] ListUsedMembersName(Expression actionSententiaIJ)
        {
            //Take names of all members used in this action sententia expression
            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            memberofLambdaListerPDDL.Visit(actionSententiaIJ);
            List<string>[] used = memberofLambdaListerPDDL.used;

            //it have to use only one argument
            if (used.Length > 1)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E11"), actionSententiaIJ.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 53, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            //this argument have to take only one member of parametr
            if (used.Length == 1)
            {
                if (used[0].Count > 1)
                {
                    string ExceptionMess = String.Format(GloCla.ResMan.GetString("E12"), actionSententiaIJ.ToString());
                    GloCla.Tracer?.TraceEvent(TraceEventType.Error, 54, ExceptionMess);
                    throw new Exception(ExceptionMess);
                }
            }

            return used;
        }

        private static void EnqueueSententiaPart (IEnumerable<Value> VoT, ParameterExpression CparameterExpression, string Key, Type paramType, string usedIJK, ref Expression textC)
        {
            Expression OldTxt = Expression.Constant(Key, typeof(string));

            //Getting the string.Replace(string,string) method
            MethodInfo ReplaceMethod = typeof(string).GetMethod("Replace", new Type[] { typeof(string), typeof(string) });

            if (!VoT.Any()) //One needs to read from oryginal object
            {
                Expression OrygObjExpression = Expression.Property(CparameterExpression, "OriginalObj");
                Expression Converted = Expression.Convert(OrygObjExpression, paramType);
                Expression PropertyOrFieldAccess = Expression.PropertyOrField(Converted, usedIJK);
                Expression PropertyOrFieldAccessString = Expression.Call(PropertyOrFieldAccess, PropertyOrFieldAccess.Type.GetMethod("ToString", new Type[] { }));
                textC = Expression.Call(textC, ReplaceMethod, new Expression[] { OldTxt, PropertyOrFieldAccessString });
            }
            else //Value is used in other part of library 
            {
                Expression[] argument = new[] { Expression.Constant(VoT.First().ValueOfIndexesKey) };

                //Property of ThumbnailObject.this[uint key]
                PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item");
                IndexExpression IndexAccessExpr = Expression.MakeIndex(CparameterExpression, TO_indekser, argument);
                Expression IndexAccessExprString = Expression.Call(IndexAccessExpr, IndexAccessExpr.Type.GetMethod("ToString", new Type[] { }));
                textC = Expression.Call(textC, ReplaceMethod, new Expression[] { OldTxt, IndexAccessExprString });
            }
        }

        private static Expression WorkOne((int, string, Expression[]) actionSententiaI, IReadOnlyList<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> parameters, IReadOnlyList<ParameterExpression> _parameters)
        {
            int ParamNo = actionSententiaI.Item1;
            Type paramType = parameters[ParamNo].Type;
            Expression textC = Expression.Constant(actionSententiaI.Item2, typeof(string));
            SingleTypeOfDomein singleTypeOfDomein;

            //try to take model of param. type
            try
            {
                singleTypeOfDomein = allTypes.First(T => T.Type == paramType);
            }
            catch
            {
                //todo;
                return textC;
            }

            //work with every agr. in this part of sentenia
            for (int j = 0; j != actionSententiaI.Item3.Length; j++)
            {
                string Key = "{" + j + "}";
                
                //ignore if there is no "link" in text
                if (!actionSententiaI.Item2.Contains(Key))
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 55, GloCla.ResMan.GetString("W7"), Key, actionSententiaI.Item2);
                    continue;
                }

                //arg. name
                string usedIJK = ListUsedMembersName(actionSententiaI.Item3[j])[0][0];
                var VoT = singleTypeOfDomein.CumulativeValues.Where(v => v.Name == usedIJK);

                //add next part to whole
                ParameterExpression CparameterExpression = _parameters[ParamNo];
                EnqueueSententiaPart(VoT, CparameterExpression, Key, paramType, usedIJK, ref textC);
            }

            return textC;
        }

        internal static Delegate BuildActionSententiaLamdba(IReadOnlyList<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> parameters, IReadOnlyList<(int, string, Expression[])> actionSententia)
        {
            //part of parameter checking and generate new parameters of func
            Expression CheckAllParams = CheckAllParam(parameters, out IReadOnlyList<ParameterExpression> _parameters);

            Expression[] texts = new Expression[actionSententia.Count];
            actionSententia.OrderBy(AS => AS.Item1);

            //work with every single part of sententia
            for (int i = 0; i != actionSententia.Count; i++)
            {
                //non-argument part
                if (actionSententia[i].Item3 is null)
                {
                    texts[i] = Expression.Constant(actionSententia[i].Item2, typeof(string));
                    continue;
                }

                //work with every arg. in this part
                texts[i] = WorkOne(actionSententia[i], allTypes, parameters, _parameters);

                //check extra, incorrect arg.
                if (!(GloCla.Tracer is null))
                {
                    string ExtraKey = "{" + actionSententia[i].Item3.Length + "}";
                    if (actionSententia[i].Item2.Contains(ExtraKey))
                        GloCla.Tracer.TraceEvent(TraceEventType.Warning, 56, GloCla.ResMan.GetString("W8"), ExtraKey, actionSententia[i].Item2);
                }
            }

            //merge this into lambda expr.
            LabelTarget retLabelTarget = Expression.Label(typeof(string), null);
            Expression emptyString = Expression.Constant(String.Empty, typeof(string));
            MethodInfo ConcatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(String[]) });
            Expression textsExpre = Expression.NewArrayInit(typeof(string), texts);
            Expression ConcatMethodCall = Expression.Call(ConcatMethod, textsExpre);
            Expression ArgumentExceptionExpr = Expression.Throw(Expression.Constant(new ArgumentException()));
            ConditionalExpression WholeFunctBody = Expression.IfThenElse(CheckAllParams, Expression.Return(retLabelTarget, ConcatMethodCall), ArgumentExceptionExpr);
            BlockExpression FBlock = Expression.Block(WholeFunctBody, Expression.Label(retLabelTarget, emptyString));
            LambdaExpression WholeFunc = Expression.Lambda(FBlock, _parameters);
            Delegate InstantFunct;

            //build it...
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

            //...and use next
            return InstantFunct;
        }
    }
}
