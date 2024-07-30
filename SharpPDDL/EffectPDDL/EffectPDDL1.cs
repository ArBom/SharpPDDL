using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;

namespace SharpPDDL
{
    class EffectPDDL1<T1> : EffectPDDL where T1 : class
    {
        readonly ValueType newValue;
        protected T1 t1;

        internal EffectPDDL1(string Name, ValueType newValue, ref T1 obj1, Expression<Func<T1, ValueType>> Destination) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.newValue = newValue;
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(Destination);
            this.usedMembers1Class = DestLambdaListerPDDL.used[0];
            this.DestinationMemberName = usedMembers1Class[0];
            this.t1 = obj1;
        }

        internal override Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);

            ushort FuncOutKey = allTypes.First(t => t.Type == TypeOf1Class).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            Expression FuncOutKeyExpression = Expression.Constant(FuncOutKey, typeof(ushort));

            Collection<ParameterExpression> parameterExpressions = new Collection<ParameterExpression>();
            parameterExpressions.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix + AllParamsOfAct1ClassPos.Value));
            parameterExpressions.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), "empty"));
            //_parameters = new ReadOnlyCollection<ParameterExpression>(parameterExpressions);

            var ResultType = typeof(KeyValuePair<ushort, ValueType>).GetConstructors()[0];
            Expression[] param = { FuncOutKeyExpression, Expression.Convert(Expression.Constant(newValue, newValue.GetType()), typeof(ValueType)) };
            NewExpression expectedTypeExpression = Expression.New(ResultType, param);
            Expression ModifiedFunct = Expression.Lambda<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>(expectedTypeExpression, parameterExpressions);

            return (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>)ModifiedFunct;

            ushort Key = allTypes.First(t => t.Type == TypeOf1Class).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            CompleteClassPos(Parameters); //TODO uwzględnić AllParamsOfAct1ClassPos w func
            //Expression<Func<ValueType>> toRet = () => newValue;
            Expression toRet = Expression.Constant(newValue, newValue.GetType());
            //Expression e = Expression.
            int[] paramss = { AllParamsOfAct1ClassPos.Value };
            EffectLambdaPDDL effectLambdaPDDL = new EffectLambdaPDDL(allTypes, paramss, Key);
            _ = (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>)effectLambdaPDDL.Visit(toRet);
            //_ = effectLambdaPDDL.Visit(toRet);
            var modifed = (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>)effectLambdaPDDL.ModifiedFunct;

            return modifed;
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> listOfParams)
        {
            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash1Class)
                    continue;

                if (t1.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct1ClassPos = index;
                    return;
                }
            }

            throw new Exception("There is no that param at list.");
        }
    }
}
