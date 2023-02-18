using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal abstract class EffectPDDL : ObjectPDDL
    {
        //albo ustawia stala, albo z iinej klasy, albo zewn

        public abstract Action<Parametr, Parametr> ExecutePDDP();

        public abstract Func<dynamic, dynamic, EventHandler> Execute();


        /*internal static EffectPDDL Instance<T1>(string Name, ref T1 obj1, ValueType value) where T1 : class //przypisanie stałej wartosci
        {
        }

        internal static EffectPDDL Instance<T1>(string Name, ref T1 obj1, ref ValueType value) where T1 : class //przypisanie wartosci z innej klasy
        {
        }

        internal static EffectPDDL Instance<T1, T2>(string Name, ref T1 obj1, ref T2 obj2, bool value) where T1 : class //przypisanie zewnętrzne
        {
        }*/

        protected EffectPDDL(string Name) : base(Name) { }
    }

    /*internal class EffectPDDL<T1> : EffectPDDL
    {
        new readonly internal Type TypeOf1Class;
        new readonly internal Int32 Hash1Class;
        readonly T1 t1;

        protected EffectPDDL(string Name, ref T1 obj1) : base(Name)
        {
            this.t1 = obj1;
            TypeOf1Class = obj1.GetType();
            Hash1Class = obj1.GetHashCode();
        }
    }

    internal class EffectPDDL<T1, T2> : EffectPDDL<T1>
    {
        readonly internal Type TypeOf2Class;
        readonly internal Int32 Hash2Class;
        readonly T2 t2;

        protected EffectPDDL(string Name, ref T1 obj1, ref T2 obj2) : base(Name, ref obj1)
        {
            this.t2 = obj2;
            TypeOf2Class = obj2.GetType();
            Hash2Class = obj2.GetHashCode();
        }
    }*/
}
