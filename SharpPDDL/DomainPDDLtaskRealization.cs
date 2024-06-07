using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace SharpPDDL
{
    public partial class DomeinPDDL
    {
        ConcurrentQueue<Crisscross> PossibleGoalRealization = new ConcurrentQueue<Crisscross>();
        ConcurrentQueue<Crisscross> PossibleToCrisscrossReduce = new ConcurrentQueue<Crisscross>();
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

            Thread thread = new Thread(CheckGoalProces);
            thread.Start();

            CheckAction(states, 1);
            CheckAction(states, 0);

            if (thread.ThreadState == ThreadState.Stopped)
            {
                thread = new Thread(CheckGoalProces);
                thread.Start();
            }

            //CheckGoalProces();
            //Realizationpp.Start();
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
                var addedCh = stateToCheck.Add(newPossibleState, actionPos, ActionArg, actions[actionPos].ActionCost);
                PossibleGoalRealization.Enqueue(addedCh);
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

            if (stateToCheck.CheckedAction is null)
                return;

            if (stateToCheck.CheckedAction.Contains(actionPos))
                return;

            int ThObjCount = stateToCheck.Content.ThumbnailObjects.Count;
            int ActParCount = actions[actionPos].InstantActionParamCount;

            if (ThObjCount < ActParCount)
                return;

            Permute(stateToCheck.Content.ThumbnailObjects, new List<PossibleStateThumbnailObject>(), actions[actionPos].InstantActionParamCount);

            stateToCheck.CheckedAction.Add(actionPos);
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

                bool goalObjCorrect = false;

                foreach (IGoalObject goalObject in Goal.GoalObjects)
                {
                    foreach (var a in updatedOb.Content.ThumbnailObjects)
                    {
                        if ((bool)goalObject.GoalPDDL.DynamicInvoke(a))
                        {
                            goalObjCorrect = true;
                            break;
                        }
                    }

                    if (!goalObjCorrect)
                        break;

                    goalObjCorrect = false;
                }

                if (goalObjCorrect)
                    RealizatedList.Add(Goal);
            }

            return RealizatedList;
        }

        internal void CheckGoalProces()
        {           
            while (!PossibleGoalRealization.IsEmpty)
            {
                Crisscross possibleStatesCrisscross;
                if (!PossibleGoalRealization.TryDequeue(out possibleStatesCrisscross))
                    continue;

                var a = CheckNewGoalsReach(possibleStatesCrisscross);
                PossibleToCrisscrossReduce.Enqueue(possibleStatesCrisscross);
            }
        }
    }
}
