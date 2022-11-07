using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    abstract internal class PreconditionPDDL : ObjectPDDL
    {
        public abstract (Func<Parametr, Parametr, List<object>, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams);

        ////
        internal static PreconditionPDDL Instance<T1>(string Name, ref T1 obj1, ValueType TrigerValue = default) where T1 : class
        {
            PropertyInfo propInfo = obj1.GetType().GetProperty(Name);
            bool? Prop = propInfo?.CanRead;

            FieldInfo fieldInfo = obj1.GetType().GetField(Name);
            bool? Field = fieldInfo?.IsPublic;

            if ((Prop | Field) == true)
            {
                Type TypeOfPredicate = null;

                if (Prop == true)
                {
                    return new PreconditionInternalPropertyPDDL<T1>(Name, ref obj1, TrigerValue);
                }

                if (Field == true)
                {
                    return new PreconditionInternalFieldPDDL<T1>(Name, ref obj1, TrigerValue);
                }

                if (!TypeOfPredicate.IsValueType)
                {
                    throw new Exception(""); //nie właściwy typ
                }
            }

            if ((Prop & Field) == false)
                throw new Exception(""); //dany typ istnieje, ale nie moze byc odczytany

            if (TrigerValue is bool)
                return new PreconditionExtPDDL<T1>(Name, ref obj1, (bool)TrigerValue);

            throw new Exception(""); //niby zewnątrzny, ale oczekiwana wartość nie jest bool
        }

        internal static PreconditionPDDL Instance<T1,T2>(string Name, ref T1 obj1, ref T2 obj2) where T1 : class
        {
            if (obj2.GetType().IsClass)
                return new PreconditionExtPDDL<T1, T2>(Name, ref obj1, ref obj2);

            if (obj2.GetType().IsValueType)
                return PreconditionConstPDDL<T1, T2>.Instance(Name, ref obj1, ref obj2);

              throw new Exception(""); //niewłaściwy typ obj2
        }

        /*internal static PreconditionPDDL Instance<T1, T2>(string Name, ref T1 obj1, ref T2 obj2, Func<T1,T2,bool> func) where T1 : class where T2 : class
        {
            return new PreconditionFuncPDDL<T1,T2>(Name, ref obj1, ref obj2, func);
        }*/
        ////

        protected PreconditionPDDL(string Name) : base(Name) {}
    }

    internal abstract class PreconditionPDDL<T1> : PreconditionPDDL
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

        override internal (int, int?) FindIndexesOnList(List<Parametr> listOfParams)
        {
            return (IndexOf1OnList(listOfParams), null);
        }

        protected PreconditionPDDL(string Name, ref T1 obj1) : base(Name)
        {
            this.t1 = obj1;
            TypeOf1Class = obj1.GetType();
            Hash1Class = obj1.GetHashCode();
        }
    }

    internal abstract class PreconditionPDDL<T1, T2> : PreconditionPDDL<T1>
    {
        internal readonly Type TypeOf2Class;
        internal readonly Int32 Hash2Class;

        protected int IndexOf2OnList(List<Parametr> listOfParams)
        {
            for (int listPos = 0; listPos != listOfParams.Count; listPos++)
            {
                if (listOfParams[listPos].HashCode != this.Hash2Class)
                    continue;

                if (!object.ReferenceEquals(listOfParams[listPos], this.Hash2Class))
                    continue;

                return listPos;
            }

            throw new Exception(); //Brak na liście
        }

        override internal (int, int?) FindIndexesOnList(List<Parametr> listOfParams)
        {
            return (IndexOf1OnList(listOfParams), IndexOf2OnList(listOfParams));
        }

        protected PreconditionPDDL(string Name, ref T1 obj1, ref T2 obj2) : base(Name, ref obj1)
        {
            TypeOf2Class = obj2.GetType();
            Hash2Class = obj2.GetHashCode();
        }
    }

    internal abstract class PreconditionFuncPDDL<T1, T2> : PreconditionPDDL<T1, T2>
    { 


        internal PreconditionFuncPDDL(string Name, ref T1 obj1, ref T2 obj2, Func<T1,T2,bool> func = null) : base( Name, ref obj1, ref obj2) { }

        public override (Func<Parametr, Parametr, List<object>, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            throw new NotImplementedException();
        }
    }

    internal class PreconditionExtPDDL<T1, T2> : PreconditionPDDL<T1, T2>
    {

        public override (Func<Parametr, Parametr, List<object>, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            throw new NotImplementedException();
        }

        internal PreconditionExtPDDL(string Name, ref T1 obj1, ref T2 obj2) : base(Name, ref obj1, ref obj2)
        { }
    }

    internal abstract class PreconditionConstPDDL<T1, T2> : PreconditionFuncPDDL<T1, T2>
    {
        internal static PreconditionPDDL Instance(string Name, ref T1 obj1, ref T2 obj2)
        {
            PropertyInfo propInfo = obj1.GetType().GetProperty(Name);
            bool? Prop = propInfo?.CanRead;
            if (Prop == true)
                return new PreconditionConstPropertyPDDL<T1, T2>(Name, ref obj1, ref obj2);

            FieldInfo fieldInfo = obj1.GetType().GetField(Name);
            bool? Field = fieldInfo?.IsPublic;
            if (Field == true)
                return new PreconditionConstFieldPDDL<T1, T2>(Name, ref obj1, ref obj2);

            throw new Exception(); //Ani field ani property
        }

        internal PreconditionConstPDDL(string name, ref T1 obj1, ref T2 obj2) : base(name, ref obj1, ref obj2) { }

        private (string, Type) InfoOf2;
        private (int, (string, Type)) IndexOf2OnList(List<Parametr> listOfParams)
        {
            for (int listPos = 0; listPos != listOfParams.Count; listPos++)
            {
                for (int listPred = 0; listPred != listOfParams[listPos].predicates.Count; listPred++)
                {
                    if (listOfParams[listPos].predicates[listPred].Hash != this.Hash2Class)
                        continue;

                    if (!object.ReferenceEquals(listOfParams[listPos].predicates[listPred].Hash, this.Hash2Class))
                        continue;

                    return (listPos, (listOfParams[listPos].predicates[listPred].name, listOfParams[listPos].predicates[listPred].type));
                }
            }

            throw new Exception(); //Brak na liście
        }

        bool? Check(Parametr parametr1, Parametr parametr2, List<object> list)
        {
            if (parametr1.Type != this.TypeOf1Class || parametr2.Type != this.TypeOf2Class)
                return null;

            ValueType a = parametr1.predicates.Where(p => p.name == this.Name)?.Where(p => p.type == TypeOf1Class)?.First().value;
            ValueType b = parametr1.predicates.Where(p => p.name == InfoOf2.Item1).Where(p => p.type == InfoOf2.Item2)?.First().value;

            if (a == null || b == null)
                return null;

            return (a == b);
        }

        public override (Func<Parametr, Parametr, List<object>, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            int IndexOf1 = IndexOf1OnList(listOfParams);
            var Parametr2 = IndexOf2OnList(listOfParams);
            InfoOf2 = Parametr2.Item2;

            Func<Parametr, Parametr, List<object>, bool?> func = Check;

            return (func, IndexOf1, Parametr2.Item1);
        }
    }

    internal class PreconditionConstPropertyPDDL<T1, T2> : PreconditionConstPDDL<T1, T2>
    {
        internal PreconditionConstPropertyPDDL(string name, ref T1 obj1, ref T2 obj2) : base(name, ref obj1, ref obj2)
        {
            //Func<T1, T2, bool> func = func1;
        }
    }

    internal class PreconditionConstFieldPDDL<T1, T2> : PreconditionConstPDDL<T1, T2>
    {
        internal PreconditionConstFieldPDDL(string name, ref T1 obj1, ref T2 obj2) : base(name, ref obj1, ref obj2)
        {
            //Func<T1, T2, bool> func = func1;
        }
    }

    internal class PreconditionExtPDDL<T1> : PreconditionPDDL<T1>
    {
        readonly bool TrigerValue;

        public override (Func<Parametr, Parametr, List<object>, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            throw new NotImplementedException();
        }

        internal PreconditionExtPDDL(string Name, ref T1 obj1, bool TrigerValue) : base(Name, ref obj1)
        {
            this.TrigerValue = TrigerValue;
        }
    }

    internal abstract class PreconditionInternalPDDL<T1> : PreconditionPDDL<T1>
    {
        ValueType CorrectValue;

        bool? Check(Parametr parametr1, Parametr parametr2, List<object> list)
        {
            if (parametr1.Type != this.TypeOf1Class)
                return null;

            ValueType a = parametr1.predicates.Where(p => p.name == this.Name)?.Where(p => p.type == TypeOf1Class)?.First().value;

            if (a == null)
                return null;

            return (a == CorrectValue);
        }

        public override (Func<Parametr, Parametr, List<object>, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            int IndexOfParam = IndexOf1OnList(listOfParams);
            Func<Parametr, Parametr, List<object>, bool?> func = Check;

            return (func, IndexOfParam, null);
        }

        internal PreconditionInternalPDDL(string Name, ref T1 obj1, ValueType CorrectValue) : base(Name, ref obj1)
        {
            this.CorrectValue = CorrectValue;
        }
    }

    internal class PreconditionInternalPropertyPDDL<T1> : PreconditionInternalPDDL<T1>
    {
        internal PreconditionInternalPropertyPDDL(string Name, ref T1 obj1, ValueType CorrectValue) : base(Name, ref obj1, CorrectValue) {}
    }
    internal class PreconditionInternalFieldPDDL<T1> : PreconditionInternalPDDL<T1>
    {
        internal PreconditionInternalFieldPDDL(string Name, ref T1 obj1, ValueType CorrectValue) : base(Name, ref obj1, CorrectValue) {}
    }
}
