using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    /*internal class ExternalValue
    {
        internal readonly string name;
        //bool Value = false;

        internal Int32 Hash1 { get { return class1.GetHashCode(); } }
        internal readonly Type type1;
        internal readonly object class1;

        internal Int32 Hash2 { get { return class2.GetHashCode(); } }
        internal readonly Type type2;
        internal readonly object class2;

        ExternalValue(string name, ref object class1, ref object class2) //TODO sprawdzić czy muszą być refy
        {
            this.name = name;

            this.type1 = class1.GetType();
            this.class1 = class1;

            this.type2 = class2.GetType();
            this.class2 = class1;
        }
    }*/

    internal class Value
    {
        readonly internal string name;
        readonly Type type;

        //true for field, false for properties
        readonly bool IsField;
        Int32? Hash;

        //In the beginning one premise it will be not in use
        private bool IsInUse = false;

        internal bool isInUse
        {
            get { return IsInUse; }
            set
            {
                //It can be change only for true
                if (value)
                    IsInUse = true;
            }
        }

        internal Value(string Name, Type type, bool IsField, Int32? Hash)
        {
            if (!type.IsValueType)
                throw new InvalidOperationException();
        }

        internal ValueType value;
    }

    internal class Parametr 
    {
        public readonly Type Type;
        public readonly Int32 HashCode;
        public List<Value> values;

        public Parametr(Type Type, Int32 hashCode)
        {
            this.Type = Type;
            this.HashCode = hashCode;
            //Zawartość listowana w ActionPDDL.CheckThisAction() dla elementów na liście
        }
    }
    
    public class ActionPDDL
    {
        public readonly string Name;
        internal uint Id; //todo ist like name but smaller
        private List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private List<Parametr> Parameters;         //typy wykorzystywane w tej akcji (patrz powyzej)

        internal List<SingleType> allTypes; //przekopionawe wszystkie zdefiniowane typy

        private List<EffectPDDL> Effects;        //efekty
        //private Stopwatch stopwatch;
        //private TimeSpan ts;

        public void CheckThisAction()
        {
            if (allTypes == null)
                throw new Exception(); //nieprawidłowe wywołanie metody tj. bez wcześniejszego przepisania allTypes

            foreach (Parametr parametr in Parameters) //dla każdego obiektu (klasy)
            {
                SingleType thisType = allTypes.Find(KoT => KoT.type == parametr.Type); //we wszystkich typach znajdz o takim samym typie jak obecny parametr

                if (thisType == null)
                {
                    throw new Exception(); //Wsród wcześniej zdefiniowanych typów nie ma podanego dla tej akcji
                }

                parametr.values = new List<Value>(); //utwórz nową listę predykatów dla obecnego typu

                PropertyInfo[] allProperties = thisType.GetType().GetProperties(); //pobierz properties z odpowiednika we wszystkich typach
                foreach (PropertyInfo propertyInfo in allProperties) //dla kazdego propertis...
                {
                    Type typeOfPropertie = propertyInfo.GetType(); //...pobierz jego typ...

                    if(typeOfPropertie.IsValueType && typeOfPropertie.IsPublic) //...jesli jest wartoscia i jest publiczny...
                    {
                        string PropertyName = propertyInfo.Name;

                        if (!thisType.predicates.Exists(p => p.Name == PropertyName)) //nie zdefioniowano wcześniej takiego predykatu
                            continue;

                        Value newValue = new Value(PropertyName, propertyInfo.GetType(), false, parametr.GetType().GetProperty(PropertyName).GetHashCode());    //...utworz nową wartość...
                        parametr.values.Add(newValue); //...i dodaj na listę
                    }
                }

                FieldInfo[] allFields = thisType.GetType().GetFields(); //pobierz fields z odpowiednika we wszystkich typach
                foreach (FieldInfo fieldInfo in allFields) //dla kazdego field...
                {
                    Type typeOfValue = fieldInfo.GetType(); //...pobierz jego typ...

                    if (typeOfValue.IsValueType && typeOfValue.IsPublic) //...jesli jest wartoscia i jest publiczny...
                    {
                        string fieldName = fieldInfo.Name;

                        if (!thisType.predicates.Exists(p => p.Name == fieldName)) //nie zdefioniowano wcześniej takiego predykatu
                            continue;

                        Value newValue = new Value(fieldName, fieldInfo.GetType(), true, parametr.GetType().GetProperty(fieldName).GetHashCode());
                        parametr.values.Add(newValue);
                    }
                }
            }
        }

        internal void BuildIt()
        {
            CheckThisAction();

            foreach(var precondition in Preconditions)
            {
                //TODO full instance
                var a = precondition.BuildFunct(/*Parameters*/);
            }
        }

        public void AddParameter<T>(out T destination) where T : class
        {
            destination = Activator.CreateInstance<T>();
            Type Type = destination.GetType();
            Int32 HashCode = destination.GetHashCode();

            Parametr TempParametr = new Parametr(Type, HashCode);
            Parameters.Add(TempParametr);
        }

        private void CheckExistPreconditionName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Preconditions.Exists(precondition => precondition.Name == Name))
                throw new Exception(); //juz istnieje warunek poczatkowy o takiej nazwie
        }

        /*
        public void AddPrecondiction<T1, T2>(string Name, ref T1 obj, ref T2 value) where T1 : class //warunek poczatkowy z wartoscia w typie, lub 2 klasy i zewnatrzny
        {
            CheckExistPreconditionName(Name);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj, ref value);
            Preconditions.Add(temp);
        }*/

        public void AddPrecondiction<T1>(string Name, ref T1 obj, Expression<Predicate<T1>> func) where T1 : class //warunek w postaci Predicate
        {
            CheckExistPreconditionName(Name);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj, func);

            ThumbnailObLambdaModif thumbnailObLambdaModif = new ThumbnailObLambdaModif();
            var a = thumbnailObLambdaModif.Visit(func);
            Preconditions.Add(temp);
        }

        public void AddPrecondiction<T1, T2>(string Name, ref T1 obj1, ref T2 obj2, Expression<Predicate<T1, T2>> func) where T1 : class where T2 : class //warunek w postaci Predicate
        {
            CheckExistPreconditionName(Name);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj1, ref obj2, func);
            Preconditions.Add(temp);
        }

        /*
        public void AddPrecondiction<T1>(string Name, ref T1 obj, System.ValueType value = default) where T1 : class //warunek poczatkowy ze stałą wartoscia
        {
            CheckExistPreconditionName(Name);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj, value);
            Preconditions.Add(temp);
        }*/

        public ActionPDDL(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or emty

            //TODO zadbać o unikalność nazw

            this.Name = Name;

            Parameters = new List<Parametr>();
            Preconditions = new List<PreconditionPDDL>();
            //Effects = new List<PredicatePDDL>();
        }
    }
}
