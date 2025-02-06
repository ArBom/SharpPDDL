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
            {
                goal.BUILDIT(this.types.allTypes);
            }

            DomainPlanner = new DomainPlanner(this);
            PlanImplementor.UpdateIt(this);
            PlanImplementor.cancelationToken = CancellationDomein;

            foreach (Delegate d in this.PlanGenerated.GetInvocationList())
                DomainPlanner.PlanGeneratedInDomainPlanner += (ListOfString)d;

            DomainPlanner.ToRealize += PlanImplementor.RealizeIt;
            this.domainGoals.CollectionChanged += DomainGoals_CollectionChanged;

            DomainPlanner.Start(options);
        }

        private void DomainGoals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (!domainGoals.Any())
                    DomainPlanner.InternalCancellationTokenSrc.Cancel();

                return;
            }                

            ICollection<GoalPDDL> ToCheckGoals;

            if (this.domainGoals.Any())
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 14, GloCla.ResMan.GetString("E5"));
                throw new Exception(GloCla.ResMan.GetString("E5"));
            }

            try
            {
                ToCheckGoals = (ICollection<GoalPDDL>)sender;
            }
            catch
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 13, GloCla.ResMan.GetString("C0"));
                throw new Exception(GloCla.ResMan.GetString("C0"));
            }

            foreach (GoalPDDL ToCheckGoal in ToCheckGoals)
            {
                //CheckGoalInCol.CheckNewGoal(CancellationDomein, states, ToCheckGoal, foundSols);
            }
        }

        protected void ExternalCancellationOfProc()
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 12, GloCla.ResMan.GetString("V0"), this.Name);
            this.domainGoals.CollectionChanged -= DomainGoals_CollectionChanged;
        }    
    }
}
