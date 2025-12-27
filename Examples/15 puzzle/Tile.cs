using System;

namespace _15_puzzle
{
    public class Tile
    {
        public int TileValue
        {
            get;
            set;
        }
        public int Col { get; private set; }
        public int Row { get; private set; }
        public bool JustMoved { get; private set; }

        public Tile(int TileValue, int Col, int Row)
        {
            this.TileValue = TileValue;
            this.Col = Col;
            this.Row = Row;
            this.JustMoved = false;
        }

        public void WriteIt()
        {
            if (JustMoved)
                Console.ForegroundColor = ConsoleColor.Green;

            if (TileValue < 10)
                Console.Write(" ");

            if (TileValue == 16)
                Console.Write("  ");
            else
                Console.Write(TileValue);

            JustMoved = false;
            Console.ResetColor();
        }

        public void MoveIt(int newCol, int newRow)
        {
            this.JustMoved = true;
            this.Col = newCol;
            this.Row = newRow;
        }
    }
}
