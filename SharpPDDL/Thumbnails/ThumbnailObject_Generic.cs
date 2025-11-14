using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPDDL
{
    internal class ThumbnailObject<TOriginalObj> : ThumbnailObject
        where TOriginalObj : class
    {
        internal override object OriginalObj => _Precursor.OriginalObj;
        private readonly ThumbnailObject _Precursor;
        public override ThumbnailObject Precursor => _Precursor;
        //new internal List<ThumbnailObject<TOriginalObj>> child;
        internal override Type OriginalObjType => _Precursor.OriginalObjType;

        internal override ushort[] ValuesIndeksesKeys => _Precursor.ValuesIndeksesKeys;

        internal ThumbnailObject(ThumbnailObject Precursor, ThumbnailObject parent, List<KeyValuePair<ushort, ValueType>> changes)
        {
            this._Precursor = Precursor;
            this.Parent = parent;
            this.Dict = changes.ToDictionary(c => c.Key, c => c.Value);
            //child = new List<ThumbnailObject<TOriginalObj>>();
        }

        internal override ThumbnailObject CreateChild(List<KeyValuePair<ushort, ValueType>> Changes)
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

            //this.child.Add(NewChild);
            return NewChild;
        }
    }
}
