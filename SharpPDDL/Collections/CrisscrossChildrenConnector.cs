namespace SharpPDDL
{
    internal struct CrisscrossChildrenCon
    {
        internal Crisscross Child;
        internal readonly int ActionNr;
        internal readonly object[] ActionArgOryg;
        internal readonly uint ActionCost;

        internal CrisscrossChildrenCon(Crisscross Child, int ActionNr, object[] ActionArgOryg, uint ActionCost)
        {
            this.Child = Child;
            this.ActionNr = ActionNr;
            this.ActionArgOryg = ActionArgOryg;
            this.ActionCost = ActionCost;
        }

        internal CrisscrossChildrenCon(CrisscrossChildrenCon OldOne, Crisscross Update)
        {
            this.Child = Update;
            this.ActionNr = OldOne.ActionNr;
            this.ActionArgOryg = OldOne.ActionArgOryg;
            this.ActionCost = OldOne.ActionCost;
        }
    }
}
