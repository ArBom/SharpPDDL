using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Security.Cryptography;
using System.Diagnostics;

namespace SharpPDDL
{
    internal abstract class PossibleStateThumbnailObject
    {
        internal abstract object OriginalObj { get; }
        internal abstract Type OriginalObjType { get; }
        protected Dictionary<ushort, ValueType> Dict;
        internal abstract ushort[] ValuesIndeksesKeys { get; }
        public abstract PossibleStateThumbnailObject Precursor { get; }
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

        abstract internal PossibleStateThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes);

        internal bool Compare(PossibleStateThumbnailObject With)
        {
            if (!this.OriginalObj.Equals(With.OriginalObj))
                return false;

            for (ushort arrayCounter = 0; arrayCounter != ValuesIndeksesKeys.Count(); ++arrayCounter)
            {
                if (!this[ValuesIndeksesKeys[arrayCounter]].Equals(With[ValuesIndeksesKeys[arrayCounter]]))
                    return false;
            }

            return true;
        }

    }

    internal class ThumbnailObject<TOriginalObj> : PossibleStateThumbnailObject 
        where TOriginalObj : class
    {
        internal override object OriginalObj { get { return _Precursor.OriginalObj; } }
        internal PossibleStateThumbnailObject _Precursor;
        public override PossibleStateThumbnailObject Precursor { get { return _Precursor; } }
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

            if (Changes.Any())
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

    internal class ThumbnailObjectPrecursor<TOriginalObj> : PossibleStateThumbnailObject 
        where TOriginalObj : class
    {       
        readonly internal TOriginalObj _OriginalObj;
        internal override Type OriginalObjType => _OriginalObj.GetType();
        internal readonly SingleTypeOfDomein Model;
        public override PossibleStateThumbnailObject Precursor { get { return this; } }
        internal override ushort[] ValuesIndeksesKeys
        {
            get { return Model.ValuesKeys; }
        }

        internal override object OriginalObj { get { return this._OriginalObj; } }

        public ThumbnailObjectPrecursor(PossibleStateThumbnailObject brokenEl)
        {
            this.Model = ((ThumbnailObjectPrecursor<TOriginalObj>)brokenEl.Precursor).Model;
            this.Parent = null;
            this._OriginalObj = (TOriginalObj)brokenEl.Precursor.OriginalObj;
            this.Dict = new Dictionary<ushort, ValueType>();
            this.child = new List<PossibleStateThumbnailObject>();

            foreach (Value VOT in Model.CumulativeValues)
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

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 37, GloCla.ResMan.GetString("I4"), OriginalObjType.ToString(), CheckSum);
        }

        public ThumbnailObjectPrecursor(TOriginalObj originalObj, IReadOnlyList<SingleTypeOfDomein> allTypes) : base()
        {
            this.Parent = null;
            this._OriginalObj = originalObj;
            this.Dict = new Dictionary<ushort, ValueType>();
            this.child = new List<PossibleStateThumbnailObject>();

            Type originalObjTypeCand = originalObj.GetType();
            do
            {
                IEnumerator<SingleTypeOfDomein> ModelsEnum = allTypes.Where(t => t.Type == originalObjTypeCand).GetEnumerator();
                if (ModelsEnum.MoveNext())
                    this.Model = ModelsEnum.Current;

                originalObjTypeCand = originalObjTypeCand.BaseType;
            }
            while (this.Model is null && !(originalObjTypeCand is null));

            if (this.Model is null)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 38, GloCla.ResMan.GetString("C5"), OriginalObjType.ToString());
                throw new Exception();
            }

            foreach (Value VOT in Model.CumulativeValues)
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

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 37, GloCla.ResMan.GetString("I4"), OriginalObjType.ToString(), CheckSum);
        }

        new public ValueType this[ushort key]
        {
            get
            {
                Value TempVOT = null;

                foreach(Value valueOfThumbnail in Model.Values)
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

            if (Changes.Any())
            {
                NewChild.FigureCheckSum();
            }
            else
                NewChild.CheckSum = this.CheckSum;

            this.child.Add(NewChild);
            return NewChild;
        }
    }
}
