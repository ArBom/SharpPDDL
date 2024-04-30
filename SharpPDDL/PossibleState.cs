using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    internal class PossibleState
    {
        internal List<PossibleStateThumbnailObject> ThumbnailObjects;
        internal List<PossibleStateThumbnailObject> ChangedThumbnailObjects;

        internal PossibleState()
        {
            ThumbnailObjects = new List<PossibleStateThumbnailObject>();
        }
    }
}
