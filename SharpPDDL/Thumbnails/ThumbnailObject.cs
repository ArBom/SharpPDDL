using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SharpPDDL
{
    internal abstract class ThumbnailObject : IDisposable
    {
        internal abstract object OriginalObj { get; }
        internal abstract Type OriginalObjType { get; }
        protected Dictionary<ushort, ValueType> Dict;
        internal abstract ushort[] ValuesIndeksesKeys { get; }
        public abstract ThumbnailObject Precursor { get; }
        internal ThumbnailObject Parent;
        internal byte[] CheckSum = new byte[GloCla.ThObCheckSumSize];

        internal void FigureCheckSum()
        {
            IEnumerable<ValueType> values = ValuesIndeksesKeys.Select(VIK => this[VIK]);
            string MD5input = string.Join(";", values);
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MD5input);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                Array.Copy(hashBytes, CheckSum, GloCla.ThObCheckSumSize);
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

            if (!this.CheckSum.SequenceEqual(With.CheckSum))
                return false;

            return true;
        }

        public void Dispose()
        {
            Dict = null;
            Parent = null;
        }
    }
}
