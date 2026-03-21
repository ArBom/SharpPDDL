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
            Console.WriteLine(" Plan generated in time:");
            Console.WriteLine(" " + Stopwatch.Elapsed);
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
            Console.WriteLine();

            Stopwatch = new Stopwatch();
            Stopwatch.Start();

            DomainPDDL SolitaireDomein = new DomainPDDL("Solitaire");

            Spot JumpingPeg = null;
            Spot RemovePeg = null;
            Spot FinalPegPos = null;

            Expression<Predicate<Spot>> FullSpot = S => S.Full;
            Expression<Predicate<Spot>> EmptySpot = S => !S.Full;

            ActionPDDL VerticalJump = new ActionPDDL("Vertical jump");

            VerticalJump.AddPrecondition<Spot,Spot>("Jumping peg exists", ref JumpingPeg, FullSpot); //JumpingPeg.Full
            VerticalJump.AddPrecondition<Spot, Spot>("Remove peg exists", ref RemovePeg, FullSpot); //RemovePeg.Full
            VerticalJump.AddPrecondition<Spot, Spot>("Final position of peg is empty", ref FinalPegPos, EmptySpot); //!FinalPeg.Full

            Expression<Predicate<Spot, Spot, Spot>> Verticalcollinear = ((JP, RP, FPP) => (JP.Column == RP.Column && RP.Column == FPP.Column));
            
            //JumpingPeg.Column == RemovePeg.Column && RemovePeg.Column == FinalPegPos.Column
            VerticalJump.AddPrecondition("The same vertical line", ref JumpingPeg, ref RemovePeg, ref FinalPegPos, Verticalcollinear);

            Expression<Predicate<Spot, Spot>> HorizontalClose = ((S1, S2) => (Math.Abs(S1.Row - S2.Row) == 1));
            VerticalJump.AddPrecondition("Jumper is close", ref JumpingPeg, ref RemovePeg, HorizontalClose); //Math.Abs(JumpingPeg.Row - HorizontalClose.Row) == 1
            VerticalJump.AddPrecondition("Hole is close", ref FinalPegPos, ref RemovePeg, HorizontalClose); //Math.Abs(FinalPegPos.Row - RemovePeg.Row) == 1

            VerticalJump.AddEffect("Jumping Peg Spot is empty", ref JumpingPeg, JP => JP.Full, false); //JumpingPeg.Full = false
            VerticalJump.AddEffect("Remove Peg Spot is empty", ref RemovePeg, RP => RP.Full, false); //RemovePeg.Full = false
            VerticalJump.AddEffect("Final Peg Spot is full", ref FinalPegPos, RP => RP.Full, true); //FinalPegPos.Full = true

            VerticalJump.AddExecution("Reset colours", () => Reset(), false);
            VerticalJump.AddExecution("Jumping Peg Spot is empty");
            VerticalJump.AddExecution("Remove Peg Spot is empty");
            VerticalJump.AddExecution("Final Peg Spot is full");
            VerticalJump.AddExecution("Draw it", () => Board.Draw(spots), true);
            VerticalJump.AddExecution("Wait", () => Thread.Sleep(1500), true);

            SolitaireDomein.AddAction(VerticalJump);

            ActionPDDL HorizontalJump = new ActionPDDL("Horizontal jump");

            HorizontalJump.AddPrecondition<Spot, Spot>("Jumping peg exists", ref JumpingPeg, FullSpot); //JumpingPeg.Full
            HorizontalJump.AddPrecondition<Spot, Spot>("Remove peg exists", ref RemovePeg, FullSpot); //RemovePeg.Full
            HorizontalJump.AddPrecondition<Spot, Spot>("Final position of peg is empty", ref FinalPegPos, EmptySpot); //!FinalPeg.Full

            //JumpingPeg.Row == RemovePeg.Row && RemovePeg.Row == FinalPegPos.Row
            Expression<Predicate<Spot, Spot, Spot>> Horizontalcollinear = ((JP, RP, FPP) => (JP.Row == RP.Row && RP.Row == FPP.Row));
            HorizontalJump.AddPrecondition("The same vertical line", ref JumpingPeg, ref RemovePeg, ref FinalPegPos, Horizontalcollinear);

            Expression<Predicate<Spot, Spot>> VerticalClose = ((S1, S2) => (Math.Abs(S1.Column - S2.Column) == 1));
            HorizontalJump.AddPrecondition("Jumper is close", ref JumpingPeg, ref RemovePeg, VerticalClose); //Math.Abs(JumpingPeg.Column - RemovePeg.Column) == 1
            HorizontalJump.AddPrecondition("Hole is close", ref FinalPegPos, ref RemovePeg, VerticalClose); //Math.Abs(FinalPegPos.Column - RemovePeg.Column) == 1

            HorizontalJump.AddEffect("Jumping Peg Spot is empty", ref JumpingPeg, JP => JP.Full, false); //JumpingPeg.Full = false
            HorizontalJump.AddEffect("Remove Peg Spot is empty", ref RemovePeg, RP => RP.Full, false); //RemovePeg.Full = false
            HorizontalJump.AddEffect("Final Peg Spot is full", ref FinalPegPos, RP => RP.Full, true); //FinalPegPos.Full = true

            HorizontalJump.AddExecution("Reset colours", () => Reset(), false);
            HorizontalJump.AddExecution("Jumping Peg Spot is empty");
            HorizontalJump.AddExecution("Remove Peg Spot is empty");
            HorizontalJump.AddExecution("Final Peg Spot is full");
            HorizontalJump.AddExecution("Draw it", () => Board.Draw(spots), true);
            HorizontalJump.AddExecution("Wait", () => Thread.Sleep(1500), true);

            SolitaireDomein.AddAction(HorizontalJump);

            ActionPDDL SkewJump = new ActionPDDL("Skew jump");

            SkewJump.AddPrecondition<Spot, Spot>("Jumping peg exists", ref JumpingPeg, FullSpot); //JumpingPeg.Full
            SkewJump.AddPrecondition<Spot, Spot>("Remove peg exists", ref RemovePeg, FullSpot); //RemovePeg.Full
            SkewJump.AddPrecondition<Spot, Spot>("Final position of peg is empty", ref FinalPegPos, EmptySpot); //!FinalPegPos.Full

            SkewJump.AddPrecondition("Jumper is close", ref JumpingPeg, ref RemovePeg, VerticalClose); //Math.Abs(JumpingPeg.Column - RemovePeg.Column) == 1
            SkewJump.AddPrecondition("Hole is close", ref FinalPegPos, ref RemovePeg, VerticalClose); //Math.Abs(FinalPegPos.Column - RemovePeg.Column) == 1
            SkewJump.AddPrecondition("Jumper is close2", ref JumpingPeg, ref RemovePeg, HorizontalClose); //Math.Abs(JumpingPeg.Row - RemovePeg.Row) == 1
            SkewJump.AddPrecondition("Hole is close2", ref FinalPegPos, ref RemovePeg, HorizontalClose); //Math.Abs(FinalPegPos.Row - RemovePeg.Row) == 1
            Expression<Predicate<Spot, Spot>> CorrectWay = ((S1, S2) => ((S1.Row - S2.Row) == (S1.Column - S2.Column)));
            SkewJump.AddPrecondition("Correct Way 1", ref JumpingPeg, ref RemovePeg, CorrectWay); //(JumpingPeg.Row - RemovePeg.Row) == (JumpingPeg.Column - RemovePeg.Column)
            SkewJump.AddPrecondition("Correct Way 2", ref FinalPegPos, ref RemovePeg, CorrectWay); //(FinalPegPos.Row - RemovePeg.Row) == (FinalPegPos.Column - RemovePeg.Column)

            SkewJump.AddEffect("Jumping Peg Spot is empty", ref JumpingPeg, JP => JP.Full, false); //JumpingPeg.Full = false
            SkewJump.AddEffect("Remove Peg Spot is empty", ref RemovePeg, RP => RP.Full, false); //RemovePeg.Full = false
            SkewJump.AddEffect("Final Peg Spot is full", ref FinalPegPos, RP => RP.Full, true); //FinalPegPos.Full = true

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