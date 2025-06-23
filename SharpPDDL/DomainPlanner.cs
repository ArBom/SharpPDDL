using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace SharpPDDL
{
    public delegate void ListOfString(List<List<string>> planGenerated);
    internal delegate void FoundSols(KeyValuePair<Crisscross, List<GoalPDDL>> foundSolutions);

    internal class FoungingGoalDetail
    {
        internal readonly GoalPDDL GoalPDDL;
        internal bool IsFoundingChippest;

        internal FoungingGoalDetail(GoalPDDL goalPDDL)
        {
            this.GoalPDDL = goalPDDL;
            IsFoundingChippest = false;
        }

        public override int GetHashCode()
        {
            return GoalPDDL.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as FoungingGoalDetail);
        }

        public bool Equals(FoungingGoalDetail obj)
        {
            if (obj != null && GoalPDDL != null && obj.GoalPDDL != null)
                return obj.GoalPDDL.Name.Equals(GoalPDDL.Name);

            return false;
        }
    }

    internal class DomainPlanner
    {
        protected Dictionary<FoungingGoalDetail, SortedSet<Crisscross>> FoundedGoals;
        protected Dictionary<Crisscross, List<GoalPDDL>> FoundedCrisscrosses;
        protected Crisscross CurrentBuilded;
        protected CrisscrossGenerator CurrentBuilder;
        internal PlanImplementor PlanImplementor;
        protected ObservableCollection<GoalPDDL> Goals;
        readonly List<ActionPDDL> Actions;
        internal Action<uint> currentMinCumulativeCostUpdate;
        internal Action<KeyValuePair<Crisscross, List<GoalPDDL>>> FoundSols;
        internal ListOfString PlanGeneratedInDomainPlanner;
        internal List<PossibleStateThumbnailObject> OneUnuseObjects;

        CancellationToken ExternalCancellationDomein;
        internal CancellationTokenSource InternalCancellationTokenSrc;
        CancellationToken CurrentCancelTokenS;

        internal DomainPlanner(DomeinPDDL Owner)
        {
            Actions = Owner.actions;
            Goals = Owner.domainGoals;
            FoundSols += FoundSolsVoid;
            CurrentBuilded = new Crisscross
            {
                Content = Owner.CurrentState
            };

            PlanImplementor = new PlanImplementor(Owner);
            CurrentBuilder = new CrisscrossGenerator(CurrentBuilded, Owner, FoundSols, currentMinCumulativeCostUpdate);
            FoundedGoals = new Dictionary<FoungingGoalDetail, SortedSet<Crisscross>>();
            FoundedCrisscrosses = new Dictionary<Crisscross, List<GoalPDDL>>(Crisscross.IContentEqualityComparer);
        }

        internal void Start(ParallelOptions options)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 15, GloCla.ResMan.GetString("Sa1"));

            //remember External CancelationToken to reuse it after reset
            this.ExternalCancellationDomein = options.CancellationToken;

            //Token used to reset CrisscrossGenerator process when it is too big
            InternalCancellationTokenSrc = new CancellationTokenSource();

            CurrentCancelTokenS = CancellationTokenSource.CreateLinkedTokenSource(ExternalCancellationDomein, InternalCancellationTokenSrc.Token).Token;

            OneUnuseObjects = new List<PossibleStateThumbnailObject>();
            CurrentBuilder.CrisscrossesGenerated += AtAllStateGenerated;
            CurrentBuilder.Start(CurrentCancelTokenS);
        }

        internal void DomainGoals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (!Goals.Any())
                    InternalCancellationTokenSrc.Cancel();

                return;
            }

            ICollection<GoalPDDL> ToCheckGoals;

            if (Goals.Any())
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

            CurrentBuilder.Stop().Wait(100);
            foreach (GoalPDDL ToCheckGoal in ToCheckGoals)
            {
                CheckGoalInCol.CheckNewGoal(CurrentCancelTokenS, CurrentBuilded, ToCheckGoal, FoundSols);
            }
            CurrentBuilder.ReStart(CurrentBuilded);
        }

        private void FoundSolsVoid(KeyValuePair<Crisscross, List<GoalPDDL>> Found)
        {
            FoundedCrisscrosses[Found.Key] = Found.Value;
            foreach (GoalPDDL goalPDDL in Found.Value)
            {
                FoungingGoalDetail TempFoungingGoalDetail = new FoungingGoalDetail(goalPDDL);
                currentMinCumulativeCostUpdate?.Invoke(Found.Key.CumulativedTransitionCharge);

                if (FoundedGoals.ContainsKey(TempFoungingGoalDetail))
                    FoundedGoals[TempFoungingGoalDetail].Add(Found.Key);
                else
                {
                    FoundedGoals.Add(TempFoungingGoalDetail, new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge()) { Found.Key });

                    if (currentMinCumulativeCostUpdate is null)
                        currentMinCumulativeCostUpdate += CheckingIfGenerateActionList;
                }
            }
        }

        private void CheckingIfGenerateActionList(uint ActMinCumulativeCost)
        {
            //check if still exists cheaper state in pool
            if (ActMinCumulativeCost >= CurrentBuilder.CheckCost())
                return;

            var NOTIsFoundingChippest = FoundedGoals.Where(FG => !FG.Key.IsFoundingChippest);
            if (NOTIsFoundingChippest is null)
            {
                currentMinCumulativeCostUpdate -= CheckingIfGenerateActionList;
                //TODO jakiś błąd tu program nie powinien wejść
                return;
            }

            var FoundedChipStates = NOTIsFoundingChippest.Where(FG => (1.02 * FG.Value.First().CumulativedTransitionCharge < ActMinCumulativeCost));
            if (FoundedChipStates is null)
                return;

            int MaxCountSolution = FoundedChipStates.Max(FG => FG.Value.Count);

            var Solution = FoundedChipStates.First(FG => FG.Value.Count == MaxCountSolution);
            Crisscross StateToHit = Solution.Value.Min();
            currentMinCumulativeCostUpdate -= CheckingIfGenerateActionList;

            KeyValuePair<Crisscross, List<GoalPDDL>> GenerList = FoundedCrisscrosses.First(FC => FC.Key == StateToHit);
            FoundedCrisscrosses.Clear();

            foreach (GoalPDDL goalPDDL in GenerList.Value)
                Goals.Remove(goalPDDL);

            GenList(GenerList);
        }

        object AtAllStateGeneratedLocker = new object();

        private void AtAllStateGenerated()
        {
            lock (AtAllStateGeneratedLocker)
            if (FoundedCrisscrosses.Any() && !(CurrentBuilder.CrisscrossesGenerated is null))
            {
                CurrentBuilder.CrisscrossesGenerated -= AtAllStateGenerated;
                int Mfounded = FoundedCrisscrosses.Max(K => K.Value.Count());
                GenList(FoundedCrisscrosses.Where(K => K.Value.Count() == Mfounded).OrderBy(K => K.Key.CumulativedTransitionCharge).First());
            }
        }

        internal void GenList(KeyValuePair<Crisscross, List<GoalPDDL>> Found)
        {
            Crisscross state = CurrentBuilded;
            List<CrisscrossChildrenCon> FoKePo = Found.Key.Position();

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 16, GloCla.ResMan.GetString("I1"), Found.Value[0].Name, Found.Key.CumulativedTransitionCharge);

            if (!(FoKePo is null))
            {
                List<List<string>> Plan = new List<List<string>>();

                for (int i = 0; i != FoKePo.Count; i++)
                {
                    PossibleStateThumbnailObject[] arg = new PossibleStateThumbnailObject[Actions[FoKePo[i].ActionNr].InstantActionParamCount];

                    for (int j = 0; j != arg.Length; j++)
                        arg[j] = state.Content.ThumbnailObjects.First(ThOb => ThOb.OriginalObj.Equals(FoKePo[i].ActionArgOryg[j]));

                    Plan.Add(new List<string> { String.Format(GloCla.ResMan.GetString("Txt1"), Actions[FoKePo[i].ActionNr].Name), (string)Actions[FoKePo[i].ActionNr].InstantActionSententia.DynamicInvoke(arg), String.Format(GloCla.ResMan.GetString("Txt2"), FoKePo[i].ActionCost) } );

                    state = FoKePo[i].Child;
                }

                PlanGeneratedInDomainPlanner?.Invoke(Plan);

                Task Stopping = CurrentBuilder.Stop();
                Task Realizing = PlanImplementor.RealizeIt(FoKePo, CurrentCancelTokenS);
                Task<(Crisscross NewRoot, SortedSet<Crisscross> ChildlessCrisscrosses)> Transcribing = CurrentBuilder.TranscribeState(FoKePo.Last().Child, CurrentCancelTokenS);

                Task.WaitAll(Stopping, Realizing, Transcribing);

                //internal fault
                if(Realizing.IsFaulted)
                {
                    if (Realizing.Exception.InnerException is PrecondExecutionException)
                    {
                        List<PossibleStateThumbnailObject> RefreshedThumbnails = new List<PossibleStateThumbnailObject>(OneUnuseObjects);
                        OneUnuseObjects.Clear();

                        foreach (PossibleStateThumbnailObject UsedThObj in CurrentBuilded.Content.ThumbnailObjects)
                        {
                            PossibleStateThumbnailObject UpdatedThObj = new ThumbnailObjectPrecursor<object>(UsedThObj) as PossibleStateThumbnailObject;
                            RefreshedThumbnails.Add(UpdatedThObj);
                        }

                        CurrentBuilded = new Crisscross()
                        {
                            Content = new PossibleState(RefreshedThumbnails)
                        };

                        var ToGoalCheck = new ConcurrentQueue<Crisscross>();
                        ToGoalCheck.Enqueue(CurrentBuilded);

                        CurrentBuilder.InitBuffors(ToGoalCheck, null, null);
                        CurrentBuilder.ReStart(CurrentBuilded);
                    }
                    else
                        throw Realizing.Exception.InnerException;
                }
                //Canceled
                else if (Realizing.IsCanceled)
                {
                    //TODO
                }
                //OK
                else
                {
                    CurrentBuilded = Transcribing.Result.NewRoot;
                    CurrentBuilder.InitBuffors(null, Transcribing.Result.ChildlessCrisscrosses, null);                   
                    CurrentBuilder.ReStart(CurrentBuilded);
                }
            }
        }
    }
}
