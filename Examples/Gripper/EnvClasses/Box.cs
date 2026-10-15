using System;
using System.Collections.Generic;
using System.Text;

namespace Gripper.EnvClasses
{
    class Box : MobileObj
    {
        public Ball Content;

        private const string ToDraw1 = "║";
        private const string ToDraw2 = "╚═══╝";

        public Box(ConsoleColor colour) : base(colour) { }

        override public void Draw()
        {
            int coursorL = Console.CursorLeft;

            Console.ForegroundColor = (ConsoleColor)Colour;
            Console.Write(ToDraw1);
            Console.Write(" ");

            if (Content is null)
                Console.Write(" ");
            else
                Content.Draw();

            Console.Write(" ");

            Console.ForegroundColor = (ConsoleColor)Colour;
            Console.Write(ToDraw1);
            Console.CursorTop++;
            Console.CursorLeft = coursorL;
            Console.Write(ToDraw2);

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
