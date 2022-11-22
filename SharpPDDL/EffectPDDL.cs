using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal abstract class EffectPDDL : ObjectPDDL
    {
        //albo ustawia stala, albo z iinej klasy, albo zewn

        //public abstract Func<Parametr, Parametr, List<object>, bool?> ExecutePDDP { get; }

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

    internal class EffectPDDL<T1> : EffectPDDL
    {
        new readonly internal Type TypeOf1Class;
        new readonly internal Int32 Hash1Class;
        readonly T1 t1;
        
        protected int IndexOf1OnList(List<Parametr> listOfParams)
        {
            for (int listPos = 0; listPos != listOfParams.Count; listPos++)
            {
                if (listOfParams[listPos].HashCode != this.Hash1Class)
                    continue;

                if (!object.ReferenceEquals(listOfParams[listPos], this.t1))
                    continue;

                return listPos;
            }

            throw new Exception(); //Brak na liście
        }

        protected EffectPDDL(string Name, ref T1 obj1) : base(Name)
        {
            this.t1 = obj1;
            TypeOf1Class = obj1.GetType();
            Hash1Class = obj1.GetHashCode();
        }

        internal override (int, int?) FindIndexesOnList(List<Parametr> listOfParams)
        {
            return (IndexOf1OnList(listOfParams), null);
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

        protected int IndexOf2OnList(List<Parametr> listOfParams)
        {
            for (int listPos = 0; listPos != listOfParams.Count; listPos++)
            {
                if (listOfParams[listPos].HashCode != this.Hash2Class)
                    continue;

                if (!object.ReferenceEquals(listOfParams[listPos], this.t2))
                    continue;

                return listPos;
            }

            throw new Exception(); //Brak na liście
        }

        internal override (int, int?) FindIndexesOnList(List<Parametr> listOfParams)
        {
            return (IndexOf1OnList(listOfParams), IndexOf2OnList(listOfParams));
        }
    }
}
