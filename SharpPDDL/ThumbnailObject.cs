﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    internal class ThumbnailObject
    {
        protected ThumbnailObject parent;
        internal List<ThumbnailObject> child;
        protected Dictionary<string, ValueType> Dict;
        //todo dictionary checksum


        internal ValueType this[string key]
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

        /*internal void setValue (string key, ValueType value)
        {
            if (Dict.ContainsKey(key))
                Dict[key] = value;
            else
                Dict.Add(key, value);
        }

        public ValueType getValue (string key)
        {
            if (Dict.ContainsKey(key))
                return Dict[key];

            return parent.getValue(key);
        }*/

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

            foreach (var a in parametr.values)
            {
                Dict.Add(a.name, a.value);
            }
        }
    }
}
