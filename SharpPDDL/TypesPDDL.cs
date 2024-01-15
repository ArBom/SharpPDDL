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

        public SingleType(Type type, IReadOnlyList<Value> values)
        {
            this.Type = type;
            this.Values =new List<Value>();

            MemberInfo[] AllTypeMembers = type.GetMembers();

            foreach (Value value in values)
                if (AllTypeMembers.Any(allM => allM.Name == value.Name))
                    this.Values.Add(value);
        }

        /// <returns>List of Interfaces, List of Base Type; from orygilal type to object</returns>
        internal (IReadOnlyList<Type> Interfaces, IReadOnlyList<Type> Types) InheritedTypes()
        {
            List<Type> ToReturnInterfaces = Type.GetInterfaces().ToList<Type>();
            List<Type> ToReturnBaseTypes = new List<Type>();
            Type typeUp = Type;
            while (typeUp != typeof(object))
            {
                ToReturnBaseTypes.Add(typeUp);
                typeUp = typeUp.BaseType;
            }

            return (ToReturnInterfaces, ToReturnBaseTypes);
        }
    }

    internal class TreeNode<T> where T : class
    {
        public T Value;
        internal TreeNode<T> Root;
        internal TreeNode<T> Littermate;
        internal List<TreeNode<T>> Children;

        internal TreeNode(T value)
        {
            this.Root = null;
            this.Littermate = null;
            this.Children = new List<TreeNode<T>>();

            this.Value = value;
        }

        internal void ChangeParentIntoGrandpa(TreeNode<T> newParent)
        {
            //TODO przetestować czy OK
            newParent.Root = this.Root;
            newParent.Children.Add(newParent);
            this.Root.Children.Remove(this);
            this.Root = newParent;
        }
    }
}
