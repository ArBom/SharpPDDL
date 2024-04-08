using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    internal abstract class ThumbnailObject
    {
        internal Type OriginalObjType;
        internal Dictionary<ushort, ValueType> Dict;

        internal abstract ushort[] ValuesIndeksesKeys { get; }

        public abstract ValueType this[ushort key] { get; }
    }

    internal abstract class PossibleStateThumbnailObject : ThumbnailObject
    {     
        internal Type OriginalObjType;
        internal ThumbnailObject Precursor;
        internal PossibleStateThumbnailObject Parent;
        internal List<PossibleStateThumbnailObject> child;
        internal string CheckSum;

        internal void FigureCheckSum()
        {
            string MD5input = "";

            //TODO
            for (int arrayCounter = 0; arrayCounter != ValuesIndeksesKeys.Count(); ++arrayCounter)
            {
                MD5input = MD5input + ValuesIndeksesKeys[arrayCounter].ToString() + ";";
            }

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MD5input);

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
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

        abstract internal void CreateChild(Dictionary<ushort, ValueType> Changes);
    }

    internal class ThumbnailObject<TOriginalObj> : PossibleStateThumbnailObject where TOriginalObj : class
    {
        new ThumbnailObject<TOriginalObj> Precursor;
        new internal List<ThumbnailObject<TOriginalObj>> child;
        new internal Type OriginalObjType => Precursor.OriginalObjType;

        internal override ushort[] ValuesIndeksesKeys => Precursor.ValuesIndeksesKeys;

        internal override void CreateChild(Dictionary<ushort, ValueType> Changes)
        {
            ThumbnailObject<TOriginalObj> NewChild = new ThumbnailObject<TOriginalObj>()
            {
                Precursor = this.Precursor,
                Parent = this,
                Dict = Changes,
                child = new List<ThumbnailObject<TOriginalObj>>()
            };

            if (Changes.Count != 0)
            {
                foreach (var update in Changes)
                    NewChild.Dict[update.Key] = update.Value;

                NewChild.FigureCheckSum();
            }
            else
                NewChild.CheckSum = this.CheckSum;

            this.child.Add(NewChild);
        }
    }

    internal class ThumbnailObjectPrecursor<TOriginalObj> : PossibleStateThumbnailObject where TOriginalObj : class
    {       
        readonly internal TOriginalObj OriginalObj;
        new internal Type OriginalObjType => OriginalObj.GetType();
        readonly SingleTypeOfDomein Model;
        new ThumbnailObjectPrecursor<TOriginalObj> Precursor => this;
        protected readonly ushort[] _ValuesIndeksesKeys;
        internal override ushort[] ValuesIndeksesKeys
        {
            get { return Model.ValuesKeys; }
        }

        public ThumbnailObjectPrecursor(TOriginalObj originalObj, IReadOnlyList<SingleTypeOfDomein> allTypes) : base()
        {
            this.Parent = null;
            this.OriginalObj = originalObj;
            this.Dict = new Dictionary<ushort, ValueType>();

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

        internal override void CreateChild(Dictionary<ushort, ValueType> Changes)
        {
            ThumbnailObject<TOriginalObj> NewChild = new ThumbnailObject<TOriginalObj>()
            {
                Precursor = this.Precursor,
                Parent = this,
                Dict = Changes,
                child = new List<ThumbnailObject<TOriginalObj>>()
            };

            if (Changes.Count != 0)
            {
                foreach (var update in Changes)
                    NewChild.Dict[update.Key] = update.Value;

                NewChild.FigureCheckSum();
            }
            else
                NewChild.CheckSum = this.CheckSum;

            this.child.Add(NewChild);
        }
    }
}
