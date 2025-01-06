using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace SharpPDDL
{
    public delegate void ListOfString(List<List<string>> planGenerated);
    internal delegate void FoundSols(KeyValuePair<Crisscross, List<GoalPDDL>> foundSolutions);
    internal delegate void CurrentMinCumulativeCostUpdate(UInt32 foundSolutions);

    class DomainPlanner
    {
    }
}
