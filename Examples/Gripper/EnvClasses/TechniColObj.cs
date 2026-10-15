using System;
using System.Collections.Generic;
using System.Text;

namespace Gripper.EnvClasses
{
    class TechniColObj : EnvObj
    {
        public readonly int Colour;

        public TechniColObj (ConsoleColor colour)
        {
            this.Colour = (int)colour;
        }
    }
}
