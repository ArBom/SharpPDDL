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
        internal EffectPDDL2(string Name, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> Destination, ref T2c SourceObj, Expression<Func<T1p, T2p, ValueType>> SourceFunct) 
            : base(Name, Destination, new object[2] { DestinationObj, SourceObj })
            => this.SourceFunc = SourceFunct;

        internal EffectPDDL2(string Name, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> Destination, ref T2c SourceObj, Expression<Func<T2p, ValueType>> SourceFunct) 
            : base(Name, Destination, new object[2] { DestinationObj, SourceObj })
            => this.SourceFunc = SourceFunct;

        private string MutualPartOfConstructors(Expression DestinationFunct)
        {
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(DestinationFunct);
            string temp = DestLambdaListerPDDL.used[0][0];

            if (!Elements[1].usedMembersClass.Exists(m => m == temp))
                Elements[1].usedMembersClass.Add(temp);

            return temp;
        }

        override internal void CompleteActinParams(IList<Parametr> Parameters)
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunc);

            if (SourceFunc is Expression<Func<T1p, T2p, ValueType>>)
            {
                Elements[0].usedMembersClass = SourceLambdaListerPDDL.used[0];
                Elements[1].usedMembersClass = SourceLambdaListerPDDL.used[1];
            }
            else if (SourceFunc is Expression<Func<T2p, ValueType>>)
            {
                Elements[0].usedMembersClass = new List<string>();
                Elements[1].usedMembersClass = SourceLambdaListerPDDL.used[0];
            }

            this.DestinationMemberName = MutualPartOfConstructors(DestinationMember);

            //Tag destination parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != Elements[0].HashClass)
                    continue;

                if (!parametr.Oryginal.Equals(Elements[0].Object))
                    continue;

                int ToTagIndex = parametr.values.FindIndex(v => v.Name == DestinationMemberName);
                parametr.values[ToTagIndex].IsInUse_EffectOut = true;

                foreach (string valueName in Elements[0].usedMembersClass)
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
                if (parametr.HashCode != Elements[1].HashClass)
                    continue;

                if (!parametr.Oryginal.Equals(Elements[1].Object))
                    continue;

                foreach (string valueName in Elements[1].usedMembersClass)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse_EffectIn = true;
                }

                parametr.UsedInEffect = true;
                break;
            }
        }

        internal override Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = { Elements[0].AllParamsOfActClassPos.Value, Elements[1].AllParamsOfActClassPos.Value };
            ushort Key = allTypes.First(t => t.Type == Elements[0].TypeOfClass).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            EffectLambdaPDDL effectLambdaPDDL = new EffectLambdaPDDL(allTypes, ParamsIndexesInAction, Key);
            effectLambdaPDDL.Visit(SourceFunc);
            return effectLambdaPDDL.ModifiedFunct;
        }
    }
}
