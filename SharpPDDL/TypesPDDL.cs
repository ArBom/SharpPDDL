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
        protected ushort ValuesIndeksCount = 1;
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
                    //check whether this value was added before...
                    bool AnyValueOfName;
                    lock (allTypes[ToTagAllTypesIndex.Value])
                        AnyValueOfName = allTypes[ToTagAllTypesIndex.Value].Values.Any(v => v.Name == value.Name);

                    //...if so, set InUse_ props...
                    if (AnyValueOfName)
                    {
                        Value ToTagV = allTypes[ToTagAllTypesIndex.Value].Values.Find(aV => aV.Name == value.Name);
                        ToTagV.IsInUse_EffectIn = value.IsInUse_EffectIn;
                        ToTagV.IsInUse_EffectOut = value.IsInUse_EffectOut;
                        ToTagV.IsInUse_ActionCostIn = value.IsInUse_ActionCostIn;
                        ToTagV.IsInUse_PreconditionIn = value.IsInUse_PreconditionIn;
                        continue;
                    }

                    //...or add it first time
                    lock (allTypes[ToTagAllTypesIndex.Value])
                        allTypes[ToTagAllTypesIndex.Value].Values.Add(value);
                }
            }
        }

        #region CreateTypesTree_Medhods
        private void AddNonValueType()
        {
            //create temp list to add
            List<SingleType> NonValueTypes = new List<SingleType>();
            
            //for every past type...
            foreach (SingleTypeOfDomein singleType in this.allTypes)
            {
                //...check every single value...
                foreach (Value singleTypeValue in singleType.Values)
                    //...if it's class or any other non-value type it is
                    if (!singleTypeValue.Type.IsValueType)
                    {
                        Value PointerV = new Value(GloCla.PointerVName, typeof(IntPtr), typeof(object), false)
                        {
                            IsInUse_ActionCostIn = singleTypeValue.IsInUse_ActionCostIn,
                            IsInUse_EffectIn = singleTypeValue.IsInUse_EffectIn,
                            IsInUse_PreconditionIn = singleTypeValue.IsInUse_PreconditionIn,
                        };

                        //if so add it to temp list, but without _OUTPUTS cause its static, and it influence next optimization
                        NonValueTypes.Add(new SingleType(singleTypeValue.Type, new List<Value> { PointerV }));
                    }
            }

            //add whole temp list to past types
            if (NonValueTypes.Any())
                CompleteTypes(NonValueTypes);
        }

        private void CreateRootofTree(out TreeNode<SingleTypeOfDomein> Root)
        {
            //utwórz korzeń drzewa
            Root = new TreeNode<SingleTypeOfDomein>
            {
                Content = new SingleTypeOfDomein(typeof(object), new List<Value>())
            };

            //Podepnij wszystko pod ten korzeń
            foreach (SingleTypeOfDomein singleType in this.allTypes)
            {
                SingleTypeOfDomein ParentV = new SingleTypeOfDomein(typeof(object), new List<Value>());
                TreeNode<SingleTypeOfDomein> Parent = new TreeNode<SingleTypeOfDomein>
                {
                    Content = ParentV
                };

                //make a list af inherited types...
                IEnumerable<Type> types = singleType.Type.InheritedTypes().Types.Reverse();
                //...and create 'concatenation' of their
                foreach (Type type in types)
                {                   
                    if (type == singleType.Type) //case of end, but no-object type
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
                    else //case of inner type
                    {
                        ParentV = new SingleTypeOfDomein(type, new List<Value>());
                        TreeNode<SingleTypeOfDomein> ParentN = new TreeNode<SingleTypeOfDomein>
                        {
                            Content = ParentV
                        };
                        Parent.Children.Add(ParentN);
                        Parent = ParentN;

                        //close to object-type end (Root)
                        if (type.BaseType == typeof(object))
                            Root.Children.Add(Parent);
                    }
                }
            }
        }

        private void GetBranchRight(TreeNode<SingleTypeOfDomein> root)
        {
            //group branches by next type
            IEnumerable<IGrouping<Type, TreeNode<SingleTypeOfDomein>>> GroupedCh = root.Children.GroupBy(c => c.Content.Type);
            //create the child list for next branch step
            List<TreeNode<SingleTypeOfDomein>> newRootChild = new List<TreeNode<SingleTypeOfDomein>>();
            
            //for every group...
            foreach (IGrouping<Type, TreeNode<SingleTypeOfDomein>> OneGroupOfCh in GroupedCh)
            {
                //...create new root / head...
                TreeNode<SingleTypeOfDomein> singleTypeOfDomeins = new TreeNode<SingleTypeOfDomein>
                {
                    Root = root,
                    Content = OneGroupOfCh.First().Content
                };

                //...add closser types..
                foreach (TreeNode<SingleTypeOfDomein> ListOfOneGroup in OneGroupOfCh)
                {
                    //...and rest of 'concatenations'
                    if (ListOfOneGroup.Children.Any())
                        singleTypeOfDomeins.Children.Add(ListOfOneGroup.Children[0]);

                    //Set values of new root/head
                    foreach (Value v in ListOfOneGroup.Content.Values)
                    {
                        //Update IsInUse_ tags...
                        if (singleTypeOfDomeins.Content.Values.Any(va => va.Name == v.Name))
                        {
                            Value toTagV = singleTypeOfDomeins.Content.Values.First(va => va.Name == v.Name);
                            toTagV.IsInUse_EffectIn = v.IsInUse_EffectIn;
                            toTagV.IsInUse_EffectOut = v.IsInUse_EffectOut;
                            toTagV.IsInUse_ActionCostIn = v.IsInUse_ActionCostIn;
                            toTagV.IsInUse_PreconditionIn = v.IsInUse_PreconditionIn;
                        }
                        else //...or copy if there wasnt be before
                            singleTypeOfDomeins.Content.Values.Add(v);
                    }
                }

                newRootChild.Add(singleTypeOfDomeins);
            }

            //do it again in great depth
            foreach (TreeNode<SingleTypeOfDomein> newRootOfGroup in newRootChild)
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

                    //if the i-th value from node's content was moved up remove it
                    if (MovedUp)
                        node.Content.Values.Remove(TempValue);
                }
            }
        }

        private void TagValues(TreeNode<SingleTypeOfDomein> node)
        {
            //do it in great depth
            if (node.Children.Any())
                foreach (TreeNode<SingleTypeOfDomein> child in node.Children)
                    TagValues(child);

            //from the end of branch
            if (node.Content != null)
            {
                TreeNode<SingleTypeOfDomein> tempNode = node;
                //go to root
                while (tempNode.Root != null)
                {
                    if (tempNode.Root.Content is null) //todo is it necessary?
                    {
                        tempNode = tempNode.Root;
                        continue;
                    }

                    //update every value's IsItUse_ tag at root
                    foreach (Value v in tempNode.Content.Values)
                    {
                        if (tempNode.Root.Content.Values.Any(aV => aV.Name == v.Name))
                        {
                            Value ToTagV = tempNode.Root.Content.Values.Find(aV => aV.Name == v.Name);
                            ToTagV.IsInUse_EffectIn = v.IsInUse_EffectIn;
                            ToTagV.IsInUse_EffectOut = v.IsInUse_EffectOut;
                            ToTagV.IsInUse_ActionCostIn = v.IsInUse_ActionCostIn;
                            ToTagV.IsInUse_PreconditionIn = v.IsInUse_PreconditionIn;
                        }
                    }

                    tempNode = tempNode.Root;
                }
            }
        }

        private void CumulateValues(TreeNode<SingleTypeOfDomein> node)
        {
            //if root exists...
            if (node.Root?.Content != null)
            {
                //...take CumulativeValues from it...
                node.Content.CumulativeValues = new List<Value>(node.Root.Content.CumulativeValues);
                IEnumerable<Value> newValues = node.Content.Values.Where(v => !node.Content.CumulativeValues.Any(cv => cv.Name == v.Name));
                //...and add itself values
                node.Content.CumulativeValues.AddRange(newValues);
            }

            //do it again for child
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
                        if (childValue.Name == GloCla.PointerVName)
                        {
                            childValue.ValueOfIndexesKey = 0;
                            ChangeAtChildren(node, childValue, 0);
                            continue;
                        }

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
