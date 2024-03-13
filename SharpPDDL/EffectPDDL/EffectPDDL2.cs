using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    class EffectPDDL2<T1, T2> : EffectPDDL where T1 : class where T2 : class
    {
        new protected readonly string DestinationMemberName;
        readonly Expression SourceFunc = null;
        protected T1 t1;
        protected T2 t2;

        internal EffectPDDL2(string Name, ref T1 SourceObj, Expression<Func<T1, T2, ValueType>> SourceFunct, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationFunct) :
        base(Name, SourceObj.GetType(), SourceObj.GetHashCode(), DestinationObj.GetType(), DestinationObj.GetHashCode())
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunct);
            this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
            this.usedMembers2Class = SourceLambdaListerPDDL.used[1];
            this.SourceFunc = SourceFunct;
            this.DestinationMemberName = MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);
        }

        internal EffectPDDL2(string Name, ref T1 SourceObj, Expression<Func<T1, ValueType>> SourceFunct, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationFunct) : 
        base(Name, SourceObj.GetType(), SourceObj.GetHashCode(), DestinationObj.GetType(), DestinationObj.GetHashCode())
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunct);
            this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
            this.usedMembers2Class = new List<string>();
            this.SourceFunc = SourceFunct;
            this.DestinationMemberName = MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);
        }

        private string MutualPartOfConstructors(ref T1 SourceObj, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationFunct)
        {
            this.t1 = SourceObj;
            this.t2 = DestinationObj;

            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(DestinationFunct);
            string temp = DestLambdaListerPDDL.used[0][0];

            if (!usedMembers2Class.Exists(m => m == temp))
                usedMembers2Class.Add(temp);

            return temp;
        }

        internal override Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = { AllParamsOfAct1ClassPos.Value, AllParamsOfAct2ClassPos.Value };
            ushort Key = allTypes.First(t => t.Type == TypeOf1Class).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            EffectLambdaPDDL effectLambdaPDDL = new EffectLambdaPDDL(allTypes, ParamsIndexesInAction, Key);
            effectLambdaPDDL.Visit(SourceFunc);
            return effectLambdaPDDL.ModifiedFunct;
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
                    break;
                }
            }

            if (AllParamsOfAct1ClassPos is null)
                throw new Exception("There is no that param at list.");

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash2Class)
                    continue;

                if (t2.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct2ClassPos = index;
                    return;
                }
            }

            throw new Exception("There is no that param at list.");
        }
    }
}
