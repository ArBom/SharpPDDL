using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal class Parametr
    {
        internal readonly object Oryginal;
        public readonly Type Type;
        public readonly Int32 HashCode;
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

        public Parametr(Type Type, Int32 hashCode, object oryginal)
        {
            this.Oryginal = oryginal;
            this.Type = Type;
            this.HashCode = hashCode;
            values = new List<Value>();

            PropertyInfo[] allProperties = Type.GetProperties(); //pobierz properties z odpowiednika we wszystkich typach
            foreach (PropertyInfo propertyInfo in allProperties) //dla kazdego propertis...
            {
                Type typeOfPropertie = propertyInfo.GetType(); //...pobierz jego typ...

                if (typeOfPropertie.IsValueType && typeOfPropertie.IsPublic) //...jesli jest wartoscia i jest publiczny...
                {
                    string PropertyName = propertyInfo.Name;

                    Value newValue = new Value(PropertyName, propertyInfo.GetType(), false);    //...utworz nową wartość...
                    this.values.Add(newValue); //...i dodaj na listę
                }
            }

            FieldInfo[] allFields = Type.GetFields(); //pobierz fields z odpowiednika we wszystkich typach
            foreach (FieldInfo fieldInfo in allFields) //dla kazdego field...
            {
                Type typeOfValue = fieldInfo.GetType(); //...pobierz jego typ...

                if (typeOfValue.IsValueType && typeOfValue.IsPublic) //...jesli jest wartoscia i jest publiczny...
                {
                    string fieldName = fieldInfo.Name;

                    Value newValue = new Value(fieldName, fieldInfo.GetType(), true);
                    this.values.Add(newValue);
                }
            }
        }

        internal void RemoveUnuseValue()
        {
            values.RemoveAll(x => !x.IsInUse);
        }
    }
}
