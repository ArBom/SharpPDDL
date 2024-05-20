using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpPDDL
{    
    public class ActionPDDL
    {
        public readonly string Name;
        internal uint ActionCost;
        private List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private List<EffectPDDL> Effects; //efekty
        private List<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)
        internal Delegate InstantActionPDDL { get; private set; }
        internal int InstantActionParamCount { get; private set; }

        internal List<SingleType> TakeSingleTypes()
        {
            List<SingleType> ToRet = new List<SingleType>();

            foreach (Parametr parametr in Parameters)
            {
                parametr.RemoveUnuseValue();
                SingleType singleType = null;

                if (ToRet.Count != 0)
                {
                    var singleTypes = ToRet.Where(sT => sT.Type == parametr.Type);
                    if (singleTypes.Count() != 0)
                        singleType = singleTypes.First();
                }

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

            return ToRet;
        }

        public void AddUnassignedParametr<T>(out T destination) where T : class
        {
            destination = (T)FormatterServices.GetUninitializedObject(typeof(T));
            AddParameter(ref destination);
        }

        public void AddAssignedParametr<T1c>(ref T1c destination) where T1c : class
        {
            if (typeof(T1c).IsAbstract)
                throw new Exception("Sorry, You cannot to use abstract parameter at this version");

            if (destination is null)
                destination = (T1c)FormatterServices.GetUninitializedObject(typeof(T1c));

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

            Parametr TempParametr = new Parametr(HashCode, destination);
            Parameters.Add(TempParametr);
        }

        private void CheckExistEffectName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Effects.Exists(effect => effect.Name == Name))
                throw new Exception(); //juz istnieje efekt o takiej nazwie
        }

        private void CheckExistPreconditionName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Preconditions.Exists(precondition => precondition.Name == Name))
                throw new Exception(); //juz istnieje warunek poczatkowy o takiej nazwie
        }

        public void AddPrecondiction<T1>(string Name, ref T1 obj, Expression<Predicate<T1>> func) where T1 : class => AddPrecondiction<T1, T1>(Name, ref obj, func);

        public void AddPrecondiction<T1c, T1p>(string Name, ref T1c obj, Expression<Predicate<T1p>> func) 
            where T1p : class where T1c : class, T1p
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

        public void AddPrecondiction<T1c, T1p, T2c, T2p>(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func) 
            where T1p : class where T2p : class where T1c : class, T1p where T2c : class, T2p
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

        public void AddEffect<T1>(string Name, ValueType newValue_Static, ref T1 destinationObj, Expression<Func<T1, ValueType>> destinationMember) where T1 : class 
        {
            CheckExistEffectName(Name);
            this.AddAssignedParametr(ref destinationObj);
            EffectPDDL temp = EffectPDDL.Instance(Name, newValue_Static, ref destinationObj, destinationMember);

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != destinationObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(destinationObj))
                    continue;

                parametr.values.First(v => v.Name == temp.DestinationMemberName).IsInUse = true;
                parametr.UsedInEffect = true;
                break;
            }

            Effects.Add(temp);
        }

        public void AddEffect<T1, T2>(string Name, ref T1 SourceObj, Expression<Func<T1, ValueType>> Source, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationMember) where T1 : class where T2 : class
        {
            CheckExistEffectName(Name);
            this.AddAssignedParametr(ref SourceObj);
            this.AddAssignedParametr(ref DestinationObj);
            EffectPDDL temp = EffectPDDL.Instance(Name, ref SourceObj, Source, ref DestinationObj, DestinationMember);

            //Tag destination parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != DestinationObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(DestinationObj))
                    continue;

                parametr.values.First(v => v.Name == temp.DestinationMemberName).IsInUse = true;
                parametr.UsedInEffect = true;
                break;
            }

            //Tak source parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != SourceObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(SourceObj))
                    continue;

                foreach (string valueName in temp.usedMembers2Class)
                    parametr.values.First(v => v.Name == valueName).IsInUse = true;

                parametr.UsedInEffect = true;
                break;
            }

            Effects.Add(temp);
        }

        public void AddEffect<T1, T2>(string Name, ref T1 SourceObj, Func<T1, T2, ValueType> SourceFunct, ref T2 DestinationObj, Func<T2, ValueType> DestinationFunct) where T1 : class where T2 : class
        {
            CheckExistEffectName(Name);
            this.AddAssignedParametr(ref SourceObj);
            this.AddAssignedParametr(ref DestinationObj);
            EffectPDDL temp = EffectPDDL.Instance(Name, ref SourceObj, SourceFunct, ref DestinationObj, DestinationFunct);

            //Tag destination parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != DestinationObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(DestinationObj))
                    continue;

                foreach (string valueName in temp.usedMembers1Class)
                    parametr.values.First(v => v.Name == valueName).IsInUse = true;

                parametr.UsedInEffect = true;
                break;
            }

            //Tak source parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != SourceObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(SourceObj))
                    continue;

                foreach (string valueName in temp.usedMembers2Class)
                    parametr.values.First(v => v.Name == valueName).IsInUse = true;

                parametr.UsedInEffect = true;
                break;
            }

            Effects.Add(temp);
        }

        internal void BuildAction(List<SingleTypeOfDomein> allTypes)
        {
            List<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>>> PrecondidionExpressions = new List<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>>>();
            foreach (PreconditionPDDL Precondition in Preconditions)
            {
                Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>> ExpressionOfPrecondition = Precondition.BuildCheckPDDP(allTypes, Parameters);
                PrecondidionExpressions.Add(ExpressionOfPrecondition);
            }

            List<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>> EffectExpressions = new List<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>>();
            foreach (EffectPDDL Effect in Effects)
            {
                Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> ExpressionOfEffect = Effect.BuildEffectPDDP(allTypes, Parameters);
                EffectExpressions.Add(ExpressionOfEffect);
            }

            ActionLambdaPDDL actionLambdaPDDL = new ActionLambdaPDDL(Parameters, PrecondidionExpressions, EffectExpressions);
            InstantActionPDDL = actionLambdaPDDL.InstantFunct;
            InstantActionParamCount = Parameters.Count;
        }

        public ActionPDDL(string Name, uint ActionCost = 1)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or emty

            //TODO zadbać o unikalność nazw

            this.ActionCost = ActionCost;
            this.Name = Name;
            this.Parameters = new List<Parametr>();
            this.Preconditions = new List<PreconditionPDDL>();
            this.Effects = new List<EffectPDDL>();
        }
    }
}
