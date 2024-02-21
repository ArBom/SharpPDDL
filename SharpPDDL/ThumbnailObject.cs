using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    internal abstract class ThumbnailObject
    {
        internal ThumbnailObject Parent;
        internal List<ThumbnailObject> child;
        internal Type OriginalObjType;
        protected Dictionary<ushort, ValueType> Dict;
        internal string CheckSum;

        internal abstract ushort[] ValuesIndeksesKeys { get; }

        internal void FigureCheckSum()
        {
            string MD5input = String.Empty;

            for (int arrayCounter = 0; arrayCounter != ValuesIndeksesKeys.Count(); ++arrayCounter)
            {
                MD5input = MD5input + ValuesIndeksesKeys[arrayCounter].ToString() + ";";
            }

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MD5input);

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                CheckSum = Convert.ToBase64String(hashBytes);
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

            // updates if exists, adds if doesn't exist
            set { Dict[key] = value; }
        }

        abstract internal void CreateChild(Dictionary<ushort, ValueType> Changes);
    }

    internal class ThumbnailObject<TOriginalObj> : ThumbnailObject where TOriginalObj : class
    {
        ThumbnailObjectPrecursor<TOriginalObj> Precursor;
        new internal ThumbnailObject<TOriginalObj> Parent;
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

    internal class ThumbnailObjectPrecursor<TOriginalObj> : ThumbnailObject where TOriginalObj : class
    {
        readonly internal TOriginalObj OriginalObj;
        new internal Type OriginalObjType => typeof(TOriginalObj);
        readonly SingleType Model;
        protected readonly ushort[] _ValuesIndeksesKeys;
        internal override ushort[] ValuesIndeksesKeys
        {
            get { return Model.ValuesKeys; }
        }

        internal ThumbnailObjectPrecursor(TOriginalObj originalObj, IReadOnlyList<SingleType> allTypes)
        {
            this.Parent = null;
            this.OriginalObj = originalObj;

            this.Model = allTypes.Where(t => t.Type == typeof(TOriginalObj)).First();
            //todo co jak null

            foreach (ValueOfThumbnail VOT in Model.Values)
            {
                ValueType value;

                if (VOT.IsField)
                {
                    FieldInfo myFieldInfo = this.OriginalObjType.GetField(VOT.Name);

                    if (myFieldInfo is null)
                        myFieldInfo = this.OriginalObjType.GetField(VOT.Name, BindingFlags.Public | BindingFlags.Instance);

                    value = (ValueType)myFieldInfo.GetValue(myFieldInfo);
                }
                else
                {
                    PropertyInfo propertyInfo = this.OriginalObjType.GetProperty(VOT.Name);

                    if (propertyInfo is null)
                        propertyInfo = this.OriginalObjType.GetProperty(VOT.Name, BindingFlags.Public | BindingFlags.Instance);

                    value = (ValueType)propertyInfo.GetValue(propertyInfo);
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
            throw new NotImplementedException();
        }
    }
}
