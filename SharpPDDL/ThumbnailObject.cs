using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
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

    internal class ThumbnailObject<TOriginalObj> : ThumbnailObject where TOriginalObj  : class
    {
        readonly TOriginalObj OriginalObj;

        new public ValueType this[string key]
        {
            get
            {
                ValueType ToRet;
                ToRet = (ValueType)originalObjType.GetProperty(key)?.GetValue(OriginalObj);

                if (ToRet is null)
                    ToRet = (ValueType)originalObjType.GetField(key)?.GetValue(OriginalObj);

                return ToRet;
            }
        }

        internal ThumbnailObject(TOriginalObj originalObj, List<SingleType> singleTypes)
        {
            parent = null;
            this.OriginalObj = originalObj;
            this.originalObjType = typeof(TOriginalObj);
            SingleType singleType = singleTypes.Where(sT => sT.Type == this.originalObjType).First();

            if (this.originalObjType != singleType.Type)
                //todo zobaczyć jeszcze na dziedziczenie
                throw new Exception();

            foreach (Value SomeValue in singleType.Values)
            {
                string ValueName = SomeValue.Name;
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
