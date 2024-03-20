using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

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

        internal override Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            ushort Key = allTypes.First(t => t.Type == TypeOf1Class).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            CompleteClassPos(Parameters); //TODO uwzględnić AllParamsOfAct1ClassPos w func
            Expression<Func<ValueType>> toRet = () => newValue;
            int[] paramss = { AllParamsOfAct1ClassPos.Value };
            EffectLambdaPDDL effectLambdaPDDL = new EffectLambdaPDDL(allTypes, paramss, Key);
            _ = (Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>)effectLambdaPDDL.Visit(toRet);
            var modifed = (Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>)effectLambdaPDDL.ModifiedFunct;

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
