﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;

namespace SharpPDDL
{
    public enum GoalPriority { Ignore = 0, LowPriority = 1, MediumPriority = 3, HighPriority = 7, TopHihtPriority = 17 };

    public class GoalPDDL
    {
        public readonly string Name;
        public readonly GoalPriority goalPriority;
        internal List<IGoalObject> GoalObjects;

        /// <summary>
        /// Class representing the expected state (consisting of some object(s) or descritpion of attributes of them) resulting from the execution of previously declared actions.
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// GoalPDDL goalPDDL = new GoalPDDL("Goal name", GoalPriority.MediumPriority);<br/>
        /// goalPDDL.AddExpectedObjectState(...);<br/>
        /// domein.AddGoal(goalPDDL);<br/>
        /// </code>
        /// </para></example>
        /// </summary>
        /// <param name="Name">Name of goal</param>
        /// <param name="goalPriority"><see cref="GoalPriority"/>Goal priority description. Default - MediumPriority</param>
        public GoalPDDL(string Name, GoalPriority goalPriority = GoalPriority.MediumPriority)
        {
            this.Name = Name;
            this.goalPriority = goalPriority;
            this.GoalObjects = new List<IGoalObject>();
        }

        /// <summary>
        /// Method adds a description of a specific object previously added to domainObjects which attribute's attainment is goal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="goalExpectation">Description of expected attribute</param>
        /// <param name="originalObj">One of object used previesly at domein.domainObjects.Add(...) method</param>
        /// <param name="newPDDLdomain">Not in use yet</param>
        public void AddExpectedObjectState<T>(T originalObj, Expression<Predicate<T>> goalExpectation, DomeinPDDL newPDDLdomain = null) where T : class
        {
            List<Expression<Predicate<T>>> goalExpectations = new List<Expression<Predicate<T>>>() { goalExpectation };
            AddExpectedObjectState(originalObj, new List<Expression<Predicate<T>>>(goalExpectations), newPDDLdomain);
        }

        /// <summary>
        /// Method adds a description of a specific object previously added to domainObjects which attributes' attainment is goal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="goalExpectation">Collection of description of expected attributes</param>
        /// <param name="originalObj">One of object used previesly at domein.domainObjects.Add(...) method</param>
        /// <param name="newPDDLdomain">Not in use yet</param>
        public void AddExpectedObjectState<T>(T originalObj, ICollection<Expression<Predicate<T>>> goalExpectations, DomeinPDDL newPDDLdomain = null) where T : class
        {
            if (originalObj is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E30"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 107, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            GoalObject<T> temp = new GoalObject<T>(originalObj, typeof(T), newPDDLdomain, goalExpectations.ToList());
            GoalObjects.Add(temp);
        }

        public void AddExpectedObjectState<T>(Type originalObjType, ICollection<Expression<Predicate<T>>> goalExpectations, DomeinPDDL newPDDLdomain = null) where T : class
        {
            //todo sprawdzenie dziedziczenia typów

            GoalObject<T> temp = new GoalObject<T>(null, originalObjType, newPDDLdomain, goalExpectations.ToList());
            GoalObjects.Add(temp);
        }

        public void AddExpectedObjectState<T>(ICollection<Expression<Predicate<T>>> goalExpectations, DomeinPDDL newPDDLdomain = null) where T : class
        {
            AddExpectedObjectState(typeof(T), goalExpectations, newPDDLdomain);
        }

        internal void BUILDIT(List<SingleTypeOfDomein> allTypes)
        {
            if (!GoalObjects.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C30"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 108, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            foreach (IGoalObject GoalObjects in GoalObjects)
            {
                _ = GoalObjects.BuildGoalPDDP(allTypes);
            }
        }
    }
}
