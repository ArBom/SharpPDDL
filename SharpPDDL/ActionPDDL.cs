using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal class ExternalValue
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
    }

    internal class Value
    {
        internal string name;
        internal Int32? Hash; //null dla external
        internal Type type;

        internal List<Parametr> TheOtherClasses = null;
        internal ValueType value;
    }

    internal class Parametr 
    {
        public readonly Type Type;
        public readonly Int32 HashCode;
        public List<Value> predicates;

        public Parametr(Type Type, Int32 hashCode)
        {
            this.Type = Type;
            this.HashCode = hashCode;
        }
    }
    
    public class ActionPDDL
    {
        public readonly string Name;
        private List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private List<Parametr> Parameters;         //typy wykorzystywane w tej akcji (patrz powyzej)

        internal static List<KnotOfTree> allTypes; //przekopionawe wszystkie zdefiniowane typy

        private List<EffectPDDL> Effects;        //efekty
        //private Stopwatch stopwatch;
        //private TimeSpan ts;

        protected void CheckThisAction()
        {
            if (allTypes == null)
                throw new Exception(); //nieprawidłowe wywołanie metody tj. bez wcześniejszego przepisania allTypes

            foreach (Parametr parametr in Parameters) //dla każdego obiektu (klasy)
            {
                KnotOfTree thisType = allTypes.Find(KoT => KoT.type == parametr.Type); //we wszystkich typach znajdz o takim samym typie jak obecny parametr

                if (thisType == null)
                {
                    throw new Exception(); //Wsród wcześniej zdefiniowanych typów nie ma podanego dla tej akcji
                }

                parametr.predicates = new List<Value>(); //utwórz nową listę predykatów dla obecnego typu

                PropertyInfo[] allProperties = thisType.GetType().GetProperties(); //pobierz properties z odpowiednika we wszystkich typach
                foreach (PropertyInfo propertyInfo in allProperties) //dla kazdego propertis...
                {
                    Type typeOfPropertie = propertyInfo.GetType(); //...pobierz jego typ...

                    if(typeOfPropertie.IsValueType && typeOfPropertie.IsPublic) //...jesli jest wartoscia i jest publiczny...
                    {
                        string PropertyName = propertyInfo.Name;

                        if (!thisType.predicates.Exists(p => p.Name == PropertyName)) //nie zdefioniowano wcześniej takiego predykatu
                            continue;

                        Value newValue = new Value //...utworz nową wartość...
                        {
                            name = PropertyName,
                            type = propertyInfo.GetType(),
                            Hash = parametr.GetType().GetProperty(PropertyName).GetHashCode()
                        };

                        parametr.predicates.Add(newValue); //...i dodaj na listę
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

                        Value newValue = new Value
                        {
                            name = fieldName,
                            type = fieldInfo.GetType(),
                            Hash = parametr.GetType().GetProperty(fieldName).GetHashCode()
                        };

                        parametr.predicates.Add(newValue);
                    }
                }

                //zewnetrzne i funkcje
            }
        }

        internal void BuildIt()
        {
            CheckThisAction();

            foreach(var precondition in Preconditions)
            {
                //TODO utworzenie zewnętrznych w tym miejscu
                precondition.FindIndexesOnList(Parameters);
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

        public void AddPrecondiction<T1, T2>(string Name, ref T1 obj, ref T2 value) where T1 : class //warunek poczatkowy z wartoscia w typie, lub 2 klasy i zewnatrzny
        {
            CheckExistPreconditionName(Name);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj, ref value);
            Preconditions.Add(temp);
        }

        public void AddPrecondiction<T1>(string Name, ref T1 obj, System.ValueType value = default) where T1 : class //warunek poczatkowy ze stałą wartoscia
        {
            CheckExistPreconditionName(Name);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj, value);
            Preconditions.Add(temp);
        }

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
