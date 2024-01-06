using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    public class DomeinPDDL
    {
        public readonly string Name;
        public TypesPDDL types;
        public List<ActionPDDL> actions;

        private void CheckActions()
        {
            foreach (ActionPDDL act in actions)
            {
                act.BuildIt();
            }
        }

        public DomeinPDDL (string name)
        {
            this.Name = name;
            this.types = new TypesPDDL();
            this.actions = new List<ActionPDDL>();
        }
    }
}