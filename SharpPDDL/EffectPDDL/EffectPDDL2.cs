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
        protected T1c t1;
        protected T2c t2;

        Expression<Func<T1p, ValueType>> DestinationFunct;

        internal EffectPDDL2(string Name, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> DestinationFunct, ref T2c SourceObj, Expression<Func<T1p, T2p, ValueType>> SourceFunct) :
        base(Name, DestinationObj.GetType(), DestinationObj.GetHashCode(), SourceObj.GetType(), SourceObj.GetHashCode())
        {
            this.t1 = DestinationObj;
            this.t2 = SourceObj;

            this.SourceFunc = SourceFunct;
            this.DestinationFunct = DestinationFunct;
        }

        internal EffectPDDL2(string Name, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> DestinationFunct, ref T2c SourceObj, Expression<Func<T2p, ValueType>> SourceFunct) : 
        base(Name, DestinationObj.GetType(), DestinationObj.GetHashCode(), SourceObj.GetType(), SourceObj.GetHashCode())
        {
            this.t1 = DestinationObj;
            this.t2 = SourceObj;

            this.SourceFunc = SourceFunct;
            this.DestinationFunct = DestinationFunct;
        }

        private string MutualPartOfConstructors(Expression<Func<T1p, ValueType>> DestinationFunct)
        {
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(DestinationFunct);
            string temp = DestLambdaListerPDDL.used[0][0];

            if (!usedMembers2Class.Exists(m => m == temp))
                usedMembers2Class.Add(temp);

            return temp;
        }

        override internal void CompleteActinParams(IList<Parametr> Parameters)
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunc);

            if (SourceFunc is Expression<Func<T1p, T2p, ValueType>>)
            {
                this.usedMembers1Class = SourceLambdaListerPDDL.used[0];
                this.usedMembers2Class = SourceLambdaListerPDDL.used[1];
            }
            else if (SourceFunc is Expression<Func<T2p, ValueType>>)
            {
                this.usedMembers1Class = new List<string>();
                this.usedMembers2Class = SourceLambdaListerPDDL.used[0];
            }

            this.DestinationMemberName = MutualPartOfConstructors(DestinationFunct);

            //Tag destination parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != t1.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(t1))
                    continue;

                int ToTagIndex = parametr.values.FindIndex(v => v.Name == DestinationMemberName);
                parametr.values[ToTagIndex].IsInUse_EffectOut = true;

                foreach (string valueName in usedMembers1Class)
                {
                    ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse_EffectIn = true;
                }

                parametr.UsedInEffect = true;
                break;
            }

            //Tag source parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != t2.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(t2))
                    continue;

                foreach (string valueName in usedMembers2Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse_EffectIn = true;
                }

                parametr.UsedInEffect = true;
                break;
            }
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

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(t1, 1, Parameters) == false)
                throw new Exception("There is no that param at list.");

            if (TXIndex(t2, 2, Parameters) == false)
                throw new Exception("There is no that param at list.");
        }
    }
}
