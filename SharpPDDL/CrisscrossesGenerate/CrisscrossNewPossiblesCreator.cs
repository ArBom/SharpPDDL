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
        protected readonly ICollection<Crisscross> PossibleNewCrisscrossCre;
        protected readonly object PossibleNewSrisscrossCreLocker;

        protected readonly AutoResetEvent ReducingCrisscrossARE;
        protected readonly ICollection<Crisscross> PossibleToCrisscrossReduce;
        protected readonly object CrisscrossReduceLocker;

        internal CrisscrossNewPossiblesCreator(List<ActionPDDL> actions, AutoResetEvent BuildingNewCrisscrossARE, ICollection<Crisscross> PossibleNewCrisscrossCre, object PossibleNewSrisscrossCreLocker, AutoResetEvent ReducingCrisscrossARE, ICollection<Crisscross> PossibleToCrisscrossReduce, object CrisscrossReduceLocker)
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
                        object ResultOfCheck = Actions[actionPos].InstantActionPDDLSimplified.DynamicInvoke(SetToCheck);

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

                        lock (CrisscrossReduceLocker)
                        {
                            PossibleToCrisscrossReduce.Add(AddedItem);
                        }
                        ReducingCrisscrossARE.Set();
                    }

                    void CheckAllPos(int actionPos, ThumbnailObject[] SetToCheck, int currentIndex, IList<ThumbnailObject>[] possibleOld, IList<ThumbnailObject>[] possibleNew, bool UntilNowOnlyOld)
                    {
                        ThumbnailObject[] ToRemove = new ThumbnailObject[0];
                        
                        if (currentIndex != 0)
                        {
                            ToRemove = new ThumbnailObject[currentIndex];
                            Array.Copy(SetToCheck, ToRemove, currentIndex);
                        }

                        IEnumerable<ThumbnailObject> newone = possibleNew[currentIndex].Except(ToRemove);

                        foreach (ThumbnailObject thisOne in newone)
                        {
                            SetToCheck[currentIndex] = thisOne;

                            if (currentIndex == SetToCheck.Length - 1)
                                TryActionPossibility(SetToCheck, actionPos);
                            else
                                CheckAllPos(actionPos, SetToCheck, currentIndex + 1, possibleOld, possibleNew, false);
                        }

                        if (UntilNowOnlyOld)
                            if (SetToCheck.Length == currentIndex + 1)
                            {
                                //return;
                            }

                        newone = possibleOld[currentIndex].Except(ToRemove);

                        foreach (ThumbnailObject thisOne in newone)
                        {
                            SetToCheck[currentIndex] = thisOne;

                            if (currentIndex == SetToCheck.Length - 1)
                                TryActionPossibility(SetToCheck, actionPos);
                            else
                                CheckAllPos(actionPos, SetToCheck, currentIndex + 1, possibleOld, possibleNew, UntilNowOnlyOld);
                        }
                    }

                    void CheckTheOldPoss(int actionPos, IEnumerable<ThumbnailObject>[] possibleCha)
                    {
                        if (stateToCheck.Root is null)
                            return;
                        //stateToCheck.Root.Children
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

                        CheckTheOldPoss(actionPos, possibleCha);

                        if (possibleCha.All(p => !p.Any()))
                        {
                            //tylko sprawdzenie poprzednich i wyjście
                        }

                        ThumbnailObject[] SetToCheck = new ThumbnailObject[Actions[actionPos].InstantActionParamCount];
                        IList<ThumbnailObject>[] possibleNew = new IList<ThumbnailObject>[SetToCheck.Length];
                        IList<ThumbnailObject>[] possibleOld = new IList<ThumbnailObject>[SetToCheck.Length];

                        for (int i = 0; i != SetToCheck.Length; i++)
                        {
                            possibleNew[i] = possibleCha[i].ToList().AsReadOnly();
                            possibleOld[i] = possibleAll[i].Except(possibleNew[i]).ToList().AsReadOnly();
                        }

                        CheckAllPos(actionPos, SetToCheck, 0, possibleOld, possibleNew, true);
                    }
                }
                NoNewData.BeginInvoke(null, null);
                IsWaiting = true;
            }

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 65, GloCla.ResMan.GetString("Sp7"), Task.CurrentId);
        }
    }
}
