using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpPDDL
{
    internal class Parametr
    {
        internal readonly object Oryginal;
        public readonly Type Type;
        public readonly Int32 HashCode;
        internal BinaryExpression CheckType;
        internal ParametrPreconditionLambda parametrPreconditionLambda;
        internal Func<ThumbnailObject, bool> Func;
        protected bool _UsedInPrecondition = false;
        protected bool _UsedInEffect = false;
        internal bool UsedInPrecondition
        {
            get { return _UsedInPrecondition; }
            set
            {
                if (value)
                    _UsedInPrecondition = true;
            }
        }

        internal bool UsedInEffect
        {
            get { return _UsedInEffect; }
            set
            {
                if (value)
                    _UsedInEffect = true;
            }
        }

        internal List<Value> values;

        public Parametr(Int32 hashCode, object oryginal)
        {
            this.Oryginal = oryginal;
            Type = this.Oryginal.GetType();

            if (!Type.IsClass)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 21, GloCla.ResMan.GetString("C3"), Type.ToString());
                throw new Exception(GloCla.ResMan.GetString("C3"));
            }

            this.HashCode = hashCode;
            values = new List<Value>();

            PropertyInfo[] allProperties = Type.GetProperties(); //pobierz properties z odpowiednika we wszystkich typach
            foreach (PropertyInfo propertyInfo in allProperties) //dla kazdego propertis...
            {
                if (propertyInfo.PropertyType.IsValueType && propertyInfo.CanRead) //...jesli jest wartoscia i jest odczytywalny...
                {
                    string PropertyName = propertyInfo.Name;

                    Value newValue = new Value(PropertyName, propertyInfo.PropertyType, Type, false); //...utworz nową wartość...
                    this.values.Add(newValue); //...i dodaj na listę
                }
            }

            FieldInfo[] allFields = Type.GetFields(); //pobierz fields z odpowiednika we wszystkich typach
            foreach (FieldInfo fieldInfo in allFields) //dla kazdego field...
            {
                if (fieldInfo.FieldType.IsValueType && fieldInfo.IsPublic) //...jesli jest wartoscia i jest publiczny...
                {
                    string fieldName = fieldInfo.Name;

                    Value newValue = new Value(fieldName, fieldInfo.FieldType, Type, true); //...utworz nową wartość...
                    this.values.Add(newValue); //...i dodaj na listę
                }
            }
        }

        internal static void GetTheInstance_TryAddToList<T> (List<Parametr> Parameters, ref T ToInstance) where T : class
        {
            if (typeof(T).IsAbstract)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 19, GloCla.ResMan.GetString("E6"), typeof(T).ToString());
                throw new Exception(GloCla.ResMan.GetString("E6"));
            }

            if (ToInstance is null)
                ToInstance = (T)FormatterServices.GetUninitializedObject(typeof(T));

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 20, GloCla.ResMan.GetString("I3"), typeof(T).ToString());

            Int32 HashCode = ToInstance.GetHashCode();

            if (Parameters.Any(t => t.HashCode == HashCode))
            {
                Parametr p = Parameters.Where(t => t.HashCode == HashCode).First();
                if (p.Oryginal.Equals(ToInstance))
                    return;
            }

            Parametr TempParametr = new Parametr(HashCode, ToInstance);
            Parameters.Add(TempParametr);
        }

        internal void Init1ArgPrecondition(List<SingleTypeOfDomein> allTypes, int i)
        {
            SingleTypeOfDomein ThisSingleTypeOfDomein = allTypes.First(t => t.Type == Type);

            if (ThisSingleTypeOfDomein.NeedToTypeCheck)
            {
                //Temp param
                var TempParam = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix + i);

                //checking if types equals
                var ConType = Expression.Constant(Type, typeof(Type));
                var keyOrygObjType = typeof(ThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObjType");
                var ThObOryginalType = Expression.MakeMemberAccess(TempParam, keyOrygObjType);
                var TypeEqals = Expression.Equal(ThObOryginalType, ConType);

                //checking if type is inherited
                var keyOrygObj = typeof(ThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObj");
                var OrygObj = Expression.MakeMemberAccess(TempParam, keyOrygObj);
                var ISCorrectType = Expression.TypeIs(OrygObj, Type);

                //connect upper...
                CheckType = Expression.OrElse(TypeEqals, ISCorrectType);
            }
            else
                CheckType = null;

            parametrPreconditionLambda = new ParametrPreconditionLambda(CheckType);
        }

        internal void RemoveUnuseValue()
        {
            values.RemoveAll(x => !x.IsInUse);
        }
    }
}
