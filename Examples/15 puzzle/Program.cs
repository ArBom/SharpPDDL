/*
 * Treatment the puzzle: https://en.wikipedia.org/wiki/15_puzzle
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpPDDL;

namespace _15_puzzle
{
    class Program
    {
        static List<Tile> tiles = new List<Tile>();
        static DomeinPDDL GemPuzzleDomein;

        static ICollection<Expression<Predicate<Tile>>> ExpressionsOfXTile(int i)
        {
            List<Expression<Predicate<Tile>>> TileAtSpot = new List<Expression<Predicate<Tile>>>
            {
                T => T.Col == i % 4,
                T => T.Row == i / 4,
                T => T.TileValue == i + 1
            };
            return TileAtSpot;
        }

        static void AddTileXGoal(object a, object b)
        {
            GoalPDDL goalPDDL = (GoalPDDL)a;
            int c = int.Parse(goalPDDL.Name);
            AddTileXGoal(c);
        }

        static void AddTileXGoal(int i)
        {
            if (i < 16)
            {
                GoalPDDL Tile1Goal = new GoalPDDL((i + 1).ToString());

                for (int j = 0; j < i + 1; j++)
                    Tile1Goal.AddExpectedObjectState(ExpressionsOfXTile(j));

                //GoalPDDL NextOne = ExpressionsOfXTile(i + 1);
                GemPuzzleDomein.AddGoal(Tile1Goal);
                Tile1Goal.GoalRealized += AddTileXGoal;
            }
        }

        static void Main(string[] args)
        {        
            tiles.Add(new Tile(12, 0, 0));
            tiles.Add(new Tile(1, 1, 0));
            tiles.Add(new Tile(2, 2, 0));
            tiles.Add(new Tile(15, 3, 0));

            tiles.Add(new Tile(11, 0, 1));
            tiles.Add(new Tile(6, 1, 1));
            tiles.Add(new Tile(5, 2, 1));
            tiles.Add(new Tile(8, 3, 1));

            tiles.Add(new Tile(7, 0, 2));
            tiles.Add(new Tile(10, 1, 2));
            tiles.Add(new Tile(9, 2, 2));
            tiles.Add(new Tile(4, 3, 2));

            tiles.Add(new Tile(16, 0, 3));
            tiles.Add(new Tile(13, 1, 3));
            tiles.Add(new Tile(14, 2, 3));
            tiles.Add(new Tile(3, 3, 3));

            Board.DrawBoard(tiles);

            GemPuzzleDomein = new DomeinPDDL("GemPuzzle");

            foreach (Tile tile in tiles)
                GemPuzzleDomein.domainObjects.Add(tile);

            Tile Empty = null;
            Tile Sliding = null;

            Expression<Predicate<Tile>> EmptyIs16 = E => E.TileValue == 16;
            Expression<Predicate<Tile, Tile>> DystansEq1 = (E, S) => Math.Abs(E.Col - S.Col) + Math.Abs(E.Row - S.Row) == 1;

            ActionPDDL SlideTile = new ActionPDDL("Slide Tile");

            SlideTile.AddPrecondiction<Tile, Tile>("Empty is 16", ref Empty, EmptyIs16);
            SlideTile.AddPrecondiction("Dystans between tiles is 1", ref Empty, ref Sliding, DystansEq1);

            SlideTile.AddEffect("Sliding tile is empty one now", ref Sliding, S => S.TileValue, 16);
            SlideTile.AddEffect("Empty tile is slining one now", ref Empty, E => E.TileValue, ref Sliding, S => S.TileValue);

            SlideTile.AddExecution("Wait", () => Thread.Sleep(750), false);
            SlideTile.AddExecution("Empty tile is slining one now");
            SlideTile.AddExecution("Sliding tile is empty one now");
            SlideTile.AddExecution("Draw it", () => Board.DrawBoard(tiles), true);

            GemPuzzleDomein.AddAction(SlideTile);

            AddTileXGoal(0);

            GemPuzzleDomein.SetExecutionOptions(null, null, AskToAgree.GO_AHEAD);
            GemPuzzleDomein.Start();

            Console.ReadKey();
        }
    }
}
