using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace SharpPDDL
{    
    public class ActionPDDL
    {
        public readonly string Name;
        internal readonly uint ActionCost;
        private List<PreconditionPDDL> Preconditions; //warunki konieczne do wykonania
        private List<EffectPDDL> Effects; //efekty
        private List<Parametr> Parameters; //typy wykorzystywane w tej akcji (patrz powyzej)
        private List<(int, string, Expression[])> ActionSententia;
        private List<Execution> Executions;
        internal Delegate InstantActionPDDL { get; private set; }
        internal Delegate InstantActionSententia { get; private set; }
        internal int InstantActionParamCount { get; private set; }

        internal List<SingleType> TakeSingleTypes()
        {
            List<SingleType> ToRet = new List<SingleType>();

            foreach (Parametr parametr in Parameters)
            {
                parametr.RemoveUnuseValue();
                SingleType singleType = null;

                if (ToRet.Count != 0)
                {
                    var singleTypes = ToRet.Where(sT => sT.Type == parametr.Type);
                    if (singleTypes.Count() != 0)
                        singleType = singleTypes.First();
                }

                if (singleType is null)
                {
                    singleType = new SingleType(parametr.Type, parametr.values);
                    ToRet.Add(singleType);
                    continue;
                }

                foreach (ValueOfParametr valueP in parametr.values)
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

        public void AddUnassignedParametr<T>(out T destination, string Text = null, params Expression<Func<T, object>>[] TextParams) where T : class
        {
            destination = (T)FormatterServices.GetUninitializedObject(typeof(T));
            AddParameter(ref destination, Text, TextParams);
        }

        public void AddAssignedParametr<T>(ref T destination, string Text = null, params Expression<Func<T, object>>[] TextParams) where T : class
        {
            if (typeof(T).IsAbstract)
                throw new Exception("Sorry, You cannot to use abstract parameter at this version");

            if (destination is null)
                destination = (T)FormatterServices.GetUninitializedObject(typeof(T));

            Int32 HashCode = destination.GetHashCode();
            AddParameter(ref destination, Text, TextParams);
        }

        internal void AddParameter<T>(ref T destination, string Text, Expression<Func<T, object>>[] TextParams) where T : class
        {
            Int32 HashCode = destination.GetHashCode();

            if (Parameters.Any(t => t.HashCode == HashCode))
            {
                Parametr p = Parameters.Where(t => t.HashCode == HashCode).First();
                if (p.Oryginal.Equals(destination))
                    return;
            }

            if (!String.IsNullOrEmpty(Text))
                ActionSententia.Add((Parameters.Count, Text, TextParams));

            Parametr TempParametr = new Parametr(HashCode, destination);
            Parameters.Add(TempParametr);
        }

        private void CheckExistEffectName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Effects.Exists(effect => effect.Name == Name))
                throw new Exception(); //juz istnieje efekt o takiej nazwie
        }

        private void CheckExistPreconditionName(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or empty

            if (this.Preconditions.Exists(precondition => precondition.Name == Name))
                throw new Exception(); //juz istnieje warunek poczatkowy o takiej nazwie
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
        public void AddPrecondiction<T1>(string Name, ref T1 obj, Expression<Predicate<T1>> func) where T1 : class => AddPrecondiction<T1, T1>(Name, ref obj, func);

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
            CheckExistPreconditionName(Name);
            this.AddAssignedParametr(ref obj);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj, func);

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != obj.GetHashCode())
                    continue;

                if (!(parametr.Oryginal.Equals(obj)))
                    continue;

                foreach (string valueName in temp.usedMembers1Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse = true;
                    parametr.values[ToTagIndex].IsInUse_PreconditionIn = true;
                }

                parametr.UsedInPrecondition = true;
                break;
            }

            Preconditions.Add(temp);
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
        {
            CheckExistPreconditionName(Name);
            this.AddAssignedParametr(ref obj1);
            this.AddAssignedParametr(ref obj2);
            PreconditionPDDL temp = PreconditionPDDL.Instance(Name, ref obj1, ref obj2, func);

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != obj1.GetHashCode())
                    continue;

                if (!(parametr.Oryginal.Equals(obj1)))
                    continue;

                foreach (string valueName in temp.usedMembers1Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse = true;
                    parametr.values[ToTagIndex].IsInUse_PreconditionIn = true;
                }

                parametr.UsedInPrecondition = true;
                break;
                    
            }

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != obj2.GetHashCode())
                    continue;

                if (!(parametr.Oryginal.Equals(obj2)))
                    continue;

                foreach (string valueName in temp.usedMembers2Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse = true;
                    parametr.values[ToTagIndex].IsInUse_PreconditionIn = true;
                }

                parametr.UsedInPrecondition = true;
                    break;

            }

            Preconditions.Add(temp);
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
        public EffectPDDL AddEffect<T>(string Name, ValueType newValue_Static, ref T destinationObj, Expression<Func<T, ValueType>> destinationMember) where T : class 
        {
            CheckExistEffectName(Name);
            this.AddAssignedParametr(ref destinationObj);
            EffectPDDL temp = EffectPDDL.Instance(Name, newValue_Static, ref destinationObj, destinationMember);

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != destinationObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(destinationObj))
                    continue;

                int ToTagIndex = parametr.values.FindIndex(v => v.Name == temp.DestinationMemberName);
                parametr.values[ToTagIndex].IsInUse = true;
                parametr.values[ToTagIndex].IsInUse_EffectOut = true;

                parametr.UsedInEffect = true;
                break;
            }

            Effects.Add(temp);
            return temp;
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
        public EffectPDDL AddEffect<T1c, T1p, T2c, T2p>(string Name, ref T1c SourceObj, Expression<Func<T1p, ValueType>> Source, ref T2c DestinationObj, Expression<Func<T2p, ValueType>> DestinationMember)
            where T1p : class
            where T2p : class
            where T1c : class, T1p
            where T2c : class, T2p
        {
            CheckExistEffectName(Name);
            this.AddAssignedParametr(ref SourceObj);
            this.AddAssignedParametr(ref DestinationObj);
            EffectPDDL temp = EffectPDDL.Instance(Name, ref SourceObj, Source, ref DestinationObj, DestinationMember);

            //Tag destination parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != DestinationObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(DestinationObj))
                    continue;


                int ToTagIndex = parametr.values.FindIndex(v => v.Name == temp.DestinationMemberName);
                parametr.values[ToTagIndex].IsInUse = true;
                parametr.values[ToTagIndex].IsInUse_EffectOut = true;

                parametr.UsedInEffect = true;
                break;
            }

            //Tag source parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != SourceObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(SourceObj))
                    continue;

                foreach (string valueName in temp.usedMembers1Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse = true;
                    parametr.values[ToTagIndex].IsInUse_EffectIn = true;
                }

                parametr.UsedInEffect = true;
                break;
            }

            Effects.Add(temp);
            return temp;
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
        public EffectPDDL AddEffect<T1, T2>(string Name, ref T1 SourceObj, Expression<Func<T1, ValueType>> Source, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationMember)
            where T1 : class
            where T2 : class
            =>
            AddEffect<T1, T1, T2, T2>(Name, ref SourceObj, Source, ref DestinationObj, DestinationMember);

        public EffectPDDL AddEffect<T1, T2>(string Name, ref T1 SourceObj, Expression<Func<T1, T2, ValueType>> SourceFunct, ref T2 DestinationObj, Expression<Func<T2, ValueType>> DestinationFunct) 
            where T1 : class 
            where T2 : class
        {
            CheckExistEffectName(Name);
            this.AddAssignedParametr(ref SourceObj);
            this.AddAssignedParametr(ref DestinationObj);
            EffectPDDL temp = EffectPDDL.Instance(Name, ref SourceObj, SourceFunct, ref DestinationObj, DestinationFunct);

            //Tag destination parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != DestinationObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(DestinationObj))
                    continue;

                foreach (string valueName in temp.usedMembers1Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse = true;
                    parametr.values[ToTagIndex].IsInUse_EffectIn = true;
                }

                parametr.UsedInEffect = true;
                break;
            }

            //Tak source parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != SourceObj.GetHashCode())
                    continue;

                if (!parametr.Oryginal.Equals(SourceObj))
                    continue;

                foreach (string valueName in temp.usedMembers2Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse = true;
                    parametr.values[ToTagIndex].IsInUse_EffectIn = true;
                }

                parametr.UsedInEffect = true;
                break;
            }

            Effects.Add(temp);
            return temp;
        }
        #endregion

        internal void BuildAction(List<SingleTypeOfDomein> allTypes)
        {
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

            InstantActionParamCount = Parameters.Count;

            ActionLambdaPDDL actionLambdaPDDL = new ActionLambdaPDDL(Parameters, PrecondidionExpressions, EffectExpressions);
            InstantActionPDDL = actionLambdaPDDL.InstantFunct;

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
        public ActionPDDL(string Name, uint ActionCost = 1)
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception(); //is null or emty

            //TODO zadbać o unikalność nazw

            this.ActionCost = ActionCost;
            this.Name = Name;
            this.Parameters = new List<Parametr>();
            this.Preconditions = new List<PreconditionPDDL>();
            this.Effects = new List<EffectPDDL>();
            this.Executions = new List<Execution>();
            this.ActionSententia = new List<(int, string, Expression[])>();
        }
    }
}
