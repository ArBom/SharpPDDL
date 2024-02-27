using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    class EffectPDDL2<T1, T2> : EffectPDDL where T1 : class where T2 : class
    {
        new protected string DestinationMemberName;
        readonly Expression<Func<T1, ValueType>> func1 = null;
        readonly Expression<Func<T1,T2, ValueType>> func2 = null;
        protected T1 t1;
        protected T2 t2;

        internal EffectPDDL2(string Name, ref T1 SourceObj, Expression<Func<T1, T2, ValueType>> SourceFunct, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationFunct) :
        base(Name, SourceObj.GetType(), SourceObj.GetHashCode(), DestinationObj.GetType(), DestinationObj.GetHashCode())
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunct);
            this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
            this.usedMembers2Class = SourceLambdaListerPDDL.used[1];
            func2 = SourceFunct;
            MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);
        }

        internal EffectPDDL2(string Name, ref T1 SourceObj, Expression<Func<T1, ValueType>> SourceFunct, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationFunct) : 
        base(Name, SourceObj.GetType(), SourceObj.GetHashCode(), DestinationObj.GetType(), DestinationObj.GetHashCode())
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunct);
            this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
            this.usedMembers2Class = new List<string>();
            func1 = SourceFunct;
            MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);
        }

        private void MutualPartOfConstructors(ref T1 SourceObj, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationFunct)
        {
            this.t1 = SourceObj;
            this.t2 = DestinationObj;

            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(DestinationFunct);
            string temp = DestLambdaListerPDDL.used[0][0];
            usedMembers2Class.Add(temp);
            this.DestinationMemberName = usedMembers2Class[0];
        }
    }
}
