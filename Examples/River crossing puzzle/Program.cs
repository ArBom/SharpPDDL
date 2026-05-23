/*
 * Treatment the puzzle: https://en.wikipedia.org/wiki/Wolf,_goat_and_cabbage_problem
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SharpPDDL;

namespace River_crossing_puzzle
{
    #region EnvironmentClasses
    public abstract class Goods { }

    public class Cabbage : Goods { }
    public class Goat : Goods { }
    public class Wolf : Goods { }

    public class Boat
    {
        public Goods good;
    }

    public class RiverBank
    {
        public Boat boat;
        public Goat goat;
        public Wolf wolf;
        public Cabbage cabbage;
    }
    #endregion

    class Program
    {
        #region DomainActions
        static ActionPDDL Action_TakingCabbage()
        {
            RiverBank nextToBank = null;
            Boat boat = null;
            Cabbage cabbage = null;

            ActionPDDL TakingCabbage = new ActionPDDL("Taking cabbage");
            TakingCabbage.AddPartOfActionSententia("Take the cabbage.");
            TakingCabbage.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is near the bank", ref nextToBank, ref boat, (bank, b) => bank.boat == b);
            TakingCabbage.AddPrecondition<RiverBank, RiverBank, Cabbage, Cabbage>("Cabbage is at the bank", ref nextToBank, ref cabbage, (bank, c) => bank.cabbage == c);
            TakingCabbage.AddPrecondition("Boat is empty", ref boat, b => b.good == null);
            TakingCabbage.AddEffect("Remove the cabbage from the bank", ref nextToBank, b => b.cabbage, null);
            TakingCabbage.AddEffect("Put the cabbage on the boat", ref boat, b => b.good, ref cabbage);
            return TakingCabbage;
        }

        static ActionPDDL Action_TakingGoat()
        {
            RiverBank nextToBank = null;
            Boat boat = null;
            Goat goat = null;

            ActionPDDL TakingGoat = new ActionPDDL("Taking goat");
            TakingGoat.AddPartOfActionSententia("Take the goat.");
            TakingGoat.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is next to the bank", ref nextToBank, ref boat, (n, b) => n.boat == b);
            TakingGoat.AddPrecondition<RiverBank, RiverBank, Goat, Goat>("Goat is at the bank", ref nextToBank, ref goat, (n, g) => n.goat == g);
            TakingGoat.AddPrecondition("Boat is empty", ref boat, b => b.good == null);
            TakingGoat.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.goat, null);
            TakingGoat.AddEffect("Add the goat to the boat", ref boat, b => b.good, ref goat);
            return TakingGoat;
        }

        static ActionPDDL Action_TakingWolf()
        {
            RiverBank nextToBank = null;
            Boat boat = null;
            Wolf wolf = null;

            ActionPDDL TakingWolf = new ActionPDDL("Taking wolf");
            TakingWolf.AddPartOfActionSententia("Take the wolf.");
            TakingWolf.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is next the bank", ref nextToBank, ref boat, (n, b) => n.boat == b);
            TakingWolf.AddPrecondition<RiverBank, RiverBank, Wolf, Wolf>("Wolf is at the bank", ref nextToBank, ref wolf, (b, w) => b.wolf == w);
            TakingWolf.AddPrecondition("Boat is empty", ref boat, b => b.good == null);
            TakingWolf.AddEffect("Remove the wolf from the bank", ref nextToBank, b => b.wolf, null);
            TakingWolf.AddEffect("Add the wolf to the boat", ref boat, b => b.good, ref wolf);
            return TakingWolf;
        }

        static ActionPDDL Action_PutCabbageAway()
        {
            RiverBank nextToBank = null;
            Boat boat = null;
            Cabbage cabbage = null;

            ActionPDDL PutCabbageAway = new ActionPDDL("Putting cabbage away");
            PutCabbageAway.AddPartOfActionSententia("Put the cabbage away.");
            PutCabbageAway.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is near the bank", ref nextToBank, ref boat, (n, b) => n.boat == b);
            PutCabbageAway.AddPrecondition<Boat, Boat, Cabbage, Cabbage>("There is the cabbage on the boat", ref boat, ref cabbage, (b, c) => b.good == c);
            PutCabbageAway.AddEffect("Empty the boat", ref boat, b => b.good, null);
            PutCabbageAway.AddEffect("Add the cabbage to the bank", ref nextToBank, b => b.cabbage, ref cabbage);
            return PutCabbageAway;
        }

        static ActionPDDL Action_PutGoatAway()
        {
            RiverBank nextToBank = null;
            Boat boat = null;
            Goat goat = null;

            ActionPDDL PutGoatAway = new ActionPDDL("Putting goat away");
            PutGoatAway.AddPartOfActionSententia("Put the goat away.");
            PutGoatAway.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is next the bank", ref nextToBank, ref boat, (n, b) => n.boat == b);
            PutGoatAway.AddPrecondition<Boat, Boat, Goat, Goat>("Goat is at the bank", ref boat, ref goat, (b, g) => b.good == g);
            PutGoatAway.AddEffect("Empty the boat", ref boat, b => b.good, null);
            PutGoatAway.AddEffect("Add the goat to the bank", ref nextToBank, b => b.goat, ref goat);
            return PutGoatAway;
        }

        static ActionPDDL Action_PutWolfAway()
        {
            RiverBank nextToBank = null;
            Boat boat = null;
            Wolf wolf = null;

            ActionPDDL PutWolfAway = new ActionPDDL("Putting wolf away");
            PutWolfAway.AddPartOfActionSententia("Put the wolf away.");
            PutWolfAway.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is next the bank", ref nextToBank, ref boat, (n, b) => n.boat == b);
            PutWolfAway.AddPrecondition<Boat, Boat, Wolf, Wolf>("Wolf is at the bank", ref boat, ref wolf, (b, w) => b.good == w);
            PutWolfAway.AddEffect("Empty the boat", ref boat, b => b.good, null);
            PutWolfAway.AddEffect("Add the wolf to the bank", ref nextToBank, b => b.wolf, ref wolf);
            return PutWolfAway;
        }

        static ActionPDDL Action_CrossTheRivar()
        {
            RiverBank nextToBank = null;
            RiverBank SecendBank = null;
            Boat boat = null;

            ActionPDDL CrossTheRiver = new ActionPDDL("Crossing the river");
            CrossTheRiver.AddPartOfActionSententia("Cross the river.");
            CrossTheRiver.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is near the bank", ref nextToBank, ref boat, (ba, bo) => ba.boat == bo);
            CrossTheRiver.AddPrecondition("Nothing won't be eaten", ref nextToBank, b => b.goat != null ? (b.cabbage == null && b.wolf == null) : true);
            CrossTheRiver.AddEffect("Leave the river bank", ref nextToBank, b => b.boat, null);
            CrossTheRiver.AddEffect("Go to the other bank", ref SecendBank, b => b.boat, ref boat);
            return CrossTheRiver;
        }
        #endregion

        static void PrintPlan(List<List<string>> plan)
        {
            for (int i = 0; i != plan.Count; i++)
                Console.WriteLine(i + 1 + ": " + plan[i][1]);
        }

        static void Main(string[] args)
        {
            //create the domain of this puzzle
            DomainPDDL RiverCrossing = new DomainPDDL("ProblemOfRiver");

            //define actions of domain
            RiverCrossing.AddAction(Action_TakingCabbage());
            RiverCrossing.AddAction(Action_TakingGoat());
            RiverCrossing.AddAction(Action_TakingWolf());
            RiverCrossing.AddAction(Action_PutCabbageAway());
            RiverCrossing.AddAction(Action_PutGoatAway());
            RiverCrossing.AddAction(Action_PutWolfAway());
            RiverCrossing.AddAction(Action_CrossTheRivar());

            //define objects existed beyond the domain
            RiverBank NorthVistulaBank = new RiverBank();
            RiverBank SouthVistulaBank = new RiverBank();
            Boat Titatinic = new Boat();
            Cabbage Vegeta = new Cabbage();
            Wolf Wolfgang = new Wolf();
            Goat Pilgor = new Goat();

            //create relationships between objects
            SouthVistulaBank.boat = Titatinic;
            SouthVistulaBank.cabbage = Vegeta;
            SouthVistulaBank.goat = Pilgor;
            SouthVistulaBank.wolf = Wolfgang;

            //add objects to domain
            RiverCrossing.domainObjects.Add(Vegeta);
            RiverCrossing.domainObjects.Add(Pilgor);
            RiverCrossing.domainObjects.Add(Wolfgang);
            RiverCrossing.domainObjects.Add(NorthVistulaBank);
            RiverCrossing.domainObjects.Add(SouthVistulaBank);
            RiverCrossing.domainObjects.Add(Titatinic);

            //set a goal
            var ExpectedState = new List<Expression<Predicate<RiverBank>>>
            {
                RB => 
                    RB.cabbage != null && 
                    RB.goat != null && 
                    RB.wolf != null
            };

            GoalPDDL crossTheRiver = new GoalPDDL("Cross the river");
            crossTheRiver.AddExpectedObjectState(NorthVistulaBank, ExpectedState);
            RiverCrossing.AddGoal(crossTheRiver);

            //print plan with PrintPlan the method
            RiverCrossing.PlanGenerated += PrintPlan;

            //make diagrams while run
            RiverCrossing.GenerateDiagrams("diagram.dgml", Diagram.UseCase | Diagram.Class | Diagram.States);

            //start making the solution and diagrams
            RiverCrossing.Start();
            Console.ReadKey();
        }
    }
}
