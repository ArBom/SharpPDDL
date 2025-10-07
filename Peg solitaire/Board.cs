using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Peg_solitaire
{
    static class Board
    {
        const char UpLeft = '┌';
        const char UpRight = '┐';
        const char DownLeft = '└';
        const char DownRight = '┘';
        const char SideFrame = '│';
        const char DownFrame = '─';

        const ConsoleColor Frame = ConsoleColor.Gray;

        public static void Draw(ICollection<Spot> pegs)
        {
            Console.CursorVisible = false;

            ushort MaxHeight = pegs.Max(p => p.Row);
            ushort MaxWight = pegs.Max(p => p.Column);

            try
            {
                Console.SetCursorPosition(0, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = Frame;
            Console.WriteLine();
            Console.Write(" " + UpLeft);
            for (int i = 0; i <= 2 * MaxWight; i++)
                Console.Write(DownFrame);
            Console.WriteLine(UpRight);

            for (int j = 0; j <= MaxWight; j++)
            {
                IEnumerable<Spot> RowPeg = pegs.Where(p => p.Row == j);
                int TriSide = MaxHeight - RowPeg.Count() + 1;  

                Console.ForegroundColor = Frame;
                Console.Write(" " + SideFrame);

                for (int l = 0; l != TriSide; ++l)
                    Console.Write(' ');

                for (int k = 0; k <= MaxWight; k++)
                {
                    if (RowPeg.Any(p => p.Column == k))
                    {
                        RowPeg.First(p => p.Column == k).Draw();
                        if (k != j)
                            Console.Write(' ');
                    }
                    else
                        Console.Write(' ');
                }

                Console.ForegroundColor = Frame;
                Console.WriteLine(SideFrame);
            }

            Console.Write(" " + DownLeft);
            for (int i = 0; i <= 2 * MaxWight; i++)
                Console.Write(DownFrame);
            Console.CursorVisible = true;
            Console.WriteLine(DownRight);
        }
    }
}
