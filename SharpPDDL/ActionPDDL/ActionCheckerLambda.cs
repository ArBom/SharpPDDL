using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

namespace SharpPDDL
{
    internal static class ActionChecker
    {
        static private Predicate<ThumbnailObject> LambdaOfEq(object value)
            => PSTO => PSTO.OriginalObj.Equals(value);

        internal static Delegate ActionCheckerDel(String ActionName, List<EffectPDDL> Effects, List<SingleTypeOfDomein> allTypes)
        {
            ParameterExpression Input_parameter = Expression.Parameter(typeof(CrisscrossChildrenCon), "input");
            LabelTarget retLabel = Expression.Label(typeof(bool));

            ConstantExpression ActionNameExp = Expression.Constant(ActionName, typeof(string));

            ConstantExpression TrueExp = Expression.Constant(true, typeof(bool));
            ConstantExpression FalseExp = Expression.Constant(false, typeof(bool));

            Expression[] EffectsArray = new Expression[Effects.Count + 4];

            ParameterExpression InteriorParamIsOK = Expression.Parameter(typeof(bool), "IsOK");
            ParameterExpression InteriorParamObjs = Expression.Parameter(typeof(object[]), "Objs");

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

            MethodInfo ActionArgOrygM = typeof(CrisscrossChildrenCon).GetMethod("ActionArgOryg",
                BindingFlags.NonPublic |
                BindingFlags.Instance);                

            FieldInfo ContentF = typeof(Crisscross).GetField("Content");

            FieldInfo ThumbnailObjectsF = typeof(PossibleState).GetField("ThumbnailObjects",
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            FieldInfo ChildList = typeof(ThumbnailObject).GetField("child",
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            MethodInfo ItemOfPSTO = typeof(ThumbnailObject).GetMethod("get_Item", new Type[] { typeof(UInt16) });

            MethodInfo VTTostring = typeof(ValueType).GetMethod("ToString", new Type[] { });

            MethodInfo TraceEventMethod = typeof(TraceSource).GetMethod("TraceEvent", new Type[]{typeof(TraceEventType), typeof(int), typeof(string), typeof(object[])});
            MethodInfo GetString = typeof(ResourceManager).GetMethod("GetString", new Type[] { typeof(string) });
            ConstantExpression NullExp = Expression.Constant(null);

            //GloCla.Tracer
            MemberExpression TracerExp = Expression.Field(null, TracerF);

            //(GloCla.Tracer != null)
            BinaryExpression TracerNotNull = Expression.NotEqual(TracerExp, NullExp);

            //GloCla.ResMan
            MemberExpression ResManExp = Expression.Field(null, ResManF);

            //input.Child
            MemberExpression ChildExp = Expression.Field(Input_parameter, ChildF);

            //input.Child.Content
            MemberExpression ContentExp = Expression.Field(ChildExp, ContentF);

            //input.Child.Content.ThumbnailObjects
            MemberExpression ThumbnailObjectsExp = Expression.Field(ContentExp, ThumbnailObjectsF);

            //input.ActionArgOryg()
            Expression ActionArgOrygMExp = Expression.Call(Input_parameter, ActionArgOrygM);

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

            MethodInfo Find = typeof(List<ThumbnailObject>).GetMethod("Find", new Type[] { typeof(Predicate<ThumbnailObject>) });
            MethodInfo CheckForEqualsMethodInfo = typeof(ActionChecker).GetMethod("LambdaOfEq", BindingFlags.NonPublic | BindingFlags.Static);

            ///TOTO
            ParameterExpression parameter = Expression.Parameter(typeof(ThumbnailObject), "invoice");
            //MemberExpression memberExpressionList = Expression.MakeMemberAccess(parameter, ChildList);

            EffectsArray[0] = Expression.Assign(InteriorParamIsOK, TrueExp);
            EffectsArray[1] = Expression.Assign(InteriorParamObjs, ActionArgOrygMExp);
            BinaryExpression AssignFalse = Expression.Assign(InteriorParamIsOK, FalseExp);

            for (int EfC = 0; EfC != Effects.Count; EfC++)
            {
                //Effect nad którym pracuje bieżąca iteracja pętli
                EffectPDDL CurrentEffectPDDL = Effects[EfC];

                //Effect name
                ConstantExpression EffectNameExp = Expression.Constant(Effects[EfC].Name, typeof(string));

                //Do którego nr param jest zapisywana wartość 
                int DestParamNo = CurrentEffectPDDL.Elements[0].AllParamsOfActClassPos.Value;
                ConstantExpression DestParamNoExp = Expression.Constant(DestParamNo, typeof(int));

                BinaryExpression arrayAccessExpr = Expression.ArrayIndex(InteriorParamObjs, DestParamNoExp);
                Expression ConvertedArrayAccessExpr = Expression.Convert(arrayAccessExpr, CurrentEffectPDDL.Elements[0].TypeOfClass);

                //Gdzie zapisywana jest wartość value    
                Value DestMember = allTypes.First(t => t.Type == CurrentEffectPDDL.Elements[0].TypeOfClass).CumulativeValues.First(v => v.Name == CurrentEffectPDDL.DestinationMemberName);

                //odczyt oryginału z inputu 
                MemberTypes memberType = DestMember.IsField ? MemberTypes.Field : MemberTypes.Property;
                MemberInfo memberInfo = CurrentEffectPDDL.Elements[0].TypeOfClass.GetMember(CurrentEffectPDDL.DestinationMemberName, memberType, BindingFlags.Instance | BindingFlags.Public).First();

                MemberExpression OrygInput = Expression.MakeMemberAccess(ConvertedArrayAccessExpr, memberInfo);

                //numer na tablicy gdzie zapisywana jest wartość       
                ConstantExpression FuncOutKeyExp = Expression.Constant(DestMember.ValueOfIndexesKey, typeof(ushort));

                MethodCallExpression CheckForEqualsExp = Expression.Call(null, CheckForEqualsMethodInfo, arrayAccessExpr);

                //input.Child.Content.ThumbnailObject.Find()
                MethodCallExpression FieldCondition = Expression.Call(ThumbnailObjectsExp, Find, CheckForEqualsExp);
                MethodCallExpression toread = Expression.Call(FieldCondition, ItemOfPSTO, FuncOutKeyExp);
                UnaryExpression ToReadC = Expression.Convert(toread, DestMember.Type);

                MethodInfo ToString = DestMember.Type.GetMethod("ToString", new Type[] { });

                BinaryExpression Test = Expression.NotEqual(ToReadC, OrygInput);

                //Name of destination variable
                ConstantExpression DestName = Expression.Constant(CurrentEffectPDDL.DestinationMemberName, typeof(string));

                //Current (after assignation) value of destination variable
                MethodCallExpression CurrV = Expression.Call(OrygInput, ToString);

                //expected value of destination variable
                MethodCallExpression ExpeV = Expression.Call(toread, VTTostring);

                Expression NamesArray = Expression.NewArrayInit(typeof(string), new Expression[]{ DestName, CurrV, ExpeV, EffectNameExp, ActionNameExp });

                Expression IfNotEqual = null;
                if (DestMember.IsInUse_PreconditionIn)
                {
                    MethodCallExpression callId130 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { ErrorExp, EventId130, stringGetterE34, NamesArray });
                    IfNotEqual = Expression.Block(Expression.IfThen(TracerNotNull, callId130), AssignFalse);
                }
                else if (DestMember.IsInUse_EffectIn)
                {
                    MethodCallExpression callId131 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { WarningExp, EventId131, stringGetterW12, NamesArray });
                    IfNotEqual = Expression.Block(Expression.IfThen(TracerNotNull, callId131), AssignFalse);
                }
                else if (DestMember.IsInUse_ActionCostIn)
                {
                    MethodCallExpression callId133 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { WarningExp, EventId133, stringGetterW13, NamesArray });
                    IfNotEqual = Expression.IfThen(TracerNotNull, callId133);
                }
                else
                {
                    MethodCallExpression callId132 = Expression.Call(TracerExp, TraceEventMethod, new Expression[] { InfoExp, EventId132, stringGetterI8, NamesArray });
                    IfNotEqual = Expression.IfThen(TracerNotNull, callId132);
                }

                Expression expression = Expression.IfThen(Test, IfNotEqual);
                EffectsArray[EfC + 2] = expression;
            }

            EffectsArray[EffectsArray.Length - 2] = Expression.Return(retLabel, InteriorParamIsOK);
            EffectsArray[EffectsArray.Length - 1] = Expression.Label(retLabel, FalseExp);

            BlockExpression CheckingBlock = Expression.Block(new ParameterExpression[]{ parameter, InteriorParamIsOK, InteriorParamObjs }, EffectsArray);

            LambdaExpression WholeLambda = Expression.Lambda(CheckingBlock, Input_parameter);
            Delegate DelRes;
            try
            {
                DelRes = WholeLambda.Compile();
            }
            catch (Exception e)
            {
                string m = e.ToString();
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 36, GloCla.ResMan.GetString("E35"), ActionName);
                DelRes = Expression.Lambda(Expression.Empty(), Input_parameter).Compile();
            }

            return DelRes;
        }
    }
}