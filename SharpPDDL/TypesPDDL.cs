using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace SharpPDDL
{
    internal class SingleType
    {
        internal readonly Type Type;
        internal List<Value> Values;
        internal ushort[] ValuesKeys;

        public SingleType(Type type, IReadOnlyList<Value> values)
        {
            this.Type = type;
            this.Values =new List<Value>();

            MemberInfo[] AllTypeMembers = type.GetMembers();

            if (values is null)
                return;

            foreach (Value value in values)
                if (AllTypeMembers.Any(allM => allM.Name == value.Name))
                    this.Values.Add(value);
        }

        internal void CreateValuesKeys()
        {
            List<ushort> TempList = new List<ushort>();

            foreach (ValueOfThumbnail valueOfThumbnail in Values)
                TempList.Add(valueOfThumbnail.ValueOfIndexesKey);

            TempList.Sort();
            ValuesKeys = TempList.ToArray();
        }
    }

    internal class TreeNode<T> where T : class
    {
        public T Content;
        internal TreeNode<T> Root;
        internal List<TreeNode<T>> Children;

        internal TreeNode(T Content)
        {
            this.Root = null;
            this.Children = new List<TreeNode<T>>();

            this.Content = Content;
        }

        internal void ChangeParentIntoGrandpa(ref TreeNode<T> newParent)
        {
            int index = this.Root.Children.IndexOf(this);
            newParent.Root = this.Root;
            newParent.Children.Add(this);
            this.Root.Children[index] = newParent;
        }
    }
}
