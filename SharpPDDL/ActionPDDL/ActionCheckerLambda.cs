using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

namespace SharpPDDL
{
    class ActionCheckerLambda : ExpressionVisitor
    {
        static private Predicate<PossibleStateThumbnailObject> LambdaOfEq(object value)
            => PSTO => PSTO.OriginalObj.Equals(value);

        internal ActionCheckerLambda(String ActionName, List<EffectPDDL> Effects, List<SingleTypeOfDomein> allTypes)
        {
            ParameterExpression Input_parameter = Expression.Parameter(typeof(CrisscrossChildrenCon), "input");
            LabelTarget retLabel = Expression.Label(typeof(bool));

            ConstantExpression ActionNameExp = Expression.Constant(ActionName, typeof(string));

            ConstantExpression TrueExp = Expression.Constant(true, typeof(bool));
            ConstantExpression FalseExp = Expression.Constant(false, typeof(bool));

            Expression[] EffectsArray = new Expression[Effects.Count + 1];

            ParameterExpression ef = Expression.Parameter(typeof(bool), "IsOK");
            LabelTarget Correct = Expression.Label(typeof(bool));

            FieldInfo TracerF = typeof(GloCla).GetField("Tracer",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.Static);

            FieldInfo ResManF = typeof(GloCla).GetField("ResMan",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.Static);

            FieldInfo ChildF = typeof(CrisscrossChildrenCon).GetField("Child",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.Static);

            FieldInfo ActionArgOrygF = typeof(CrisscrossChildrenCon).GetField("ActionArgOryg",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.Static);

            FieldInfo ContentF = typeof(Crisscross).GetField("Content");

            FieldInfo ThumbnailObjectsF = typeof(PossibleState).GetField("ThumbnailObjects",
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            FieldInfo ChildList = typeof(PossibleStateThumbnailObject).GetField("child",
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            MethodInfo ItemOfPSTO = typeof(PossibleStateThumbnailObject).GetMethod("get_Item", new Type[] { typeof(UInt16) });

            MethodInfo VTTostring = typeof(ValueType).GetMethod("ToString", new Type[] { });


            MethodInfo TraceEventMethod = typeof(TraceSource).GetMethod("TraceEvent", new Type[]{typeof(TraceEventType), typeof(int), typeof(string), typeof(object[])});
            MethodInfo GetString = typeof(ResourceManager).GetMethod("GetString", new Type[] { typeof(string) });
            ConstantExpression NullExp = Expression.Constant(null);

            MemberExpression TracerExp = Expression.Field(null, TracerF);
            BinaryExpression TracerNotNull = Expression.NotEqual(TracerExp, NullExp);
            MemberExpression ResManExp = Expression.Field(null, ResManF);
            MemberExpression ChildExp = Expression.Field(Input_parameter, ChildF);
            MemberExpression ContentExp = Expression.Field(ChildExp, ContentF);
            MemberExpression ThumbnailObjectsExp = Expression.Field(ContentExp, ThumbnailObjectsF);

            Expression ActionArgOrygExp = Expression.Field(Input_parameter, ActionArgOrygF);

            //Wrong assignation of value used in Precondition
            ConstantExpression ErrorExp = Expression.Constant(TraceEventType.Error, typeof(TraceEventType));
            ConstantExpression EventId130 = Expression.Constant(130, typeof(int)); 
            ConstantExpression E34 = Expression.Constant("E34", typeof(string));
            MethodCallExpression stringGetterE34 = Expression.Call(ResManExp, GetString, E34);

            ConstantExpression WarningExp = Expression.Constant(TraceEventType.Warning, typeof(TraceEventType));

            //Wrong assignation of value used in Effect
            ConstantExpression EventId131 = Expression.Constant(131, typeof(int));
            ConstantExpression W12 = Expression.Constant("W12", typeof(string));
            MethodCallExpression stringGetterW12 = Expression.Call(ResManExp, GetString, W12);

            //Wrong assignation of value used in Cost
            ConstantExpression EventId133 = Expression.Constant(133, typeof(int));
            ConstantExpression W13 = Expression.Constant("W13", typeof(string));
            MethodCallExpression stringGetterW13 = Expression.Call(ResManExp, GetString, W13);

            //Wrong assignation of value not used as input
            ConstantExpression InfoExp = Expression.Constant(TraceEventType.Information, typeof(TraceEventType));
            ConstantExpression EventId132 = Expression.Constant(132, typeof(int));
            ConstantExpression I8 = Expression.Constant("I8", typeof(string));
            MethodCallExpression stringGetterI8 = Expression.Call(ResManExp, GetString, I8);

            MethodInfo Find = typeof(List<PossibleStateThumbnailObject>).GetMethod("Find", new Type[] { typeof(Predicate<PossibleStateThumbnailObject>) });
            MethodInfo CheckForEqualsMethodInfo = this.GetType().GetMethod("LambdaOfEq", BindingFlags.NonPublic | BindingFlags.Static);

            var SortedEffects = Effects.Select(e => (Eff: e, Val: allTypes.First(t => t.Type == e.TypeOf1Class).CumulativeValues.First(v => v.Name == e.DestinationMemberName))).
                OrderBy(k => k.Val.IsInUse_PreconditionIn).
                ThenBy(k => k.Val.IsInUse_EffectIn).
                ThenBy(k => k.Val.IsInUse_ActionCostIn).
                ToList();

            for (int EfC = 0; EfC != Effects.Count; EfC++)
            {
                //Effect name
                ConstantExpression EffectNameExp = Expression.Constant(Effects[EfC].Name, typeof(string));

                //Do którego nr param jest zapisywana wartość 
                int DestParamNo = Effects[EfC].AllParamsOfAct1ClassPos.Value;
                ConstantExpression DestParamNoExp = Expression.Constant(DestParamNo, typeof(int));

                BinaryExpression arrayAccessExpr = Expression.ArrayIndex(ActionArgOrygExp, DestParamNoExp);
                Expression ConvertedArrayAccessExpr = Expression.Convert(arrayAccessExpr, Effects[EfC].TypeOf1Class);

                //Gdzie zapisywana jest wartość value    
                Value DestMember = allTypes.First(t => t.Type == Effects[EfC].TypeOf1Class).CumulativeValues.First(v => v.Name == Effects[EfC].DestinationMemberName);

                //odczyt oryginału z inputu 
                MemberTypes memberType = DestMember.IsField ? MemberTypes.Field : MemberTypes.Property;
                MemberInfo memberInfo = Effects[EfC].TypeOf1Class.GetMember(Effects[EfC].DestinationMemberName, memberType, BindingFlags.Instance | BindingFlags.Public).First();

                MemberExpression OrygInput = Expression.MakeMemberAccess(ConvertedArrayAccessExpr, memberInfo);

                //numer na tablicy gdzie zapisywana jest wartość       
                ConstantExpression FuncOutKeyExp = Expression.Constant(DestMember.ValueOfIndexesKey, typeof(ushort));

                ParameterExpression parameter = Expression.Parameter(typeof(PossibleStateThumbnailObject), "invoice");

                MemberExpression memberExpressionList = Expression.MakeMemberAccess(parameter, ChildList);


                MethodCallExpression CheckForEqualsExp = Expression.Call(null, CheckForEqualsMethodInfo, arrayAccessExpr);

                MethodCallExpression FieldCondition = Expression.Call(memberExpressionList, Find, CheckForEqualsExp);
                MethodCallExpression toread = Expression.Call(FieldCondition, ItemOfPSTO, FuncOutKeyExp);
                UnaryExpression ToReadC = Expression.Convert(toread, DestMember.Type);

                MethodInfo ToString = DestMember.Type.GetMethod("ToString", new Type[] { });

                BinaryExpression Test = Expression.NotEqual(ToReadC, OrygInput);

                ConstantExpression DestName = Expression.Constant(Effects[EfC].DestinationMemberName, typeof(string));
                MethodCallExpression CurrV = Expression.Call(OrygInput, ToString);
                MethodCallExpression ExpeV = Expression.Call(toread, VTTostring);
                Expression NamesArray = Expression.NewArrayInit(typeof(string), new Expression[]{ DestName, CurrV, ExpeV, EffectNameExp, ActionNameExp });

                ConditionalExpression CallIfCan = null;
                if (DestMember.IsInUse_PreconditionIn)
                {
                    MethodCallExpression callId130 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { ErrorExp, EventId130, stringGetterE34, NamesArray });
                    CallIfCan = Expression.IfThen(TracerNotNull, callId130);
                }
                else if (DestMember.IsInUse_EffectIn)
                {
                    MethodCallExpression callId131 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { WarningExp, EventId131, stringGetterW12, NamesArray });
                    CallIfCan = Expression.IfThen(TracerNotNull, callId131);
                }
                else if (DestMember.IsInUse_ActionCostIn)
                {
                    MethodCallExpression callId133 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { WarningExp, EventId133, stringGetterW13, NamesArray });
                    CallIfCan = Expression.IfThen(TracerNotNull, callId133);
                }
                else
                {
                    MethodCallExpression callId132 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { InfoExp, EventId132, stringGetterI8, NamesArray });
                    CallIfCan = Expression.IfThen(TracerNotNull, callId132);
                }

                BlockExpression IfNotEqual = Expression.Block(CallIfCan);

                Expression expression = Expression.IfThen(Test, IfNotEqual);

                int AO = 1500100900;
            }
        }
    }
}
