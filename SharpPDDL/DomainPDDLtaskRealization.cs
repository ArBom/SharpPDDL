using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            PossibleState possibleState = new PossibleState();
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
                ThumbnailObjectPrecursor<dynamic> k = new ThumbnailObjectPrecursor<dynamic>(ob, types.allTypes);
                possibleState.ThumbnailObjects.Add(k);
            }

            states = new Crisscross<PossibleState>();
            states.Content = possibleState;

            TryAction(states.Content, 1);

            //Realizationpp.Start();
        }

        private void TryAction(PossibleState stateToCheck, int actionPos)
        {
            int ThObjCount = stateToCheck.ThumbnailObjects.Count;
            int ActParCount = actions[actionPos].InstantActionParamCount;
            PossibleStateThumbnailObject[] arr = new PossibleStateThumbnailObject[ActParCount];

            #region nestedVoid
            void CheckVariationsNoRepetitions(int index)
            {
                List<PossibleStateThumbnailObject> temperaryList = new List<PossibleStateThumbnailObject>(stateToCheck.ThumbnailObjects);
                if (index >= ActParCount)
                {
                    var iop = actions[actionPos].InstantActionPDDL.DynamicInvoke(arr);
                }
                else
                {
                    for (int i = index; i < ThObjCount; i++)
                    {
                        arr[index] = temperaryList[i];
                        Swap(temperaryList[i], temperaryList[index]);
                        CheckVariationsNoRepetitions(index + 1);
                        Swap(temperaryList[i], temperaryList[index]);
                    }
                }
            }

            void Swap<T>(T v1, T v2)
            {
                T old = v1;
                v1 = v2;
                v2 = old;
            }
            #endregion

            if (ThObjCount < ActParCount)
                return;

            CheckVariationsNoRepetitions(0);
        }


    }
}
