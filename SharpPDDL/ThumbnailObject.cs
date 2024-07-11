using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SharpPDDL
{
    internal abstract class ThumbnailObject
    {
        internal Type OriginalObjType;
        protected Dictionary<ushort, ValueType> Dict;

        internal abstract ushort[] ValuesIndeksesKeys { get; }

        public abstract ValueType this[ushort key] { get; }
    }

    internal abstract class PossibleStateThumbnailObject : ThumbnailObject
    {
        internal abstract object OriginalObj { get; }
        internal abstract new Type OriginalObjType { get; }
        internal abstract PossibleStateThumbnailObject Precursor { get; }
        internal PossibleStateThumbnailObject Parent;
        internal List<PossibleStateThumbnailObject> child;
        internal string CheckSum;

        internal void FigureCheckSum()
        {
            string MD5input = Precursor.GetHashCode().ToString();

            for (int arrayCounter = 0; arrayCounter != ValuesIndeksesKeys.Count(); ++arrayCounter)
            {
                MD5input = MD5input + ";" + this[ValuesIndeksesKeys[arrayCounter]].ToString();
            }

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MD5input);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                CheckSum = Convert.ToBase64String(hashBytes).Substring(0,4);
            }
        }

        public override ValueType this[ushort key]
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

        abstract internal PossibleStateThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes);

        internal bool Compare(PossibleStateThumbnailObject With)
        {
            if (!this.OriginalObj.Equals(With.OriginalObj))
                return false;

            for (ushort arrayCounter = 0; arrayCounter != ValuesIndeksesKeys.Count(); ++arrayCounter)
            {
                if (this[ValuesIndeksesKeys[arrayCounter]] != With[ValuesIndeksesKeys[arrayCounter]])
                    return false;
            }

            return true;
        }

    }

    internal class ThumbnailObject<TOriginalObj> : PossibleStateThumbnailObject where TOriginalObj : class
    {
        internal override object OriginalObj { get { return _Precursor.OriginalObj; } }
        internal PossibleStateThumbnailObject _Precursor;
        internal override PossibleStateThumbnailObject Precursor { get { return _Precursor; } }
        new internal List<ThumbnailObject<TOriginalObj>> child;
        internal override Type OriginalObjType { get { return _Precursor.OriginalObjType; } }

        internal override ushort[] ValuesIndeksesKeys => Precursor.ValuesIndeksesKeys;

        internal ThumbnailObject(PossibleStateThumbnailObject Precursor, PossibleStateThumbnailObject parent, List<KeyValuePair<ushort, ValueType>> changes)
        {
            this._Precursor = Precursor;
            this.Parent = parent;
            this.Dict = changes.ToDictionary(c => c.Key, c => c.Value);
            child = new List<ThumbnailObject<TOriginalObj>>();
        }

        internal override PossibleStateThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes)
        {
            ThumbnailObject<TOriginalObj> NewChild = new ThumbnailObject<TOriginalObj>(_Precursor, this, Changes);
            /*{
                _Precursor = this._Precursor,
                Parent = this,
                Dict = Changes,
                child = new List<ThumbnailObject<TOriginalObj>>()
            };*/

            if (Changes.Count != 0)
            {
                foreach (var update in Changes)
                    NewChild.Dict[update.Key] = update.Value;

                NewChild.FigureCheckSum();
            }
            else
                NewChild.CheckSum = this.CheckSum;

            this.child.Add(NewChild);
            return NewChild;
        }
    }

    internal class ThumbnailObjectPrecursor<TOriginalObj> : PossibleStateThumbnailObject where TOriginalObj : class
    {       
        readonly internal TOriginalObj _OriginalObj;
        internal override Type OriginalObjType => _OriginalObj.GetType();
        readonly SingleTypeOfDomein Model;
        internal override PossibleStateThumbnailObject Precursor { get { return this; } }
        protected readonly ushort[] _ValuesIndeksesKeys;
        internal override ushort[] ValuesIndeksesKeys
        {
            get { return Model.ValuesKeys; }
        }

        internal override object OriginalObj { get { return this._OriginalObj; } }


        public ThumbnailObjectPrecursor(TOriginalObj originalObj, IReadOnlyList<SingleTypeOfDomein> allTypes) : base()
        {
            this.Parent = null;
            this._OriginalObj = originalObj;
            this.Dict = new Dictionary<ushort, ValueType>();
            this.child = new List<PossibleStateThumbnailObject>();

            Type originalObjTypeCand = originalObj.GetType();
            do
            {
                this.Model = allTypes.Where(t => t.Type == originalObjTypeCand).First();
                originalObjTypeCand = originalObjTypeCand.BaseType;
            }
            while (this.Model is null && !(originalObjTypeCand is null));

            if (this.Model is null)
                throw new Exception();

            foreach (ValueOfThumbnail VOT in Model.CumulativeValues)
            {
                ValueType value;

                if (VOT.IsField)
                {
                    FieldInfo myFieldInfo = this.OriginalObjType.GetField(VOT.Name);

                    if (myFieldInfo is null)
                        myFieldInfo = this.OriginalObjType.GetField(VOT.Name, BindingFlags.Instance);

                    value = (ValueType)myFieldInfo.GetValue(OriginalObj);
                }
                else
                {
                    PropertyInfo propertyInfo = this.OriginalObjType.GetProperty(VOT.Name);

                    if (propertyInfo is null)
                        propertyInfo = this.OriginalObjType.GetProperty(VOT.Name, BindingFlags.Instance);

                    value = (ValueType)propertyInfo.GetValue(OriginalObj);
                }

                Dict.Add(VOT.ValueOfIndexesKey, value);
            }

            FigureCheckSum();
        }

        new public ValueType this[ushort key]
        {
            get
            {
                ValueOfThumbnail TempVOT = null;

                foreach(ValueOfThumbnail valueOfThumbnail in Model.Values)
                {
                    if (valueOfThumbnail.ValueOfIndexesKey == key)
                    {
                        TempVOT = valueOfThumbnail;
                        break;
                    }
                }

                if (TempVOT.IsField)
                    return (ValueType)OriginalObjType.GetField(TempVOT.Name).GetValue(OriginalObj);
                else
                    return (ValueType)OriginalObjType.GetProperty(TempVOT.Name).GetValue(OriginalObj);
            }
        }

        internal override PossibleStateThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes)
        {
            ThumbnailObject<TOriginalObj> NewChild = new ThumbnailObject<TOriginalObj>(this, this, Changes);
            /*{
                _Precursor = this,
                Parent = this,
                Dict = Changes,
                child = new List<ThumbnailObject<TOriginalObj>>()
            };*/

            if (Changes.Count != 0)
            {
                //foreach (KeyValuePair<ushort, ValueType> update in Changes)
                 //   NewChild.Dict[update.Key] = update.Value;

                NewChild.FigureCheckSum();
            }
            else
                NewChild.CheckSum = this.CheckSum;

            this.child.Add(NewChild);
            return NewChild;
        }
    }
}
