using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    public class ActionPDDL
    {
        public readonly string Name;
        private List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private List<EffectPDDL> Effects; //efekty
        internal List<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)
        private List<(int, string, Expression[])> ActionSententia;
        internal ActionCost actionCost;
        private List<string> EffectsUsedAlsoAsExecution;
        private List<ExpressionExecution> Executions;
        internal Delegate InstantActionPDDL { get; private set; }
        internal Delegate InstantExecution { get; private set; }
        internal Delegate InstantExecutionChecker { get; private set; }
        internal Delegate InstantActionSententia { get; private set; }
        internal int InstantActionParamCount => Parameters.Count;

        internal List<SingleType> TakeSingleTypes()
        {
            foreach (PreconditionPDDL precondition in Preconditions)
                precondition.CompleteActinParams(Parameters);

            foreach (EffectPDDL effect in Effects)
                effect.CompleteActinParams(Parameters);

            actionCost.CompleteActinParams(Parameters);

            List<SingleType> ToRet = new List<SingleType>();

            foreach (Parametr parametr in Parameters)
            {
                parametr.RemoveUnuseValue();
                SingleType singleType = null;

                if (ToRet.Any())
                {
                    var singleTypes = ToRet.Where(sT => sT.Type == parametr.Type);
                    if (singleTypes.Any())
                        singleType = singleTypes.First();
                }

                if (singleType is null)
                {
                    singleType = new SingleType(parametr.Type, parametr.values);
                    ToRet.Add(singleType);
                    continue;
                }

                foreach (Value valueP in parametr.values)
                {
                    if (singleType.Values.Exists(t => t.Name == valueP.Name))
                    {
                        int ToTagIndex = singleType.Values.FindIndex(t => t.Name == valueP.Name);
                        singleType.Values[ToTagIndex].IsInUse_EffectIn = valueP.IsInUse_EffectIn;
                        singleType.Values[ToTagIndex].IsInUse_EffectOut = valueP.IsInUse_EffectOut;
                        singleType.Values[ToTagIndex].IsInUse_PreconditionIn = valueP.IsInUse_PreconditionIn;

                        continue;
                    }
                        
                    singleType.Values.Add(valueP);
                }
            }

            return ToRet;
        }

        #region ActionSenteniae
        /// <summary>
        /// It make possible to define text describe the action execution in actions plan
        /// </summary>
        /// <param name="Text">This text will be shown in plan to realization</param>
        public void AddPartOfActionSententia(string Text)
        {
            if (String.IsNullOrEmpty(Text))
                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 46, GloCla.ResMan.GetString("W3"));
            else
                ActionSententia.Add((-1, Text, null));
        }

        /// <summary>
        /// It make possible to define text describe the action execution in actions plan
        /// </summary>
        /// <typeparam name="T">Non-abstract class</typeparam>
        /// <param name="destination">Instance of class used in actionPDDL, the owner of parameter(s) used in this Sententia</param>
        /// <param name="Text">This text will be shown in plan to realization</param>
        /// <param name="TextParams">Function(s) of destionation class, it/these replace the {n} substring(s) in Text</param>
        public void AddPartOfActionSententia<T>(ref T destination, string Text, params Expression<Func<T, object>>[] TextParams) 
            where T : class
        {
            if (TextParams.Length == 0)
            {
                if (Text.Contains("{0}"))
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 138, GloCla.ResMan.GetString("W14"), ActionSententia.Count, Text);
                else
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 139, GloCla.ResMan.GetString("V9"), Text);

                AddPartOfActionSententia(Text);
                return;
            }

            Parametr.GetTheInstance_TryAddToList(Parameters, ref destination);

            if (!String.IsNullOrEmpty(Text))
            {
                int DestHashCode = destination.GetHashCode();
                Parametr p = Parameters.Where(t => t.HashCode == DestHashCode).First();
                int index = Parameters.IndexOf(p);
                ActionSententia.Add((index, Text, TextParams));
            }
            else
                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 46, GloCla.ResMan.GetString("W3"));
        }
        #endregion ActionSenteniae

        #region Adding Precondictions
        /// <summary>
        /// This method adds a condition whose fulfillment is necessary to perform the action.
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// using System.Linq.Expressions;<br/>
        /// ⋮<br/>
        /// Foo foo = null;<br/>
        /// actionPDDL.AddPrecondiction("FooMemberInt big enough", ref foo, f => f.FooMemberInt > 100);
        /// </code>
        /// </para></example>
        /// </summary>
        /// <typeparam name="T1">Non-abstract class of precondition param object</typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty precondition name</param>
        /// <param name="obj">Instance of T1 class (could be null) which representant parameter of action</param>
        /// <param name="func">Predicate uses member(s) of T1 class to check possibility of action ececute</param>
        public void AddPrecondiction<T1>(string Name, ref T1 obj, Expression<Predicate<T1>> func) 
            where T1 : class 
            => AddPrecondiction<T1, T1>(Name, ref obj, func);

        /// <summary>
        /// This method adds a condition whose fulfillment is necessary to perform the action.
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// using System.Linq.Expressions;<br/>
        /// ⋮<br/>
        /// public class Container { public bool IsOpen; }<br/>
        /// public class Carton : Container {…}<br/>
        /// ⋮<br/>
        /// Expression<Predicate<Conteiner>> IsContainerOpen = (c => c.IsOpen);<br/>
        /// Carton carton = null;<br/>
        /// actionPDDL.AddPrecondiction("Carton is open", ref carton, IsContainerOpen);<br/>
        /// </code>
        /// </para></example>
        /// </summary>
        /// <typeparam name="T1c">Non-abstract class inheriting from T1p</typeparam>
        /// <typeparam name="T1p">Non-abstract class inherited by T1c</typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty precondition name</param>
        /// <param name="obj">Instance of T1c class (could be null) which representant parameter of action</param>
        /// <param name="func">Predicate uses member(s) of T1p class to check possibility of action ececute</param>
        public void AddPrecondiction<T1c, T1p>(string Name, ref T1c obj, Expression<Predicate<T1p>> func)
            where T1p : class
            where T1c : class, T1p
        {
            _ = PreconditionPDDL.Instance(Name, Parameters, Preconditions, ref obj, func);
            if (!(InstantActionPDDL is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 134, GloCla.ResMan.GetString("I9"), this.Name, Name);
        }

        /// <summary>
        /// This method adds a condition (of 2 params) whose fulfillment is necessary to perform the action.
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// using System.Linq.Expressions;<br/>
        /// ⋮<br/>
        /// public class Container { public int capacity; }<br/>
        /// public class Carton : Container {…}<br/>
        /// public class Foo { public int size; }<br/>
        /// public class Moo : Foo {…}<br/>
        /// ⋮<br/>
        /// Expression<Predicate<Container, Moo>> capa = ((Co, Mo) => (Co.capacity >= Mo.size));
        /// actionPDDL.AddPrecondiction("Carton capacity enouh for Moo", ref carton, ref moo, capa);<br/>
        /// </code>
        /// </para></example>
        /// </summary>
        /// <typeparam name="T1c">Non-abstract class of 1st param inheriting from T1p</typeparam>
        /// <typeparam name="T1p">Non-abstract class of 1st param inherited by T1c</typeparam>
        /// <typeparam name="T2c">Non-abstract class of 2nd param inheriting from T2p</typeparam>
        /// <typeparam name="T2p">Non-abstract class of 2nd param inherited by T2c</typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty precondition name</param>
        /// <param name="obj1">Instance of T1c class (could be null) which representant 1st parameter of action</param>
        /// <param name="obj2">Instance of T2c class (could be null) which representant 2nd parameter of action</param>
        /// <param name="func">Predicate uses member(s) of T1p and T2p classes to check possibility of action ececute</param>
        public void AddPrecondiction<T1c, T1p, T2c, T2p>(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func)
            where T1p : class 
            where T2p : class 
            where T1c : class, T1p 
            where T2c : class, T2p
        {
            _ = PreconditionPDDL.Instance(Name, Parameters, Preconditions, ref obj1, ref obj2, func);
            if (!(InstantActionPDDL is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 134, GloCla.ResMan.GetString("I9"), this.Name, Name);
        }

        /// <summary>
        /// This method adds a condition (of 3 params) whose fulfillment is necessary to perform the action.
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// using System.Linq.Expressions;<br/>
        /// ⋮<br/>
        /// public class Point { public int X, Y; }<br/>
        /// ⋮<br/>
        /// Expression<Predicate<Point, Point, Point>> Collinearity = ((P1, P2, P3) => ...);<br/>
        /// actionPDDL.AddPrecondiction("Points are collinear", ref Point1, ref Point2, Point3, Collinearity);<br/>
        /// </code>
        /// </para></example>
        /// </summary>
        /// <typeparam name="T1c">Non-abstract class of 1st param inheriting from T1p</typeparam>
        /// <typeparam name="T1p">Non-abstract class of 1st param inherited by T1c</typeparam>
        /// <typeparam name="T2c">Non-abstract class of 2nd param inheriting from T2p</typeparam>
        /// <typeparam name="T2p">Non-abstract class of 2nd param inherited by T2c</typeparam>
        /// <typeparam name="T3c">Non-abstract class of 3rd param inheriting from T3p</typeparam>
        /// <typeparam name="T3p">Non-abstract class of 3rd param inherited by T3c</typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty precondition name</param>
        /// <param name="obj1">Instance of T1c class (could be null) which representant 1st parameter of action</param>
        /// <param name="obj2">Instance of T2c class (could be null) which representant 2nd parameter of action</param>
        /// <param name="obj3">Instance of T3c class (could be null) which representant 3rd parameter of action</param>
        /// <param name="func">Predicate uses member(s) of T1p, T2p and T3p classes to check possibility of action ececute</param>
        public void AddPrecondiction<T1c, T1p, T2c, T2p, T3c, T3p>(string Name, ref T1c obj1, ref T2c obj2, ref T3c obj3, Expression<Predicate<T1p, T2p, T3p>> func)
            where T1p : class
            where T2p : class
            where T3p : class
            where T1c : class, T1p
            where T2c : class, T2p
            where T3c : class, T3p
        {
            _ = PreconditionPDDL.Instance(Name, Parameters, Preconditions, ref obj1, ref obj2, ref obj3, func);
            if (!(InstantActionPDDL is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 134, GloCla.ResMan.GetString("I9"), this.Name, Name);
        }
        #endregion

        #region Adding Effects
        /// <summary>
        /// This method adds the effect of assigning a constant value to a parameter's member after the action is performed
        /// <example><para>
        /// For example:<br/>
        /// <code>
        /// using System.Linq.Expressions;<br/>
        /// ⋮<br/>
        /// public class Container { public bool IsOpen = false; }<br/>
        /// ⋮<br/>
        /// actionPDDL.AddEffect("Open Container", true, ref container, C => C.IsOpen);<br/>
        /// </code>
        /// results in <c>container</c>'s next simulation state IsOpen member having the value "true"
        /// </para></example>
        /// </summary>
        /// <typeparam name="T">Non-abstract class of effect param object</typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty effect name</param>
        /// <param name="newValue_Static">New value assigning to <c>destinationObj</c>>'s member</param>
        /// <param name="destinationObj">Instance of T1 class which representant parameter which we assign the member value to</param>
        /// <param name="destinationMember">A description of the parameter member to whom one is assigning <c>newValue_Static</c> value</param>
        public void AddEffect<T>(string Name, ref T destinationObj, Expression<Func<T, ValueType>> destinationMember, ValueType newValue_Static) where T : class
        {
            _ = EffectPDDL.Instance(Name, Parameters, Effects, ref destinationObj, destinationMember, newValue_Static);
            if (!(InstantActionPDDL is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 135, GloCla.ResMan.GetString("I10"), this.Name, Name);
        }

        /// <summary>
        /// This method uses one object to assigning new value to another parameter's member after the action is performed
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty effect name</param>
        /// <param name="SourceObj">One of action parametr from which is taken value</param>
        /// <param name="Source">Point of source value to take</param>
        /// <param name="DestinationObj">One of action parametr to which is moved value</param>
        /// <param name="DestinationMember">Point of destination value to move</param>
        public void AddEffect<T1c, T1p, T2c, T2p>(string Name, ref T1c DestinationObj, Expression<Func<T1p, ValueType>> DestinationMember, ref T2c SourceObj, Expression<Func<T2p, ValueType>> Source)
            where T1p : class
            where T2p : class
            where T1c : class, T1p
            where T2c : class, T2p
        { 
             _ = EffectPDDL.Instance(Name, Parameters, Effects, ref DestinationObj, DestinationMember, ref SourceObj, Source);
            if (!(InstantActionPDDL is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 135, GloCla.ResMan.GetString("I10"), this.Name, Name);
        }

        /// <summary>
        /// This method uses one object to assigning new value to another parameter's member after the action is performed
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty effect name</param>
        /// <param name="SourceObj">Action parametr from which is taken value</param>
        /// <param name="Source">Point of source value to take</param>
        /// <param name="DestinationObj">One of action parametr to which is moved value</param>
        /// <param name="DestinationMember">Point of destination value to move</param>
        public void AddEffect<T1, T2>(string Name, ref T1 DestinationObj, Expression<Func<T1, ValueType>> DestinationMember, ref T2 SourceObj, Expression<Func<T2, ValueType>> Source)
            where T1 : class
            where T2 : class
        {
            AddEffect<T1, T1, T2, T2>(Name, ref DestinationObj, DestinationMember, ref SourceObj, Source);
            if (!(InstantActionPDDL is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 135, GloCla.ResMan.GetString("I10"), this.Name, Name);
        }

        /// <summary>
        /// This method uses one object to assigning new value to another parameter's member after the action is performed
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty effect name</param>
        /// <param name="DestinationObj">One of action parametr to which is moved value</param>
        /// <param name="DestinationFunct">Point of destination value to assingnation</param>
        /// <param name="SourceObj">One of action parametr from which is taken value</param>
        /// <param name="SourceFunct">Function of DestinationObj and SourceObj the source value to take</param>
        public void AddEffect<T1, T2>(string Name, ref T1 DestinationObj, Expression<Func<T1, ValueType>> DestinationFunct, ref T2 SourceObj, Expression<Func<T1, T2, ValueType>> SourceFunct)
            where T1 : class 
            where T2 : class
        { 
            _ = EffectPDDL.Instance(Name, Parameters, Effects, ref DestinationObj, DestinationFunct, ref SourceObj, SourceFunct);
            if (!(InstantActionPDDL is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 135, GloCla.ResMan.GetString("I10"), this.Name, Name);
        }
        #endregion

        #region Adding Execution
        [Obsolete("This method is deprecated use AddExecution(string EffectName)", false)]
        public void UseEffectAlsoAsExecution(string EffectName)
        {
            Trace.WriteLine("SharpPDDL: Method UseEffectAlsoAsExecution(string EffectName) is obsolete and it will be removed soon! Use AddExecution(string EffectName) method.");
            GloCla.Tracer?.TraceEvent(TraceEventType.Warning, -1, "Method UseEffectAlsoAsExecution(string EffectName) is obsolete and it will be removed soon! Use AddExecution(string EffectName) method.");
            AddExecution(EffectName);
        }
        
        /// <summary>
        /// Use EffectPDDL also as execution
        /// </summary>
        /// <param name="EffectName">Name of Effect which will be use by SharpPDDL as execution too</param>
        public void AddExecution(string EffectName)
        {
            EffectsUsedAlsoAsExecution.Add(EffectName);
            if (!(InstantExecution is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 136, GloCla.ResMan.GetString("I11"), this.Name, Name);
        }
            
        /// <summary>
        /// Add the action to do in time of this point of plan realization
        /// </summary>
        /// <param name="Name">Execution's name</param>
        /// <param name="action">Action to do</param>
        /// <param name="WorkEithNewValues"><c>false</c> for realization before 'Effects also as execution', <c>true</c> for after these</param>
        public void AddExecution(string Name, Expression<Action> action, bool WorkEithNewValues)
        {
            this.Executions.Add(new ExpressionExecution(Name, action, WorkEithNewValues, null));
            if (!(InstantExecution is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 136, GloCla.ResMan.GetString("I11"), this.Name, Name);
        }

        /// <summary>
        /// Add the action to do in time of this point of plan realization
        /// </summary>
        /// <param name="Name">Execution's name</param>
        /// <param name="t1">Instance of class used in actionPDDL, the owner of parameter(s) used in this execution</param>
        /// <param name="action">Action of t1 class which will be called in this execution</param>
        /// <param name="WorkWithNewValues"><c>false</c> for realization before 'Effects also as execution', <c>true</c> for after these</param>
        public void AddExecution<T1>(string Name, ref T1 t1, Expression<Action<T1>> action, bool WorkWithNewValues)
        {
            this.Executions.Add(new ExpressionExecution(Name, action, WorkWithNewValues, new object[1] { t1 }));
            if (!(InstantExecution is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 136, GloCla.ResMan.GetString("I11"), this.Name, Name);
        }

        /// <summary>
        /// Add the action to do in time of this point of plan realization
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="t1">1st instance of class used in actionPDDL, the owner of parameter(s) used in this execution</param>
        /// <param name="t2">2nd instance of class used in actionPDDL, the owner of parameter(s) used in this execution</param>
        /// <param name="action">Action of t1 and t2 classes which will be called in this execution</param>
        /// <param name="WorkWithNewValues"><c>false</c> for realization before 'Effects also as execution', <c>true</c> for after these</param>
        public void AddExecution<T1,T2>(string Name, ref T1 t1, ref T2 t2, Expression<Action<T1, T2>> action, bool WorkWithNewValues)
        {
            this.Executions.Add(new ExpressionExecution(Name, action, WorkWithNewValues, new object[2] { t1, t2 }));
            if (!(InstantExecution is null))
                GloCla.Tracer?.TraceEvent(TraceEventType.Information, 136, GloCla.ResMan.GetString("I11"), this.Name, Name);
        }
        #endregion

        #region ActionCost
        public void DefineActionCost<T1>(ref T1 In1, Expression<Func<T1, int>> CostExpression)
            where T1 : class
            => actionCost.DefineActionCostF(ref In1, CostExpression);

        public void DefineActionCost<T1, T2>(ref T1 In1, ref T2 In2, Expression<Func<T1, T2, int>> CostExpression)
            where T1 : class
            where T2 : class
            => actionCost.DefineActionCostF(ref In1, ref In2, CostExpression);

        public void DefineActionCost<T1, T2, T3>(ref T1 In1, ref T2 In2, ref T3 In3, Expression<Func<T1, T2, T3, int>> CostExpression)
            where T1 : class
            where T2 : class
            where T3 : class
            => actionCost.DefineActionCostF(ref In1, ref In2, ref In3, CostExpression);

        public void DefineActionCost<T1, T2, T3, T4>(ref T1 In1, ref T2 In2, ref T3 In3, ref T4 In4, Expression<Func<T1, T2, T3, T4, int>> CostExpression)
             where T1 : class
             where T2 : class
             where T3 : class
             where T4 : class
            => actionCost.DefineActionCostF(ref In1, ref In2, ref In3, ref In4, CostExpression);
        #endregion

        internal void BuildAction(List<SingleTypeOfDomein> allTypes)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 48, GloCla.ResMan.GetString("Sa5"), Name);

            if (!Parameters.Any())
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 47, GloCla.ResMan.GetString("W4"), Name);
                return;
            }

            for (int i = 0; i != Parameters.Count(); i++)
                Parameters[i].Init1ArgPrecondition(allTypes, i);

            actionCost.BuildActionCost(allTypes, InstantActionParamCount);

            var PrecondidionExpressions = new List<Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>>>();
            foreach (PreconditionPDDL Precondition in Preconditions)
            {
                Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> ExpressionOfPrecondition = Precondition.BuildCheckPDDP(allTypes, Parameters);
                PrecondidionExpressions.Add(ExpressionOfPrecondition);
            }

            var EffectExpressions = new List<Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>>();
            foreach (EffectPDDL Effect in Effects)
            {
                Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>> ExpressionOfEffect = Effect.BuildEffectPDDP(allTypes, Parameters);
                EffectExpressions.Add(ExpressionOfEffect);
            }

            ActionLambdaPDDL actionLambdaPDDL = new ActionLambdaPDDL(Parameters, PrecondidionExpressions, EffectExpressions);
            InstantActionPDDL = actionLambdaPDDL.InstantFunct;

            //Generate correction of execution checker delegate
            InstantExecutionChecker = ActionChecker.ActionCheckerDel(this.Name, this.Effects, allTypes);

            List<EffectPDDL> EffectsUsingAsExecution = new List<EffectPDDL>();
            foreach (string EffectAsExecution in EffectsUsedAlsoAsExecution)
            {
                EffectPDDL UsedAlso;
                try
                {
                    UsedAlso = Effects.First(E => E.Name == EffectAsExecution);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                EffectsUsingAsExecution.Add(UsedAlso);
            }

            if (this.Executions.Any() || EffectsUsingAsExecution.Any())
            {
                WholeActionExecutionLambda p = new WholeActionExecutionLambda(this.Name, this.Parameters, this.Preconditions, EffectsUsingAsExecution, this.Executions);
                InstantExecution = p.InstantExecutionPDDL;
            }
            else
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 50, GloCla.ResMan.GetString("W5"), Name);
            }

            ActionSententiaLamdba actionSententiaLamdba = new ActionSententiaLamdba(allTypes, Parameters, ActionSententia);
            InstantActionSententia = actionSententiaLamdba.InstantFunct;

            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 49, GloCla.ResMan.GetString("Sp5"), Name);
        }

        internal void ClearActionDelegates()
        {
            InstantActionPDDL = null;
            InstantExecution = null;
            InstantExecutionChecker = null;
            InstantActionSententia = null;
            actionCost.CostExpressionFunc = null;
        }

        /// <summary>
        /// A class representing a possible action within the domain.<br/>
        /// Conditions necessary for its execution and effects must be added to the action. Then join to the domain.<br/>
        /// <example><para>
        /// For example:
        /// <code>
        /// DomeinPDDL domeinPDDL = new DomeinPDDL("domein name");<br/>
        /// ActionPDDL actionPDDL = new ActionPDDL("action name");<br/>
        /// actionPDDL.AddPrecondiction(...);<br/>
        /// actionPDDL.AddEffect(...);<br/>
        /// domeinPDDL.AddAction(actionPDDL);<br/>
        /// </code>
        /// </para></example>
        /// </summary>
        /// <param name="Name">Unique, non-empty action name</param>
        /// <param name="ActionCost">Resource consumption of the action, for example execution time. The smaller it is, the better it is to trigger the action</param>
        public ActionPDDL(string Name, uint actionCost = 1/*, bool IsSpecial = false*/)
        {
            if (String.IsNullOrEmpty(Name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 51, GloCla.ResMan.GetString("E10"));
                throw new Exception(GloCla.ResMan.GetString("E10"));
            }

            if (Name.StartsWith(GloCla.SpecialFuncPrefix))
            {
                do
                    Name = Name.Substring(1);
                while
                    (!Name.StartsWith(GloCla.SpecialFuncPrefix));

                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 52, GloCla.ResMan.GetString("W6"), GloCla.SpecialFuncPrefix, Name);
            }

            //this.Name = IsSpecial ? ExtensionMethods.SpecialFuncPrefix + Name : Name;
            this.Name = Name;
            this.Parameters = new List<Parametr>();
            this.Preconditions = new List<PreconditionPDDL>();
            this.Effects = new List<EffectPDDL>();
            this.EffectsUsedAlsoAsExecution = new List<string>();
            this.Executions = new List<ExpressionExecution>();
            this.ActionSententia = new List<(int, string, Expression[])>();
            this.actionCost = new ActionCost(actionCost);

            ClearActionDelegates();
        }
    }
}