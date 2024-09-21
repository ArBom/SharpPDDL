using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    public struct CrisscrossRefEnum
    {
        internal ref Crisscross Current => ref chain[chainInd];
        private int chainInd;
        Crisscross[] chain;
        List<int> chainChildNo;

        internal CrisscrossRefEnum(ref Crisscross creator)
        {
            Crisscross MinusOnePos = new Crisscross
            {
                Children = new List<CrisscrossChildrenCon> { new CrisscrossChildrenCon(creator, 0, null) }
            };

            chain = new Crisscross[] {MinusOnePos, creator, null};
            chainChildNo = new List<int> {0, 0, 0};
            chainInd = 0;
        }

        internal bool MoveNext()
        {
            if (chain[chainInd].Children.Count != 0)
            {
                if (chainInd == 0)
                {
                    chainInd++;
                    return true;
                }

                for (int i = 0; i != chain[chainInd].Children.Count - 1; i++)
                {
                    //the root of whole Crisscross could be replaced inside them
                    if (chain[chainInd].Children[i].Child.Root is null)
                        continue;

                    //Avoid the loopping
                    if (chain[chainInd].Children[i].Child.Root.Equals(chain[chainInd]))
                    {
                        if (chain.Length == chainInd + 1)
                            MakeCurrentsBigger();

                        chainChildNo[chainInd] = i;
                        chain[chainInd + 1] = chain[chainInd].Children[i].Child;
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

            //string CheckSumOfDeepIndeks = chain[DeepIndeks].Content.CheckSum;
            //int CurrentAtRootList = chain[DeepIndeks - 1].Children.FindIndex(c => (c.Child.Content.CheckSum == CheckSumOfDeepIndeks && true )); //TODO checking root
            int CurrentAtRootList = chainChildNo[DeepIndeks-1] +1;

            for (int i = CurrentAtRootList; i != chain[DeepIndeks - 1].Children.Count; i++)
            {
                if (chain[DeepIndeks - 1].Children[i].Child.Root is null)
                    continue;

                if (chain[DeepIndeks - 1].Children[i].Child.Root.Equals(chain[DeepIndeks - 1]))
                {
                    chainChildNo[DeepIndeks - 1] = i;
                    chain[DeepIndeks] = chain[DeepIndeks - 1].Children[i].Child;
                    chainInd = DeepIndeks;
                    return true;
                }
            }

            return MoveNextFromLine(DeepIndeks - 1);
        }

        private void MakeCurrentsBigger()
        {
            Crisscross[] NewChain = new Crisscross[chain.Length + 1];

            for (int a = 0; a < NewChain.Length; a++)
            {
                if (a < chain.Length)
                    NewChain[a] = chain[a];
                else
                    NewChain[a] = null;
            }

            chain = NewChain;
            chainChildNo.Add(0);
        }

        internal void Reset()
        {
            chainInd = 0;

            for(int i = 2; i != chain.Length; i++)
            {
                chain[i] = null;
            }
        }
    }
}
