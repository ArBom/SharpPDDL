using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SharpPDDL
{    
    public class ActionPDDL
    {
        public readonly string Name;
        private List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private List<EffectPDDL> Effects; //efekty
        private List<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)

        internal List<SingleType> TakeSingleTypes()
        {
            List<SingleType> ToRet = new List<SingleType>();

            foreach (Parametr parametr in Parameters)
            {
                parametr.RemoveUnuseValue();
                SingleType singleType = null;

                if (ToRet.Count != 0)
                    singleType = ToRet.Where(sT => sT.Type == parametr.Type)?.First();

                if (singleType is null)
                {
                    singleType = new SingleType(parametr.Type, parametr.values);
                    ToRet.Add(singleType);
                }
                else
                {
                    foreach (ValueOfParametr valueP in parametr.values)
                    {
                        if (singleType.Values.Exists(t => t.Name == valueP.Name))
                            continue;

                        singleType.Values.Add(valueP);
                    }
                }
            }

            //TODO efekty

            return ToRet;
        }

        public void AddUnassignedParametr<T>(out T destination) where T : class
        {
            destination = (T)FormatterServices.GetUninitializedObject(typeof(T));
            AddParameter(ref destination);
        }

        public void AddAssignedParametr<T>(ref T destination) where T : class
        {
            if (destination is null)
                destination = (T)FormatterServices.GetUninitializedObject(typeof(T));

            Int32 HashCode = destination.GetHashCode();
            AddParameter(ref destination);
        }

        internal void AddParameter<T>(ref T destination) where T : class
        {
            Int32 HashCode = destination.GetHashCode();

            if (Parameters.Any(t => t.HashCode == HashCode))
            {
                Parametr p = Parameters.Where(t => t.HashCode == HashCode).First();
                if (p.Oryginal.Equals(destination))
                    return;
            }

            Type Type = destination.GetType();
            Parametr TempParametr = new Parametr(HashCode, destination);
            Parameters.Add(TempParametr);
        }

        private void CheckExistEffectName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Effects.Exists(effect => effect.Name == Name))
                throw new Exception(); //juz istnieje warunek poczatkowy o takiej nazwie
        }

        private void CheckExistPreconditionName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Preconditions.Exists(precondition => precondition.Name == Name))
                throw new Exception(); //juz istnieje warunek poczatkowy o takiej nazwie
        }

        public void AddPrecondiction<T1>(string Name, ref T1 obj, Expression<Predicate<T1>> func) where T1 : class
        {   
            CheckExistPreconditionName(Name);
            this.AddAssignedParametr(ref obj);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj, func);

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode == obj.GetHashCode())
                {
                    if (parametr.Oryginal.Equals(obj))
                    {
                        foreach (string valueName in temp.usedMembers1Class)
                        {
                            parametr.values.Where(v => v.Name == valueName).First().IsInUse = true;
                        }
                        parametr.UsedInPrecondition = true;
                        break;
                    }
                }
            }

            Preconditions.Add(temp);
        }

        public void AddPrecondiction<T1, T2>(string Name, ref T1 obj1, ref T2 obj2, Expression<Predicate<T1, T2>> func) where T1 : class where T2 : class //warunek w postaci Predicate
        {
            CheckExistPreconditionName(Name);
            this.AddAssignedParametr(ref obj1);
            this.AddAssignedParametr(ref obj2);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj1, ref obj2, func);

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode == obj1.GetHashCode())
                {
                    if (parametr.Oryginal.Equals(obj1))
                    {
                        foreach (string valueName in temp.usedMembers1Class)
                        {
                            parametr.values.Where(v => v.Name == valueName).First().IsInUse = true;
                        }
                        parametr.UsedInPrecondition = true;
                        break;
                    }
                }
            }

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode == obj2.GetHashCode())
                {
                    if (parametr.Oryginal.Equals(obj2))
                    {
                        foreach (string valueName in temp.usedMembers2Class)
                        {
                            parametr.values.Where(v => v.Name == valueName).First().IsInUse = true;
                        }
                        parametr.UsedInPrecondition = true;
                        break;
                    }
                }
            }

            Preconditions.Add(temp);
        }

        internal void BuildAction(List<SingleTypeOfDomein> allTypes)
        {
            foreach (PreconditionPDDL Precondition in Preconditions)
            {

                //Precondition.BuildCheckPDDP(allTypes);
            }
        }

        public ActionPDDL(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or emty

            //TODO zadbać o unikalność nazw

            this.Name = Name;
            this.Parameters = new List<Parametr>();
            this.Preconditions = new List<PreconditionPDDL>();
            //Effects = new List<PredicatePDDL>();
        }
    }
}
