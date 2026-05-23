using System;

namespace _15_puzzle
{
    public class Tile
    {
        public int TileValue { get; set; }
        public int Col { get; private set; }
        public int Row { get; private set; }

        public Tile(int TileValue, int Col, int Row)
        {
            this.TileValue = TileValue;
            this.Col = Col;
            this.Row = Row;
        }

        public void WriteIt()
        {
            if (TileValue < 10)
                Console.Write(" ");

            if (TileValue == 16)
                Console.Write("  ");
            else
                Console.Write(TileValue);
        }

        public void MoveIt(int newCol, int newRow)
        {
            this.Col = newCol;
            this.Row = newRow;
        }
    }
}
