﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    public enum GoalPriority { Ignore = 0, LowPriority = 1, MediumPriority = 3, HighPriority = 7, TopHihtPriority = 17 };

    public class GoalPDDL
    {
        public readonly string Name;
        public readonly GoalPriority goalPriority;
        internal List<IGoalObject> GoalObjects;

        public GoalPDDL(string Name, GoalPriority goalPriority = GoalPriority.MediumPriority)
        {
            this.Name = Name;
            this.goalPriority = goalPriority;
            this.GoalObjects = new List<IGoalObject>();
        }

        public void AddExpectedObjectState<T>(Expression<Predicate<T>> goalExpectation, T originalObj, DomeinPDDL newPDDLdomain = null) where T : class
        {
            List<Expression<Predicate<T>>> goalExpectations = new List<Expression<Predicate<T>>>() { goalExpectation };
            AddExpectedObjectState<T>(new List<Expression<Predicate<T>>>(goalExpectations), originalObj, newPDDLdomain);
        }

        public void AddExpectedObjectState<T>(ICollection<Expression<Predicate<T>>> goalExpectations, T originalObj, DomeinPDDL newPDDLdomain = null) where T : class
        {
            if (originalObj is null)
                throw new Exception();

            if (GoalObjects.Exists(g => g.OriginalObj.Equals(originalObj)))
                throw new Exception();

            GoalObject<T> temp = new GoalObject<T>(originalObj, typeof(T), newPDDLdomain, (List<Expression<Predicate<T>>>)goalExpectations);
            GoalObjects.Add(temp);
        }

        public void AddExpectedObjectState<T>(ICollection<Expression<Predicate<T>>> goalExpectations, Type originalObjType, DomeinPDDL newPDDLdomain = null) where T : class
        {
            //todo sprawdzenie dziedziczenia typów

            GoalObject<T> temp = new GoalObject<T>(null, originalObjType, newPDDLdomain, (List<Expression<Predicate<T>>>)goalExpectations);
            GoalObjects.Add(temp);
        }

        public void AddExpectedObjectState<T>(ICollection<Expression<Predicate<T>>> goalExpectations, DomeinPDDL newPDDLdomain = null) where T : class
        {
            AddExpectedObjectState(goalExpectations, typeof(T), newPDDLdomain);
        }

        internal void BUILDIT(List<SingleTypeOfDomein> allTypes)
        {
            if (GoalObjects.Count == 0)
                throw new Exception();

            foreach (IGoalObject GoalObjects in GoalObjects)
            {
                _ = GoalObjects.BuildGoalPDDP(allTypes);
            }
        }
    }
}
