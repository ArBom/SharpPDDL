using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL.CrisscrossesGenerate
{
    class CrisscrossNewPossiblesCreator
    {
        protected readonly IReadOnlyList<ActionPDDL> Actions;
        protected readonly Dictionary<int, int[]> actionsByParamCount;
        protected readonly int MinActionParamCount;
        protected readonly int MaxActionParamCount;

        internal Task BuildingNewCrisscross = null;

        internal readonly AutoResetEvent BuildingNewCrisscrossARE;
        protected readonly List<Crisscross> PossibleNewCrisscrossCre;
        protected readonly object PossibleNewSrisscrossCreLocker;

        protected readonly AutoResetEvent ReducingCrisscrossARE;
        protected readonly List<Crisscross> PossibleToCrisscrossReduce;
        protected readonly object CrisscrossReduceLocker;

        internal CrisscrossNewPossiblesCreator(List<ActionPDDL> actions, AutoResetEvent BuildingNewCrisscrossARE, List<Crisscross> PossibleNewCrisscrossCre, object PossibleNewSrisscrossCreLocker, AutoResetEvent ReducingCrisscrossARE, List<Crisscross> PossibleToCrisscrossReduce, object CrisscrossReduceLocker)
        {
            this.Actions = actions;

            //Group action by number of parameter to optimalization in time of creation new states exacly for VariationsWithoutRepetition
            actionsByParamCount = Actions.GroupBy(a => a.InstantActionParamCount, (ParamCount, c) => (ParamCount, c.Select(d => actions.IndexOf(d)).ToArray())).ToDictionary(g => g.ParamCount, g => g.Item2);

            MinActionParamCount = actionsByParamCount.Keys.Min();
            MaxActionParamCount = actionsByParamCount.Keys.Max();

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
            Crisscross stateToCheck;
            while (!token.IsCancellationRequested)
            {
                BuildingNewCrisscrossARE.WaitOne();

                while (PossibleNewCrisscrossCre.Count() != 0 && !token.IsCancellationRequested)
                {
                    lock (PossibleNewSrisscrossCreLocker)
                    {
                        try
                        {
                            stateToCheck = PossibleNewCrisscrossCre[0];
                        }
                        catch
                        {
                            continue;
                        }
                        PossibleNewCrisscrossCre.RemoveAt(0);
                    }

                    List<Crisscross> ToAddList = new List<Crisscross>();

                    void TryActionPossibility(PossibleStateThumbnailObject[] SetToCheck, int actionPos)
                    {
                        object ResultOfCheck = Actions[actionPos].InstantActionPDDL.DynamicInvoke(SetToCheck);

                        if (ResultOfCheck is null)
                            return;

                        var ResultOfCheckasList = (List<List<KeyValuePair<ushort, ValueType>>>)ResultOfCheck;
                        var ChangedThObs = new List<PossibleStateThumbnailObject>();
                        object[] ActionArg = new object[SetToCheck.Length];

                        for (int j = 0; j < SetToCheck.Length; j++)
                        {
                            var ChangedThumbnailObject = SetToCheck[j].CreateChild(ResultOfCheckasList[j]);
                            ChangedThObs.Add(ChangedThumbnailObject);
                            ActionArg[j] = SetToCheck[j].OriginalObj;
                        }

                        PossibleState newPossibleState = new PossibleState(stateToCheck.Content, ChangedThObs);
                        stateToCheck.Add(newPossibleState, actionPos, ActionArg, Actions[actionPos].ActionCost, out Crisscross AddedItem);

                        ToAddList.Add(AddedItem);
                    }

                    void VariationsWithoutRepetition(List<PossibleStateThumbnailObject> Source, List<PossibleStateThumbnailObject> PrevHead, int ExpectedSLenght)
                    {
                        for (int i = 0; i < Source.Count; i++)
                        {
                            List<PossibleStateThumbnailObject> Head = new List<PossibleStateThumbnailObject>(PrevHead)
                            {
                                Source[i]
                            };

                            var tail = new List<PossibleStateThumbnailObject>();
                            tail.AddRange(Source);
                            tail.RemoveAt(i);

                            if (Head.Count != ExpectedSLenght)
                            {
                                VariationsWithoutRepetition(tail, Head, ExpectedSLenght);
                                continue;
                            }

                            PossibleStateThumbnailObject[] SetToCheck = Head.ToArray();
                            foreach (int actionPos in actionsByParamCount[ExpectedSLenght])
                            {
                                TryActionPossibility(SetToCheck, actionPos);
                            }

                            if (ExpectedSLenght == MaxActionParamCount)
                                continue;

                            int NewExpectedSLenght = actionsByParamCount.Keys.Where(k => k > ExpectedSLenght).Min();
                            VariationsWithoutRepetition(tail, Head, NewExpectedSLenght);
                        }
                    }

                    VariationsWithoutRepetition(stateToCheck.Content.ThumbnailObjects, new List<PossibleStateThumbnailObject>(), MinActionParamCount);

                    if (ToAddList.Count() != 0)
                    {
                        ToAddList.OrderBy(c => c.CumulativedTransitionCharge);
                        lock (CrisscrossReduceLocker)
                        {
                            PossibleToCrisscrossReduce.AddRange(ToAddList);
                        }
                        ReducingCrisscrossARE.Set();
                    }
                }
            }
        }
    }
}
