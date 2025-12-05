using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace SharpPDDL
{
    public delegate void ListOfString(List<List<string>> planGenerated);

    internal class DomainPlanner

    //       |￣￣￣￣￣￣￣￣|
    //       |Clean the mess!|
    //       |＿＿＿＿＿＿＿＿|
    //       (\__/) ||
    //       (•ㅅ•) ||
    //        /   づ

    {
        protected Dictionary<FoungingGoalDetail, SortedSet<Crisscross>> FoundedGoals;
        protected Dictionary<Crisscross, List<GoalPDDL>> FoundedCrisscrosses;
        protected Crisscross CurrentBuilded;
        internal CrisscrossGenerator CurrentBuilder { get; private set; }
        protected Executor DomainExecutor;
        protected readonly DomeinPDDL Owner;
        internal Action<uint> currentMinCumulativeCostUpdate;
        protected Action<KeyValuePair<Crisscross, List<GoalPDDL>>> FoundSols;
        internal ListOfString PlanGeneratedInDomainPlanner;
        internal List<ThumbnailObject> OneUnuseObjects;

        CancellationToken ExternalCancellationDomein;
        internal CancellationTokenSource InternalCancellationDomeinSrc;
        CancellationToken CancellationDomein;

        internal DomainPlanner(DomeinPDDL Owner)
        {
            this.Owner = Owner;
            FoundSols += FoundSolsVoid;
            CurrentBuilded = new Crisscross
            {
                Content = Owner.CurrentState
            };

            DomainExecutor = new Executor(Owner);
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
            InternalCancellationDomeinSrc = new CancellationTokenSource();

            CancellationDomein = CancellationTokenSource.CreateLinkedTokenSource(ExternalCancellationDomein, InternalCancellationDomeinSrc.Token).Token;

            OneUnuseObjects = new List<ThumbnailObject>();
            CurrentBuilder.CrisscrossesGenerated += AtAllStateGenerated;
            CurrentBuilder.Start(CancellationDomein);
        }

        internal void DomainGoals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add &&
                e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                return;

            Task CurrentBuilderStopping = null;
            bool PrevRuning = !CurrentBuilder.IsCrisscrossGeneratorCancellationRequested;
            if (PrevRuning)
                CurrentBuilderStopping = CurrentBuilder.Stop();

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (!(CurrentBuilderStopping is null))
                    CurrentBuilderStopping.Wait();

                foreach (var RemGoal in e.OldItems)
                {
                    RemoveFoundedGoal((GoalPDDL)RemGoal);
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 141, GloCla.ResMan.GetString("V10"), ((GoalPDDL)RemGoal).Name);
                }

                if (!(CurrentBuilderStopping is null))
                    CurrentBuilder.ReStart();

                return;
            }

            ICollection<GoalPDDL> ToCheckGoals;

            try
            {
                ToCheckGoals = (ICollection<GoalPDDL>)sender;
            }
            catch
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 13, GloCla.ResMan.GetString("C0"));
                throw new Exception(GloCla.ResMan.GetString("C0"));
            }

            if (ToCheckGoals.Any(TCG => Owner.domainGoals.Any(G => TCG.Name != G.Name)))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 14, GloCla.ResMan.GetString("E5"));
                throw new Exception(GloCla.ResMan.GetString("E5"));
            }

            foreach (GoalPDDL ToCheckGoal in ToCheckGoals)
                ToCheckGoal.BuildIt(Owner);

            foreach (Crisscross ToCheck in CurrentBuilded)
            {
                List<GoalPDDL> RealizatedList = CheckGoalInCol.CheckNewGoalsReach(ToCheck, ToCheckGoals);
                if (RealizatedList.Any())
                    this.FoundSols.Invoke(new KeyValuePair<Crisscross, List<GoalPDDL>>(ToCheck, RealizatedList));
            }

            if (PrevRuning && CurrentBuilder.IsCrisscrossGeneratorCancellationRequested)
                CurrentBuilder.ReStart();
        }

        private void FoundSolsVoid(KeyValuePair<Crisscross, List<GoalPDDL>> Found)
        {
            FoundedCrisscrosses[Found.Key] = Found.Value;
            string StartChecksum = CurrentBuilded.Content.CheckSum;

            foreach (GoalPDDL goalPDDL in Found.Value)
            {
                FoungingGoalDetail TempFoungingGoalDetail = new FoungingGoalDetail(goalPDDL);
                currentMinCumulativeCostUpdate?.Invoke(Found.Key.CumulativedTransitionCharge);
                if (CurrentBuilded.Content.CheckSum != StartChecksum)
                    break;

                if (FoundedGoals.ContainsKey(TempFoungingGoalDetail))
                    FoundedGoals[TempFoungingGoalDetail].Add(Found.Key);
                else
                {
                    FoundedGoals.Add(TempFoungingGoalDetail, new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge()) { Found.Key });

                    if (currentMinCumulativeCostUpdate is null)
                        currentMinCumulativeCostUpdate += CheckingIfGenerateActionList;
                }
            }

            if (this.DomainExecutor.ImplementorTask?.Status == TaskStatus.Running)
                return;

            if (Found.Value.Any(G => G.goalPriority == GoalPriority.TopHihtPriority))
            {
                this.GenList(Found);
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

            var FoundedChipStates = NOTIsFoundingChippest.Where(FG => (1.05 * FG.Value.First().CumulativedTransitionCharge < ActMinCumulativeCost));
            if (!FoundedChipStates.Any())
                return;

            int MaxCountSolution = FoundedChipStates.Max(FG => FG.Value.Count);

            KeyValuePair<FoungingGoalDetail, SortedSet<Crisscross>> Solution = FoundedChipStates.First(FG => FG.Value.Count == MaxCountSolution);

            Crisscross StateToHit = Solution.Value.Aggregate((curMin, v) => (curMin == null || (v.CumulativedTransitionCharge) < curMin.CumulativedTransitionCharge ? v : curMin));

            //Crisscross StateToHit = Solution.Value.Min(v => v.CumulativedTransitionCharge);
            currentMinCumulativeCostUpdate -= CheckingIfGenerateActionList;

            KeyValuePair<Crisscross, List<GoalPDDL>> GenerList = FoundedCrisscrosses.First(FC => FC.Key == StateToHit);
            //FoundedCrisscrosses.Clear();

            //foreach (GoalPDDL goalPDDL in GenerList.Value)
            //  Owner.domainGoals.Remove(goalPDDL);

            GenList(GenerList);

            for (int i = GenerList.Value.Count-1; i > 0; --i)
                Owner.domainGoals.Remove(GenerList.Value[i]);
        }

        private readonly object AtAllStateGeneratedLocker = new object();
        internal void AtAllStateGenerated()
        {
            //Do not use this whole function again
            lock (AtAllStateGeneratedLocker)
            {
                if (CurrentBuilder.CrisscrossesGenerated is null)
                    return;

                if (!CurrentBuilder.CrisscrossesGenerated.GetInvocationList().Any())
                    return;

                CurrentBuilder.CrisscrossesGenerated -= AtAllStateGenerated;
            }

            //info about generated all attainable states
            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 63, GloCla.ResMan.GetString("I7"));

            RealizeGoalsPlanifFound();
        }

        private void RealizeGoalsPlanifFound()
        {
            //realize goals if plan is found
            if (FoundedCrisscrosses.Any())
            {
                int Mfounded = FoundedCrisscrosses.Max(K => K.Value.Count());
                var Found = FoundedCrisscrosses.Where(K => K.Value.Count() == Mfounded).OrderBy(K => K.Key.CumulativedTransitionCharge).First();
                GenList(Found);
            }

            if (GloCla.Tracer is null)
                return;

            //Info about unattainable goals
            IEnumerable<GoalPDDL> UNattainable = Owner.domainGoals.Where(G => !(FoundedGoals.Keys.Any(K => K.GoalPDDL.Name == G.Name)));
            if (UNattainable.Any())
            {
                //unattainable goals with top high priority
                IList<GoalPDDL> UNattTHP = UNattainable.Where(G => G.goalPriority == GoalPriority.TopHihtPriority).ToList();
                int UNattTHPcount = UNattTHP.Count();

                //unattainable goals with high priority
                IList<GoalPDDL> UNattHP = UNattainable.Where(G => G.goalPriority == GoalPriority.HighPriority).ToList();
                int UNattHPcount = UNattHP.Count();

                //statistics of unnattainables goals
                GloCla.Tracer.TraceEvent(TraceEventType.Information, 140, GloCla.ResMan.GetString("I13"), UNattainable.Count(), UNattTHP, UNattHP);

                //enumerate unnattainable top high goals
                for (int i = 0; i != UNattTHPcount; i++)
                    GloCla.Tracer.TraceEvent(TraceEventType.Information, 141, GloCla.ResMan.GetString("I14"), i, UNattTHP[i].Name);

                //enumerate unnattainable high goals
                for (int i = 0; i != UNattHPcount; i++)
                    GloCla.Tracer.TraceEvent(TraceEventType.Information, 142, GloCla.ResMan.GetString("I15"), i, UNattHP[i].Name);
            }
        }

        internal void RemoveFoundedGoal(GoalPDDL goalPDDL)
        {
            if (!FoundedGoals.Any(FG => FG.Key.GoalPDDL.Equals(goalPDDL)))
                return;

            KeyValuePair<FoungingGoalDetail, SortedSet<Crisscross>> toRem = FoundedGoals.First(FG => FG.Key.GoalPDDL.Equals(goalPDDL));

            List<Crisscross> ToRemFromFoundedCrisscrosses = new List<Crisscross>();

            foreach (KeyValuePair<Crisscross, List<GoalPDDL>> FoundedCrisscross in FoundedCrisscrosses)
            {
                if (!FoundedCrisscross.Value.Any(v => v.Name == goalPDDL.Name))
                    continue;

                FoundedCrisscross.Value.Remove(goalPDDL);

                if (!FoundedCrisscross.Value.Any())
                    ToRemFromFoundedCrisscrosses.Add(FoundedCrisscross.Key);
            }

            if (ToRemFromFoundedCrisscrosses.Any())
                foreach (Crisscross crisscross in ToRemFromFoundedCrisscrosses)
                    FoundedCrisscrosses.Remove(crisscross);

            FoundedGoals.Remove(toRem.Key);
        }

        internal void GenList(KeyValuePair<Crisscross, List<GoalPDDL>> Found)
        {
            Crisscross state = CurrentBuilded;
            List<CrisscrossChildrenCon> FoKePo = Found.Key.Position();

            GloCla.Tracer?.TraceEvent(TraceEventType.Information, 16, GloCla.ResMan.GetString("I1"), Found.Value[0].Name, Found.Key.CumulativedTransitionCharge);

            if (FoKePo is null)
                return;

            List<List<string>> Plan = new List<List<string>>();

            for (int i = 0; i != FoKePo.Count; i++)
            {
                ThumbnailObject[] arg = new ThumbnailObject[Owner.actions[FoKePo[i].ActionNr].InstantActionParamCount];

                for (int j = 0; j != arg.Length; j++)
                    arg[j] = state.Content.ThumbnailObjects.First(ThOb => ThOb.OriginalObj.Equals(FoKePo[i].ActionArgOryg[j]));

                Plan.Add(new List<string> { String.Format(GloCla.ResMan.GetString("Txt1"), Owner.actions[FoKePo[i].ActionNr].Name), (string)Owner.actions[FoKePo[i].ActionNr].InstantActionSententia.DynamicInvoke(arg), String.Format(GloCla.ResMan.GetString("Txt2"), FoKePo[i].ActionCost) });

                state = FoKePo[i].Child;
            }

            PlanGeneratedInDomainPlanner?.Invoke(Plan);

            Task Stopping = CurrentBuilder.Stop();
            Task<(Crisscross NewRoot, SortedSet<Crisscross>, SortedList<string, Crisscross> NewIndexedStates)> Transcribing = CurrentBuilder.TranscribeState(FoKePo.Last().Child, CancellationDomein);
            Task.WaitAll(Stopping, Transcribing);

            //CurrentBuilded = Transcribing.Result.NewRoot;

            Task Realizing = DomainExecutor.RealizeIt(FoKePo, Found.Value, CancellationDomein);
            //CurrentBuilded.Dispose();
            Realizing.Wait();

            //internal fault
            if (Realizing.IsFaulted)
            {
                if (Realizing.Exception.InnerException is PrecondExecutionException)
                {
                    List<ThumbnailObject> RefreshedThumbnails = new List<ThumbnailObject>(OneUnuseObjects);
                    OneUnuseObjects.Clear();

                    /*foreach (ThumbnailObject UsedThObj in CurrentBuilded.Content.ThumbnailObjects)
                    {
                        ThumbnailObject UpdatedThObj = new ThumbnailObjectPrecursor<object>(UsedThObj) as ThumbnailObject;
                        RefreshedThumbnails.Add(UpdatedThObj);
                    }*/

                    /*CurrentBuilded = new Crisscross()
                    {
                        Content = new PossibleState(RefreshedThumbnails)
                    };*/

                    var ToGoalCheck = new ConcurrentQueue<Crisscross>();
                    ToGoalCheck.Enqueue(CurrentBuilded);

                    CurrentBuilder.InitBuffors(ToGoalCheck, null, null, null);
                    CurrentBuilder.ReStart();
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
                //CurrentBuilded.Dispose();
                CurrentBuilded = Transcribing.Result.NewRoot;
                CurrentBuilder.InitBuffors(Transcribing.Result.Item2, null, null, Transcribing.Result.NewIndexedStates);

                if(Owner.domainGoals.Except(Found.Value).Any())
                  CurrentBuilder.ReStart();
            }
        }
    }
}
