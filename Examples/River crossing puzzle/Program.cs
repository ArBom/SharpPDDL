/*
 * Treatment the puzzle: https://en.wikipedia.org/wiki/Wolf,_goat_and_cabbage_problem
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SharpPDDL;

namespace River_crossing_puzzle
{
    class Program
    {
        public abstract class PlaceForGoods
        {
            public bool IsCabbage = false;
            public bool IsGoat = false;
            public bool IsWolf = false;
        }

        public class Boat : PlaceForGoods { }

        public class RiverBank : PlaceForGoods
        {
            public bool IsBoat;
        }

        static void PrintPlan(List<List<string>> plan)
        {
            for (int i = 0; i != plan.Count; i++)
                Console.WriteLine(i+1 + ": " + plan[i][1]);
        }

        static void Main(string[] args)
        {
            DomeinPDDL RiverCrossing = new DomeinPDDL("ProblemOfRiver");
           
            RiverBank nextToBank = null;
            Boat boat = null;

            ActionPDDL TakingCabbage = new ActionPDDL("TakingCabbage");
            TakingCabbage.AddPartOfActionSententia("Take the cabbage.");
            TakingCabbage.AddPrecondiction("Boat is near the bank", ref nextToBank, b => b.IsBoat);
            TakingCabbage.AddPrecondiction("Cabbage is at the bank", ref nextToBank, b => b.IsCabbage);
            TakingCabbage.AddPrecondiction("Boat is empty", ref boat, b => !b.IsCabbage && !b.IsGoat && !b.IsWolf);
            TakingCabbage.AddEffect("Remove the cabbage from the bank", ref nextToBank, b => b.IsCabbage, false);
            TakingCabbage.AddEffect("Put the cabbage on the boat", ref boat, b => b.IsCabbage, true);
            RiverCrossing.AddAction(TakingCabbage);

            ActionPDDL TakingGoat = new ActionPDDL("TakingGoat");
            TakingGoat.AddPartOfActionSententia("Take the goat.");
            TakingGoat.AddPrecondiction("Boat is next the bank", ref nextToBank, b => b.IsBoat);
            TakingGoat.AddPrecondiction("Goat is at the bank", ref nextToBank, b => b.IsGoat);
            TakingGoat.AddPrecondiction("Boat is empty", ref boat, b => !b.IsCabbage && !b.IsGoat && !b.IsWolf);
            TakingGoat.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.IsGoat, false);
            TakingGoat.AddEffect("Add the goat to the boat", ref boat, b => b.IsGoat, true);
            RiverCrossing.AddAction(TakingGoat);

            ActionPDDL TakingWolf = new ActionPDDL("TakingWolf");
            TakingWolf.AddPartOfActionSententia("Take the wolf.");
            TakingWolf.AddPrecondiction("Boat is next the bank", ref nextToBank, b => b.IsBoat);
            TakingWolf.AddPrecondiction("Goat is at the bank", ref nextToBank, b => b.IsWolf);
            TakingWolf.AddPrecondiction("Boat is empty", ref boat, b => !b.IsCabbage && !b.IsGoat && !b.IsWolf);
            TakingWolf.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.IsWolf, false);
            TakingWolf.AddEffect("Add the goat to the boat", ref boat, b => b.IsWolf, true);
            RiverCrossing.AddAction(TakingWolf);

            ActionPDDL PutCabbageAway = new ActionPDDL("PuttingCabbageAway");
            PutCabbageAway.AddPartOfActionSententia("Put the cabbage away.");
            PutCabbageAway.AddPrecondiction("Boat is near the bank", ref nextToBank, b => b.IsBoat);
            PutCabbageAway.AddPrecondiction("Goat is on the bank", ref boat, b => b.IsCabbage);
            PutCabbageAway.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.IsCabbage, true);
            PutCabbageAway.AddEffect("Add the goat to the boat", ref boat, b => b.IsCabbage, false);
            RiverCrossing.AddAction(PutCabbageAway);

            ActionPDDL PutGoatAway = new ActionPDDL("PuttingGoatAway");
            PutGoatAway.AddPartOfActionSententia("Put the goat away.");
            PutGoatAway.AddPrecondiction("Boat is next the bank", ref nextToBank, b => b.IsBoat);
            PutGoatAway.AddPrecondiction("Goat is at the bank", ref boat, b => b.IsGoat);
            PutGoatAway.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.IsGoat, true);
            PutGoatAway.AddEffect("Add the goat to the boat", ref boat, b => b.IsGoat, false);
            RiverCrossing.AddAction(PutGoatAway);

            ActionPDDL PutWolfAway = new ActionPDDL("PuttingWolfAway");
            PutWolfAway.AddPartOfActionSententia("Put the wolf away.");
            PutWolfAway.AddPrecondiction("Boat is next the bank", ref nextToBank, b => b.IsBoat);
            PutWolfAway.AddPrecondiction("Goat is at the bank", ref boat, b => b.IsWolf);
            PutWolfAway.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.IsWolf, true);
            PutWolfAway.AddEffect("Add the goat to the boat", ref boat, b => b.IsWolf, false);
            RiverCrossing.AddAction(PutWolfAway);

            ActionPDDL CrossTheRiver = new ActionPDDL("CrossingTheRiver");
            CrossTheRiver.AddPartOfActionSententia("Cross the river.");
            CrossTheRiver.AddPrecondiction("Boat is near the bank", ref nextToBank, b => b.IsBoat);
            CrossTheRiver.AddPrecondiction("Nothing won't be eaten", ref nextToBank, b => b.IsGoat ? (!b.IsCabbage && !b.IsWolf) : true );
            RiverBank SecendBank = null;
            CrossTheRiver.AddEffect("Leave the river bank", ref nextToBank, b => b.IsBoat, false);
            CrossTheRiver.AddEffect("Go to the other bank", ref SecendBank, b => b.IsBoat, true);
            RiverCrossing.AddAction(CrossTheRiver);

            RiverBank NorthRiverBank = new RiverBank();
            RiverBank SouthRiverBank = new RiverBank();

            SouthRiverBank.IsBoat = true;
            SouthRiverBank.IsCabbage = true;
            SouthRiverBank.IsGoat = true;
            SouthRiverBank.IsWolf = true;

            RiverCrossing.domainObjects.Add(NorthRiverBank);
            RiverCrossing.domainObjects.Add(SouthRiverBank);
            RiverCrossing.domainObjects.Add(new Boat());

            GoalPDDL crossTheRiver = new GoalPDDL("Cross the river");
            crossTheRiver.AddExpectedObjectState(NorthRiverBank, new List<Expression<Predicate<RiverBank>>> { RB => RB.IsCabbage && RB.IsGoat && RB.IsWolf });
            RiverCrossing.AddGoal(crossTheRiver);

            RiverCrossing.PlanGenerated += PrintPlan;

            RiverCrossing.Start();
            Console.ReadKey();
        }
    }
}
