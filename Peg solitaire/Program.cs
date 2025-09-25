/*
 * Treatment the game: https://en.wikipedia.org/wiki/Peg_solitaire
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using SharpPDDL;

namespace Peg_solitaire
{
    class Program
    {
        static ICollection<Spot> spots = new List<Spot>();
        static Stopwatch Stopwatch;

        static void Reset()
        {
            foreach (var s in spots)
                s.ResetMoved();
        }

        static void PrintPlan(List<List<string>> plan)
        {
            Stopwatch.Stop();
            Console.WriteLine("Plan generated in time: " + Stopwatch.Elapsed);
            Thread.Sleep(3000);
        }

        static void Main(string[] args)
        {
            for (ushort i = 0; i != 5; i++)
                for (ushort j = 0; j <= i; j++)
                    if (i == 0 && j == 0)
                        spots.Add(new Spot(j, i, false));
                    else             
                        spots.Add(new Spot(j, i));

            Board.Draw(spots);

            Stopwatch = new Stopwatch();
            Stopwatch.Start();

            DomeinPDDL SolitaireDomein = new DomeinPDDL("Solitaire");

            Spot JumpingPeg = null;
            Spot RemovePeg = null;
            Spot FinalPegPos = null;

            Expression<Predicate<Spot>> FullSpot = S => S.Full;
            Expression<Predicate<Spot>> EmptySpot = S => !S.Full;

            ActionPDDL VerticalJump = new ActionPDDL("Vertical jump");

            VerticalJump.AddPrecondiction<Spot,Spot>("Jumping peg exists", ref JumpingPeg, FullSpot);
            VerticalJump.AddPrecondiction<Spot, Spot>("Remove peg exists", ref RemovePeg, FullSpot);
            VerticalJump.AddPrecondiction<Spot, Spot>("Final position of peg is empty", ref FinalPegPos, EmptySpot);

            Expression<Predicate<Spot, Spot, Spot>> Verticalcollinear = ((JP, RP, FPP) => (JP.Column == RP.Column && RP.Column == FPP.Column));
            VerticalJump.AddPrecondiction("The same vertical line", ref JumpingPeg, ref RemovePeg, ref FinalPegPos, Verticalcollinear);

            Expression<Predicate<Spot, Spot>> HorizontalClose = ((S1, S2) => ((S1.Row - S2.Row) == 1 || (S1.Row - S2.Row) == -1));
            VerticalJump.AddPrecondiction("Jumper is close", ref JumpingPeg, ref RemovePeg, HorizontalClose);
            VerticalJump.AddPrecondiction("Hole is close", ref FinalPegPos, ref RemovePeg, HorizontalClose);

            VerticalJump.AddEffect("Jumping Peg Spot is empty", ref JumpingPeg, JP => JP.Full, false);
            VerticalJump.AddEffect("Remove Peg Spot is empty", ref RemovePeg, RP => RP.Full, false);
            VerticalJump.AddEffect("Final Peg Spot is full", ref FinalPegPos, RP => RP.Full, true);

            VerticalJump.AddExecution("Reset colours", () => Reset(), false);
            VerticalJump.AddExecution("Jumping Peg Spot is empty");
            VerticalJump.AddExecution("Remove Peg Spot is empty");
            VerticalJump.AddExecution("Final Peg Spot is full");
            VerticalJump.AddExecution("Draw it", () => Board.Draw(spots), true);
            VerticalJump.AddExecution("Wait", () => Thread.Sleep(1500), true);

            SolitaireDomein.AddAction(VerticalJump);

            ActionPDDL HorizontalJump = new ActionPDDL("Horizontal jump");

            HorizontalJump.AddPrecondiction<Spot, Spot>("Jumping peg exists", ref JumpingPeg, FullSpot);
            HorizontalJump.AddPrecondiction<Spot, Spot>("Remove peg exists", ref RemovePeg, FullSpot);
            HorizontalJump.AddPrecondiction<Spot, Spot>("Final position of peg is empty", ref FinalPegPos, EmptySpot);

            Expression<Predicate<Spot, Spot, Spot>> Horizontalcollinear = ((JP, RP, FPP) => (JP.Row == RP.Row && RP.Row == FPP.Row));
            HorizontalJump.AddPrecondiction("The same vertical line", ref JumpingPeg, ref RemovePeg, ref FinalPegPos, Horizontalcollinear);

            Expression<Predicate<Spot, Spot>> VerticalClose = ((S1, S2) => ((S1.Column - S2.Column) == 1 || (S1.Column - S2.Column) == -1));
            HorizontalJump.AddPrecondiction("Jumper is close", ref JumpingPeg, ref RemovePeg, VerticalClose);
            HorizontalJump.AddPrecondiction("Hole is close", ref FinalPegPos, ref RemovePeg, VerticalClose);

            HorizontalJump.AddEffect("Jumping Peg Spot is empty", ref JumpingPeg, JP => JP.Full, false);
            HorizontalJump.AddEffect("Remove Peg Spot is empty", ref RemovePeg, RP => RP.Full, false);
            HorizontalJump.AddEffect("Final Peg Spot is full", ref FinalPegPos, RP => RP.Full, true);

            HorizontalJump.AddExecution("Reset colours", () => Reset(), false);
            HorizontalJump.AddExecution("Jumping Peg Spot is empty");
            HorizontalJump.AddExecution("Remove Peg Spot is empty");
            HorizontalJump.AddExecution("Final Peg Spot is full");
            HorizontalJump.AddExecution("Draw it", () => Board.Draw(spots), true);
            HorizontalJump.AddExecution("Wait", () => Thread.Sleep(1500), true);

            SolitaireDomein.AddAction(HorizontalJump);

            ActionPDDL SkewJump = new ActionPDDL("Skew jump");

            SkewJump.AddPrecondiction<Spot, Spot>("Jumping peg exists", ref JumpingPeg, FullSpot);
            SkewJump.AddPrecondiction<Spot, Spot>("Remove peg exists", ref RemovePeg, FullSpot);
            SkewJump.AddPrecondiction<Spot, Spot>("Final position of peg is empty", ref FinalPegPos, EmptySpot);

            SkewJump.AddPrecondiction("Jumper is close", ref JumpingPeg, ref RemovePeg, VerticalClose);
            SkewJump.AddPrecondiction("Hole is close", ref FinalPegPos, ref RemovePeg, VerticalClose);
            SkewJump.AddPrecondiction("Jumper is close2", ref JumpingPeg, ref RemovePeg, HorizontalClose);
            SkewJump.AddPrecondiction("Hole is close2", ref FinalPegPos, ref RemovePeg, HorizontalClose);
            Expression<Predicate<Spot, Spot>> CorrectWay = ((S1, S2) => ((S1.Row - S2.Row) == (S1.Column - S2.Column)));
            SkewJump.AddPrecondiction("Correct Way 1", ref JumpingPeg, ref RemovePeg, CorrectWay);
            SkewJump.AddPrecondiction("Correct Way 2", ref FinalPegPos, ref RemovePeg, CorrectWay);

            SkewJump.AddEffect("Jumping Peg Spot is empty", ref JumpingPeg, JP => JP.Full, false);
            SkewJump.AddEffect("Remove Peg Spot is empty", ref RemovePeg, RP => RP.Full, false);
            SkewJump.AddEffect("Final Peg Spot is full", ref FinalPegPos, RP => RP.Full, true);

            SkewJump.AddExecution("Reset colours", () => Reset(), false);
            SkewJump.AddExecution("Jumping Peg Spot is empty");
            SkewJump.AddExecution("Remove Peg Spot is empty");
            SkewJump.AddExecution("Final Peg Spot is full");
            SkewJump.AddExecution("Draw it", () => Board.Draw(spots), true);
            SkewJump.AddExecution("Wait", () => Thread.Sleep(1500), true);

            SolitaireDomein.AddAction(SkewJump);

            foreach (var s in spots)
                SolitaireDomein.domainObjects.Add(s);

            GoalPDDL goalPDDL = new GoalPDDL("Only one");

            foreach (var o in spots)
            {
                if (o.Column == 0 && o.Row == 0)
                    goalPDDL.AddExpectedObjectState(o, FullSpot);
                else
                    goalPDDL.AddExpectedObjectState(o, EmptySpot);
            }

            SolitaireDomein.AddGoal(goalPDDL);
            SolitaireDomein.SetExecutionOptions(null, null, AskToAgree.GO_AHEAD);
            SolitaireDomein.PlanGenerated += PrintPlan;

            SolitaireDomein.Start();
            Console.ReadKey();
        }
    }
}