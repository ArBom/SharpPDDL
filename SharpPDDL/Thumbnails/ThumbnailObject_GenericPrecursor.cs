using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
        protected ICollection<GCHandle> ObjHandles;

        internal override object OriginalObj => this._OriginalObj;

        public ThumbnailObjectPrecursor(ThumbnailObject thumbnailObject, bool IsBroken)
        {
            this.Model = ((ThumbnailObjectPrecursor<TOriginalObj>)thumbnailObject.Precursor).Model;
            this.Parent = null;
            this._OriginalObj = (TOriginalObj)thumbnailObject.Precursor.OriginalObj;
            this.Dict = new Dictionary<ushort, ValueType>();

            if (IsBroken)
                ReadAllVals();
            else
                foreach (Value VOT in Model.CumulativeValues)
                {
                    Dict.Add(VOT.ValueOfIndexesKey, thumbnailObject[VOT.ValueOfIndexesKey]);
                }

            FigureCheckSum();

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 37, GloCla.ResMan.GetString("I4"), OriginalObjType.ToString(), CheckSum);
        }

        public ThumbnailObjectPrecursor(TOriginalObj originalObj, IReadOnlyList<SingleTypeOfDomein> allTypes) : base()
        {
            this.Parent = null;
            this._OriginalObj = originalObj;
            this.Dict = new Dictionary<ushort, ValueType>();

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

            ReadAllVals();

            FigureCheckSum();

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 37, GloCla.ResMan.GetString("I4"), OriginalObjType.ToString(), CheckSum);
        }

        protected void ReadAllVals()
        {
            foreach (Value VOT in Model.CumulativeValues)
            {
                if (VOT.ValueOfIndexesKey == 0)
                {
                    ObjHandles = new List<GCHandle>();
                    GCHandle ObjHandle = GCHandle.Alloc(_OriginalObj, GCHandleType.Pinned);
                    IntPtr Addr = (IntPtr)ObjHandle;
                    ObjHandles.Add(ObjHandle);
                    Dict.Add(VOT.ValueOfIndexesKey, Addr);

                    continue;
                }

                ValueType value;

                if (VOT.IsField)
                {
                    FieldInfo myFieldInfo = this.OriginalObjType.GetField(VOT.Name);

                    if (myFieldInfo is null)
                        myFieldInfo = this.OriginalObjType.GetField(VOT.Name, BindingFlags.Instance);

                    object ObjField = myFieldInfo.GetValue(OriginalObj);

                    if (VOT.Type.IsValueType)
                        value = (ValueType)ObjField;
                    else
                    {
                        throw new NotImplementedException();
                        if (ObjField is null)
                            value = new IntPtr(0);
                        else
                        {
                            GCHandle OryginalObjField = GCHandle.Alloc(ObjField);
                            value = (IntPtr)OryginalObjField;
                            ObjHandles.Add(OryginalObjField);
                        }
                    }
                }
                else
                {
                    PropertyInfo propertyInfo = this.OriginalObjType.GetProperty(VOT.Name);

                    if (propertyInfo is null)
                        propertyInfo = this.OriginalObjType.GetProperty(VOT.Name, BindingFlags.Instance);

                    if (VOT.Type.IsValueType)
                        value = (ValueType)propertyInfo.GetValue(OriginalObj);
                    else
                        throw new NotImplementedException();
                        //patrz powyższy kod nieosiągalny
                }

                Dict.Add(VOT.ValueOfIndexesKey, value);
            }
        }

        internal override ThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes)
        {
            ThumbnailObject<TOriginalObj> NewChild = new ThumbnailObject<TOriginalObj>(this, this, Changes);

            if (Changes.Any())
                NewChild.FigureCheckSum();
            else
                NewChild.CheckSum = this.CheckSum;

            return NewChild;
        }

        ~ThumbnailObjectPrecursor()
        {
            if (!(ObjHandles is null))
                foreach (GCHandle Handle in ObjHandles)
                    Handle.Free();
        }
    }
}
