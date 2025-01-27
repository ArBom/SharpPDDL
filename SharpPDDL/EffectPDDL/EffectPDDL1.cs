using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.ObjectModel;

namespace SharpPDDL
{
    class EffectPDDL1<T1c, T1p> : EffectPDDL
        where T1p : class 
        where T1c : class, T1p
    {
        internal readonly Expression<Func<T1p, ValueType>> Destination;
        protected readonly T1c t1;

        internal EffectPDDL1(string Name, ref T1c obj1, Expression<Func<T1p, ValueType>> Destination, ValueType newValue) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.SourceFunc = Expression.Constant(newValue, newValue.GetType());
            this.Destination = Destination;
            this.t1 = obj1;
        }

        override internal void CompleteActinParams(IList<Parametr> Parameters)
        {
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(Destination);
            this.usedMembers1Class = DestLambdaListerPDDL.used[0];
            this.DestinationMemberName = usedMembers1Class[0];

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != t1.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(t1))
                    continue;

                int ToTagIndex = parametr.values.FindIndex(v => v.Name == DestinationMemberName);
                parametr.values[ToTagIndex].IsInUse_EffectOut = true;

                parametr.UsedInEffect = true;
                break;
            }
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(t1, 1, Parameters) == false)
                throw new Exception("There is no that param at list.");
        }

        internal override Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);

            ushort FuncOutKey = allTypes.First(t => t.Type == TypeOf1Class).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            Expression FuncOutKeyExpression = Expression.Constant(FuncOutKey, typeof(ushort));

            Collection<ParameterExpression> parameterExpressions = new Collection<ParameterExpression>
            {
                Expression.Parameter(typeof(PossibleStateThumbnailObject), GloCla.LamdbaParamPrefix + AllParamsOfAct1ClassPos.Value),
                Expression.Parameter(typeof(PossibleStateThumbnailObject), "empty")
            };

            var ResultType = typeof(KeyValuePair<ushort, ValueType>).GetConstructors()[0];
            Expression[] param = { FuncOutKeyExpression, Expression.Convert(SourceFunc, typeof(ValueType)) };
            NewExpression expectedTypeExpression = Expression.New(ResultType, param);
            Expression ModifiedFunct = Expression.Lambda<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>(expectedTypeExpression, parameterExpressions);

            return (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>)ModifiedFunct;
        }
    }
}
