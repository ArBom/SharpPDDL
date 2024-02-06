using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

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
                types.CompleteTypes(act.TakeSingleTypes());
            }

            types.CreateTypesTree();
            //types.PopulateInheritedTypes();
            types.CompleteValuesIndekses();
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
        internal Dictionary<ushort, Value> ValuesIndekses;
        private List<SingleType> allTypes = new List<SingleType>();
        internal TreeNode<SingleType> Root;

        internal void CompleteTypes(List<SingleType> singleTypes)
        {
            foreach (SingleType singleType in singleTypes)
            {
                SingleType SingleTypeInList = null;

                if (allTypes.Count != 0)
                    SingleTypeInList = allTypes.First(st => st.Type == singleType.Type);

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

        internal void CompleteValuesIndekses()
        {
            #region NestedVoid
            void CompleteValues(TreeNode<SingleType> node)
            {
                if (!(node.Content is null))
                {
                    foreach (Value childValue in node.Content.Values)
                    {
                        if (!ValuesIndekses.Values.Contains(childValue))
                        {
                            ushort newKey;

                            if (ValuesIndekses.Count() != 0)
                                newKey = (ushort)(ValuesIndekses.Keys.Max()+1);
                            else
                                newKey = 1;

                            ValuesIndekses.Add(newKey, childValue);
                        }
                    }
                }

                foreach (TreeNode<SingleType> child in node.Children)
                    CompleteValues(child);
            }
            #endregion

            if (this.Root is null)
                throw new Exception();

            ValuesIndekses = new Dictionary<ushort, Value>();

            CompleteValues(this.Root);
        }

        internal void CreateTypesTree()
        {
            #region NestedVoids
            void GetBranchRight(TreeNode<SingleType> root)
            {
                for (int i = 0; i < root.Children.Count(); i++) //dla kazdego el. na liscie korzenia
                {
                    var types = root.Children[i].Content.Type.InheritedTypes().Types; //pobierz typy
                    int j = types.Count()-1;

                    if (!(root.Content is null))
                        while(types[j] != root.Content.Type)
                        {
                            --j;
                            if (j<0)
                                new Exception();
                        }

                    List<TreeNode<SingleType>> ty = root.Children.GetRange(i, root.Children.Count() - i);

                    while (j != 0)
                    {
                        List<TreeNode<SingleType>> tghu = ty.Where(t => t.Content.Type == types[j])?.ToList();
                        if (tghu.Count() != 0)
                        {
                            SingleType singleType = new SingleType(types[j], root.Content.Values);
                            TreeNode<SingleType> newType = new TreeNode<SingleType>(singleType)
                            {
                                Root = root,
                                Children = tghu
                            };
                            //TODO czy nie sa gubione typy (konkretnie pierwszy)
                            foreach (TreeNode<SingleType> newTypeCh in tghu)
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

            void PopulateInheritedTypes(TreeNode<SingleType> node)
            {
                if (node.Children.Count == 0)
                    return;

                int NumbofValueInherNodeTypes = node.Content is null ? 0 : node.Content.Type.InheritedTypes().Types.Count;
                var NodeMembers = node.Content?.Type.GetMembers().Where(memb => (memb.MemberType == MemberTypes.Field || memb.MemberType == MemberTypes.Property));

                for (int a = 0; a < node.Children.Count; ++a)
                {
                    PopulateInheritedTypes(node.Children[a]);

                    int NumbofValueInherChildTypes = node.Children[a].Content.Type.InheritedTypes().Types.Count;
                    TreeNode<SingleType> AnalysedNode = node.Children[a];

                    while (NumbofValueInherNodeTypes != NumbofValueInherChildTypes + 1)
                    {
                        Type TypeUp = AnalysedNode.Content.Type.BaseType;
                        List<(Value m, string Name)> AnalysedNodeValuesTuple = AnalysedNode.Content.Values.Select(m => (m, m.Name)).ToList();
                        List<Value> newSingleTypeMembers = TypeUp.GetMembers().Select(mtmember => AnalysedNodeValuesTuple.FirstOrDefault(anvt => (anvt.Name == mtmember.Name && (mtmember.MemberType == MemberTypes.Field || mtmember.MemberType == MemberTypes.Property ))))?.Where(el => !(el.m is null)).Select(el => el.m).ToList();

                        foreach (Value newSingleTypeMember in newSingleTypeMembers)
                        {
                            AnalysedNode.Content.Values.Remove(newSingleTypeMember);
                            newSingleTypeMember.OwnerType = TypeUp;
                        }

                        SingleType newSingleType = new SingleType(TypeUp, newSingleTypeMembers);
                        TreeNode<SingleType> NewTreeNode = new TreeNode<SingleType>(newSingleType);
                        AnalysedNode.ChangeParentIntoGrandpa(ref NewTreeNode);
                        AnalysedNode = NewTreeNode;
                        NumbofValueInherChildTypes--;

                        if (NumbofValueInherChildTypes == 0)
                            break;
                    }

                    for(int ValueCount = node.Children[a].Content.Values.Count - 1; ValueCount >= 0; --ValueCount )
                    {
                        Value tempChVal = node.Children[a].Content.Values[ValueCount];

                        //value znajduje się u child i w node
                        if (node.Content.Values.Any(v => (v.Name == tempChVal.Name && v.Type == tempChVal.Type)))
                        {
                            node.Children[a].Content.Values.Remove(tempChVal);
                            continue;
                        }

                        //node zawiera takiego members, ale nie value
                        if (NodeMembers.Any(nm => (nm.Name == tempChVal.Name)))
                        {
                            node.Children[a].Content.Values.Remove(tempChVal);
                            tempChVal.OwnerType = node.Content.Type;
                            node.Content.Values.Add(tempChVal);                          
                        }
                    }
                }
            }

            void RemoveTreeNodeBeyondList(TreeNode<SingleType> node, List<SingleType> stay)
            {
                if (node.Children.Count() == 0)
                    return;

                foreach (TreeNode<SingleType> child in node.Children)
                    RemoveTreeNodeBeyondList(child, stay);

                if (node.Root == null)
                    return;

                if (!stay.Contains(node.Content))
                {
                    foreach(TreeNode<SingleType> child in node.Children)
                    {
                        child.Root = node.Root;
                        node.Children.Add(child);
                    }

                    node.Root.Children.Remove(node);
                    node.Root = null;
                }
            }
            #endregion

            if (this.allTypes is null)
                throw new Exception();

            this.Root = new TreeNode<SingleType>(null); //utwórz korzeń drzewa

            foreach(SingleType singleType in this.allTypes) //Podepnij wszystko pod ten korzeń
            {
                TreeNode<SingleType> ToAdd = new TreeNode<SingleType>(singleType)
                {
                    Root = this.Root
                };
                this.Root.Children.Add(ToAdd);
            }

            GetBranchRight(Root);
            PopulateInheritedTypes(Root);
            //RemoveTreeNodeBeyondList(Root, this.allTypes);

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