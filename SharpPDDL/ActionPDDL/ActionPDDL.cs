using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    public class ActionPDDL
    {
        public readonly string Name;
        private List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private List<EffectPDDL> Effects; //efekty
        private List<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)
        private List<(int, string, Expression[])> ActionSententia;
        internal ActionCost actionCost;
        private List<string> EffectsUsedAlsoAsExecution;
        private List<ExpressionExecution> Executions;
        internal Delegate InstantActionPDDL { get; private set; }
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

        public void AddPartOfActionSententia<T>(ref T destination, string Text, params Expression<Func<T, object>>[] TextParams) where T : class
        {
            Parametr.GetTheInstance_TryAddToList(Parameters, ref destination);

            if (!String.IsNullOrEmpty(Text))
            {
                int DestHashCode = destination.GetHashCode();
                Parametr p = Parameters.Where(t => t.HashCode == DestHashCode).First();
                int index = Parameters.IndexOf(p);
                ActionSententia.Add((index, Text, TextParams));
            }
            else
            {
                //TODO jakiś komentarz w sprawie
            }
        }

        private void CheckExistEffectName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Effects.Exists(effect => effect.Name == Name))
                throw new Exception(); //juz istnieje efekt o takiej nazwie
        }

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
            => _ = PreconditionPDDL.Instance(Name, Parameters, Preconditions, ref obj, func);

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
        /// <typeparam name="T2c">Non-abstract class of 1st param inheriting from T2p</typeparam>
        /// <typeparam name="T2p">Non-abstract class of 1st param inherited by T2c</typeparam>
        /// <param name="Name">Unique (on a scale of action), non-empty precondition name</param>
        /// <param name="obj1">Instance of T1c class (could be null) which representant 1st parameter of action</param>
        /// <param name="obj2">Instance of T2c class (could be null) which representant 2nd parameter of action</param>
        /// <param name="func">Predicate uses member(s) of T1p and T2p classes to check possibility of action ececute</param>
        public void AddPrecondiction<T1c, T1p, T2c, T2p>(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func)
            where T1p : class 
            where T2p : class 
            where T1c : class, T1p 
            where T2c : class, T2p
            => _ = PreconditionPDDL.Instance(Name, Parameters, Preconditions, ref obj1, ref obj2, func);
        
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
            => _ = EffectPDDL.Instance(Name, Parameters, Effects, ref destinationObj, destinationMember, newValue_Static);       

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
            => _ = EffectPDDL.Instance(Name, Parameters, Effects, ref DestinationObj, DestinationMember, ref SourceObj, Source);

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
        public void AddEffect<T1, T2>(string Name, ref T1 DestinationObj, Expression<Func<T1, ValueType>> DestinationMember, ref T2 SourceObj, Expression<Func<T2, ValueType>> Source)
            where T1 : class
            where T2 : class
            => AddEffect<T1, T1, T2, T2>(Name, ref DestinationObj, DestinationMember, ref SourceObj, Source);

        public void AddEffect<T1, T2>(string Name, ref T1 DestinationObj, Expression<Func<T1, ValueType>> DestinationFunct, ref T2 SourceObj, Expression<Func<T1, T2, ValueType>> SourceFunct)
            where T1 : class 
            where T2 : class
            => _ = EffectPDDL.Instance(Name, Parameters, Effects, ref DestinationObj, DestinationFunct, ref SourceObj, SourceFunct);
        #endregion
        #region Adding Execution
        public void UseEffectAlsoAsExecution(string ExecutionName) 
            => EffectsUsedAlsoAsExecution.Add(ExecutionName);       

        public void AddExecution(string Name, Expression<Action> action, bool WorkEithNewValues) 
            => this.Executions.Add(new ExpressionExecution(Name, action, WorkEithNewValues, null, 0));

        public void AddExecution<T1>(string Name, ref T1 t1, Expression<Action<T1>> action, bool WorkWithNewValues) 
            => this.Executions.Add(new ExpressionExecution<T1>(Name, ref t1, action, WorkWithNewValues));

        public void AddExecution<T1,T2>(string Name, ref T1 t1, ref T2 t2, Expression<Action<T1, T2>> action, bool WorkWithNewValues) 
            => this.Executions.Add(new ExpressionExecution<T1, T2>(Name, ref t1, ref t2, action, WorkWithNewValues));

        #endregion
        #region ActionCost
        public void DefineActionCost<T1>(ref T1 In1, Expression<Func<T1, int>> CostExpression)
            where T1 : class
            => this.actionCost = new ActionCost<T1>(ref In1, CostExpression, this.actionCost.defaultCost);

        public void DefineActionCost<T1, T2>(ref T1 In1, ref T2 In2, Expression<Func<T1, T2, int>> CostExpression)
            where T1 : class
            where T2 : class
            => this.actionCost = new ActionCost<T1, T2>(ref In1, ref In2, CostExpression, this.actionCost.defaultCost);

        public void DefineActionCost<T1, T2, T3>(ref T1 In1, ref T2 In2, ref T3 In3, Expression<Func<T1, T2, T3, int>> CostExpression)
            where T1 : class
            where T2 : class
            where T3 : class
            => this.actionCost = new ActionCost<T1, T2, T3>(ref In1, ref In2, ref In3, CostExpression, this.actionCost.defaultCost);

        public void DefineActionCost<T1, T2, T3, T4>(ref T1 In1, ref T2 In2, ref T3 In3, ref T4 In4, Expression<Func<T1, T2, T3, T4, int>> CostExpression)
             where T1 : class
             where T2 : class
             where T3 : class
             where T4 : class
             => this.actionCost = new ActionCost<T1, T2, T3, T4>(ref In1, ref In2, ref In3, ref In4, CostExpression, this.actionCost.defaultCost);
        #endregion

        internal void BuildAction(List<SingleTypeOfDomein> allTypes)
        {
            if (!Parameters.Any())
                return;

            actionCost.BuildActionCost(allTypes, InstantActionParamCount);

            var PrecondidionExpressions = new List<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>>>();
            foreach (PreconditionPDDL Precondition in Preconditions)
            {
                Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>> ExpressionOfPrecondition = Precondition.BuildCheckPDDP(allTypes, Parameters);
                PrecondidionExpressions.Add(ExpressionOfPrecondition);
            }

            var EffectExpressions = new List<Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>>();
            foreach (EffectPDDL Effect in Effects)
            {
                Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> ExpressionOfEffect = Effect.BuildEffectPDDP(allTypes, Parameters);
                EffectExpressions.Add(ExpressionOfEffect);
            }

            ActionLambdaPDDL actionLambdaPDDL = new ActionLambdaPDDL(Parameters, PrecondidionExpressions, EffectExpressions);
            InstantActionPDDL = actionLambdaPDDL.InstantFunct;

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
            EffectsUsedAlsoAsExecution = null;

            if (this.Executions.Any() || EffectsUsingAsExecution.Any())
            {
                WholeActionExecutionLambda p = new WholeActionExecutionLambda(this.Parameters, this.Preconditions, EffectsUsingAsExecution, this.Executions);
            }

            ActionSententiaLamdba actionSententiaLamdba = new ActionSententiaLamdba(allTypes, Parameters, ActionSententia);
            InstantActionSententia = actionSententiaLamdba.InstantFunct;
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
        public ActionPDDL(string Name, uint actionCost = 1)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or emty

            this.Name = Name;
            this.Parameters = new List<Parametr>();
            this.Preconditions = new List<PreconditionPDDL>();
            this.Effects = new List<EffectPDDL>();
            this.EffectsUsedAlsoAsExecution = new List<string>();
            this.Executions = new List<ExpressionExecution>();
            this.ActionSententia = new List<(int, string, Expression[])>();
            this.actionCost = new ActionCost(actionCost);
        }
    }
}