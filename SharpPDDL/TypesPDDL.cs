using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace SharpPDDL
{
    public class TypesPDDL
    {
        protected ushort ValuesIndeksCount = 0;
        internal List<SingleTypeOfDomein> allTypes = new List<SingleTypeOfDomein>();
        internal TreeNode<SingleTypeOfDomein> Root;
        object locker = new object();

        //method is call for every Action
        internal void CompleteTypes(List<SingleType> singleTypes)
        {
            foreach (SingleType singleType in singleTypes)
            {
                int? ToTagAllTypesIndex = null;

                //If its some type was added before...
                lock (locker)
                {
                    if (allTypes.Any())
                    {
                        ///...try to find the same type added before
                        var SingleTypeInLists = allTypes.Any(st => st.Type == singleType.Type);
                        if (SingleTypeInLists)
                            ToTagAllTypesIndex = allTypes.FindIndex(st => st.Type == singleType.Type);
                    }

                    //if it didnt find the same type added before...
                    if (ToTagAllTypesIndex is null)
                    {
                        //...add it now...
                        allTypes.Add(new SingleTypeOfDomein(singleType));
                        //...and work with another
                        continue;
                    }
                }

                //in the other case update values of Parameter
                foreach (Value value in singleType.Values)
                {
                    bool AnyValueOfName;

                    lock (allTypes[ToTagAllTypesIndex.Value])
                        AnyValueOfName = allTypes[ToTagAllTypesIndex.Value].Values.Any(v => v.Name == value.Name);

                    if (AnyValueOfName)
                    {
                        int ToTagIndex = allTypes[ToTagAllTypesIndex.Value].Values.FindIndex(v => v.Name == value.Name);
                        allTypes[ToTagAllTypesIndex.Value].Values[ToTagIndex].IsInUse_EffectIn = value.IsInUse_EffectIn;
                        allTypes[ToTagAllTypesIndex.Value].Values[ToTagIndex].IsInUse_EffectOut = value.IsInUse_EffectOut;
                        allTypes[ToTagAllTypesIndex.Value].Values[ToTagIndex].IsInUse_PreconditionIn = value.IsInUse_PreconditionIn;
                        continue;
                    }

                    lock (allTypes[ToTagAllTypesIndex.Value])
                        allTypes[ToTagAllTypesIndex.Value].Values.Add(value);
                }
            }
        }

        internal void CompleteValuesIndekses()
        {
            #region NestedVoid
            void ChangeAtChildren(TreeNode<SingleTypeOfDomein> node, Value childValue, ushort ValuesIndeksCount)
            {
                node.Content.CumulativeValues.First(v => v.Name == childValue.Name).ValueOfIndexesKey = ValuesIndeksCount;

                foreach (var ch in node.Children)
                {
                    ChangeAtChildren(ch, childValue, ValuesIndeksCount);
                }
            }

            void CompleteValues(TreeNode<SingleTypeOfDomein> node)
            {
                if (!(node.Content is null))
                {
                    foreach (Value childValue in node.Content.Values)
                    {
                        if (node.Content.CumulativeValues.Any(cv => cv.Name == childValue.Name && cv.ValueOfIndexesKey != 0))
                            continue;

                        ValuesIndeksCount++;
                        childValue.ValueOfIndexesKey = ValuesIndeksCount;
                        ChangeAtChildren(node, childValue, ValuesIndeksCount);
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
                for (int i = 0; i < root.Children.Count(); i++) //for every element at root's list...
                {
                    var types = root.Children[i].Content.Type.InheritedTypes().Types; //...take inherited types
                    int currentTypesArg = 0;
                    int maxTypesArg = types.Count() - 1; //Create argument for read the list

                    if (!(root.Content is null)) //find argument for last mutual ancistor (with root)
                        while (types[currentTypesArg] != root.Content.Type)
                        {
                            currentTypesArg++;
                            if (types[currentTypesArg] == typeof(object))
                                break;
                        }

                    List<TreeNode<SingleTypeOfDomein>> UncheckedYetRootCh = root.Children.GetRange(i, root.Children.Count() - i); //every else elements from root's list put to new list

                    for (currentTypesArg++; currentTypesArg <= maxTypesArg; currentTypesArg++)
                    {
                        Type findedType = types[currentTypesArg];
                        List<TreeNode<SingleTypeOfDomein>> littermate = UncheckedYetRootCh.Where(U => U.Content.Type.InheritedTypes().Types.Contains(findedType)).ToList();

                        if (littermate.Count() > 1)
                        {
                            TreeNode<SingleTypeOfDomein> newType;

                            if (littermate.Any(l => l.Content.Type == types[currentTypesArg]))
                            {
                                newType = littermate.First(l => l.Content.Type == types[currentTypesArg]);
                            }
                            else
                            {
                                SingleTypeOfDomein singleType = new SingleTypeOfDomein(types[currentTypesArg], new List<Value>()); //TODO lista

                                newType = new TreeNode<SingleTypeOfDomein>()
                                {
                                    Root = root,
                                    Content = singleType,
                                };

                                root.Children.Add(newType);
                            }

                            foreach (var l in littermate)
                            {
                                if (l == newType)
                                    continue;

                                l.Root = newType;
                                newType.Children.Add(l);
                                root.Children.Remove(l);
                            }

                            GetBranchRight(newType);
                            break;
                        }
                    }
                }
            }

            void PopulateInheritedTypes(TreeNode<SingleTypeOfDomein> node)
            {
                if (!node.Children.Any())
                    return;

                //take number of node's content inhered types
                int NumbofValueInherNodeTypes = node.Content is null ? 0 : node.Content.Type.InheritedTypes().Types.Count;
                //take members of node's content
                var NodeMembers = node.Content?.Type.GetMembers().Where(memb => (memb.MemberType == MemberTypes.Field || memb.MemberType == MemberTypes.Property));

                for (int a = 0; a < node.Children.Count; ++a)
                {
                    //take number of child's content inhered types
                    int NumbofValueInherChildTypes = node.Children[a].Content.Type.InheritedTypes().Types.Count;
                    TreeNode<SingleTypeOfDomein> AnalysedNode = node.Children[a];

                    while (NumbofValueInherNodeTypes + 1 != NumbofValueInherChildTypes)
                    {
                        Type TypeUp = AnalysedNode.Content?.Type.BaseType;
                        Type TypeUp2 = AnalysedNode.Content.Type.BaseType;
                        List<(Value m, string Name)> AnalysedNodeValuesTuple = AnalysedNode.Content.Values.Select(m => (m, m.Name)).ToList();
                        List<Value> newSingleTypeMembers = TypeUp.GetMembers().Select(mtmember => AnalysedNodeValuesTuple.FirstOrDefault(anvt => (anvt.Name == mtmember.Name && (mtmember.MemberType == MemberTypes.Field || mtmember.MemberType == MemberTypes.Property))))?.Where(el => !(el.m is null)).Select(el => el.m).ToList();

                        foreach (Value newSingleTypeMember in newSingleTypeMembers)
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

                    if (node.Content is null)
                        continue;

                    for (int ValueCount = node.Children[a].Content.Values.Count - 1; ValueCount >= 0; --ValueCount)
                    {
                        Value tempChVal = node.Children[a].Content.Values[ValueCount];

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

            void TagValues(TreeNode<SingleTypeOfDomein> node)
            {
                if (node.Children.Any())
                    foreach (TreeNode<SingleTypeOfDomein> child in node.Children)
                        TagValues(child);

                if (node.Content != null)
                {
                    TreeNode<SingleTypeOfDomein> tempNode = node;
                    while (tempNode.Root != null)
                    {
                        if (tempNode.Root.Content is null)
                        {
                            tempNode = tempNode.Root;
                            continue;
                        }

                        foreach (Value v in tempNode.Content.Values)
                        {
                            if (tempNode.Root.Content.Values.Any(aV => aV.Name == v.Name))
                            {
                                int ToTagIndex = tempNode.Root.Content.Values.FindIndex(aV => aV.Name == v.Name);
                                tempNode.Root.Content.Values[ToTagIndex].IsInUse_EffectIn = v.IsInUse_EffectIn;
                                tempNode.Root.Content.Values[ToTagIndex].IsInUse_EffectOut = v.IsInUse_EffectOut;
                                tempNode.Root.Content.Values[ToTagIndex].IsInUse_PreconditionIn = v.IsInUse_PreconditionIn;
                            }
                        }

                        tempNode = tempNode.Root;
                    }
                }
            }

            void CumulateValues(TreeNode<SingleTypeOfDomein> node)
            {
                if (node.Content != null)
                    node.Content.CumulativeValues = new List<Value>(node.Content.Values);

                if (node.Root?.Content != null)
                {
                    node.Content.CumulativeValues = new List<Value>(node.Root.Content.CumulativeValues);
                    var newValues = node.Content.Values.Where(v => !node.Content.CumulativeValues.Any(cv => cv.Name == v.Name));
                    node.Content.CumulativeValues.AddRange(newValues);
                }

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

            foreach (SingleTypeOfDomein singleType in this.allTypes) //Podepnij wszystko pod ten korzeń
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
            TagValues(Root);
            CumulateValues(Root);
            CompleteValuesIndekses();

            this.allTypes = new List<SingleTypeOfDomein>();
            MoveNodesToList(Root, allTypes);

            foreach (var elem in allTypes)
                elem.CreateValuesKeys();
        }
    }
}
