using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    abstract internal class PreconditionPDDL : ObjectPDDL
    {
        //internal abstract (Func<Parametr, Parametr, bool?>, Func<dynamic, dynamic, List<ExternalValue>, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams);

        protected Func<Parametr, Parametr, bool?> CheckPDDP;
        protected Func<dynamic, dynamic, List<ExternalValue>, bool?> Check;

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

        internal static PreconditionPDDL Instance<T1, T2>(string Name, ref T1 obj1, ref T2 obj2) where T1 : class
        {
            if (obj2.GetType().IsClass)
                return new PreconditionExtPDDL<T1, T2>(Name, ref obj1, ref obj2);

            if (obj2.GetType().IsValueType)
                return PreconditionConstPDDL<T1, T2>.Instance(Name, ref obj1, ref obj2);

            throw new Exception(""); //niewłaściwy typ obj2
        }

        protected PreconditionPDDL(string Name) : base(Name) { }
    }

    internal abstract class PreconditionPDDL<T1> : PreconditionPDDL
    {
        new readonly internal Type TypeOf1Class;
        new readonly internal Int32 Hash1Class;
        protected T1 t1;

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
        internal T2 t2;

        internal bool? IsField2Class()
        {
            FieldInfo fieldInfo = TypeOf2Class.GetField(Name);
            bool? Field = fieldInfo?.IsPublic;
            if (Field == true)
                return true;

            PropertyInfo propInfo = TypeOf2Class.GetProperty(Name);
            bool? Prop = propInfo?.CanRead;
            if (Prop == true)
                return false;

            return null;
        }

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
            t2 = obj2;
            TypeOf2Class = obj2.GetType();
            Hash2Class = obj2.GetHashCode();

            CheckPDDP = (Param1, Param2) =>
            {
                var V1null = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;
                var V2null = Param2.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf2Class)?.value;

                if (V1null == null || V2null == null)
                    return null;

                return V1null == V2null;
            };
        }
    }

    internal class PreconditionExtPDDL<T1, T2> : PreconditionPDDL<T1, T2>
    {
        internal PreconditionExtPDDL(string Name, ref T1 obj1, ref T2 obj2) : base(Name, ref obj1, ref obj2)
        {
            if (!TypeOf2Class.IsClass)
            {
                throw new Exception(); //2. obj nie jest klasą
            }

            CheckPDDP = (Param1, Param2) =>
            {
                var Param2Hash = Param2.GetHashCode();
                var V1null = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.TheOtherClasses; //TODO ?ref ref?
                var a = V1null.Where(p => p.HashCode == Param2Hash)?.First(p => Object.ReferenceEquals(p, Param2));

                if (a == null)
                    return false;
                else
                    return true;
            };
        }
    }

    internal abstract class PreconditionConstPDDL<T1, T2> : PreconditionPDDL<T1, T2> //T2 jest value type w innej klasie
                                                                                     //todo rozgalęzienie na 2 argument
    {
        protected string NameAt2class;
        protected bool? Is2Field = null;

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

        internal PreconditionConstPDDL(string name, ref T1 obj1, ref T2 obj2) : base(name, ref obj1, ref obj2)
        {
            CheckPDDP = (Param1, Param2) =>
            {
                var V1null = Param1.predicates.Where(p => p.name == Name).First().value;
                var V2null = Param2.predicates.Where(p => p.name == Name).First().value; //TODO druga nazwa raczej taka nie bedzie.

                if (V1null == null || V2null == null)
                    return false;

                return V1null == V2null;
            };
        }

        internal PreconditionConstPDDL(string name, ref T1 obj1, ref T2 obj2, string NameAt2class) : base(name, ref obj1, ref obj2)
        {
            this.NameAt2class = NameAt2class;
        }

        internal abstract PreconditionPDDL FullInstance();

        private (string, Type) InfoOf2; //TODO zobaczyć czy to jest na peno ok.
        new private (int, (string, Type)) IndexOf2OnList(List<Parametr> listOfParams)
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
    }

    internal class PreconditionConstPropertyPDDL<T1, T2> : PreconditionConstPDDL<T1, T2>
    {
        /*new internal PreconditionPDDL Instance(string Name, ref T1 obj1, ref T2 obj2)
        {
            PreconditionPDDL<T1, T2> p = new PreconditionConstPropertyPDDL<T1, T2>(Name, ref obj1, ref obj2);

            /*FieldInfo fieldInfo = obj2.GetType().GetField(Name);
            bool? Field = fieldInfo?.IsPublic;
            if (Field == true)
                return new PreconditionConstPropertyFieldPDDL<T1, T2>(Name, ref obj1, ref obj2);

            PropertyInfo propInfo = obj2.GetType().GetProperty(Name);
            bool? Prop = propInfo?.CanRead;
            if (Prop == true)
                return new PreconditionConstPropertyPropertyPDDL<T1, T2>(Name, ref obj1, ref obj2);

            throw new Exception(); //brak odwołania w 2. arg.
        }*/

        override internal PreconditionPDDL FullInstance()
        {
            if (string.IsNullOrEmpty(this.NameAt2class) || Is2Field == null)
                throw new Exception(); //najpierw należy wykonac inna f.

            if (Is2Field.Value)
                return new PreconditionConstPropertyFieldPDDL<T1, T2>(Name, ref t1, ref t2, NameAt2class);
            else
                return new PreconditionConstPropertyPropertyPDDL<T1, T2>(Name, ref t1, ref t2, NameAt2class);
        }

        internal PreconditionConstPropertyPDDL(string name, ref T1 obj1, ref T2 obj2) : base(name, ref obj1, ref obj2) { }
        internal PreconditionConstPropertyPDDL(string name, ref T1 obj1, ref T2 obj2, string NameAt2class) : base(name, ref obj1, ref obj2, NameAt2class) { }
    }

    internal class PreconditionConstPropertyFieldPDDL<T1, T2> : PreconditionConstPropertyPDDL<T1, T2>
    {
        internal PreconditionConstPropertyFieldPDDL(string name, ref T1 obj1, ref T2 obj2, string NameAt2class) : base(name, ref obj1, ref obj2, NameAt2class)
        {
            CheckPDDP = (Param1, Param2) =>
            {
                var V1null = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;
                var V2null = Param2.predicates.First(p => p.name == NameAt2class)?.value;

                return V1null == V2null;
            };

            Check = (Param1, Param2, List) =>
            {
                T1 RealObject1 = Param1;
                var ValueOfRealProperty1 = RealObject1.GetType().GetProperty(Name)?.GetValue(RealObject1);

                T2 RealObject2 = Param2;
                var ValueOfRealField2 = RealObject2.GetType().GetField(NameAt2class)?.GetValue(RealObject2); //TODO to jebnie

                return Equals(ValueOfRealProperty1, ValueOfRealField2);
            };
        }
    }

    internal class PreconditionConstPropertyPropertyPDDL<T1, T2> : PreconditionConstPropertyPDDL<T1, T2>
    {
        internal new PreconditionPDDL FullInstance()
        {
            return this;
        }

        internal PreconditionConstPropertyPropertyPDDL(string name, ref T1 obj1, ref T2 obj2, string NameAt2class) : base(name, ref obj1, ref obj2, NameAt2class)
        {
            CheckPDDP = (Param1, Param2) =>
            {
                var V1null = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;
                var V2null = Param2.predicates.First(p => p.name == NameAt2class)?.value;

                return V1null == V2null;
            };

            Check = (Param1, Param2, List) =>
            {
                T1 RealObject1 = Param1;
                var ValueOfRealProperty1 = RealObject1.GetType().GetProperty(Name).GetValue(RealObject1);

                T2 RealObject2 = Param2;
                var ValueOfRealProperty2 = RealObject2.GetType().GetProperty(NameAt2class).GetValue(RealObject2);

                return Equals(ValueOfRealProperty1, ValueOfRealProperty2);
            };
        }
    }

    internal class PreconditionConstFieldPDDL<T1, T2> : PreconditionConstPDDL<T1, T2>
    {
        new PreconditionConstFieldPDDL<T1, T2> Instance(string Name, ref T1 obj1, ref T2 obj2)
        {
            if (string.IsNullOrEmpty(this.NameAt2class) || Is2Field == null)
                throw new Exception(); //najpierw należy wykonac inna f.

            FieldInfo fieldInfo = obj2.GetType().GetField(Name);
            bool? Field = fieldInfo?.IsPublic;
            if (Field == true)
                return new PreconditionConstFieldFieldPDDL<T1, T2>(Name, ref obj1, ref obj2, NameAt2class);

            PropertyInfo propInfo = obj2.GetType().GetProperty(Name);
            bool? Prop = propInfo?.CanRead;
            if (Prop == true)
                return new PreconditionConstFieldPropertyPDDL<T1, T2>(Name, ref obj1, ref obj2, NameAt2class);

            throw new Exception(); //brak odwołania w 2. arg.
        }

        internal PreconditionConstFieldPDDL(string name, ref T1 obj1, ref T2 obj2) : base(name, ref obj1, ref obj2)
        {
            //TODO checkPDDL zaimplementować w klasie rodzic
        }
        internal PreconditionConstFieldPDDL(string name, ref T1 obj1, ref T2 obj2, string NameAt2class) : base(name, ref obj1, ref obj2, NameAt2class) { }

        override internal PreconditionPDDL FullInstance()
        {
            if (string.IsNullOrEmpty(this.NameAt2class) || Is2Field == null)
                throw new Exception(); //najpierw należy wykonac inna f.

            if (Is2Field.Value)
                return new PreconditionConstPropertyFieldPDDL<T1, T2>(Name, ref t1, ref t2, NameAt2class);
            else
                return new PreconditionConstPropertyPropertyPDDL<T1, T2>(Name, ref t1, ref t2, NameAt2class);
        }
    }

    internal class PreconditionConstFieldFieldPDDL<T1, T2> : PreconditionConstFieldPDDL<T1, T2>
    {
        internal new PreconditionPDDL FullInstance()
        {
            return this;
        }

        internal PreconditionConstFieldFieldPDDL(string name, ref T1 obj1, ref T2 obj2, string NameAt2class) : base(name, ref obj1, ref obj2, NameAt2class)
        {
            CheckPDDP = (Param1, Param2) =>
            {
                var V1null = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;
                var V2null = Param2.predicates.First(p => p.name == NameAt2class)?.value;

                return V1null == V2null;
            };

            Check = (Param1, Param2, List) =>
            {
                T1 RealObject1 = Param1;
                var ValueOfRealField1 = RealObject1.GetType().GetField(Name)?.GetValue(RealObject1);

                T2 RealObject2 = Param2;
                var ValueOfRealField2 = RealObject2.GetType().GetField(NameAt2class)?.GetValue(RealObject2); //TODO to jebnie

                return Equals(ValueOfRealField1, ValueOfRealField2);
            };
        }
    }
    

    internal class PreconditionConstFieldPropertyPDDL<T1, T2> : PreconditionConstFieldPDDL<T1, T2>
    {
        internal new PreconditionPDDL FullInstance()
        {
            return this;
        }

        internal PreconditionConstFieldPropertyPDDL(string name, ref T1 obj1, ref T2 obj2, string NameAt2class) : base(name, ref obj1, ref obj2, NameAt2class)
        {
            if (!obj2.GetType().IsClass) //TODO dorobić do podobnych
                throw new Exception(""); //2. obiekt musi być klasą

            CheckPDDP = (Param1, Param2) =>
            {
                var V1null = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;
                var V2null = Param2.predicates.First(p => p.name == NameAt2class)?.value;

                return V1null == V2null;
            };

            Check = (Param1, Param2, List) =>
            {
                T1 RealObject1 = Param1;
                var ValueOfRealField1 = RealObject1.GetType().GetField(Name)?.GetValue(RealObject1);

                T2 RealObject2 = Param2;
                var ValueOfRealProp2 = RealObject2.GetType().GetProperty(NameAt2class)?.GetValue(RealObject2); //TODO to jebnie

                return Equals(RealObject1, ValueOfRealProp2);
            };
        }
    }

    internal class PreconditionExtPDDL<T1> : PreconditionPDDL<T1>
    {
        readonly bool TrigerValue; //TODO brzydka nazwa

        internal PreconditionExtPDDL(string Name, ref T1 obj1, bool TrigerValue) : base(Name, ref obj1)
        {
            //TODO dodanie zewnętrzności
            //  internal Int32? Hash; //null dla external

            this.TrigerValue = TrigerValue;

            CheckPDDP = (Param1, Param2) => //dodać zewnętrzność jako kolejny predicates
            {
                var Vnull = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;

                if (Vnull == null)
                    return null;

                bool V = Convert.ToBoolean(Vnull);
                return V == TrigerValue;
            };
        }
    }

    internal abstract class PreconditionInternalPDDL<T1> : PreconditionPDDL<T1>
    {
        ValueType CorrectValue;

        internal PreconditionInternalPDDL(string Name, ref T1 obj1, ValueType CorrectValue) : base(Name, ref obj1)
        {
            CheckPDDP = (Param1, Param2) => //TODO sprawdzić czy się poprawnie nadpisuje typ
            {
                ValueType V = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;

                if (V == null)
                    return null;

                return V == CorrectValue;
            };
        }
    }

    internal class PreconditionInternalPropertyPDDL<T1> : PreconditionInternalPDDL<T1>
    {
        internal PreconditionInternalPropertyPDDL(string Name, ref T1 obj1, ValueType CorrectValue) : base(Name, ref obj1, CorrectValue)
        {
            Check = (Param1, Param2, List) =>
            {
                T1 RealObject1 = Param1;
                var ValueOfRealProperty = RealObject1.GetType().GetProperty(Name)?.GetValue(RealObject1);

                if (ValueOfRealProperty == null)
                    return null;

                return (ValueOfRealProperty == CorrectValue);
            };
        }
    }

    internal class PreconditionInternalFieldPDDL<T1> : PreconditionInternalPDDL<T1>
    {
        internal PreconditionInternalFieldPDDL(string Name, ref T1 obj1, ValueType CorrectValue) : base(Name, ref obj1, CorrectValue)
        {
            Check = (Param1, Param2, List) =>
            {
                T1 RealObject1 = Param1;
                var ValueOfRealField = RealObject1.GetType().GetField(Name)?.GetValue(RealObject1);

                if (ValueOfRealField == null)
                    return null;

                return (ValueOfRealField == CorrectValue);
            };
        }
    }

}