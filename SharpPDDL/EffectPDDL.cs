using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal abstract class EffectPDDL : ObjectPDDL
    {
        //albo ustawia stala, albo z iinej klasy, albo zewn

        internal static EffectPDDL Instance<T1>(string Name, ref T1 obj1, ValueType TrigerValue = default) where T1 : class //przypisanie stałej wartosci
        {
            PropertyInfo propInfo = obj1.GetType().GetProperty(Name);
            bool? Prop = propInfo?.CanWrite;

            FieldInfo fieldInfo = obj1.GetType().GetField(Name);
            bool? Field = fieldInfo?.IsPublic;

            if ((Prop | Field) == true)
            {
                Type TypeOfValue = TrigerValue.GetType();

                Type TypeOfPredicate = null;

                if (Prop == true)
                {
                    if (propInfo.GetType() != TypeOfValue) { } //todo


                }

                if (Field == true)
                {
                    if (fieldInfo.GetType() != TypeOfValue) { } //todo


                }

                if (!TypeOfPredicate.IsValueType)
                {
                    throw new Exception(""); //nie właściwy typ
                }
            }

            if ((Prop & Field) == false)
                throw new Exception(""); //dany typ istnieje, ale nie moze byc odczytany

            if (TrigerValue is bool)
            {

            }

            throw new Exception(""); //niby zewnątrzny, ale oczekiwana wartość nie jest bool
        }

        internal static EffectPDDL Instance<T1>(string Name, ref T1 obj1, ref ValueType TrigerValue) where T1 : class //przypisanie wartosci z innej klasy
        {
        }

        internal static EffectPDDL Instance<T1, T2>(string Name, ref T1 obj1, ref T2 obj2, bool value) where T1 : class //przypisanie zewnętrzne
        {
        }

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
        protected EffectPDDL(string Name, ref T1 obj1) : base(Name, ref obj1) { }

        internal override (int, int?) FindIndexesOnList(List<Parametr> listOfParams) //TODO
        {
            throw new NotImplementedException();
        }
    }
}
