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
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            for (ushort i = 0; i != 7; i++)
                for (ushort j = 0; j != 7; j++)
                    if ((i < 2 && j < 2) || 
                        (i < 2 && j > 4) || 
                        (i > 4 && j < 2) || 
                        (i > 4 && j > 4))
                        continue;
                    else
                    {
                        Spot NewPeg;

                        if (i == 3 && j == 3)
                            NewPeg = new Spot(i, j, false);
                        else
                            NewPeg = new Spot(i, j);

                        spots.Add(NewPeg);
                    }

            Console.WriteLine("The library is unoptimised. Work about that example lasts eternity... or even longer");

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

            SolitaireDomein.AddAction(HorizontalJump);

            foreach (var s in spots)
                SolitaireDomein.domainObjects.Add(s);

            GoalPDDL goalPDDL = new GoalPDDL("Only one");

            foreach (var o in spots)
            {
                if (o.Column == 3 && o.Row == 3)
                    goalPDDL.AddExpectedObjectState(o, FullSpot);
                else
                    goalPDDL.AddExpectedObjectState(o, EmptySpot);
            }

            SolitaireDomein.AddGoal(goalPDDL);

            SolitaireDomein.PlanGenerated += PrintPlan;

            SolitaireDomein.Start();
            Console.ReadKey();
        }
    }
}