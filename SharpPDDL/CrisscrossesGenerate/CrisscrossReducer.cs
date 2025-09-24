using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    class CrisscrossReducer
    {
        Crisscross states;
        private SortedList<string, Crisscross> IndexedStates;

        internal Task BuildingNewCrisscross;
        internal bool IsWaiting = true;
        internal Action NoNewData;

        internal AutoResetEvent ReducingCrisscrossARE;
        ICollection<Crisscross> PossibleToCrisscrossReduce;
        object CrisscrossReduceLocker;

        ConcurrentQueue<Crisscross> PossibleGoalRealization;
        AutoResetEvent CheckingGoalRealizationARE;

        internal CrisscrossReducer(Crisscross states, AutoResetEvent ReducingCrisscrossARE, ICollection<Crisscross> PossibleToCrisscrossReduce, object CrisscrossReduceLocker, ConcurrentQueue<Crisscross> PossibleGoalRealization, AutoResetEvent CheckingGoalRealizationARE)
        {
            this.states = states;
            IndexStates();

            this.ReducingCrisscrossARE = ReducingCrisscrossARE;
            this.PossibleToCrisscrossReduce = PossibleToCrisscrossReduce;
            this.CrisscrossReduceLocker = CrisscrossReduceLocker;

            this.PossibleGoalRealization = PossibleGoalRealization;
            this.CheckingGoalRealizationARE = CheckingGoalRealizationARE;
        }

        internal void Start(CancellationToken token)
        {
            BuildingNewCrisscross = new Task(() => TryMergeCrisscross(token));
            BuildingNewCrisscross.Start();
        }

        private void IndexStates()
        {
            IndexedStates = new SortedList<string, Crisscross>();

            CrisscrossRefEnum crisscrossRefEnum = new CrisscrossRefEnum(ref states);
            while (crisscrossRefEnum.MoveNext())
            {
                IndexedStates.Add(crisscrossRefEnum.Current.Content.CheckSum, crisscrossRefEnum.Current);
            }
        }

        private void TryMergeCrisscross(CancellationToken token)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 66, GloCla.ResMan.GetString("Sa8"), Task.CurrentId);

            bool CheckPossibleOfRealization()
            {
                if (token.IsCancellationRequested)
                    return false;

                lock (CrisscrossReduceLocker)
                    return PossibleToCrisscrossReduce.Any();
            }

            while (!token.IsCancellationRequested)
            {
                ReducingCrisscrossARE.WaitOne();
                IsWaiting = false;

                while (CheckPossibleOfRealization())
                {
                    Crisscross possibleToCrisscrossReduce;

                    lock (CrisscrossReduceLocker)
                    {
                        try
                        {
                            possibleToCrisscrossReduce = PossibleToCrisscrossReduce.First();
                        }
                        catch
                        {
                            continue;
                        }

                        PossibleToCrisscrossReduce.Remove(possibleToCrisscrossReduce);
                    }

                    if (IndexedStates.ContainsKey(possibleToCrisscrossReduce.Content.CheckSum))
                    {
                        Crisscross Candidate = IndexedStates[possibleToCrisscrossReduce.Content.CheckSum];
                        if (Candidate.Content.Compare(ref possibleToCrisscrossReduce.Content))
                        {
                            Crisscross.Merge(ref Candidate, ref possibleToCrisscrossReduce);
                            continue;
                        }

                        bool Merged = false;
                        CrisscrossRefEnum crisscrossRefEnum = new CrisscrossRefEnum(ref states);
                        while (crisscrossRefEnum.MoveNext())
                        //foreach (ref Crisscross s in states) it throw cs1510
                        {
                            if (crisscrossRefEnum.Current.Content.Compare(ref possibleToCrisscrossReduce.Content))
                            {
                                if (crisscrossRefEnum.Current.Root is null)
                                    Crisscross.MergeK(ref states, ref possibleToCrisscrossReduce);
                                else
                                    Crisscross.Merge(ref crisscrossRefEnum.Current, ref possibleToCrisscrossReduce);

                                Merged = true;
                                break;
                            }
                        }

                        if (Merged)
                            continue;
                    }

                    IndexedStates.Add(possibleToCrisscrossReduce.Content.CheckSum, possibleToCrisscrossReduce);
                    PossibleGoalRealization?.Enqueue(possibleToCrisscrossReduce);
                    CheckingGoalRealizationARE.Set();
                }
                NoNewData?.BeginInvoke(null, null);
                IsWaiting = true;
            }

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 67, GloCla.ResMan.GetString("Sp8"), Task.CurrentId);
        }
    }
}
