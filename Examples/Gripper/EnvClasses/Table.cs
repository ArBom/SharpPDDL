using System;
using System.Collections.Generic;
using System.Text;

namespace Gripper.EnvClasses
{
    class Table : TechniColObj
    {
        public MobileObj Content;
        public readonly int Spot;

        private const string ToDraw = "▀▀▀▀▀▀▀";

        public Table(int Spot, ConsoleColor colour) : base(colour)
        {
            this.Spot = Spot;
        }

        override public void Draw()
        {
            Console.ForegroundColor = (ConsoleColor)Colour;
            Console.SetCursorPosition(Spot * 7, 6);
            Console.Write(ToDraw);

            if (Content is Ball)
            {
                Console.SetCursorPosition((Spot * 7) + 3, 5);
                Content.Draw();
            }
            else if (Content is Box)
            {
                Console.SetCursorPosition((Spot * 7) + 1, 4);
                Content.Draw();
            }
        }
    }
}
