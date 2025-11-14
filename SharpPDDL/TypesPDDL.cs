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
        /// 0 value shouldn't occur in program run
        /// </summary>
        protected ushort ValuesIndeksCount = 0;
        internal List<SingleTypeOfDomein> allTypes = new List<SingleTypeOfDomein>();
        object locker = new object();

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
        private void CreateRootofTree(out TreeNode<SingleTypeOfDomein> Root)
        {
            Root = new TreeNode<SingleTypeOfDomein>(); //utwórz korzeń drzewa

            foreach (SingleTypeOfDomein singleType in this.allTypes) //Podepnij wszystko pod ten korzeń
            {
                TreeNode<SingleTypeOfDomein> ToAdd = new TreeNode<SingleTypeOfDomein>()
                {
                    Root = Root,
                    Content = singleType
                };

                Root.Children.Add(ToAdd);
            }
        }

        private void GetBranchRight(TreeNode<SingleTypeOfDomein> root)
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
                            newType = littermate.First(l => l.Content.Type == types[currentTypesArg]);
                        else
                        {
                            SingleTypeOfDomein singleType = new SingleTypeOfDomein(types[currentTypesArg], new List<Value>());

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

        private void PopulateInheritedTypes(TreeNode<SingleTypeOfDomein> node)
        {
            //Go to every end of tree...
            if (node.Children.Any())
                foreach (TreeNode<SingleTypeOfDomein> Ch in node.Children)
                    PopulateInheritedTypes(Ch);

            //...in the end
            else
            {
                bool SomethingAdded = false;
                TreeNode<SingleTypeOfDomein> TempNode = node;
                TreeNode<SingleTypeOfDomein> TempRoot = TempNode.Root;

                while (!(TempRoot.Content is null))
                {
                    //take Fields and Properties of root content
                    IEnumerable<MemberInfo> RootMembers = TempRoot.Content.Type.GetMembers().Where(M => M.MemberType == MemberTypes.Field || M.MemberType == MemberTypes.Property);

                    //for every value of content
                    for (int i = node.Content.Values.Count() - 1; i != 0; i--)
                    {
                        //take i-th value from node's content
                        Value TempValue = node.Content.Values[i];

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
                            SomethingAdded = true;
                        }
                    }

                    //if no value added stop to work here
                    if (!SomethingAdded)
                        break;

                    //new value of root and actual node
                    TempNode = TempRoot;
                    TempRoot = TempRoot.Root;
                }
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

                        ValuesIndeksCount++;
                        childValue.ValueOfIndexesKey = ValuesIndeksCount;
                        ChangeAtChildren(node, childValue, ValuesIndeksCount);
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

        internal void CreateTypesTree()
        {          
            if (this.allTypes is null)
                throw new Exception();

            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 39, GloCla.ResMan.GetString("Sa4"));

            //TreeNode<SingleTypeOfDomein> Root;

            CreateRootofTree(out TreeNode < SingleTypeOfDomein > Root);
            GetBranchRight(Root);
            PopulateInheritedTypes(Root);
            TagValues(Root);
            CumulateValues(Root);
            CompleteValuesIndekses(Root);

            if (Root.Children.Count() == 1)
                Root.Children[0].Content.NeedToTypeCheck = false;

            this.allTypes = new List<SingleTypeOfDomein>();
            MoveNodesToList(Root, allTypes);
            Root = null;

            foreach (var elem in allTypes)
                elem.CreateValuesKeys();

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 40, GloCla.ResMan.GetString("Sp4"));
        }
    }
}
