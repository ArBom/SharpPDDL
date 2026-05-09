using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace SharpPDDL
{
    public partial class DomainPDDL
    {
        internal string DiagramsPath { get; private set; }
        internal Diagram DiagramTypes { get; private set; }

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

            List<ThumbnailObject> allObjects = new List<ThumbnailObject>();
            foreach (var domainObject in domainObjects)
            {
                ThumbnailObjectPrecursor<object> ObjectPrecursor = new ThumbnailObjectPrecursor<object>(domainObject, types.allTypes);

                if (!(ObjectPrecursor.ValuesIndeksesKeys is null))
                    foreach (ThumbnailObject thumbnailObject in allObjects)
                        ObjectPrecursor.TryToChangeHandle((ThumbnailObjectPrecursor<object>)thumbnailObject);

                allObjects.Add(ObjectPrecursor);
            }

            CurrentState = new PossibleState(allObjects);

            foreach (var goal in domainGoals)
                goal.BuildIt(this);

            DomainPlanner = new DomainPlanner(this);

            if (!(_PlanGenerated is null))
                foreach (Delegate GeneratedPlan in _PlanGenerated.GetInvocationList())
                    DomainPlanner.PlanGeneratedInDomainPlanner += (ListOfString)GeneratedPlan;

            this.domainGoals.CollectionChanged += DomainPlanner.DomainGoals_CollectionChanged;

            DomainPlanner.Start(options);
        }

        /// <summary>
        /// 
        /// <example><para>
        /// For example:
        /// <code>
        /// DomainPDDL.GenerateDiagrams("diagram.dgml", Diagram.UseCase | Diagram.States);<br/>
        /// DamainPDDL.Start();
        /// </code>
        /// </para></example>
        /// </summary>
        /// <param name="path">Path to write diagram(s)</param>
        /// <param name="diagrams">Diagram types to write</param>
        public void GenerateDiagrams(string path, Diagram diagrams)
        {
            if (String.IsNullOrEmpty(path))
            {

            }
            else
            {
                bool incorect = Path.HasExtension(path);
            }

            DiagramsPath = path;

            string dlist = ((Diagram)diagrams).ToString();
            DiagramTypes = diagrams;
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
