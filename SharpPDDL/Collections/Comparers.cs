using System.Collections.Generic;

namespace SharpPDDL
{
    internal class SortCumulativedTransitionChargeHelper : IComparer<Crisscross>
    {
        public int Compare(Crisscross c1, Crisscross c2)
        {
            int CumulativedTransitionChargeRes = c1.CumulativedTransitionCharge.CompareTo(c2.CumulativedTransitionCharge);

            // In case of the same CumulativedTransitionCharges' value compare CheckSums
            if (CumulativedTransitionChargeRes == 0)
            {
                if (c1.Content is null || c2.Content is null)
                    return 0;

                return c1.Content.CheckSum.CompareTo(c2.Content.CheckSum);
            }
                
            return CumulativedTransitionChargeRes;
        }
    }

    internal class TupleCo : IComparer<KeyValuePair<Crisscross, List<CrisscrossChildrenCon>>>
    {
        public int Compare(KeyValuePair<Crisscross, List<CrisscrossChildrenCon>> c1, KeyValuePair<Crisscross, List<CrisscrossChildrenCon>> c2) 
            => c1.Key.CumulativedTransitionCharge.CompareTo(c2.Key.CumulativedTransitionCharge);
    }
}
