using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    abstract internal class EffectPDDL : ObjectPDDL
    {
        internal abstract Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters);
        readonly internal Expression DestinationMember;

        protected Expression _SourceFunc;
        public virtual Expression SourceFunc
        {
            get { return _SourceFunc; }
            protected set
            {
                if (_SourceFunc is null)
                    _SourceFunc = value;
            }
        }

        internal string DestinationMemberName;

        protected EffectPDDL(string Name, Expression DestinationMember, object[] ElementsInOnbjectPDDL) 
            : base(Name, ElementsInOnbjectPDDL)
        {
            this.DestinationMember = DestinationMember;
        }

        internal static EffectPDDL Instance<T1c, T1p>(string Name, List<Parametr> Parameters, List<EffectPDDL> Effects, ref T1c destinationObj, Expression<Func<T1p, ValueType>> destinationMember, ValueType newValue_Static) //przypisanie wartosci ze stałej
            where T1p : class
            where T1c : class, T1p
        {
            CheckExistEffectName(Effects, Name);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref destinationObj);
            EffectPDDL1<T1c, T1p> NewEffectPDDL = new EffectPDDL1<T1c, T1p>(Name, ref destinationObj, destinationMember, newValue_Static);
            Effects?.Add(NewEffectPDDL);
            return NewEffectPDDL;
        }

        internal static EffectPDDL Instance<T1c, T1p, T2c, T2p>(string Name, List<Parametr> Parameters, List<EffectPDDL> Effects, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> destinationMember, ref T2c sourceObj1, Expression<Func<T2p, ValueType>> Source)
            where T1p : class
            where T2p : class
            where T1c : class, T1p
            where T2c : class, T2p
        {
            CheckExistEffectName(Effects, Name);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref sourceObj1);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref DestinationObj);
            EffectPDDL2<T1c, T1p, T2c, T2p> NewEffectPDDL = new EffectPDDL2<T1c, T1p, T2c, T2p>(Name, ref DestinationObj, destinationMember, ref sourceObj1, Source);
            Effects?.Add(NewEffectPDDL);
            return NewEffectPDDL;
        }

        internal static EffectPDDL Instance<T1c, T1p, T2c, T2p>(string Name, List<Parametr> Parameters, List<EffectPDDL> Effects, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> destinationMember, ref T2c sourceObj1, Expression<Func<T1p, T2p, ValueType>> Source)
            where T1p : class
            where T2p : class
            where T1c : class, T1p            
            where T2c : class, T2p
        {
            CheckExistEffectName(Effects, Name);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref sourceObj1);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref DestinationObj);
            EffectPDDL2<T1c, T1p, T2c, T2p> NewEffectPDDL = new EffectPDDL2<T1c, T1p, T2c, T2p>(Name, ref DestinationObj, destinationMember, ref sourceObj1, Source);
            Effects?.Add(NewEffectPDDL);
            return NewEffectPDDL;
        }

        protected static void CheckExistEffectName(List<EffectPDDL> Effects, string Name)
        {
            if (Effects is null)
                return;

            if (String.IsNullOrEmpty(Name))
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E14"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 78, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (Effects.Exists(effect => effect.Name == Name))
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E15"), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 79, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }
    }
}
