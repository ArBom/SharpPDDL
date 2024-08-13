using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace SharpPDDL
{
    /*internal class Referencer<T>
    {
        internal Referencer(ref T content)
        {
            _Content = content;
        }

        protected T _Content;
        internal ref T Content
        {
            get {return ref _Content; }
        }

        internal void ContentOut (out T content)
        {
            content = _Content;
        }
    }*/

    public partial class DomeinPDDL
    {
        object PossibleNewSrisscrossCreLocker = new object();
        List<Crisscross> PossibleNewSrisscrossCre = new List<Crisscross>();
        Thread BuildingNewCrisscross = null;

        //BlockingCollection<Crisscross> PossibleGoalRealization = new BlockingCollection<Crisscross>(new ConcurrentQueue<Crisscross>(), 10000);
        ConcurrentQueue<Crisscross> PossibleGoalRealization = new ConcurrentQueue<Crisscross>();
        Thread CheckingGoalRealization = null;

        ConcurrentQueue<Crisscross> PossibleToCrisscrossReduce = new ConcurrentQueue<Crisscross>();
        Thread ReducingCrisscross = null;

        CancellationTokenSource TaskRealizationCTS;
        ParallelOptions options;

        public void Start()
        {
            CheckActions();

            options = new ParallelOptions
            {
                CancellationToken = TaskRealizationCTS.Token,
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

            states = new Crisscross();
            states.Content = possibleState;
            PossibleGoalRealization.Enqueue(states);

            CheckingGoalRealization = new Thread(CheckGoalProces);
            CheckingGoalRealization.Start();
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
                //Referencer<Crisscross> Ref = new Referencer<Crisscross>(ref AddedItem);
                PossibleToCrisscrossReduce.Enqueue(AddedItem);
            }

            void Permute(List<PossibleStateThumbnailObject> Source, List<PossibleStateThumbnailObject> s, int n)
            {
                for (int i = 0; i < Source.Count; i++)
                {
                    List<PossibleStateThumbnailObject> head = new List<PossibleStateThumbnailObject>(s);
                    head.Add(Source[i]);
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

        internal void CheckGoalProces()
        {
            Console.WriteLine("Check Goal Proces Run");
            while (!PossibleGoalRealization.IsEmpty)
            {
                if (!PossibleGoalRealization.TryDequeue(out Crisscross possibleStatesCrisscross))
                    continue;

                List<GoalPDDL> GoalsReach = CheckNewGoalsReach(possibleStatesCrisscross);

                if (GoalsReach.Count != 0)
                {
                    Console.WriteLine(GoalsReach[0].Name + " determined!!!");
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

                    Console.ReadKey();
                    }
                }

                if (possibleStatesCrisscross.Children.Count == 0)
                    PossibleNewSrisscrossCre.Add(possibleStatesCrisscross);
            }

            Console.WriteLine("Check Goal Proces Finished");

            if (PossibleNewSrisscrossCre.Count == 0)
                return;

            if (BuildingNewCrisscross is null)
            {
                BuildingNewCrisscross = new Thread(BuildNewState);
                BuildingNewCrisscross.Start();
                return;
            }

            if (BuildingNewCrisscross.ThreadState != ThreadState.Running)
            {
                BuildingNewCrisscross = new Thread(BuildNewState);
                //TODO sortowanie
                BuildingNewCrisscross.Start();
            }
        }

        internal void TryMergeCrisscross()
        {
            Console.WriteLine("Try Merge Crisscross Run");
            while (!PossibleToCrisscrossReduce.IsEmpty)
            {
                if (!PossibleToCrisscrossReduce.TryDequeue(out Crisscross possibleToCrisscrossReduce))
                    continue;

                bool Merged = false;
                //Crisscross possibleToCrisscrossReduce = Dequeued;//.Content;

                CrisscrossRefEnum crisscrossRefEnum = new CrisscrossRefEnum(ref states);                   
                while (crisscrossRefEnum.MoveNext())
                //foreach (ref Crisscross s in states) it throw cs1510
                {
                    var s = crisscrossRefEnum.Current;
                    Console.WriteLine(s.Content.CheckSum + " " + s.CumulativedTransitionCharge);
                    if (s.Content.Compare(ref possibleToCrisscrossReduce.Content))
                    {
                        if (s.Root is null)
                            Crisscross.MergeK(ref states, ref possibleToCrisscrossReduce);
                        else
                        {
                            if (s.Equals(possibleToCrisscrossReduce))
                                throw new Exception();

                            Crisscross.Merge(ref s, ref possibleToCrisscrossReduce);
                            Console.WriteLine("Merged: " + s.Content.CheckSum + " " + s.CumulativedTransitionCharge);
                        }
                        
                        Merged = true;
                        break;
                    }
                }

                Console.WriteLine();

                if (Merged)
                    continue;

                PossibleGoalRealization.Enqueue(possibleToCrisscrossReduce);
            }
            Console.WriteLine("Try Merge Crisscross Finished");

            if (CheckingGoalRealization.ThreadState != ThreadState.Running)
            {
                CheckingGoalRealization = new Thread(CheckGoalProces);
                CheckingGoalRealization.Start();
            }
        }

        internal void BuildNewState()
        {
            Console.WriteLine("Build New State Run");
            while (PossibleNewSrisscrossCre.Count != 0)
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
            Console.WriteLine("Build New State Finished");

            if (PossibleToCrisscrossReduce.Count == 0)
                return;

            if (ReducingCrisscross is null)
            {
                ReducingCrisscross = new Thread(TryMergeCrisscross);
                ReducingCrisscross.Start();
                return;
            }

            if (ReducingCrisscross.ThreadState != ThreadState.Running)
            {
                ReducingCrisscross = new Thread(TryMergeCrisscross);
                ReducingCrisscross.Start();
            }
        }
    }
}
