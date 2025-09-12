using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    class CrisscrossNewPossiblesCreator
    {
        internal readonly IReadOnlyList<ActionPDDL> Actions;

        internal Task BuildingNewCrisscross = null;
        internal bool IsWaiting = true;
        internal Action NoNewData;

        internal readonly AutoResetEvent BuildingNewCrisscrossARE;
        protected readonly SortedSet<Crisscross> PossibleNewCrisscrossCre;
        protected readonly object PossibleNewSrisscrossCreLocker;

        protected readonly AutoResetEvent ReducingCrisscrossARE;
        protected readonly List<Crisscross> PossibleToCrisscrossReduce;
        protected readonly object CrisscrossReduceLocker;

        internal CrisscrossNewPossiblesCreator(List<ActionPDDL> actions, AutoResetEvent BuildingNewCrisscrossARE, SortedSet<Crisscross> PossibleNewCrisscrossCre, object PossibleNewSrisscrossCreLocker, AutoResetEvent ReducingCrisscrossARE, List<Crisscross> PossibleToCrisscrossReduce, object CrisscrossReduceLocker)
        {
            this.Actions = actions;

            this.BuildingNewCrisscrossARE = BuildingNewCrisscrossARE;
            this.PossibleNewCrisscrossCre = PossibleNewCrisscrossCre;
            this.PossibleNewSrisscrossCreLocker = PossibleNewSrisscrossCreLocker;

            this.ReducingCrisscrossARE = ReducingCrisscrossARE;
            this.PossibleToCrisscrossReduce = PossibleToCrisscrossReduce;
            this.CrisscrossReduceLocker = CrisscrossReduceLocker;
        }

        internal void Start(CancellationToken token)
        {
            BuildingNewCrisscross = new Task(() => BuildNewState(token));
            BuildingNewCrisscross.Start();
        }

        protected void BuildNewState(CancellationToken token)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 64, GloCla.ResMan.GetString("Sa7"), Task.CurrentId);

            bool CheckPossibleOfRealization()
            {
                if (token.IsCancellationRequested)
                    return false;

                lock (PossibleNewSrisscrossCreLocker)
                    return PossibleNewCrisscrossCre.Any();
            }

            Crisscross stateToCheck;
            List<Crisscross> ToAddList = new List<Crisscross>();

            while (!token.IsCancellationRequested)
            {
                BuildingNewCrisscrossARE.WaitOne();
                IsWaiting = false;

                while (CheckPossibleOfRealization())
                {
                    lock (PossibleNewSrisscrossCreLocker)
                    {
                        try
                        {
                            stateToCheck = PossibleNewCrisscrossCre.First();
                        }
                        catch
                        {
                            continue;
                        }

                        PossibleNewCrisscrossCre.Remove(stateToCheck);
                    }

                    void TryActionPossibility(ThumbnailObject[] SetToCheck, int actionPos)
                    {
                        object ResultOfCheck = Actions[actionPos].InstantActionPDDL.DynamicInvoke(SetToCheck);

                        if (ResultOfCheck is null)
                            return;

                        var ResultOfCheckasList = (List<List<KeyValuePair<ushort, ValueType>>>)ResultOfCheck;
                        var ChangedThObs = new List<ThumbnailObject>();
                        object[] ActionArg = new object[SetToCheck.Length];

                        for (int j = 0; j < SetToCheck.Length; j++)
                        {
                            var ChangedThumbnailObject = SetToCheck[j].CreateChild(ResultOfCheckasList[j]);
                            ChangedThObs.Add(ChangedThumbnailObject);
                            ActionArg[j] = SetToCheck[j].OriginalObj;
                        }

                        PossibleState newPossibleState = new PossibleState(stateToCheck.Content, ChangedThObs);
                        uint ActionCost = (uint)Actions[actionPos].actionCost.CostExpressionFunc.DynamicInvoke(SetToCheck);
                        stateToCheck.Add(newPossibleState, actionPos, ActionArg, ActionCost, out Crisscross AddedItem);

                        ToAddList.Add(AddedItem);
                    }

                    void CheckAllPos(int actionPos, ThumbnailObject[] SetToCheck, int currentIndex, IEnumerable<ThumbnailObject>[] possibleAll, IEnumerable<ThumbnailObject>[] possibleCha)
                    {
                        List<ThumbnailObject> newone = new List<ThumbnailObject>(possibleAll[currentIndex]);

                        for (int j = 0; j != currentIndex; j++)
                            newone.Remove(SetToCheck[j]);

                        foreach (ThumbnailObject thisOne in newone)
                        {
                            SetToCheck[currentIndex] = thisOne;

                            if (currentIndex == SetToCheck.Length - 1)
                                TryActionPossibility( SetToCheck, actionPos);
                            else
                                CheckAllPos(actionPos, SetToCheck, currentIndex + 1, possibleAll, possibleCha);
                        }
                    }

                    for (int actionPos = 0; actionPos != Actions.Count(); actionPos++)
                    {
                        IEnumerable<ThumbnailObject>[] possibleAll = new IEnumerable<ThumbnailObject>[Actions[actionPos].InstantActionParamCount];
                        IEnumerable<ThumbnailObject>[] possibleCha = new IEnumerable<ThumbnailObject>[Actions[actionPos].InstantActionParamCount];
                        for (int i = 0; i != Actions[actionPos].InstantActionParamCount; i++)
                        {
                            possibleCha[i] = stateToCheck.Content.ChangedThumbnailObjects.Where(Actions[actionPos].Parameters[i].parametrPreconditionLambda.BuildFunc());
                            possibleAll[i] = stateToCheck.Content.ThumbnailObjects.Where(Actions[actionPos].Parameters[i].parametrPreconditionLambda.BuildFunc());
                        }

                        if (possibleAll.Any(p => !p.Any()))
                            continue;

                        if (possibleCha.All(p => !p.Any()))
                        {
                            //tylko sprawdzenie poprzednich i wyjście
                        }

                        ThumbnailObject[] SetToCheck = new ThumbnailObject[Actions[actionPos].InstantActionParamCount];

                        CheckAllPos(actionPos, SetToCheck, 0, possibleAll, possibleCha);
                    }

                    if (ToAddList.Any())
                    {
                        ToAddList = ToAddList.OrderBy(c => c.CumulativedTransitionCharge).ToList();
                        bool any;

                        lock (CrisscrossReduceLocker)
                        {
                            any = PossibleToCrisscrossReduce.Any();
                            PossibleToCrisscrossReduce.AddRange(ToAddList);

                            if(any)
                                PossibleToCrisscrossReduce.Sort(Crisscross.SortCumulativedTransitionCharge());

                            ReducingCrisscrossARE.Set();
                        }

                        ToAddList.Clear();
                    }
                }
                NoNewData.BeginInvoke(null, null);
                IsWaiting = true;
            }

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 65, GloCla.ResMan.GetString("Sp7"), Task.CurrentId);
        }
    }
}
