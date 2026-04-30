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
        internal Dictionary<ushort, GCHandle> ObjHandles;

        internal override object OriginalObj => this._OriginalObj;

        internal ThumbnailObjectPrecursor(ThumbnailObject thumbnailObject, bool IsBroken)
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

        internal ThumbnailObjectPrecursor(TOriginalObj originalObj, IReadOnlyList<SingleTypeOfDomein> allTypes) : base()
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
                    ObjHandles = new Dictionary<ushort, GCHandle>();
                    GCHandle ObjHandle = GCHandle.Alloc(_OriginalObj, GCHandleType.Normal);
                    IntPtr Addr = (IntPtr)ObjHandle;
                    ObjHandles.Add(0, ObjHandle);
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
                        if (ObjField is null)
                            value = IntPtr.Zero;
                        else
                        {
                            GCHandle OryginalObjField = GCHandle.Alloc(ObjField);
                            value = (IntPtr)OryginalObjField;
                            ObjHandles.Add(VOT.ValueOfIndexesKey, OryginalObjField);
                        }
                    }
                }
                else
                {
                    PropertyInfo propertyInfo = this.OriginalObjType.GetProperty(VOT.Name);

                    if (propertyInfo is null)
                        propertyInfo = this.OriginalObjType.GetProperty(VOT.Name, BindingFlags.Instance);

                    object ObjProperty = propertyInfo.GetValue(OriginalObj);

                    if (VOT.Type.IsValueType)
                        value = (ValueType)ObjProperty;
                    else
                    {
                        if (ObjProperty is null)
                            value = IntPtr.Zero;
                        else
                        {
                            GCHandle OryginalObjField = GCHandle.Alloc(ObjProperty);
                            value = (IntPtr)OryginalObjField;
                            ObjHandles.Add(VOT.ValueOfIndexesKey, OryginalObjField);
                        }
                    }
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

        internal void TryToChangeHandle(ThumbnailObjectPrecursor<object> AnotherThObPrec)
        {
            if (this.ObjHandles is null)
                return;

            var thisq = this.ObjHandles.Where(v => v.Key != 0).ToList();
            var theseq = AnotherThObPrec.ObjHandles.ToList();

            foreach(var iq in thisq)
            {
                foreach(var eq in theseq)
                {
                    object theseObj = eq.Value.Target;

                    if (this.ObjHandles[0].Target.Equals(theseObj))
                    {
                        AnotherThObPrec.ChangeHandle(eq.Key, ObjHandles[0]);
                    }

                    if(iq.Value.Target.Equals(theseObj))
                    {
                        ChangeHandle(iq.Key, eq.Value);
                    }
                }
            }
            //((GCHandle)(thisq.First().Target)).Target
        }

        private void ChangeHandle(ushort NewKey, GCHandle NewHandle)
        {
            if (!Dict.Keys.Any(k => k == NewKey))
            {
                throw new Exception();
            }

            ObjHandles[NewKey].Free();
            ObjHandles[NewKey] = NewHandle;
            Dict[NewKey] = (IntPtr)NewHandle;
            FigureCheckSum();
        }

        ~ThumbnailObjectPrecursor()
        {
            if (!(ObjHandles is null))
                foreach (var Handle in ObjHandles)
                    Handle.Value.Free();
        }
    }
}
