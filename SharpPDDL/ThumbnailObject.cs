using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal class ThumbnailObject
    {
        protected ThumbnailObject parent;
        internal List<ThumbnailObject> child;
        internal Type originalObjType;
        protected Dictionary<string, ValueType> Dict;
        //todo dictionary checksum


        public ValueType this[string key]
        {
            // returns value if exists
            get
            {
                if (Dict.ContainsKey(key))
                    return Dict[key];
                else
                    return parent[key];
            }

            // updates if exists, adds if doesn't exist
            set { Dict[key] = value; }
        }

        protected ThumbnailObject()
        {
            Dict = new Dictionary<string, ValueType>();
        }
    }

    class ThumbnailObject<TOriginalObj> : ThumbnailObject
    {
        readonly TOriginalObj OriginalObj;

        new internal ValueType getValue(string key)
        {
            if (Dict.ContainsKey(key))
                return Dict[key];

            return null; //TODO odwołanie do obiektu
        }

        internal ThumbnailObject(TOriginalObj originalObj, Parametr parametr)
        {
            parent = null;
            this.OriginalObj = originalObj;
            this.originalObjType = typeof(TOriginalObj);

            if (this.originalObjType != parametr.Type)
                //todo zobaczyć jeszcze na dziedziczenie
                throw new Exception();

            foreach (Value SomeValue in parametr.values)
            {
                string ValueName = SomeValue.name;
                ValueType value;

                if (SomeValue.IsField)
                {
                    FieldInfo myFieldInfo = this.originalObjType.GetField(ValueName);

                    if (myFieldInfo is null)
                        myFieldInfo = this.originalObjType.GetField(ValueName, BindingFlags.NonPublic | BindingFlags.Instance);

                    value = (ValueType)myFieldInfo.GetValue(myFieldInfo);
                }
                else
                {
                    PropertyInfo propertyInfo = this.originalObjType.GetProperty(ValueName);

                    if (propertyInfo is null)
                        propertyInfo = this.originalObjType.GetProperty(ValueName, BindingFlags.NonPublic | BindingFlags.Instance);

                    value = (ValueType)propertyInfo.GetValue(propertyInfo);
                }

                Dict.Add(ValueName, value);
            }
        }
    }
}
