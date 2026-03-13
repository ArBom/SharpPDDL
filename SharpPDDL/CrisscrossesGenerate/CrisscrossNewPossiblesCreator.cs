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
        internal ICollection<Crisscross> PossibleNewCrisscrossCre;
        protected readonly object PossibleNewSrisscrossCreLocker;

        protected readonly AutoResetEvent ReducingCrisscrossARE;
        internal ICollection<Crisscross> PossibleToCrisscrossReduce;
        protected readonly object CrisscrossReduceLocker;

        private CancellationToken cancellationToken;

        internal CrisscrossNewPossiblesCreator(IList<ActionPDDL> actions, AutoResetEvent BuildingNewCrisscrossARE, ICollection<Crisscross> PossibleNewCrisscrossCre, object PossibleNewSrisscrossCreLocker, AutoResetEvent ReducingCrisscrossARE, ICollection<Crisscross> PossibleToCrisscrossReduce, object CrisscrossReduceLocker)
        {
            this.Actions = (IReadOnlyList<ActionPDDL>)actions;

            this.BuildingNewCrisscrossARE = BuildingNewCrisscrossARE;
            this.PossibleNewCrisscrossCre = PossibleNewCrisscrossCre;
            this.PossibleNewSrisscrossCreLocker = PossibleNewSrisscrossCreLocker;

            this.ReducingCrisscrossARE = ReducingCrisscrossARE;
            this.PossibleToCrisscrossReduce = PossibleToCrisscrossReduce;
            this.CrisscrossReduceLocker = CrisscrossReduceLocker;
        }

        internal void Start(CancellationToken token)
        {
            cancellationToken = token;
            BuildingNewCrisscross = new Task(() => BuildNewState());
            BuildingNewCrisscross.Start();
        }

        private bool CheckPossibleOfRealization()
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            lock (PossibleNewSrisscrossCreLocker)
                return PossibleNewCrisscrossCre.Any();
        }

        private void TryActionPossibility(Crisscross stateToCheck, ThumbnailObject[] SetToCheck, int actionPos)
        {
            object ResultOfCheck = Actions[actionPos].InstantActionPDDLSimplified.DynamicInvoke(SetToCheck);

            if (ResultOfCheck is null)
                return;

            var ResultOfCheckasList = (List<List<KeyValuePair<ushort, ValueType>>>)ResultOfCheck;
            var ChangedThObs = new List<ThumbnailObject>();

            for (int j = 0; j < SetToCheck.Length; j++)
            {
                var ChangedThumbnailObject = SetToCheck[j].CreateChild(ResultOfCheckasList[j]);
                ChangedThObs.Add(ChangedThumbnailObject);
            }

            PossibleState newPossibleState = new PossibleState(stateToCheck.Content, ChangedThObs);
            uint ActionCost = (uint)Actions[actionPos].actionCost.CostExpressionFunc.DynamicInvoke(SetToCheck);
            stateToCheck.Add(newPossibleState, actionPos, SetToCheck, ActionCost, out Crisscross AddedItem);

            lock (CrisscrossReduceLocker)
            {
                PossibleToCrisscrossReduce.Add(AddedItem);
            }
            ReducingCrisscrossARE.Set();
        }

        private void CheckAllPos(Crisscross stateToCheck, int actionPos, ThumbnailObject[] SetToCheck, int currentIndex, IList<ThumbnailObject>[] possibleOld, IList<ThumbnailObject>[] possibleNew, bool UntilNowOnlyOld)
        {
            ThumbnailObject[] ToRemove = new ThumbnailObject[0];

            if (currentIndex != 0)
            {
                ToRemove = new ThumbnailObject[currentIndex];
                Array.Copy(SetToCheck, ToRemove, currentIndex);
            }
            else
                ToRemove = new ThumbnailObject[0];

            IEnumerable<ThumbnailObject> newone = possibleNew[currentIndex].Except(ToRemove);

            foreach (ThumbnailObject thisOne in newone)
            {
                SetToCheck[currentIndex] = thisOne;

                if (currentIndex == SetToCheck.Length - 1)
                    TryActionPossibility(stateToCheck, SetToCheck, actionPos);
                else
                    CheckAllPos(stateToCheck, actionPos, SetToCheck, currentIndex + 1, possibleOld, possibleNew, false);
            }

            if (UntilNowOnlyOld)
                if (SetToCheck.Length == currentIndex + 1)
                {
                    return;
                }

            newone = possibleOld[currentIndex].Except(ToRemove);

            foreach (ThumbnailObject thisOne in newone)
            {
                SetToCheck[currentIndex] = thisOne;

                if (currentIndex == SetToCheck.Length - 1)
                    TryActionPossibility(stateToCheck, SetToCheck, actionPos);
                else
                    CheckAllPos(stateToCheck, actionPos, SetToCheck, currentIndex + 1, possibleOld, possibleNew, UntilNowOnlyOld);
            }
        }

        private void CheckTheOldPoss(Crisscross stateToCheck, int actionPos, IList<ThumbnailObject>[] possibleOld)
        {
            if (!possibleOld.All(p => p.Any()))
                return;

            IList<CrisscrossChildrenCon> RootChild;
            int actionArgC = Actions[actionPos].InstantActionParamCount;
            bool TheSame;

            lock (stateToCheck.Root.Children)
            {
                RootChild = stateToCheck.Root.Children.Where(c => c.ActionNr == actionPos).ToList();
            }

            foreach (CrisscrossChildrenCon CCC in RootChild)
            {
                TheSame = true;

                for (int i = 0; i != actionArgC; i++)
                    if (!possibleOld[i].Any(TO => TO.Compare(CCC.ActionArgThOb[i])))
                    {
                        TheSame = false;
                        break;
                    }

                if (!TheSame)
                    continue;

                TryActionPossibility(stateToCheck, CCC.ActionArgThOb, actionPos);
            }
        }

        internal void BuildNewStateForCrisscross(Crisscross stateToCheck)
        {
            for (int actionPos = 0; actionPos != Actions.Count(); actionPos++)
            {
                IList<ThumbnailObject>[] possibleCha = new IList<ThumbnailObject>[Actions[actionPos].InstantActionParamCount];
                IEnumerable<ThumbnailObject>[] possibleAll = new IEnumerable<ThumbnailObject>[Actions[actionPos].InstantActionParamCount];
                for (int i = 0; i != Actions[actionPos].InstantActionParamCount; i++)
                {
                    lock (stateToCheck.Content.ChangedThumbnailObjects)
                        possibleCha[i] = stateToCheck.Content.ChangedThumbnailObjects.Where(Actions[actionPos].Parameters[i].Func).ToList();

                    possibleAll[i] = stateToCheck.Content.ThumbnailObjects.Where(Actions[actionPos].Parameters[i].Func);
                }

                lock (stateToCheck.Content.ThumbnailObjects)
                    if (possibleAll.Any(p => !p.Any()))
                        continue;

                ThumbnailObject[] SetToCheck = new ThumbnailObject[Actions[actionPos].InstantActionParamCount];
                IList<ThumbnailObject>[] possibleOld = new IList<ThumbnailObject>[SetToCheck.Length];

                for (int i = 0; i != SetToCheck.Length; i++)
                    lock (stateToCheck.Content.ThumbnailObjects)
                        possibleOld[i] = possibleAll[i].Except(possibleCha[i]).ToList();

                CheckTheOldPoss(stateToCheck, actionPos, possibleOld);

                if (possibleCha.All(p => !p.Any()))
                    continue;

                CheckAllPos(stateToCheck, actionPos, SetToCheck, 0, possibleOld, possibleCha, true);
            }
        }

        protected void BuildNewState()
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 64, GloCla.ResMan.GetString("Sa7"), Task.CurrentId);

            Crisscross stateToCheck;

            while (!cancellationToken.IsCancellationRequested)
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

                    BuildNewStateForCrisscross(stateToCheck);
                }
                NoNewData.BeginInvoke(null, null);
                IsWaiting = true;
            }

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 65, GloCla.ResMan.GetString("Sp7"), Task.CurrentId);
        }
    }
}
