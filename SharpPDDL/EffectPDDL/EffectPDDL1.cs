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
        new readonly internal string DestinationMemberName;
        protected T1 t1;

        internal EffectPDDL1(string Name, ValueType newValue, ref T1 obj1, Expression<Func<T1, ValueType>> Destination) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.newValue = newValue;
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(Destination);
            this.usedMembers1Class = DestLambdaListerPDDL.used[0];
            this.DestinationMemberName = usedMembers1Class[0];

            if (newValue.GetType() != typeof(T1).GetMember(DestinationMemberName).GetType())
                throw new Exception("You cannot assign another type value.");

            this.t1 = obj1;
        }

        internal override Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes)
        {
            ushort Key = allTypes.First(t => t.Type == TypeOf1Class).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            KeyValuePair<ushort, ValueType> FuncOut = new KeyValuePair<ushort, ValueType>(Key, newValue);
            Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>> func = ((o1, o2) => FuncOut);
            return func;
        }
    }
}
