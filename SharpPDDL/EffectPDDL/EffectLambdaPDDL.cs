using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{ 
    internal class EffectLambdaPDDL : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> _parameters;
        private ReadOnlyCollection<ParameterExpression> OldParameters;
        public Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> ModifiedFunct;
        private readonly int[] ParamsIndexesInAction;
        readonly List<SingleTypeOfDomein> allTypes;
        readonly Expression FuncOutKey = null;

        public EffectLambdaPDDL(List<SingleTypeOfDomein> allTypes, int[] paramsIndexesInAction, ushort funcOutKey)
        {
            if (allTypes is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C10"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 70, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!allTypes.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C11"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 71, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (paramsIndexesInAction is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C12"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 72, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!paramsIndexesInAction.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C13"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 73, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            this.allTypes = allTypes;
            this.ParamsIndexesInAction = paramsIndexesInAction;
            this.FuncOutKey = Expression.Constant(funcOutKey, typeof(ushort));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            OldParameters = node.Parameters;
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");

            //the library use only 1- or 2-Parameter lambdas
            if (OldParameters.Count() > 2)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E13"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 74, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            //make 2-Parameters lambda
            else if (OldParameters.Count() == 1)
            {
                Collection<ParameterExpression> parameterExpressions;

                if (ParamsIndexesInAction[0] != ParamsIndexesInAction[1])
                {
                    parameterExpressions = new Collection<ParameterExpression>
                    {
                        Expression.Parameter(typeof(PossibleStateThumbnailObject), GloCla.LamdbaParamPrefix + ParamsIndexesInAction[0]),
                        Expression.Parameter(typeof(PossibleStateThumbnailObject), GloCla.LamdbaParamPrefix + ParamsIndexesInAction[1])
                    };
                }
                else
                {
                    parameterExpressions = new Collection<ParameterExpression>
                    {
                        Expression.Parameter(typeof(PossibleStateThumbnailObject), GloCla.LamdbaParamPrefix + ParamsIndexesInAction[0]),
                        Expression.Parameter(typeof(PossibleStateThumbnailObject), "empty")
                    };
                }

                _parameters = new ReadOnlyCollection<ParameterExpression>(parameterExpressions);

                Collection<ParameterExpression> OldParameterExpressions = new Collection<ParameterExpression>
                    {
                        Expression.Parameter(typeof(PossibleStateThumbnailObject), "empty"),
                        OldParameters[0]
                    };
                OldParameters = new ReadOnlyCollection<ParameterExpression>(OldParameterExpressions);
            }

            ConstructorInfo ResultType = typeof(KeyValuePair<ushort, ValueType>).GetConstructors()[0];
            Expression[] param = { FuncOutKey, Visit(node.Body) };
            NewExpression expectedTypeExpression = Expression.New(ResultType, param);
            ModifiedFunct = Expression.Lambda<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>(expectedTypeExpression, _parameters);

            try
            {
                _ = ModifiedFunct.Compile();
            }
            catch (Exception e)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C14"), e.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 75, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            return ModifiedFunct;
        }

        private string NewParamName(string OldNodeName)
        {
            ParameterExpression param = OldParameters.First(p => p.Name == OldNodeName);
            int index = OldParameters.IndexOf(param);

            return GloCla.LamdbaParamPrefix + ParamsIndexesInAction[index];
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            string NewParamname = NewParamName(node.Name);
            return Expression.Parameter(typeof(PossibleStateThumbnailObject), NewParamname);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            return node.Update(left, VisitAndConvert(node.Conversion, "VisitBinary"), right);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant)
                return node;

            //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example string("Parameter") 
            string memberExpressionName = node.Expression.ToString();

            string newParamName = NewParamName(memberExpressionName);
            ParameterExpression newParam = _parameters.First(p => p.Name == newParamName);

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            //intersect
            SingleTypeOfDomein ParameterModel = allTypes.Where(t => t.Type == node.Expression.Type)?.First();

            if (ParameterModel is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C15"), node.Expression.Type.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 76, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            ///TODO in next versipn
            /*var Inter = l.Select(el => (el, el.Item2.Interfaces.Count()))?.Where(k => k.Item2 != 0)?.OrderBy(k => k.Item2).Select(k => k.el).ToList();
            for (int a=0; a!=Inter.Count(); ++a)
            {
                Inter[a].p.Value
            }*/
            ///in next version

            var ValueOfIndexes = ParameterModel.CumulativeValues.Where(v => v.Name == MemberName).First();
            ushort ValueOfIndexesKey = ValueOfIndexes.ValueOfIndexesKey;
            Expression[] argument = new[] { Expression.Constant(ValueOfIndexesKey) };

            //Property of ThumbnailObject.this[uint key]
            PropertyInfo TO_indekser = typeof(PossibleStateThumbnailObject).GetProperty("Item");

            //To expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
            IndexExpression IndexAccessExpr;

            //Some kind of micro-optimalization :)
            //If the value of variable is change in athers possible state take the value 
            if (ValueOfIndexes.IsInUse_EffectOut)
            {
                IndexAccessExpr = Expression.MakeIndex(newParam, TO_indekser, argument);
            }
            //In the other case take the value from precursor
            else
            {
                PropertyInfo PrecursorPropertyInfo = typeof(PossibleStateThumbnailObject).GetProperty("Precursor");
                Expression PrecursorAccessExpression = Expression.MakeMemberAccess(newParam, PrecursorPropertyInfo);
                IndexAccessExpr = Expression.MakeIndex(PrecursorAccessExpression, TO_indekser, argument);
            }

            //Convert above expression from ValueType to particular type of frontal value
            return Expression.Convert(IndexAccessExpr, node.Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsStatic)
                return node;

            string ExceptionMess = String.Format(GloCla.ResMan.GetString("C16"), node.Method.Name);
            GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 77, ExceptionMess);
            throw new Exception(ExceptionMess);
        }
    }
}