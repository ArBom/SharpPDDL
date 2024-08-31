using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;

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

        internal List<ValueOfParametr> values;

        public Parametr(Int32 hashCode, object oryginal)
        {
            this.Oryginal = oryginal;
            Type = this.Oryginal.GetType();

            if (!Type.IsClass)
                throw new Exception("Wrong object type - its not a class");

            this.HashCode = hashCode;
            values = new List<ValueOfParametr>();

            PropertyInfo[] allProperties = Type.GetProperties(); //pobierz properties z odpowiednika we wszystkich typach
            foreach (PropertyInfo propertyInfo in allProperties) //dla kazdego propertis...
            {
                if (propertyInfo.PropertyType.IsValueType && propertyInfo.CanRead) //...jesli jest wartoscia i jest odczytywalny...
                {
                    string PropertyName = propertyInfo.Name;

                    ValueOfParametr newValue = new ValueOfParametr(PropertyName, propertyInfo.PropertyType, Type, false); //...utworz nową wartość...
                    this.values.Add(newValue); //...i dodaj na listę
                }
            }

            FieldInfo[] allFields = Type.GetFields(); //pobierz fields z odpowiednika we wszystkich typach
            foreach (FieldInfo fieldInfo in allFields) //dla kazdego field...
            {
                if (fieldInfo.FieldType.IsValueType && fieldInfo.IsPublic) //...jesli jest wartoscia i jest publiczny...
                {
                    string fieldName = fieldInfo.Name;

                    ValueOfParametr newValue = new ValueOfParametr(fieldName, fieldInfo.FieldType, Type, true); //...utworz nową wartość...
                    this.values.Add(newValue); //...i dodaj na listę
                }
            }
        }
        /*
        internal void SetAction(string MethodName, params object[] MethodParams)
        {
            var Methods = Type.GetMethods().Where(M => M.Name == MethodName);

            if (MethodParams.Length != 0)
            {
                Methods = Methods.Where(M => M.GetParameters().Length == MethodParams.Length);
            }
            else
            {

            }



        }

        internal Task ExecuteAction(object[] paramsy)
        {
            if (this.ParameterExecutionMethod is null)
                return null;
            return new Task(ParameterExecutionMethod.Invoke(Oryginal, paramsy)).Start();

        }*/

        internal void RemoveUnuseValue()
        {
            values.RemoveAll(x => !x.IsInUse);
        }
    }
}
