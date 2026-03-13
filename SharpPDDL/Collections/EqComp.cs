using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class EqComp : IEqualityComparer<Crisscross>
    {
        public bool Equals(Crisscross x, Crisscross y)
        {
            if ((x.Content is null) || (y.Content is null))
                return false;

            if (x.Content.CheckSum != y.Content.CheckSum)
                return false;

            for (ushort ListCounter = 0; ListCounter != x.Content.ThumbnailObjects.Count; ++ListCounter)
            {
                if (Convert.ToBase64String(x.Content.ThumbnailObjects[ListCounter].CheckSum) == Convert.ToBase64String(y.Content.ThumbnailObjects[ListCounter].CheckSum))
                    continue;
                else
                    return false;
            }

            return true;
        }

        public int GetHashCode(Crisscross obj)
        {
            if (obj.Content is null)
                return 0;

            return obj.Content.CheckSum.GetHashCode();
        }
    }
}
