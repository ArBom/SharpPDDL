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
        CancellationToken CancellationDomein;

        public void Start(CancellationToken CancellationDomein = default)
        {
            this.CancellationDomein = CancellationDomein;
            CancellationDomein.Register(ExternalCancellationOfProc);
            CheckActions();

            /*options = new ParallelOptions
            {
                CancellationToken = CancelCurrentTokenS.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };*/

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

            this.crisscrossGenerator = new CrisscrossGenerator(this);

            this.domainGoals.CollectionChanged += DomainGoals_CollectionChanged;

            crisscrossGenerator.Start(CancellationDomein);
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
                GoalsPDDP.CheckGoalInCol.CheckNewGoal(CancellationDomein, states, ToCheckGoal, foundSols);
        }

        protected void ExternalCancellationOfProc()
        {
            this.domainGoals.CollectionChanged -= DomainGoals_CollectionChanged;
        }
        
        internal void GenList(KeyValuePair<Crisscross, List<GoalPDDL>> Found)
        {
            Crisscross state = states;
            List<CrisscrossChildrenCon> r = Found.Key.Position();

            Console.WriteLine(ExtensionMethods.TracePrefix + Found.Value[0].Name + " determined!!! Total Cost: " + Found.Key.CumulativedTransitionCharge);

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

                    Plan.Add(new List<string> { actions[r[i].ActionNr].Name + ": ", (string)actions[r[i].ActionNr].InstantActionSententia.DynamicInvoke(arg), " Action cost: " + r[i].ActionCost });

                    state = r[i].Child;
                }

                PlanGenerated?.Invoke(Plan);               
            }
        }
    }
}
