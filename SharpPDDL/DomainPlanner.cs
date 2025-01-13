using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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
        readonly List<ActionPDDL> Actions;
        internal Action<uint> currentMinCumulativeCostUpdate;
        internal Action<KeyValuePair<Crisscross, List<GoalPDDL>>> FoundSols;
        internal ListOfString PlanGeneratedInDomainPlanner;

        CancellationToken ExternalCancellationDomein;
        CancellationTokenSource InternalCancellationTokenSrc;
        CancellationToken CurrentCancelTokenS;

        internal DomainPlanner(DomeinPDDL Owner)
        {
            Actions = Owner.actions;
            FoundSols += FoundSolsVoid;
            CurrentBuilded = new Crisscross
            {
                Content = Owner.CurrentState
            };

            CurrentBuilder = new CrisscrossGenerator(CurrentBuilded, Owner, FoundSols, currentMinCumulativeCostUpdate);
            FoundedGoals = new Dictionary<FoungingGoalDetail, SortedSet<Crisscross>>();
            FoundedCrisscrosses = new Dictionary<Crisscross, List<GoalPDDL>>(Crisscross.IContentEqualityComparer);
        }

        internal void Start(ParallelOptions options)
        {
            //remember External CancelationToken to reuse it after reset
            this.ExternalCancellationDomein = options.CancellationToken;

            //Token used to reset CrisscrossGenerator process when it is too big
            InternalCancellationTokenSrc = new CancellationTokenSource();

            CurrentCancelTokenS = CancellationTokenSource.CreateLinkedTokenSource(ExternalCancellationDomein, InternalCancellationTokenSrc.Token).Token;

            CurrentBuilder.CrisscrossesGenerated += AtAllStateGenerated;
            CurrentBuilder.Start(CurrentCancelTokenS);
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

                    //if (currentMinCumulativeCostUpdate == null)
                      //  currentMinCumulativeCostUpdate += f;
                }
            }
        }

        private void f(uint i)
        {
            var NOTIsFoundingChippest = FoundedGoals.Where(FG => !FG.Key.IsFoundingChippest);
            var FoundedChippest = NOTIsFoundingChippest.Where(FG => (FG.Value.First().CumulativedTransitionCharge < i));
            foreach (var foundedChippest in FoundedChippest)
            {
                foundedChippest.Key.IsFoundingChippest = true;
                GenList(FoundedCrisscrosses.First(FC => FC.Key.Equals(foundedChippest.Key.GoalPDDL)));
            }

            if (NOTIsFoundingChippest.Count() == FoundedChippest.Count())
            {
                currentMinCumulativeCostUpdate -= f;
            }
        }

        object AtAllStateGeneratedLocker = new object();

        private void AtAllStateGenerated()
        {
            lock (AtAllStateGeneratedLocker)
            if (FoundedCrisscrosses.Any() && !(CurrentBuilder.CrisscrossesGenerated is null))
            {
                CurrentBuilder.CrisscrossesGenerated -= AtAllStateGenerated;
                int Mfounded = FoundedCrisscrosses.Max(K => K.Value.Count());
                var t = FoundedCrisscrosses.Where(K => K.Value.Count() == Mfounded).OrderBy(K => K.Key.CumulativedTransitionCharge).First();
                GenList(t);
            }
        }

        internal void GenList(KeyValuePair<Crisscross, List<GoalPDDL>> Found)
        {
            Crisscross state = CurrentBuilded;
            List<CrisscrossChildrenCon> FoKePo = Found.Key.Position();

            Console.WriteLine(ExtensionMethods.TracePrefix + Found.Value[0].Name + " determined!!! Total Cost: " + Found.Key.CumulativedTransitionCharge);

            if (!(FoKePo is null))
            {
                List<List<string>> Plan = new List<List<string>>();

                for (int i = 0; i != FoKePo.Count; i++)
                {
                    PossibleStateThumbnailObject[] arg = new PossibleStateThumbnailObject[Actions[FoKePo[i].ActionNr].InstantActionParamCount];

                    for (int j = 0; j != arg.Length; j++)
                    {
                        arg[j] = state.Content.ThumbnailObjects.First(ThOb => ThOb.OriginalObj.Equals(FoKePo[i].ActionArgOryg[j]));
                    }

                    Plan.Add(new List<string> { Actions[FoKePo[i].ActionNr].Name + ": ", (string)Actions[FoKePo[i].ActionNr].InstantActionSententia.DynamicInvoke(arg), " Action cost: " + FoKePo[i].ActionCost });

                    state = FoKePo[i].Child;
                }

                PlanGeneratedInDomainPlanner?.Invoke(Plan);
            }
        }
    }
}
