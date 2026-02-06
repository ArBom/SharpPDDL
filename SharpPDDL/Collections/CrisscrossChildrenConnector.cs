namespace SharpPDDL
{
    internal struct CrisscrossChildrenCon
    {
        internal readonly Crisscross Child;
        internal readonly int ActionNr;
        internal readonly ThumbnailObject[] ActionArgThOb;
        internal readonly uint ActionCost;

        internal CrisscrossChildrenCon(Crisscross Child, int ActionNr, ThumbnailObject[] ActionArgThOb, uint ActionCost)
        {
            this.Child = Child;
            this.ActionNr = ActionNr;
            this.ActionArgThOb = ActionArgThOb;
            this.ActionCost = ActionCost;
        }

        internal CrisscrossChildrenCon(CrisscrossChildrenCon OldOne, Crisscross Update)
        {
            this.Child = Update;
            this.ActionNr = OldOne.ActionNr;
            this.ActionArgThOb = OldOne.ActionArgThOb;
            this.ActionCost = OldOne.ActionCost;
        }

        internal object[] ActionArgOryg()
        {
            object[] ToRet = new object[ActionArgThOb.Length];

            for (int i = 0; i != ActionArgThOb.Length; i++)
                ToRet[i] = ActionArgThOb[i].OriginalObj;

            return ToRet;
        }
    }
}
