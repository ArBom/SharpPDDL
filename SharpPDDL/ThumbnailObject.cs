using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    abstract class ThumbnailObject
    {
        protected ThumbnailObject parent;
        public Dictionary<string, ValueType> dict;

        protected ThumbnailObject()
        {
            dict = new Dictionary<string, ValueType>();
        }
    }

    class ThumbnailObject<TOriginalObj> : ThumbnailObject
    {
        TOriginalObj OriginalObj;
    }
}
