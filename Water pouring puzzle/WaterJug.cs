using System;
using System.Threading;

namespace Water_pouring_puzzle
{
    public class WaterJug
    {
        //Define Action cost function as quantify of water to decant
        public static int DecantedWater(float SourceFlood, float DestinationCapacity, float DestinationFlood)
        {
            if (SourceFlood + DestinationFlood > DestinationCapacity)
                return (int)(DestinationCapacity - DestinationFlood);
            else
                return (int)SourceFlood;
        }

        public void WaitForDecant(WaterJug DestinationWaterJug)
        {
            int time = DecantedWater(this.flood, DestinationWaterJug.Capacity, DestinationWaterJug.flood);
            Thread.Sleep(time * 1000);
        }

        public readonly float Capacity; //max level of fluid
        private float _flood;
        public float flood //current level of fluid
        {
            get { return _flood; }
            set
            {
                if (value >= 0 && value <= Capacity)
                    _flood = value;
            }
        }

        public WaterJug(float Capacity, float flood = 0)
        {
            this.Capacity = Capacity;
            this.flood = flood;
            this.DrawIt();
        }

        public void DrawIt()
        {
            int left = 0, top = 0;

            switch (Capacity)
            {
                case 8:
                    left = 1;
                    top = 1;
                    break;

                case 5:
                    left = 8;
                    top = 4;
                    break;

                case 3:
                    left = 15;
                    top = 6;
                    break;
            }

            const char DownLeft = '└';
            const char DownRight = '┘';
            const char SideFrame = '│';

            Console.SetCursorPosition(left, top);

            for (int i = 0; i != Capacity; i++)
            {
                Console.SetCursorPosition(left, top + i);
                Console.Write(SideFrame);

                if (_flood + i >= Capacity)
                    Console.BackgroundColor = ConsoleColor.Blue;

                if (i == Capacity - 1)
                    Console.Write(_flood + "/" + Capacity + "  ");
                else
                    Console.Write("     ");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(SideFrame);
            }
            Console.SetCursorPosition(left, 9);
            Console.Write("└─────┘");

            Console.Write(Environment.NewLine);
            Console.Write(Environment.NewLine);
        }
    }
}
