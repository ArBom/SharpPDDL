using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    public class DomeinPDDL
    {
        public readonly string Name;
        public TypesPDDL types;
        public List<ActionPDDL> actions;

        public void CheckActions()
        {
            foreach (ActionPDDL act in actions)
            {
                types.CompleteTypes(act.BuildIt());
            }
        }

        public DomeinPDDL (string name)
        {
            this.Name = name;
            this.types = new TypesPDDL();
            this.actions = new List<ActionPDDL>();
        }
    }

    public class TypesPDDL
    {
        private List<SingleType> allTypes = new List<SingleType>();
        internal TreeNode<SingleType> Root;

        internal void CompleteTypes(List<SingleType> singleTypes)
        {
            foreach (SingleType singleType in singleTypes)
            {
                SingleType SingleTypeInList = allTypes.First(st => st.Type == singleType.Type);

                if (SingleTypeInList is null)
                {
                    allTypes.Add(singleType);
                    continue;
                }

                foreach(Value value in singleType.Values)
                {
                    if (SingleTypeInList.Values.Any(v => v.Name == value.Name))
                        continue;

                    SingleTypeInList.Values.Add(value);
                }
            }
        }

        internal void CreateTypesTree()
        {
            #region NestedVoids
            void GetBranchRight(TreeNode<SingleType> root)
            {
                for (int i = 0; i < root.Children.Count(); i++) //dla kazdego el. na liscie korzenia
                {
                    var types = root.Children[i].Value.InheritedTypes().Types; //pobierz typy
                    int j = types.Count();

                    while(types[j] != root.Value.Type)
                    {
                        --j;
                        if (j<0)
                            new Exception();
                    }

                    List<TreeNode<SingleType>> ty = root.Children.GetRange(i, root.Children.Count() - i);

                    while (j != 0)
                    {
                        List<TreeNode<SingleType>> tghu = ty.Where(t => t.Value.Type == types[j])?.ToList();
                        if (tghu.Count() != 0)
                        {
                            SingleType singleType = new SingleType(types[j], root.Value.Values);
                            TreeNode<SingleType> newType = new TreeNode<SingleType>(singleType);
                            newType.Root = root;
                            newType.Children = tghu;
                            //TODO czy nie sa gubione typy (konkretnie pierwszy)
                            foreach(TreeNode<SingleType> newTypeCh in tghu)
                            {
                                newTypeCh.Root = newType;
                                ty.Remove(newTypeCh);
                            }
                           
                            root.Children[i] = newType;
                            GetBranchRight(newType);
                            break;
                        }
                        --j;
                    }
                }
            }

            void RemoveOnlyOneChild(TreeNode<SingleType> parent)
            { }
            #endregion

            this.Root = new TreeNode<SingleType>(null); //utwórz korzeń drzewa

            foreach(SingleType singleType in this.allTypes) //Podepnij wszystko pod ten korzeń
            {
                TreeNode<SingleType> ToAdd = new TreeNode<SingleType>(singleType);
                ToAdd.Root = this.Root;
                this.Root.Children.Add(ToAdd);
            }

            GetBranchRight(Root);
            RemoveOnlyOneChild(Root);

        }

            /*internal void CreateTypesTree()
            {
                Root = new TreeNode<SingleType>(null); //utwórz korzeń drzewa

                for(int allTypesCount = 0; allTypesCount != this.allTypes.Count(); allTypesCount++) //dla wszystkich SingleType na liście...
                {
                    bool AddedPreviesly = false;
                    TreeNode<SingleType> ToAdd = new TreeNode<SingleType>(allTypes[allTypesCount]);
                    var types = ToAdd.Value.InheritedTypes().Types; //...pobierz ich wszystkie typy bazowe pojedynczego SingleType...

                    for(int InheritedTypesCoun = 0; InheritedTypesCoun != types.Count(); InheritedTypesCoun++) //...dla tych typów bazowych...
                    {
                        for (int CheckedTypesCoun = 0; CheckedTypesCoun != allTypesCount; CheckedTypesCoun++) //...sprawdź czy nie zostały już wczesniej dodane.
                        {
                            if(types[InheritedTypesCoun] == allTypes[CheckedTypesCoun].Type) //Jeśli tak...
                            {
                                //...to znajdz odpowiednie miejsce...


                                //...i uzupełnij drzewko...



                                AddedPreviesly = true;
                                break;
                            }                      
                        }

                        if (AddedPreviesly) //...potem przejdź do następnego...
                            break;
                    }

                    if (AddedPreviesly) //..., tutaj przejdź do następnego.
                        continue;

                    //Jeśli nie to dodaj do korzenia
                    ToAdd.Root = Root;
                    Root.Children.Add(ToAdd);
                }
            }*/
        }
}