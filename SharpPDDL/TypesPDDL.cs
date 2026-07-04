using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace SharpPDDL
{
    public class TypesPDDL
    {
        /// <summary>
        /// 0 value occur in program run only for pointer of object with name: "℗"
        /// </summary>
        protected ushort ValuesIndeksCount = 0;
        internal List<SingleTypeOfDomein> allTypes = new List<SingleTypeOfDomein>();
        readonly object locker = new object();

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
                        //...try to find the same type added before
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

        #region CreateTypesTree_Medhods
        private void AddNonValueType()
        {
            List<SingleType> NonValueTypes = new List<SingleType>();
            
            foreach (SingleTypeOfDomein singleType in this.allTypes)
            {
                foreach (Value singleTypeValue in singleType.Values)
                    if (!singleTypeValue.Type.IsValueType)
                    {
                        Value PointerV = new Value("℗", typeof(IntPtr), typeof(object), false)
                        {
                            IsInUse_ActionCostIn = singleTypeValue.IsInUse_ActionCostIn,
                            IsInUse_EffectIn = singleTypeValue.IsInUse_EffectIn,
                            IsInUse_EffectOut = singleTypeValue.IsInUse_EffectOut,
                            IsInUse_PreconditionIn = singleTypeValue.IsInUse_PreconditionIn
                        };

                        NonValueTypes.Add(new SingleType(singleTypeValue.Type, new List<Value> { }));
                    }
            }

            if (NonValueTypes.Any())
                CompleteTypes(NonValueTypes);
        }

        private void CreateRootofTree(out TreeNode<SingleTypeOfDomein> Root)
        {
            Root = new TreeNode<SingleTypeOfDomein>(); //utwórz korzeń drzewa
            Root.Content = new SingleTypeOfDomein(typeof(object), new List<Value>());

            //czy wykorzystano nie wartościowy (value) typ w domenie
            bool AnyNonValueType = false;

            foreach (SingleTypeOfDomein singleType in this.allTypes) //Podepnij wszystko pod ten korzeń
            {
                SingleTypeOfDomein ParentV = new SingleTypeOfDomein(typeof(object), new List<Value>());
                TreeNode<SingleTypeOfDomein> Parent = new TreeNode<SingleTypeOfDomein>
                {
                    Content = ParentV
                };

                var types = singleType.Type.InheritedTypes().Types.Reverse().ToList();
                //var types = singleType.Type.InheritedTypes().Types.ToList();
                foreach (Type type in types)
                {
                    if (type == singleType.Type)
                    {
                        TreeNode<SingleTypeOfDomein> ToAdd = new TreeNode<SingleTypeOfDomein>()
                        {
                            Root = Parent,
                            Content = singleType
                        };
                        Parent.Children.Add(ToAdd);

                        if (type.BaseType == typeof(object))
                            Root.Children.Add(ToAdd);
                    }
                    else
                    {
                        ParentV = new SingleTypeOfDomein(type, new List<Value>());
                        TreeNode<SingleTypeOfDomein> ParentN = new TreeNode<SingleTypeOfDomein>
                        {
                            Content = ParentV
                        };
                        Parent.Children.Add(ParentN);
                        Parent = ParentN;

                        if (type.BaseType == typeof(object))
                            Root.Children.Add(Parent);
                    }
                }

                if (!AnyNonValueType)
                    foreach (Value v in singleType.Values)
                        if (!v.Type.IsValueType)
                            AnyNonValueType = true;
            }

            //if any value is not value type
            if (AnyNonValueType)
            {
                Value PointerV = new Value("℗", typeof(IntPtr), typeof(object), false)
                {
                    IsInUse_PreconditionIn = true,
                    IsInUse_EffectIn = true
                };
                SingleTypeOfDomein singleTypeOfRoot = new SingleTypeOfDomein(typeof(object), new List<Value>() { PointerV });
                Root.Content = singleTypeOfRoot;
            }
            else
                //cause 0-key value is for pointer only
                ValuesIndeksCount++;
        }

        private void GetBranchRight(TreeNode<SingleTypeOfDomein> root)
        {
            IEnumerable<IGrouping<Type, TreeNode<SingleTypeOfDomein>>> GroupedCh = root.Children.GroupBy(c => c.Content.Type);
            List<TreeNode<SingleTypeOfDomein>> newRootChild = new List<TreeNode<SingleTypeOfDomein>>();

            foreach (IGrouping<Type, TreeNode<SingleTypeOfDomein>> OneGroupOfCh in GroupedCh)
            {
                TreeNode<SingleTypeOfDomein> singleTypeOfDomeins = new TreeNode<SingleTypeOfDomein>
                {
                    Root = root,
                    Content = OneGroupOfCh.ToList()[0].Content
                };

                foreach (var ListOfOneGroup in OneGroupOfCh.ToList())
                {
                    if (ListOfOneGroup.Children.Any())
                        singleTypeOfDomeins.Children.Add(ListOfOneGroup.Children[0]);
                }

                newRootChild.Add(singleTypeOfDomeins);
            }

            foreach (var newRootOfGroup in newRootChild)
                GetBranchRight(newRootOfGroup);

            root.Children = newRootChild;

        }

        private void PopulateInheritedTypes(TreeNode<SingleTypeOfDomein> node)
        {
            //Go to every end of tree...
            if (node.Children.Any())
                foreach (TreeNode<SingleTypeOfDomein> nodeChild in node.Children)
                    PopulateInheritedTypes(nodeChild);

            //...in the end of the tree
            else
            {
                //if no value in node's content stop to work here
                if (!node.Content.Values.Any())
                    return;

                for (int i = node.Content.Values.Count() - 1; i >= 0; i--)
                {
                    //take i-th value from node's content
                    Value TempValue = node.Content.Values[i];

                    //i-th value from node's content moved up
                    bool MovedUp = false;

                    TreeNode<SingleTypeOfDomein> TempNode = node;
                    TreeNode<SingleTypeOfDomein> TempRoot = TempNode.Root;

                    while (!(TempRoot is null))
                    {
                        //take Fields and Properties of root content
                        IEnumerable<MemberInfo> RootCorectMembers = TempRoot.Content.Type.GetMember(TempValue.Name).Where(M => M.MemberType == MemberTypes.Field || M.MemberType == MemberTypes.Property);

                        if (RootCorectMembers.Any())
                        {
                            TempNode = TempRoot;
                            TempRoot = TempNode.Root;
                            MovedUp = true;
                            continue;
                        }

                        if (!TempNode.Content.Values.Any(v => v.Name == TempValue.Name))
                            TempNode.Content.Values.Add(TempValue);

                        break;
                    }

                    if (MovedUp)
                        node.Content.Values.Remove(TempValue);
                }

                /*while (!(TempRoot is null))
                {
                    //take Fields and Properties of root content
                    IEnumerable<MemberInfo> RootMembers = TempRoot.Content.Type.GetMembers().Where(M => M.MemberType == MemberTypes.Field || M.MemberType == MemberTypes.Property);

                    //for every value of content
                    for (int i = node.Content.Values.Count() - 1; i != 0; i--)
                    {


                        //if its just added - go ahead
                        if (TempRoot.Content.Values.Any(v => v.Name == TempValue.Name))
                        {
                            TempNode.Content.Values.Remove(TempValue);
                            continue;
                        }

                        //if its in root too - move it to the root
                        if (RootMembers.Any(M => M.Name == TempValue.Name))
                        {
                            TempRoot.Content.Values.Add(TempValue);
                            TempNode.Content.Values.Remove(TempValue);
                            //SomethingAdded = true;
                        }
                    }

                    //if no value added stop to work here
                    //if (!SomethingAdded)
                        break;

                    //new value of root and actual node
                    TempNode = TempRoot;
                    TempRoot = TempRoot.Root;
                }*/
            }
        }

        private void TagValues(TreeNode<SingleTypeOfDomein> node)
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

        private void CumulateValues(TreeNode<SingleTypeOfDomein> node)
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

        private void CompleteValuesIndekses(TreeNode<SingleTypeOfDomein> Root)
        {
            #region NestedVoid_CompleteValuesIndekses
            void ChangeAtChildren(TreeNode<SingleTypeOfDomein> node, Value childValue, ushort ValuesIndeksCount)
            {
                node.Content.CumulativeValues.First(v => v.Name == childValue.Name).ValueOfIndexesKey = ValuesIndeksCount;

                foreach (var ch in node.Children)
                    ChangeAtChildren(ch, childValue, ValuesIndeksCount);
            }

            void CompleteValues(TreeNode<SingleTypeOfDomein> node)
            {
                if (!(node.Content is null))
                {
                    foreach (Value childValue in node.Content.Values)
                    {
                        if (node.Content.CumulativeValues.Any(cv => cv.Name == childValue.Name && cv.ValueOfIndexesKey != 0))
                            continue;

                        childValue.ValueOfIndexesKey = ValuesIndeksCount;
                        ChangeAtChildren(node, childValue, ValuesIndeksCount);
                        ValuesIndeksCount++;
                    }
                }

                foreach (TreeNode<SingleTypeOfDomein> child in node.Children)
                    CompleteValues(child);
            }
            #endregion

            if (Root is null)
                throw new Exception();

            CompleteValues(Root);
        }

        private void MoveNodesToList(TreeNode<SingleTypeOfDomein> node, List<SingleTypeOfDomein> resultList)
        {
            if (node.Content != null)
                resultList.Add(node.Content);

            foreach (TreeNode<SingleTypeOfDomein> child in node.Children)
                MoveNodesToList(child, resultList);
        }
        #endregion

        internal void CreateTypesTreeAndDiagram(string PathOfClassDiagram)
        {
            if (this.allTypes is null)
                throw new Exception();

            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 39, GloCla.ResMan.GetString("Sa4"));

            AddNonValueType();
            CreateRootofTree(out TreeNode<SingleTypeOfDomein> Root);
            GetBranchRight(Root);
            PopulateInheritedTypes(Root);
            TagValues(Root);
            CumulateValues(Root);
            CompleteValuesIndekses(Root);

            if (Root.Children.Count() == 1)
                Root.Children[0].Content.NeedToTypeCheck = false;

            this.allTypes = new List<SingleTypeOfDomein>();
            MoveNodesToList(Root, allTypes);

            if (PathOfClassDiagram != null)
            {
                TypesPDDLVisualization typesPDDLVisualization = new TypesPDDLVisualization(Root);
                typesPDDLVisualization.MakeGraph(PathOfClassDiagram);
            }

            foreach (var elem in allTypes)
                elem.CreateValuesKeys();

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 40, GloCla.ResMan.GetString("Sp4"));
        }
    }
}
