using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class KnotOfTree
    {
        public readonly Type type;
        public List<PredicatePDDL> predicates;

        public KnotOfTree (Type type)
        {
            this.type = type;
            this.predicates = new List<PredicatePDDL>();
        }
    }

    internal class Tree
    {
        public KnotOfTree root;
        public List<KnotOfTree> list = new List<KnotOfTree>();
    }

    public class TypesPDDL
    {
        List<Tree> trees = new List<Tree>();
        internal List<KnotOfTree> allTypes = new List<KnotOfTree>();

        public void AddTypes(Type baseType, Type classes1, params Type[] classesN)
        {
            Type[] classes = new Type[classesN.Length + 1];
            classes[0] = classes1;
            classesN.CopyTo(classes, 1);

            Tree tree = new Tree();
            KnotOfTree TempRoot = new KnotOfTree(baseType);

            tree.root = TempRoot;

            foreach (var c in classes)
            {
                if (!c.IsClass)
                {
                    throw new Exception(""); //musi być klasa
                }

                Type tempUpLeaf = c.BaseType;
                while (tempUpLeaf != typeof(System.Object) && tempUpLeaf != baseType)
                {
                    tempUpLeaf = tempUpLeaf.BaseType;
                }
                if (tempUpLeaf == typeof(System.Object) && baseType != typeof(System.Object))
                {
                    throw new Exception(""); //liść nie dziedziczy od korzenia
                }

                if (tree.list.Exists( f => f.type == c))
                {
                    throw new Exception(""); //Nie może się powtarzać
                }

                if (allTypes.Exists(l => l.type == c))
                    throw new Exception(""); //Taki typ został już dodany

                KnotOfTree tempKnotOfTree = new KnotOfTree(c);
                allTypes.Add(tempKnotOfTree);

                KnotOfTree TempKnotOfTree = new KnotOfTree(c);

                tree.list.Add(TempKnotOfTree);
            }

            trees.Add(tree);
        }

        /*public void BuildTree()
        {
            mainTree = new Tree();

            List<Tree> roots = trees.Where(t => (t.root.type) == typeof(object)).ToList();
            if (roots.Count != 1)
                throw new Exception("");// musi byc tylko jeden korzeń
            else
                mainTree = roots.FirstOrDefault();

            bool Removed = false;
            while (Removed || trees.Count() != 0)
            {

            }
        }*/
    }
}
