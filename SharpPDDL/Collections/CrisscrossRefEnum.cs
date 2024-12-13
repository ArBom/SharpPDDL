using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

    public struct CrisscrossRefEnum
    {
        internal ref Crisscross Current => ref Chains[chainInd].Chain;
        List<ChainStruct> Chains;
        private int chainInd;

        internal CrisscrossRefEnum(ref Crisscross creator)
        {
            Crisscross MinusOnePos = new Crisscross
            {
                Children = new List<CrisscrossChildrenCon> { new CrisscrossChildrenCon(creator, 0, null, 0) }
            };

            Chains = new List<ChainStruct> {
                new ChainStruct(MinusOnePos, 0),
                new ChainStruct(creator, 0),
                new ChainStruct(null, 0)
            };

            chainInd = 0;
        }

        internal bool MoveNext()
        {
            Crisscross Chain_chainInd = Chains[chainInd].Chain;

            if (Chain_chainInd.Children.Count != 0)
            {
                if (chainInd == 0)
                {
                    chainInd++;
                    return true;
                }

                for (int i = 0; i != Chain_chainInd.Children.Count; i++)
                {
                    //the root of whole Crisscross could be replaced inside them
                    if (Chain_chainInd.Children[i].Child.Root is null)
                        continue;

                    //Avoid the loopping
                    if (Chain_chainInd.Children[i].Child.Root.Equals(Chain_chainInd))
                    {
                        if (Chains.Count == chainInd + 1)
                            Chains.Add(new ChainStruct(null, 0));

                        Chains[chainInd].ChainChildNo = i;
                        Chains[chainInd + 1].Chain = Chain_chainInd.Children[i].Child;
                        chainInd++;
                        return true;
                    }
                }
            }

            return MoveNextFromLine(chainInd);
        }

        private bool MoveNextFromLine(int DeepIndeks)
        {
            if (DeepIndeks <= 1)
                return false;

            var Chains_DeepIndeks_1 = Chains[DeepIndeks - 1];
            int CurrentAtRootList = Chains_DeepIndeks_1.ChainChildNo +1;

            for (int i = CurrentAtRootList; i != Chains_DeepIndeks_1.Chain.Children.Count; i++)
            {
                if (Chains_DeepIndeks_1.Chain.Children[i].Child.Root is null)
                    continue;

                if (Chains_DeepIndeks_1.Chain.Children[i].Child.Root.Equals(Chains_DeepIndeks_1.Chain))
                {
                    Chains_DeepIndeks_1.ChainChildNo = i;
                    Chains[DeepIndeks].Chain = Chains_DeepIndeks_1.Chain.Children[i].Child;
                    chainInd = DeepIndeks;
                    return true;
                }
            }

            return MoveNextFromLine(DeepIndeks - 1);
        }

        internal void Reset() => chainInd = 0;
    }
}