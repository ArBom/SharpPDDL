using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL.CrisscrossesGenerate
{
    class CrisscrossReducer
    {
        Crisscross states;

        internal Task BuildingNewCrisscross;
        internal bool IsWaiting = true;
        internal Action NoNewData;

        internal AutoResetEvent ReducingCrisscrossARE;
        List<Crisscross> PossibleToCrisscrossReduce;
        object CrisscrossReduceLocker;

        ConcurrentQueue<Crisscross> PossibleGoalRealization;
        AutoResetEvent CheckingGoalRealizationARE;

        internal CrisscrossReducer(Crisscross states, AutoResetEvent ReducingCrisscrossARE, List<Crisscross> PossibleToCrisscrossReduce, object CrisscrossReduceLocker, ConcurrentQueue<Crisscross> PossibleGoalRealization, AutoResetEvent CheckingGoalRealizationARE)
        {
            this.states = states;

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

        private void TryMergeCrisscross(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                ReducingCrisscrossARE.WaitOne();
                IsWaiting = false;

                while (PossibleToCrisscrossReduce.Count != 0 && !token.IsCancellationRequested)
                {
                    Crisscross possibleToCrisscrossReduce;

                    try
                    {
                        lock (CrisscrossReduceLocker)
                        {
                            possibleToCrisscrossReduce = PossibleToCrisscrossReduce[0];
                            PossibleToCrisscrossReduce.RemoveAt(0);
                        }
                    }
                    catch
                    {
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
                            {
                                Crisscross.MergeK(ref states, ref possibleToCrisscrossReduce);
                            }
                            else
                            {
                                Crisscross.Merge(ref crisscrossRefEnum.Current, ref possibleToCrisscrossReduce);
                            }

                            Merged = true;
                            break;
                        }
                    }

                    if (Merged)
                    {
                        continue;
                    }

                    PossibleGoalRealization.Enqueue(possibleToCrisscrossReduce);
                    CheckingGoalRealizationARE.Set();
                }
                NoNewData.BeginInvoke(null, null);
                IsWaiting = true;
            }
        }
    }
}
