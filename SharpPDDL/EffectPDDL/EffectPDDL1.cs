using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    class EffectPDDL1<T1> : EffectPDDL where T1 : class
    {
        Expression newValue;
        new readonly protected string DestinationMemberName;
        protected T1 t1;

        internal EffectPDDL1(string Name, ValueType newValue, ref T1 obj1, Expression<Func<T1, ValueType>> Destination) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.newValue = Expression.Constant(newValue, newValue.GetType());
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(Destination);
            this.usedMembers1Class = DestLambdaListerPDDL.used[0];
            this.DestinationMemberName = usedMembers1Class[0];
            this.t1 = obj1;
        }
    }
}
