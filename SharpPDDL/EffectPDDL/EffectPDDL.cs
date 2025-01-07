using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    abstract internal class EffectPDDL : ObjectPDDL
    {
        internal abstract Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters);

        internal Expression _SourceFunc;
        internal protected virtual Expression SourceFunc
        {
            get { return _SourceFunc; }
            protected set
            {
                if (_SourceFunc is null)
                    _SourceFunc = value;
            }
        }

        internal string DestinationMemberName;
        internal bool UsingAsExecution;

        public void UseAsExecution() => this.UsingAsExecution = true;

        protected EffectPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null) : base(Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class)
        {
            UsingAsExecution = false;
        }

        internal static EffectPDDL Instance<T1>(string Name, ref T1 destinationObj, Expression<Func<T1, ValueType>> destinationMember, ValueType newValue_Static) //przypisanie wartosci ze stałej
            where T1 : class
        {
            return new EffectPDDL1<T1, T1>(Name, ref destinationObj, destinationMember, newValue_Static);
        }

        internal static EffectPDDL Instance<T1c, T1p, T2c, T2p>(string Name, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> destinationMember, ref T2c sourceObj1, Expression<Func<T2p, ValueType>> Source)
            where T1p : class
            where T1c : class, T1p
            where T2p : class
            where T2c : class, T2p
        {
            return new EffectPDDL2<T1c, T1p, T2c, T2p>(Name, ref DestinationObj, destinationMember, ref sourceObj1, Source);
        }

        internal static EffectPDDL Instance<T1c, T1p, T2c, T2p>(string Name, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> destinationMember, ref T2c sourceObj1, Expression<Func<T1p, T2p, ValueType>> Source)
            where T1p : class
            where T1c : class, T1p
            where T2p : class
            where T2c : class, T2p
        {
            return new EffectPDDL2<T1c, T1p, T2c, T2p>(Name, ref DestinationObj, destinationMember, ref sourceObj1, Source);
        }
    }
}
