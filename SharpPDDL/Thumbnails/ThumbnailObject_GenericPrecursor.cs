using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace SharpPDDL
{
    internal class ThumbnailObjectPrecursor<TOriginalObj> : ThumbnailObject
        where TOriginalObj : class
    {
        readonly internal TOriginalObj _OriginalObj;
        internal override Type OriginalObjType => _OriginalObj.GetType();
        internal readonly SingleTypeOfDomein Model;
        public override ThumbnailObject Precursor => this;
        internal override ushort[] ValuesIndeksesKeys => Model.ValuesKeys;

        internal override object OriginalObj => this._OriginalObj;

        public ThumbnailObjectPrecursor(ThumbnailObject brokenEl)
        {
            this.Model = ((ThumbnailObjectPrecursor<TOriginalObj>)brokenEl.Precursor).Model;
            this.Parent = null;
            this._OriginalObj = (TOriginalObj)brokenEl.Precursor.OriginalObj;
            this.Dict = new Dictionary<ushort, ValueType>();
            //this.child = new List<ThumbnailObject>();

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
            //this.child = new List<ThumbnailObject>();

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

        //todo
        /*new public ValueType this[ushort key]
        {
            get
            {
                Value TempVOT = null;

                foreach (Value valueOfThumbnail in Model.Values)
                    if (valueOfThumbnail.ValueOfIndexesKey == key)
                    {
                        TempVOT = valueOfThumbnail;
                        break;
                    }

                if (TempVOT.IsField)
                    return (ValueType)OriginalObjType.GetField(TempVOT.Name).GetValue(OriginalObj);
                else
                    return (ValueType)OriginalObjType.GetProperty(TempVOT.Name).GetValue(OriginalObj);
            }
        }*/

        internal override ThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes)
        {
            ThumbnailObject<TOriginalObj> NewChild = new ThumbnailObject<TOriginalObj>(this, this, Changes);

            if (Changes.Any())
                NewChild.FigureCheckSum();
            else
                NewChild.CheckSum = this.CheckSum;

            //this.child.Add(NewChild);
            return NewChild;
        }
    }
}
