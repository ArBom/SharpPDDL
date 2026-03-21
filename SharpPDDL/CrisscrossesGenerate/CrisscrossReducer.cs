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

        private SortedList<string, Crisscross> _IndexedStates;

        internal Task BuildingNewCrisscross;
        internal bool IsWaiting = true;
        internal Action NoNewData;

        internal AutoResetEvent ReducingCrisscrossARE;
        internal ICollection<Crisscross> PossibleToCrisscrossReduce;
        private readonly object CrisscrossReduceLocker;

        internal ConcurrentQueue<Crisscross> PossibleGoalRealization;
        AutoResetEvent CheckingGoalRealizationARE;

        private CancellationToken cancellationToken;

        internal CrisscrossReducer(Crisscross states, AutoResetEvent ReducingCrisscrossARE, ICollection<Crisscross> PossibleToCrisscrossReduce, object CrisscrossReduceLocker, ConcurrentQueue<Crisscross> PossibleGoalRealization, AutoResetEvent CheckingGoalRealizationARE)
        {
            this.states = states;
            IndexedStates = null;

            this.ReducingCrisscrossARE = ReducingCrisscrossARE;
            this.PossibleToCrisscrossReduce = PossibleToCrisscrossReduce;
            this.CrisscrossReduceLocker = CrisscrossReduceLocker;

            this.PossibleGoalRealization = PossibleGoalRealization;
            this.CheckingGoalRealizationARE = CheckingGoalRealizationARE;
        }

        internal SortedList<string, Crisscross> IndexedStates
        {
            get { return _IndexedStates; }
            set
            {
                if (BuildingNewCrisscross?.Status == (TaskStatus.Running | TaskStatus.WaitingForChildrenToComplete))
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Error, 147, GloCla.ResMan.GetString("E36"));
                    return;
                }

                if (!(_IndexedStates is null))
                    if (_IndexedStates.Any())
                    {
                        this._IndexedStates = value;
                        return;
                    }

                _IndexedStates = new SortedList<string, Crisscross>();

                CrisscrossRefEnum crisscrossRefEnum = new CrisscrossRefEnum(ref states);
                while (crisscrossRefEnum.MoveNext())
                {
                    _IndexedStates.Add(crisscrossRefEnum.Current.Content.CheckSum, crisscrossRefEnum.Current);
                }
            }
        }

        internal void Start(CancellationToken token)
        {
            cancellationToken = token;
            BuildingNewCrisscross = new Task(() => TryMergeCrisscross());
            BuildingNewCrisscross.Start();
        }

        private bool CheckPossibleOfRealization()
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            lock (CrisscrossReduceLocker)
                return PossibleToCrisscrossReduce.Any();
        }

        internal bool MergeCrisscross(ref Crisscross crisscrossToMerge)
        {
            if (!_IndexedStates.ContainsKey(crisscrossToMerge.Content.CheckSum))
                return false;

            Crisscross Candidate = _IndexedStates[crisscrossToMerge.Content.CheckSum];
            if (Candidate.Content.Compare(ref crisscrossToMerge.Content))
            {
                Crisscross.Merge(ref Candidate, ref crisscrossToMerge);
                return true;
            }

            bool Merged = false;
            CrisscrossRefEnum crisscrossRefEnum = new CrisscrossRefEnum(ref states);
            while (crisscrossRefEnum.MoveNext())
            //foreach (ref Crisscross s in states) it throw cs1510
            {
                if (crisscrossRefEnum.Current.Content.Compare(ref crisscrossToMerge.Content))
                {
                    if (crisscrossRefEnum.Current.Root is null)
                        Crisscross.MergeK(ref states, ref crisscrossToMerge);
                    else
                        Crisscross.Merge(ref crisscrossRefEnum.Current, ref crisscrossToMerge);

                    Merged = true;
                    break;
                }
            }

            return Merged;
        }

        private void TryMergeCrisscross()
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 66, GloCla.ResMan.GetString("Sa8"), Task.CurrentId);

            while (!cancellationToken.IsCancellationRequested)
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

                    if (MergeCrisscross(ref possibleToCrisscrossReduce))
                        continue;

                    lock (_IndexedStates)
                        _IndexedStates[possibleToCrisscrossReduce.Content.CheckSum] = possibleToCrisscrossReduce;

                    PossibleGoalRealization?.Enqueue(possibleToCrisscrossReduce);
                    CheckingGoalRealizationARE?.Set();
                }

                NoNewData?.BeginInvoke(null, null);
                IsWaiting = true;
            }

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 67, GloCla.ResMan.GetString("Sp8"), Task.CurrentId);
        }
    }
}
