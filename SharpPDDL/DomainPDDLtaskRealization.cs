using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace SharpPDDL
{
    public partial class DomeinPDDL
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxDegreeOfParalleism">Not in use yet</param>
        /// <param name="CancellationDomein"></param>
        public void Start(int? MaxDegreeOfParalleism = null, CancellationToken CancellationDomein = default)
        {
            CancellationDomein.Register(ExternalCancellationOfProc);

            ParallelOptions options = new ParallelOptions
            {
                CancellationToken = CancellationDomein,
                MaxDegreeOfParallelism = MaxDegreeOfParalleism ?? Environment.ProcessorCount
            };

            CheckActions(options);

            List<PossibleStateThumbnailObject> allObjects = new List<PossibleStateThumbnailObject>();

            object locker = new object();
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

            CurrentState = new PossibleState(allObjects);

            foreach (var goal in domainGoals)
            {
                goal.BUILDIT(this.types.allTypes);
            }

            DomainPlanner = new DomainPlanner(this);

            foreach (Delegate d in this.PlanGenerated.GetInvocationList())
                DomainPlanner.PlanGeneratedInDomainPlanner += (ListOfString)d;


            this.domainGoals.CollectionChanged += DomainGoals_CollectionChanged;

            DomainPlanner.Start(options);
        }

        private void DomainGoals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                return;

            ICollection<GoalPDDL> ToCheckGoals;
           
            try
            {
                ToCheckGoals = (ICollection<GoalPDDL>)sender;
            }
            catch
            {
                Console.WriteLine("Unknown error in time of adding goal in run");
                Console.ReadKey();
                throw new Exception();
            }

            foreach (GoalPDDL ToCheckGoal in ToCheckGoals)
            {
                //CheckGoalInCol.CheckNewGoal(CancellationDomein, states, ToCheckGoal, foundSols);
            }
        }

        protected void ExternalCancellationOfProc()
        {
            this.domainGoals.CollectionChanged -= DomainGoals_CollectionChanged;
        }
      
    }
}
