using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class EffectPDDL2<T1c, T1p, T2c, T2p> : EffectPDDL
        where T1p : class
        where T2p : class
        where T1c : class, T1p
        where T2c : class, T2p
    {
        //readonly new Expression SourceFunc = null;
        protected T1c t1;
        protected T2c t2;

        internal EffectPDDL2(string Name, ref T1c SourceObj, Expression<Func<T1p, T2p, ValueType>> SourceFunct, ref T2c DestinationObj, Expression<Func<T2p, ValueType>> DestinationFunct) :
        base(Name, SourceObj.GetType(), SourceObj.GetHashCode(), DestinationObj.GetType(), DestinationObj.GetHashCode())
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunct);
            this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
            this.usedMembers2Class = SourceLambdaListerPDDL.used[1];
            this.SourceFunc = SourceFunct;
            this.DestinationMemberName = MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);
        }

        internal EffectPDDL2(string Name, ref T1c SourceObj, Expression<Func<T1c, ValueType>> SourceFunct, ref T2c DestinationObj, Expression<Func<T2c, ValueType>> DestinationFunct) :
        base(Name, SourceObj.GetType(), SourceObj.GetHashCode(), DestinationObj.GetType(), DestinationObj.GetHashCode())
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunct);
            this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
            this.usedMembers2Class = new List<string>();
            this.SourceFunc = SourceFunct;
            this.DestinationMemberName = MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);
        }

        internal EffectPDDL2(string Name, ref T1c SourceObj, Expression<Func<T1p, ValueType>> SourceFunct, ref T2c DestinationObj, Expression<Func<T2p, ValueType>> DestinationFunct) : 
        base(Name, SourceObj.GetType(), SourceObj.GetHashCode(), DestinationObj.GetType(), DestinationObj.GetHashCode())
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunct);
            this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
            this.usedMembers2Class = new List<string>();
            this.SourceFunc = SourceFunct;
            this.DestinationMemberName = MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);
        }

        private string MutualPartOfConstructors(ref T1c SourceObj, ref T2c DestinationObj, Expression<Func<T2c, ValueType>> DestinationFunct) => 
            MutualPartOfConstructors(ref SourceObj, ref DestinationObj, DestinationFunct);

        private string MutualPartOfConstructors(ref T1c SourceObj, ref T2c DestinationObj, Expression<Func<T2p, ValueType>> DestinationFunct)
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

        internal override Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
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
                if (listOfParams[index].HashCode != Hash2Class)
                    continue;

                if (t2.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct1ClassPos = index;
                    break;
                }
            }

            if (AllParamsOfAct1ClassPos is null)
                throw new Exception("There is no that param at list.");

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash1Class)
                    continue;

                if (t1.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct2ClassPos = index;
                    return;
                }
            }

            throw new Exception("There is no that param at list.");
        }
    }
}
