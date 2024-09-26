using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SharpPDDL
{
    public partial class DomeinPDDL
    {
        object PossibleNewSrisscrossCreLocker = new object();
        List<Crisscross> PossibleNewSrisscrossCre = new List<Crisscross>();
        Task BuildingNewCrisscross = null;

        //BlockingCollection<Crisscross> PossibleGoalRealization = new BlockingCollection<Crisscross>(new ConcurrentQueue<Crisscross>(), 10000);
        ConcurrentQueue<Crisscross> PossibleGoalRealization = new ConcurrentQueue<Crisscross>();
        Task CheckingGoalRealization = null;
        CancellationTokenSource InternalCancellationTokenSrc;

        List<Crisscross> PossibleToCrisscrossReduce = new List<Crisscross>();
        object CrisscrossReduceLocker = new object();
        Task ReducingCrisscross = null;

        private CancellationTokenSource CancelCurrentTokenS;
        ParallelOptions options;

        public void Start(CancellationToken CancellationDomein = default)
        {
            InternalCancellationTokenSrc = new CancellationTokenSource();
            InternalCancellationTokenSrc.Token.Register(() => ReStart(CancellationDomein));
            CancelCurrentTokenS = CancellationTokenSource.CreateLinkedTokenSource(CancellationDomein, InternalCancellationTokenSrc.Token);
            CheckActions();

            options = new ParallelOptions
            {
                CancellationToken = CancelCurrentTokenS.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            List<PossibleStateThumbnailObject> allObjects = new List<PossibleStateThumbnailObject>();

            var locker = new object();
            /*Parallel.ForEach
            (
                domainObjects,
                options,
                () => new List<PossibleStateThumbnailObject>(), // initialize aggregate for every thread 
                (Obj, loopState, subtotal) =>
                {
                    ThumbnailObjectPrecursor<dynamic> k = new ThumbnailObjectPrecursor<dynamic>(Obj, types.allTypes); //TODO dodać zabezpieczenie na wypadek braku typu obj na liście allTypes
                    subtotal.Add(k); // add current thread element to aggregate 
                    return subtotal; // return current thread aggregate
                },
                Sublist => // action to combine all threads results
                {
                    lock (locker) // lock, cause List<T> is not a thread safe collection
                    {
                        possibleState.ThumbnailObjects.AddRange(Sublist);
                    }
                }
            );*/

            foreach (var domainObject in domainObjects)
            {
                ThumbnailObjectPrecursor<object> ObjectPrecursor = new ThumbnailObjectPrecursor<object>(domainObject, types.allTypes);
                allObjects.Add(ObjectPrecursor);
            }

            PossibleState possibleState = new PossibleState(allObjects);

            foreach (var goal in domainGoals)
            {
                goal.BUILDIT(this.types.allTypes);
            }

            states = new Crisscross
            {
                Content = possibleState
            };
            PossibleGoalRealization.Enqueue(states);

            CheckingGoalRealization = new Task(() => CheckGoalProces(CancelCurrentTokenS));
            CheckingGoalRealization.Start();
        }

        private void ReStart(CancellationToken ExternalCancellation)
        {
            Task.WaitAll(BuildingNewCrisscross, CheckingGoalRealization, ReducingCrisscross);
            InternalCancellationTokenSrc = new CancellationTokenSource();
            CancelCurrentTokenS = CancellationTokenSource.CreateLinkedTokenSource(ExternalCancellation, InternalCancellationTokenSrc.Token);
        }

        private void CheckAction(Crisscross stateToCheck, int actionPos)
        {
            void TryActionPossibility (PossibleStateThumbnailObject[] SetToCheck)
            {
                object ResultOfCheck = actions[actionPos].InstantActionPDDL.DynamicInvoke(SetToCheck);

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
                stateToCheck.Add(newPossibleState, actionPos, ActionArg, actions[actionPos].ActionCost, out Crisscross AddedItem);

                lock (CrisscrossReduceLocker)
                {
                    PossibleToCrisscrossReduce.Add(AddedItem);
                }
            }

            void Permute(List<PossibleStateThumbnailObject> Source, List<PossibleStateThumbnailObject> s, int n)
            {
                for (int i = 0; i < Source.Count; i++)
                {
                    List<PossibleStateThumbnailObject> head = new List<PossibleStateThumbnailObject>(s)
                    {
                        Source[i]
                    };
                    var tail = new List<PossibleStateThumbnailObject>();
                    tail.AddRange(Source);
                    tail.RemoveAt(i);

                    if (head.Count != n)
                    {
                        Permute(tail, head, n);
                        continue;
                    }

                    TryActionPossibility(head.ToArray());
                }
            }

            int ThObjCount = stateToCheck.Content.ThumbnailObjects.Count;
            int ActParCount = actions[actionPos].InstantActionParamCount;

            if (ThObjCount < ActParCount)
                return;

            Permute(stateToCheck.Content.ThumbnailObjects, new List<PossibleStateThumbnailObject>(), actions[actionPos].InstantActionParamCount);
        }

        internal bool CheckNewGoalsReachPossibility(PossibleState possibleState, GoalPDDL possibleGoal)
        {
            foreach(var state in possibleState.ChangedThumbnailObjects)
                foreach (var goalObj in possibleGoal.GoalObjects)
                    if ((bool)goalObj.GoalPDDL.DynamicInvoke(state))
                        return true;

            return false;
        }

        internal List<GoalPDDL> CheckNewGoalsReach(Crisscross updatedOb)
        {
            List<GoalPDDL> RealizatedList = new List<GoalPDDL>();

            foreach (GoalPDDL Goal in domainGoals)
            {
                if (!CheckNewGoalsReachPossibility(updatedOb.Content, Goal))
                    continue;

                if (Goal.GoalObjects.Count() == 1)
                {
                    RealizatedList.Add(Goal);
                    continue;
                }

                bool goalObjCorrect = true;

                foreach (IGoalObject goalObject in Goal.GoalObjects)
                {
                    if (updatedOb.Content.ThumbnailObjects.Any(ThOb => (bool)goalObject.GoalPDDL.DynamicInvoke(ThOb)))
                        continue;
                    else
                    {
                        goalObjCorrect = false;
                        break;
                    }
                }

                if (goalObjCorrect)
                    RealizatedList.Add(Goal);
            }

            return RealizatedList;
        }

        internal void CheckGoalProces(CancellationTokenSource token)
        {
            //Trace.WriteLineIf(ExtensionMethods.traceLevel.TraceVerbose, ExtensionMethods.TracePrefix + "Check Goal Proces Run; ID=" + Task.CurrentId);

            while (!PossibleGoalRealization.IsEmpty /*&& !token.IsCancellationRequested*/)
            {
                if (!PossibleGoalRealization.TryDequeue(out Crisscross possibleStatesCrisscross))
                    continue;

                List<GoalPDDL> GoalsReach = CheckNewGoalsReach(possibleStatesCrisscross);

                if (GoalsReach.Count != 0)
                {
                    //if (ExtensionMethods.traceLevel.TraceInfo)
                    //Trace.TraceInformation(ExtensionMethods.TracePrefix + GoalsReach[0].Name + " determined!!");

                    Console.WriteLine(ExtensionMethods.TracePrefix + GoalsReach[0].Name + " determined!!");
                    Crisscross state = states;
                    List<CrisscrossChildrenCon> r = possibleStatesCrisscross.Position();

                    if (!(r is null))
                    {
                        List<List<string>> Plan = new List<List<string>>();

                        for (int i = 0; i != r.Count; i++)
                        {
                            PossibleStateThumbnailObject[] arg = new PossibleStateThumbnailObject[actions[r[i].ActionNr].InstantActionParamCount];

                            for (int j = 0; j != arg.Length; j++)
                            {
                                arg[j] = state.Content.ThumbnailObjects.First(ThOb => ThOb.OriginalObj.Equals(r[i].ActionArgOryg[j]));
                            }

                            Plan.Add(new List<string> { actions[r[i].ActionNr].Name + ": ", (string)actions[r[i].ActionNr].InstantActionSententia.DynamicInvoke(arg) });

                            state = r[i].Child;
                        }

                        PlanGenerated?.Invoke(Plan);

                        Console.ReadKey(); //TODO; 
                    }
                }

                if (possibleStatesCrisscross.Children.Count == 0)
                    PossibleNewSrisscrossCre.Add(possibleStatesCrisscross);
            }

            //Trace.WriteLineIf(ExtensionMethods.traceLevel.TraceVerbose, ExtensionMethods.TracePrefix + "Check Goal Proces Finished; ID=" + Task.CurrentId);

            if (PossibleNewSrisscrossCre.Count == 0)
                return;

            //Do not start building new Crisscrosses if whole state is cancelled
            if (token.IsCancellationRequested)
                return;

            if (BuildingNewCrisscross is null)
            {
                BuildingNewCrisscross = new Task(() => BuildNewState(CancelCurrentTokenS));
                BuildingNewCrisscross.Start();
                return;
            }

            if (BuildingNewCrisscross.Status != TaskStatus.Running)
            {
                BuildingNewCrisscross = new Task(() => BuildNewState(CancelCurrentTokenS));
                //TODO sortowanie
                BuildingNewCrisscross.Start();
            }
        }

        internal void TryMergeCrisscross(CancellationTokenSource token)
        {
            //Trace.WriteLineIf(ExtensionMethods.traceLevel.TraceVerbose, ExtensionMethods.TracePrefix + "Try Merge Crisscross Run; ID=" + Task.CurrentId);

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
            }
           // Trace.WriteLineIf(ExtensionMethods.traceLevel.TraceVerbose, ExtensionMethods.TracePrefix + "Try Merge Crisscross Finished; ID=" + Task.CurrentId);

            //Do not start checking goal realization if whole state is cancelled 
            if (token.IsCancellationRequested)
                return;

            if (CheckingGoalRealization.Status != TaskStatus.Running)
            {
                CheckingGoalRealization = new Task(() => CheckGoalProces(token));
                CheckingGoalRealization.Start();
            }
        }

        internal void BuildNewState(CancellationTokenSource token)
        {
            //Trace.WriteLineIf(ExtensionMethods.traceLevel.TraceVerbose, ExtensionMethods.TracePrefix + "Build New State Run; ID=" + Task.CurrentId);

            while (PossibleNewSrisscrossCre.Count != 0 && !token.IsCancellationRequested)
            {
                Crisscross Temp;
                lock (PossibleNewSrisscrossCreLocker)
                {
                    try
                    {
                        Temp = PossibleNewSrisscrossCre[0];                       
                    }
                    catch
                    {
                        continue;
                    }
                    PossibleNewSrisscrossCre.RemoveAt(0);
                }

                for (int a = 0; a != actions.Count; ++a)
                    CheckAction(Temp, a);
            }
            //Trace.WriteLineIf(ExtensionMethods.traceLevel.TraceVerbose, ExtensionMethods.TracePrefix + "Build New State Finished; ID=" + Task.CurrentId);

            if (PossibleToCrisscrossReduce.Count == 0)
                return;

            //Do not start reducing Crisscross if whole state is cancelled
            if (token.IsCancellationRequested)
                return;

            if (ReducingCrisscross is null)
            {
                ReducingCrisscross = new Task(() => TryMergeCrisscross(token));
                ReducingCrisscross.Start();
                return;
            }

            if (ReducingCrisscross.Status != TaskStatus.Running)
            {
                ReducingCrisscross = new Task(() => TryMergeCrisscross(token));
                ReducingCrisscross.Start();
            }
        }
    }
}
