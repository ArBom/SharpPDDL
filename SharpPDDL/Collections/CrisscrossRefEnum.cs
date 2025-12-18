using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPDDL
{
    public struct CrisscrossRefEnum
    {
        internal ref Crisscross Current => ref Chains[chainInd].Chain;
        List<ChainStruct> Chains;
        private int chainInd;

        internal CrisscrossRefEnum(ref Crisscross creator)
        {
            Crisscross MinusOnePos = new Crisscross
            {
                Children = new List<CrisscrossChildrenCon> { new CrisscrossChildrenCon(creator, -1, new object[0], 0) }
            };

            Chains = new List<ChainStruct>
            {
                new ChainStruct(MinusOnePos, 0),
                new ChainStruct(creator, 0),
                new ChainStruct(null, 0)
            };

            chainInd = 0;
        }

        internal bool MoveNext()
        {
            Crisscross Chain_chainInd = Chains[chainInd].Chain;

            if (Chain_chainInd.Children.Any())
            {
                if (chainInd == 0)
                {
                    chainInd++;
                    return true;
                }

                for (int i = 0; i != Chain_chainInd.Children.Count; i++)
                {
                    //the root of whole Crisscross could be replaced inside them
                    //if (Chain_chainInd.Children[i].Child.Root is null)
                    //  continue;

                    //Avoid the loopping
                    if (Chain_chainInd.Content.Equals(Chain_chainInd.Children[i].Child.Root?.Content))
                    {
                        if (Chains.Count == chainInd + 1)
                            Chains.Add(new ChainStruct(null, 0));

                        Chains[chainInd].ChainChildNo = i;
                        Chains[chainInd + 1].Chain = Chain_chainInd.Children[i].Child;
                        Chains[chainInd + 1].ChainChildNo = 0;
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

            ChainStruct Chains_DeepIndeks_1 = Chains[DeepIndeks - 1];

            for (int i = Chains_DeepIndeks_1.ChainChildNo + 1; i != Chains_DeepIndeks_1.Chain.Children.Count; i++)
            {
                if (Chains_DeepIndeks_1.Chain.Children[i].Child.Root is null)
                    continue;

                if (Object.ReferenceEquals(Chains_DeepIndeks_1.Chain.Content, Chains_DeepIndeks_1.Chain.Children[i].Child.Root.Content))
                {
                    Chains[DeepIndeks - 1].ChainChildNo = i;
                    Chains[DeepIndeks].Chain = Chains_DeepIndeks_1.Chain.Children[i].Child;
                    chainInd = DeepIndeks;
                    return true;
                }
            }

            Chains[DeepIndeks] = new ChainStruct(null, 0);
            return MoveNextFromLine(DeepIndeks - 1);
        }

        internal void Reset() => chainInd = 0;
    }
}