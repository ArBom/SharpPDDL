using System.Collections.Generic;

namespace SharpPDDL
{
    internal class BrachNodeEnum<T> : TreeNodeEnum<T> where T : class
    {
        private readonly TreeNode<T> creator;

        internal BrachNodeEnum(TreeNode<T> creator) : base(creator)
        {
            this.creator = creator;
        }

        private new bool MoveNextFromLine(TreeNode<T> e)
        {
            if (!(e.Root is null))
            {
                int IndeksOfE = e.Root.Children.IndexOf(e);

                if (e.Root.Children.Count != ++IndeksOfE)
                {
                    e = e.Root.Children[IndeksOfE];
                    return true;
                }
                else
                {
                    if (e.Root.Equals(creator))
                        return false;

                    return MoveNextFromLine(e.Root);
                }
            }

            return false;
        }

        new public void Reset()
        {
            TreeNode<T> MinusOnePos = new TreeNode<T>
            {
                Children = new List<TreeNode<T>> { creator }
            };
            this.current = MinusOnePos;
        }
    }
}
