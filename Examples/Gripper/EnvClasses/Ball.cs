using System;
using System.Collections.Generic;
using System.Text;

namespace Gripper.EnvClasses
{
    class Ball : MobileObj
    {
        private const string ToDraw = "●";

        public Ball(ConsoleColor colour) : base(colour) { }

        override public void Draw()
        {
            Console.ForegroundColor = (ConsoleColor)Colour;
            Console.Write(ToDraw);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
