using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SharpPDDL
{
    internal abstract class ThumbnailObject
    {
        internal abstract object OriginalObj { get; }
        internal abstract Type OriginalObjType { get; }
        protected Dictionary<ushort, ValueType> Dict;
        internal abstract ushort[] ValuesIndeksesKeys { get; }
        public abstract ThumbnailObject Precursor { get; }
        internal ThumbnailObject Parent;
        internal List<ThumbnailObject> child;
        internal string CheckSum;

        internal void FigureCheckSum()
        {
            string MD5input = Precursor.GetHashCode().ToString();

            for (int arrayCounter = 0; arrayCounter != ValuesIndeksesKeys.Count(); ++arrayCounter)
                MD5input = MD5input + ";" + this[ValuesIndeksesKeys[arrayCounter]].ToString();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MD5input);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                CheckSum = Convert.ToBase64String(hashBytes).Substring(0,4);
            }
        }

        public ValueType this[ushort key]
        {
            // returns value if exists
            get
            {
                if (Dict.ContainsKey(key))
                    return Dict[key];
                else
                    return Parent[key];
            }
        }

        abstract internal ThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes);

        internal bool Compare(ThumbnailObject With)
        {
            if (!this.OriginalObj.Equals(With.OriginalObj))
                return false;

            for (ushort arrayCounter = 0; arrayCounter != ValuesIndeksesKeys.Count(); ++arrayCounter)
                if (!this[ValuesIndeksesKeys[arrayCounter]].Equals(With[ValuesIndeksesKeys[arrayCounter]]))
                    return false;

            return true;
        }
    }
}
