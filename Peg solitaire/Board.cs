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
        //kernel32.dll

        const char UpLeft = '┌';
        const char UpRight = '┐';
        const char DownLeft = '└';
        const char DownRight = '┘';
        const char SideFrame = '│';
        const char DownFrame = '─';

        const char FullPool = '●';
        const char EmptyPool = '○';

        const ConsoleColor Frame = ConsoleColor.Gray;

        public static void Draw(ICollection<Spot> pegs)
        {
            ushort MaxHeight = pegs.Max(p => p.Row);
            ushort MaxWight = pegs.Max(p => p.Column);

            try
            {
                Console.Clear();
            }
            catch
            {
                Console.WriteLine("Commend: Console.Clear();");
            }

            Console.ForegroundColor = Frame;
            Console.Write(UpLeft);
            for (int i = 0; i != MaxWight; i++)
                Console.Write(DownFrame);
            Console.WriteLine(UpRight);

            for (int j = 0; j != MaxWight; j++)
            {
                IEnumerable<Spot> RowPeg = pegs.Where(p => p.Row == j);

                Console.ForegroundColor = Frame;
                Console.Write(SideFrame);

                for (int k = 0; k != MaxWight; k++)
                {
                    if (RowPeg.Any(p => p.Column == k))
                        RowPeg.First(p => p.Column == k).Draw();
                    else
                        Console.Write(' ');
                }

                Console.ForegroundColor = Frame;
                Console.WriteLine(SideFrame);
            }

            Console.Write(DownLeft);
            for (int i = 0; i != MaxWight; i++)
                Console.Write(DownFrame);
            Console.WriteLine(DownRight);
        }
    }
}
