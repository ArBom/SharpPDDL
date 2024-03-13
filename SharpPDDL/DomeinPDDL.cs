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
            //taking types from actions to types
            foreach (ActionPDDL act in actions)
            {
                types.CompleteTypes(act.TakeSingleTypes());
            }

            types.CreateTypesTree();
            types.CompleteValuesIndekses();

            foreach (ActionPDDL act in actions)
            {
                act.BuildAction(types.allTypes);
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
        protected ushort ValuesIndeksCount = 0;
        internal List<SingleTypeOfDomein> allTypes = new List<SingleTypeOfDomein>();
        internal TreeNode<SingleTypeOfDomein> Root;

        internal void CompleteTypes(List<SingleType> singleTypes)
        {
            foreach (SingleType singleType in singleTypes)
            {
                SingleTypeOfDomein SingleTypeInList = null;

                if (allTypes.Count != 0)
                    SingleTypeInList = allTypes.First(st => st.Type == singleType.Type);

                if (SingleTypeInList is null)
                {
                    allTypes.Add(new SingleTypeOfDomein(singleType));
                    continue;
                }

                foreach(ValueOfParametr value in singleType.Values)
                {
                    if (SingleTypeInList.Values.Any(v => v.Name == value.Name))
                        continue;

                    SingleTypeInList.Values.Add(new ValueOfThumbnail(value));
                }
            }
        }

        internal void CompleteValuesIndekses()
        {
            #region NestedVoid
            void CompleteValues(TreeNode<SingleTypeOfDomein> node)
            {
                if (!(node.Content is null))
                {
                    foreach (ValueOfThumbnail childValue in node.Content.Values)
                    {
                        ValuesIndeksCount++;
                        childValue.ValueOfIndexesKey = ValuesIndeksCount;
                    }
                }

                foreach (TreeNode<SingleTypeOfDomein> child in node.Children)
                    CompleteValues(child);
            }
            #endregion

            if (this.Root is null)
                throw new Exception();

            CompleteValues(this.Root);
        }

        internal void CreateTypesTree()
        {
            #region NestedVoids
            void GetBranchRight(TreeNode<SingleTypeOfDomein> root)
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

                    List<TreeNode<SingleTypeOfDomein>> ty = root.Children.GetRange(i, root.Children.Count() - i);

                    while (j != 0)
                    {
                        List<TreeNode<SingleTypeOfDomein>> tghu = ty.Where(t => t.Content.Type == types[j])?.ToList();
                        if (tghu.Count() != 0)
                        {
                            SingleTypeOfDomein singleType = new SingleTypeOfDomein(types[j], root.Content.Values);
                            TreeNode<SingleTypeOfDomein> newType = new TreeNode<SingleTypeOfDomein>()
                            {
                                Root = root,
                                Content = singleType,
                                Children = tghu
                            };
                            //TODO czy nie sa gubione typy (konkretnie pierwszy)
                            foreach (TreeNode<SingleTypeOfDomein> newTypeCh in tghu)
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

            void PopulateInheritedTypes(TreeNode<SingleTypeOfDomein> node)
            {
                if (node.Children.Count == 0)
                    return;

                int NumbofValueInherNodeTypes = node.Content is null ? 0 : node.Content.Type.InheritedTypes().Types.Count;
                var NodeMembers = node.Content?.Type.GetMembers().Where(memb => (memb.MemberType == MemberTypes.Field || memb.MemberType == MemberTypes.Property));

                for (int a = 0; a < node.Children.Count; ++a)
                {
                    PopulateInheritedTypes(node.Children[a]);

                    int NumbofValueInherChildTypes = node.Children[a].Content.Type.InheritedTypes().Types.Count;
                    TreeNode<SingleTypeOfDomein> AnalysedNode = node.Children[a];

                    while (NumbofValueInherNodeTypes != NumbofValueInherChildTypes + 1)
                    {
                        Type TypeUp = AnalysedNode.Content.Type.BaseType;
                        List<(ValueOfThumbnail m, string Name)> AnalysedNodeValuesTuple = AnalysedNode.Content.Values.Select(m => (m, m.Name)).ToList();
                        List<ValueOfThumbnail> newSingleTypeMembers = TypeUp.GetMembers().Select(mtmember => AnalysedNodeValuesTuple.FirstOrDefault(anvt => (anvt.Name == mtmember.Name && (mtmember.MemberType == MemberTypes.Field || mtmember.MemberType == MemberTypes.Property ))))?.Where(el => !(el.m is null)).Select(el => el.m).ToList();

                        foreach (ValueOfThumbnail newSingleTypeMember in newSingleTypeMembers)
                        {
                            AnalysedNode.Content.Values.Remove(newSingleTypeMember);
                            newSingleTypeMember.OwnerType = TypeUp;
                        }

                        SingleTypeOfDomein newSingleType = new SingleTypeOfDomein(TypeUp, newSingleTypeMembers);
                        TreeNode<SingleTypeOfDomein> NewTreeNode = new TreeNode<SingleTypeOfDomein>()
                        {
                            Content = newSingleType
                        };
                        AnalysedNode.AddAbove(NewTreeNode);
                        AnalysedNode = NewTreeNode;
                        NumbofValueInherChildTypes--;

                        if (NumbofValueInherChildTypes == 0)
                            break;
                    }

                    for(int ValueCount = node.Children[a].Content.Values.Count - 1; ValueCount >= 0; --ValueCount )
                    {
                        ValueOfThumbnail tempChVal = node.Children[a].Content.Values[ValueCount];

                        //value znajduje się u child i w node
                        if (node.Content.Values.Any(v => (v.Name == tempChVal.Name && v.OwnerType == tempChVal.OwnerType)))
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

            void CumulateValues(TreeNode<SingleTypeOfDomein> node)
            {
                if (node.Content != null)
                    node.Content.CumulativeValues = new List<ValueOfThumbnail>(node.Content.Values);

                if (node.Root?.Content != null)
                    node.Content.CumulativeValues.AddRange(node.Root.Content.CumulativeValues);

                foreach (TreeNode<SingleTypeOfDomein> child in node.Children)
                    CumulateValues(child);
            }

            void MoveNodesToList(TreeNode<SingleTypeOfDomein> node, List<SingleTypeOfDomein> resultList)
            {
                if (node.Content != null)
                    resultList.Add(node.Content);

                foreach (TreeNode<SingleTypeOfDomein> child in node.Children)
                    MoveNodesToList(child, resultList);
            }
            #endregion

            if (this.allTypes is null)
                throw new Exception();

            this.Root = new TreeNode<SingleTypeOfDomein>(); //utwórz korzeń drzewa

            foreach(SingleTypeOfDomein singleType in this.allTypes) //Podepnij wszystko pod ten korzeń
            {
                TreeNode<SingleTypeOfDomein> ToAdd = new TreeNode<SingleTypeOfDomein>()
                {
                    Root = this.Root,
                    Content = singleType
                };
                this.Root.Children.Add(ToAdd);
            }

            GetBranchRight(Root);
            PopulateInheritedTypes(Root);
            CumulateValues(Root);

            this.allTypes = new List<SingleTypeOfDomein>();
            MoveNodesToList(Root, allTypes);
        }
    }
}