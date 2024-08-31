using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Reflection;

namespace SharpPDDL
{
    class EffectPDDL1<T1c, T1p> : EffectPDDL
        where T1p : class 
        where T1c : class, T1p
    {
        internal readonly ValueType newValue;
        internal readonly Expression<Func<T1p, ValueType>> Destination;
        protected T1c t1;

        internal EffectPDDL1(string Name, ValueType newValue, ref T1c obj1, Expression<Func<T1p, ValueType>> Destination) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.newValue = newValue;
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            this.Destination = Destination;
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

            Collection<ParameterExpression> parameterExpressions = new Collection<ParameterExpression>
            {
                Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix + AllParamsOfAct1ClassPos.Value),
                Expression.Parameter(typeof(PossibleStateThumbnailObject), "empty")
            };

            var ResultType = typeof(KeyValuePair<ushort, ValueType>).GetConstructors()[0];
            Expression[] param = { FuncOutKeyExpression, Expression.Convert(Expression.Constant(newValue, newValue.GetType()), typeof(ValueType)) };
            NewExpression expectedTypeExpression = Expression.New(ResultType, param);
            Expression ModifiedFunct = Expression.Lambda<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>(expectedTypeExpression, parameterExpressions);

            return (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>)ModifiedFunct;
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
