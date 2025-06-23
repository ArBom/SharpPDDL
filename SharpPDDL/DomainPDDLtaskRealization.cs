using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SharpPDDL
{
    public partial class DomeinPDDL
    {
        /// <summary>
        /// Start algorithm in this domain
        /// </summary>
        /// <param name="MaxDegreeOfParalleism">Not in use yet</param>
        /// <param name="CancellationDomein"></param>
        public void Start(int? MaxDegreeOfParalleism = null, CancellationToken CancellationDomein = default)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 11, GloCla.ResMan.GetString("V1"), this.Name);
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
                goal.BUILDIT(this.types.allTypes);

            DomainPlanner = new DomainPlanner(this);

            if(!(this.PlanGenerated is null))
                foreach (Delegate GeneratedPlan in this.PlanGenerated.GetInvocationList())
                    DomainPlanner.PlanGeneratedInDomainPlanner += (ListOfString)GeneratedPlan;

            this.domainGoals.CollectionChanged += DomainPlanner.DomainGoals_CollectionChanged;

            DomainPlanner.Start(options);
        }

        protected void ExternalCancellationOfProc()
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 12, GloCla.ResMan.GetString("V0"), this.Name);
            this.domainGoals.CollectionChanged -= DomainPlanner.DomainGoals_CollectionChanged;

            foreach (ActionPDDL act in this.actions)
                act.ClearActionDelegates();
        }    
    }
}
