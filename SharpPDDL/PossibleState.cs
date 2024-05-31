using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    internal class PossibleState
    {
        internal PossibleState PreviousPossibleState;
        internal List<PossibleStateThumbnailObject> ThumbnailObjects;
        internal List<PossibleStateThumbnailObject> ChangedThumbnailObjects;

        internal PossibleState(PossibleState PreviousPossibleState)
        {
            this.PreviousPossibleState = PreviousPossibleState;
            ThumbnailObjects = new List<PossibleStateThumbnailObject>();
            ChangedThumbnailObjects = new List<PossibleStateThumbnailObject>();
        }
    }
}
