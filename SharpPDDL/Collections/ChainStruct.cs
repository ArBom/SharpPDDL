namespace SharpPDDL
{
    internal class ChainStruct
    {
        internal Crisscross Chain;
        internal int ChainChildNo;

        internal ChainStruct(Crisscross chain, int ChainChildNo)
        {
            this.Chain = chain;
            this.ChainChildNo = ChainChildNo;
        }
    }
}
