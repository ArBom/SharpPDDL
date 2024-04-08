using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;

namespace SharpPDDL
{
    public partial class DomeinPDDL
    {
        public readonly string Name;
        public TypesPDDL types;
        private List<ActionPDDL> actions;
        internal Crisscross<PossibleState> states;
        public ObservableCollection<object> domainObjects;
        internal ObservableCollection<GoalPDDL> domainGoals;
        private Task TaskRealization;

        internal void CheckActions()
        {
            this.types = new TypesPDDL();
            foreach (ActionPDDL act in actions)
            {
                types.CompleteTypes(act.TakeSingleTypes());
            }

            types.CreateTypesTree();

            foreach (ActionPDDL act in actions)
            {
                act.BuildAction(types.allTypes);
            }
        }

        private void CheckExistActionName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.actions.Exists(action => action.Name == Name))
                throw new Exception(); //juz istnieje efekt o takiej nazwie
        }

        public void AddAction(ActionPDDL newAction)
        {
            CheckExistActionName(newAction.Name);
            this.actions.Add(newAction);
        }

        public DomeinPDDL (string name, ICollection<ActionPDDL> actions = null)
        {
            this.Name = name;
            this.actions = new List<ActionPDDL>();
            this.TaskRealizationCTS = new CancellationTokenSource();

            this.domainObjects = new ObservableCollection<object>();
            this.domainObjects.CollectionChanged += DomainObjects_CollectionChanged;

            if (!(actions is null))
                foreach (ActionPDDL actionPDDL in actions)
                    this.AddAction(actionPDDL);
        }

        private void DomainObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs eventType)
        {
            foreach (dynamic Obj in eventType.NewItems)
            {
                if (!(Obj.GetType().IsClass))
                    throw new Exception();
            }

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
                {
                    var SingleTypeInLists = allTypes.Where(st => st.Type == singleType.Type);
                    if (SingleTypeInLists.Count() != 0)
                        SingleTypeInList = SingleTypeInLists.First();

                }

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
            void ChangeAtChildren(TreeNode<SingleTypeOfDomein> node, ValueOfThumbnail childValue, ushort ValuesIndeksCount)
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
                    foreach (ValueOfThumbnail childValue in node.Content.Values)
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
                    int maxTypesArg = types.Count()-1; //Create argument for read the list

                    if (!(root.Content is null)) //find argument for last mutual ancistor (with root)
                        while(types[currentTypesArg] != root.Content.Type)
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

                            if (littermate.Any(l=>l.Content.Type == types[currentTypesArg]))
                            {
                                newType = littermate.First(l => l.Content.Type == types[currentTypesArg]);
                            }
                            else
                            {
                                SingleTypeOfDomein singleType = new SingleTypeOfDomein(types[currentTypesArg], new List<ValueOfThumbnail>()); //TODO lista

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
                if (node.Children.Count == 0)
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
                        Type TypeUp = AnalysedNode.Content is null? null : AnalysedNode.Content.Type.BaseType;
                        Type TypeUp2 = AnalysedNode.Content.Type.BaseType;
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

                    if (node.Content is null)
                        continue;

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
                {
                    node.Content.CumulativeValues = new List<ValueOfThumbnail>(node.Root.Content.CumulativeValues);
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
            CompleteValuesIndekses();

            this.allTypes = new List<SingleTypeOfDomein>();
            MoveNodesToList(Root, allTypes);

            foreach (var elem in allTypes)
                elem.CreateValuesKeys();
        }
    }
}