using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;

namespace SharpPDDL
{
    public enum GoalPriority
    {
        Ignore = 0,
        LowPriority = 1,
        MediumPriority = 3,
        HighPriority = 7,
        TopHihtPriority = 17
    }

    public class GoalPDDL
    {
        public readonly string Name;
        public readonly GoalPriority goalPriority;
        public EventHandler GoalRealized;
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
        /// Method adds a description of a specific object added to domainObjects which attribute's attainment is goal, and where move object to after reach they.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="originalObj">One of object used at domein.domainObjects.Add(...) method</param>
        /// <param name="goalExpectation">Description of expected attribute</param>
        /// <param name="newPDDLdomain">Domain where to move object for, after goal realized; NULL - for remove object from algorithm</param>
        public void AddExpectedObjectState<T>(T originalObj, Expression<Predicate<T>> goalExpectation, DomeinPDDL newPDDLdomain) 
            where T : class
        {
            ICollection<Expression<Predicate<T>>> goalExpectations = new Expression<Predicate<T>>[1] { goalExpectation };
            AddExpectedObjectState(originalObj, new List<Expression<Predicate<T>>>(goalExpectations), newPDDLdomain);
        }

        /// <summary>
        /// Method adds a description of a specific object added to domainObjects which attributes' attainment is goal, and where move object to after reach they.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="originalObj">One of object used at domein.domainObjects.Add(...) method</param>
        /// <param name="goalExpectations">Collection of description of expected attributes</param>
        /// <param name="newPDDLdomain">Domain where to move object for, after goal realized; NULL - for remove object from algorithm</param>
        public void AddExpectedObjectState<T>(T originalObj, ICollection<Expression<Predicate<T>>> goalExpectations, DomeinPDDL newPDDLdomain) 
            where T : class
        {
            if (originalObj is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E30"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 107, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            AddGoalObject(originalObj, newPDDLdomain, goalExpectations, true);
        }

        /// <summary>
        /// Method adds a description of a specific object added to domainObjects which attribute's attainment is goal. Object stay in this domain after reach the goal.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="originalObj">One of object used at domein.domainObjects.Add(...) method</param>
        /// <param name="goalExpectation">Description of expected attribute</param>
        public void AddExpectedObjectState<T>(T originalObj, Expression<Predicate<T>> goalExpectation) 
            where T : class
        {
            ICollection<Expression<Predicate<T>>> goalExpectations = new Expression<Predicate<T>>[1] { goalExpectation };
            AddExpectedObjectState(originalObj, goalExpectations);
        }

        /// <summary>
        /// Method adds a description of a specific object added to domainObjects which attributes' attainment is goal. Object stay in this domain after reach the goal.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="originalObj">One of object used at domein.domainObjects.Add(...) method</param>
        /// <param name="goalExpectations">Collection of description of expected attributes</param>
        public void AddExpectedObjectState<T>(T originalObj, ICollection<Expression<Predicate<T>>> goalExpectations) 
            where T : class
        {
            if (originalObj is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E30"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 107, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            AddGoalObject(originalObj, null, goalExpectations, false);
        }

        /// <summary>
        /// Method adds a description of object (added to domainObjects) of given type, which attributes' attainment is goal. Object stay in this domain after reach the goal.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="goalExpectations">Collection of description of expected attributes</param>
        public void AddExpectedObjectState<T>(ICollection<Expression<Predicate<T>>> goalExpectations) 
            where T : class
        => AddGoalObject(null, null, goalExpectations, false);

        /// <summary>
        /// Method adds a description of object (added to domainObjects) of given type, which attributes' attainment is goal, and where move object to after reach they.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="goalExpectations">Collection of description of expected attributes</param>
        /// <param name="newPDDLdomein">Domain where to move object for, after goal realized; NULL - for remove object from algorithm</param>
        public void AddExpectedObjectState<T>(ICollection<Expression<Predicate<T>>> goalExpectations, DomeinPDDL newPDDLdomein)
            where T : class
        => AddGoalObject(null, newPDDLdomein, goalExpectations, true);

        /// <summary>
        /// Method adds a description of object (added to domainObjects) of given type, which attribute's attainment is goal, and where move object to after reach they.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="goalExpectation">Description of expected attribute</param>
        /// <param name="newPDDLdomain">Domain where to move object for, after goal realized; NULL - for remove object from algorithm</param>
        public void AddExpectedObjectState<T>(Expression<Predicate<T>> goalExpectation, DomeinPDDL newPDDLdomain)
            where T : class
        {
            ICollection<Expression<Predicate<T>>> Predications = new Expression<Predicate<T>>[1] { goalExpectation };
            AddExpectedObjectState<T>(Predications, newPDDLdomain);
        }

        /// <summary>
        /// Method adds a description of object (added to domainObjects) of given type, which attribute's attainment is goal. Object stay in this domain after reach the goal.
        /// </summary>
        /// <typeparam name="T">One of classes used to describe domain actions</typeparam>
        /// <param name="goalExpectation">Description of expected attribute</param>
        public void AddExpectedObjectState<T>(Expression<Predicate<T>> goalExpectation) 
            where T : class
        {
            ICollection<Expression<Predicate<T>>> Predications = new Expression<Predicate<T>>[1] { goalExpectation };
            AddExpectedObjectState<T>(Predications);
        }

        [Obsolete("This method is deprecated.", true)]
        public void AddExpectedObjectState<T>(Type originalObjType, ICollection<Expression<Predicate<T>>> goalExpectations, DomeinPDDL newPDDLdomain = null) where T : class { }

        private void AddGoalObject<T>(T originalObj, DomeinPDDL newPDDLdomain, ICollection<Expression<Predicate<T>>> goalExpectations, bool Migrate)
            where T : class
        {
            if (!(originalObj is null))
            {
                if (GoalObjects.Any(GO => GO.OriginalObj.Equals(originalObj)))
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 144, GloCla.ResMan.GetString("W16"), Name, typeof(T));
                    return;
                }
            }

            if (GoalObjects.Any(GO => !(GO.GoalPDDL is null)))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 140, GloCla.ResMan.GetString("W15"), Name);
                return;
            }

            GoalObject<T> NewOne = new GoalObject<T>(originalObj, typeof(T), newPDDLdomain, goalExpectations, Migrate);
            GoalObjects.Add(NewOne);
        }

        internal void BuildIt(DomeinPDDL GoalOwner)
        {
            if (!GoalObjects.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C30"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 108, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            foreach (IGoalObject GoalObjects in GoalObjects)
            {
                _ = GoalObjects.BuildGoalPDDP(GoalOwner);
            }
        }
    }
}
