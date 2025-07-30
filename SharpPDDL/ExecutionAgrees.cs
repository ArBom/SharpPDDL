namespace SharpPDDL
{
    internal static class ExecutionAgrees
    {
        internal const byte Go_AHEAD = 0b_0000;
        internal const byte SpecialAction = 0b_0001;
        internal const byte EveryAction = 0b_0011;
        internal const byte Plan = 0b_0100;
        internal const byte DONT_DO_IT = 0b_1000;
        internal const byte DONT_EVEN_TRY = 0b_1111;
    }

    public enum AskToAgree : byte
    {
        GO_AHEAD = ExecutionAgrees.Go_AHEAD,
        //SpecialAction = ExecutionAgrees.SpecialAction,
        EveryAction = ExecutionAgrees.EveryAction,
        Plan = ExecutionAgrees.Plan,
        DONT_DO_IT = ExecutionAgrees.DONT_DO_IT
    }
}