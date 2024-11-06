using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    public abstract class EffectPDDL : ObjectPDDL
    {
        internal abstract Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters);
        
        private Expression _SourceFunc;
        internal protected Expression SourceFunc
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

        public void UseAsExecution()
        {
            UsingAsExecution = true;
        }

        internal EffectPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null) : base(Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class)
        {
            UsingAsExecution = false;
        }

        internal static EffectPDDL Instance<T1>(string Name, ValueType newValue_Static, ref T1 destinationObj, Expression<Func<T1, ValueType>> destinationMember) where T1 : class //przypisanie wartosci ze stałej
        {
            return new EffectPDDL1<T1, T1>(Name, newValue_Static, ref destinationObj, destinationMember);
        }

        internal static EffectPDDL Instance<T1c, T1p, T2c, T2p>(string Name, ref T1c sourceObj1, Expression<Func<T1p, ValueType>> Source, ref T2c DestinationObj, Expression<Func<T2p, ValueType>> destinationMember)
            where T1p : class
            where T1c : class, T1p
            where T2p : class
            where T2c : class, T2p
        {
            return new EffectPDDL2<T1c, T1p, T2c, T2p>(Name, ref sourceObj1, Source, ref DestinationObj, destinationMember);
        }

        internal static EffectPDDL Instance<T1c, T1p, T2c, T2p>(string Name, ref T1c sourceObj1, Expression<Func<T1p, T2p, ValueType>> Source, ref T2c DestinationObj, Expression<Func<T2p, ValueType>> destinationMember)
            where T1p : class
            where T1c : class, T1p
            where T2p : class
            where T2c : class, T2p
        {
            return new EffectPDDL2<T1c, T1p, T2c, T2p>(Name, ref sourceObj1, Source, ref DestinationObj, destinationMember);
        }
    }
}
