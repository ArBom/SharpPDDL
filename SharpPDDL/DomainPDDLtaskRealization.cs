using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SharpPDDL
{
    public partial class DomeinPDDL
    {
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

            PossibleState possibleState = new PossibleState(null);
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

            foreach (var ob in domainObjects)
            {
                ThumbnailObjectPrecursor<object> k = new ThumbnailObjectPrecursor<object>(ob, types.allTypes);
                possibleState.ThumbnailObjects.Add(k);
            }

            foreach (var goal in domainGoals)
            {
                goal.BUILDIT(this.types.allTypes);
            }

            states = new Crisscross<PossibleState>();
            states.Content = possibleState;

            CheckAction(states, 1);

            //Realizationpp.Start();
        }

        private void CheckAction(Crisscross<PossibleState> stateToCheck, int actionPos)
        {
            void TryActionPossibility (PossibleStateThumbnailObject[] SetToCheck)
            {
                object ResultOfCheck = actions[actionPos].InstantActionPDDL.DynamicInvoke(SetToCheck);

                if (ResultOfCheck is null)
                    return;

                var ResultOfCheckasList = (List<List<KeyValuePair<ushort, ValueType>>>)ResultOfCheck;
                PossibleState newPossibleState = new PossibleState(stateToCheck.Content);

                for (int j = 0; j < SetToCheck.Length; j++)
                {
                    var ChangedThumbnailObject = SetToCheck[j].CreateChild(ResultOfCheckasList[j]);
                    newPossibleState.ChangedThumbnailObjects.Add(ChangedThumbnailObject);
                }     
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

        internal List<int> CheckChangedOb(PossibleStateThumbnailObject updatedOb)
        {
            List<int> ToCheck = new List<int>();
            bool Possible = false;

            foreach (GoalPDDL Goal in domainGoals)
                foreach(IGoalObject goalObject in Goal.GoalObjects)
                {
                    if (updatedOb.OriginalObj == goalObject.OriginalObj)
                    {
                        int b = 100;
                    }

                    var a = (bool)goalObject.GoalPDDL.DynamicInvoke(updatedOb);
                    if (a == true)
                    {
                        int ĄĘ = 1500;
                    }
                }

            if (Possible)
                return ToCheck;
            else
                return null;
        }
    }
}
