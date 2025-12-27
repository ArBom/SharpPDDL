using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _15_puzzle
{
    static class Board
    {
        //Exterior part
        static readonly char LeftUp = '╔';
        static readonly string LeftRight = "║";
        static readonly char LeftB = '╟';
        static readonly char LeftDown = '╚';
        static readonly char RightUp = '╗';
        static readonly char RightB = '╢';
        static readonly char RightDown = '╝';
        static readonly char UpDown = '═';
        static readonly char UpB = '╤';
        static readonly char DownB = '╧';

        //Interior part
        static readonly string Vertical = "│";
        static readonly char Horizontal = '─';
        static readonly char Cross = '┼';

        private static void LeftMargin(int MarginV) => Console.CursorLeft = MarginV;

        private static void PrintLine(IEnumerable<Tile> Line)
        {
            Console.Write(LeftRight);
            Line.OrderBy(L => L.Col);
            foreach (Tile t in Line)
            {
                t.WriteIt();
                Console.Write(Vertical);
            }
            Console.CursorLeft = Console.CursorLeft - 1;
            Console.WriteLine(LeftRight);
        }

        public static void DrawBoard (IEnumerable<Tile> tiles)
        {
            int LeftPos = 1;
            char[] HorizontalInt = new char[] { LeftB, Horizontal, Horizontal, Cross, Horizontal, Horizontal, Cross, Horizontal, Horizontal, Cross, Horizontal, Horizontal, RightB };
            IEnumerable<IGrouping<int, Tile>> Lines = tiles.GroupBy(t => t.Row);
            Console.SetCursorPosition(LeftPos, 1);

            Console.WriteLine(new char[] { LeftUp, UpDown, UpDown, UpB, UpDown, UpDown, UpB, UpDown, UpDown, UpB, UpDown, UpDown, RightUp });

            for (int i = 0; i!=4; ++i)
            {
                LeftMargin(LeftPos);
                PrintLine(tiles.Where(t => t.Row == i));

                if (i != 3)
                {
                    LeftMargin(LeftPos);
                    Console.WriteLine(HorizontalInt);
                }
            }

            LeftMargin(LeftPos);
            Console.WriteLine( new char[] { LeftDown, UpDown, UpDown, DownB, UpDown, UpDown, DownB, UpDown, UpDown, DownB, UpDown, UpDown, RightDown });

        }
    }
}
