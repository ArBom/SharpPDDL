using System;

namespace Gripper.EnvClasses
{
    class Claw : EnvObj
    {
        public MobileObj Content;
        public bool IsBigger;
        public bool IsOpen
        { get { return !(Content is null); }  }
        public int Xpos;
        public int AboveSpot => Xpos / 7;
        public int Depth;

        private const string ToDraw1 = "┃";
        private const string ToDraw2 = "┍";
        private const string ToDraw3 = "━";
        private const string ToDraw4 = "┻";
        private const string ToDraw5 = "┑";
        private const string ToDraw6 = "╵";
        private const string ToDraw7 = "╰";
        private const string ToDraw8 = "╯";

        public Claw()
        {
            this.IsBigger = true;
            this.Xpos = 3;
            this.Depth = 1;
        }

        override public void Draw()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(Xpos, 0);

            for (int i = 0; i != Depth; i++)
            {
                Console.SetCursorPosition(Xpos, i);
                Console.Write(ToDraw1);
            }

            if (IsBigger)
            {
                Console.SetCursorPosition(Xpos - 3, Depth);
                Console.Write(ToDraw2 + ToDraw3 + ToDraw3 + ToDraw4 + ToDraw3 + ToDraw3 + ToDraw5);
            }
            else
            {
                Console.SetCursorPosition(Xpos - 1, Depth);
                Console.Write(ToDraw2 + ToDraw4 + ToDraw5);
            }

            if (IsBigger)
                Console.SetCursorPosition(Xpos - 3, Depth + 1);
            else
                Console.SetCursorPosition(Xpos - 1, Depth + 1);

            if (IsOpen)
                Console.Write(ToDraw7);
            else
                Console.Write(ToDraw6);

            Content?.Draw();

            if (IsBigger)
                Console.SetCursorPosition(Xpos + 3, Depth + 1);
            else
                Console.SetCursorPosition(Xpos + 1, Depth + 1);

            Console.ForegroundColor = ConsoleColor.White;
            if (IsOpen)
                Console.Write(ToDraw8);
            else
                Console.Write(ToDraw6);
        }
    }
}
