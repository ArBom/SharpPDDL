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

        /// <summary>
        /// refresh the ThumbnailObject as new precursor
        /// </summary>
        /// <param name="thumbnailObject">previous version of returned ThumbnailObject</param>
        /// <param name="IsBroken">Some unexpected value in previous version</param>
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

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 37, GloCla.ResMan.GetString("I4"), OriginalObjType.ToString(), BitConverter.ToString(CheckSum));
        }

        /// <summary>
        /// Create a Thumbnail (Precursor) from oryginal object
        /// </summary>
        /// <param name="originalObj">Oryginal object - source of thumbnail</param>
        /// <param name="allTypes">types used in domain</param>
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

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 37, GloCla.ResMan.GetString("I4"), OriginalObjType.ToString(), BitConverter.ToString(CheckSum));
        }

        private ValueType HandleNonValueMember(object ToHandle, ushort index)
        {
            //If member is empty now - return zero
            if (ToHandle is null)
                return IntPtr.Zero;

            //Make handle of member
            GCHandle OryginalObjField = GCHandle.Alloc(ToHandle, GCHandleType.Normal);
            //Add it to collection of handled members
            ObjHandles.Add(index, OryginalObjField);
            //return "pointer"
            IntPtr ToRet = (IntPtr)OryginalObjField;
            return ToRet;
        }

        private ValueType ReadValueOfField(Value VOT)
        {
            //Take FieldInfo of reading value
            FieldInfo myFieldInfo = this.OriginalObjType.GetField(VOT.Name);

            if (myFieldInfo is null)
                myFieldInfo = this.OriginalObjType.GetField(VOT.Name, BindingFlags.Instance);

            //read value from oryginal object...
            object ObjField = myFieldInfo.GetValue(OriginalObj);

            //return value if it is ValueType
            if (VOT.Type.IsValueType)
                return (ValueType)ObjField;

            //of handle it and return "pointer" if it is non-ValueType
            return HandleNonValueMember(ObjField, VOT.ValueOfIndexesKey);
        }

        private ValueType ReadValueOfProperty(Value VOT)
        {
            //Take PropertyInfo of reading value
            PropertyInfo propertyInfo = this.OriginalObjType.GetProperty(VOT.Name);

            if (propertyInfo is null)
                propertyInfo = this.OriginalObjType.GetProperty(VOT.Name, BindingFlags.Instance);

            //read value from oryginal object...
            object ObjProperty = propertyInfo.GetValue(OriginalObj);

            //return value if it is ValueType
            if (VOT.Type.IsValueType)
                return (ValueType)ObjProperty;

            //of handle it and return "pointer" if it is non-ValueType
            return HandleNonValueMember(ObjProperty, VOT.ValueOfIndexesKey);
        }

        protected void ReadAllVals()
        {
            //create collection of handled members if it will be in use
            if (Model.CumulativeValues.Any(cv => cv.Name == GloCla.PointerVName || !cv.Type.IsValueType))
                ObjHandles = new Dictionary<ushort, GCHandle>();

            //read all value based on model values list
            foreach (Value VOT in Model.CumulativeValues)
            {
                ValueType value;

                if (VOT.Name == GloCla.PointerVName)
                    //read pointer of oryginal object and use it as value
                    value = HandleNonValueMember(_OriginalObj, 0);
                else if (VOT.IsField)
                    //read value of field member
                    value = ReadValueOfField(VOT);
                else
                    //read value of property member
                    value = ReadValueOfProperty(VOT);

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
            if (this.ObjHandles is null || AnotherThObPrec.ObjHandles is null)
                return;

            var theseObjHandles = this.ObjHandles.ToList();
            var thoseObjHandles = AnotherThObPrec.ObjHandles.ToList();

            foreach (var thisOH in theseObjHandles)
            {
                foreach (var thatOH in thoseObjHandles)
                {
                    object theseObj = thatOH.Value.Target;

                    if (this.ObjHandles.ContainsKey(0))
                        if (this.ObjHandles[0].Target.Equals(theseObj))
                        {
                            AnotherThObPrec.ChangeHandle(thatOH.Key, this.ObjHandles[0]);
                            continue;
                        }

                    object TargetObjLoop = thisOH.Value.Target;

                    if (TargetObjLoop is null)
                        continue;

                    if (TargetObjLoop.Equals(theseObj))
                        ChangeHandle(thisOH.Key, thatOH.Value);
                }
            }
        }

        private void ChangeHandle(ushort NewKey, GCHandle NewHandle)
        {
            if (!Dict.Keys.Any(k => k == NewKey))
            {
                throw new Exception();
            }

            ObjHandles[NewKey] = NewHandle;
            Dict[NewKey] = (IntPtr)NewHandle;
            FigureCheckSum();
        }

        ~ThumbnailObjectPrecursor()
        {
            if (ObjHandles is null)
                return;

            if (this.ObjHandles.ContainsKey(0))
                this.ObjHandles[0].Free();

            this.ObjHandles = null;
        }
    }
}
