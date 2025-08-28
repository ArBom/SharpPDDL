using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class PreconditionLambdaModif : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> _parameters;
        private ReadOnlyCollection<ParameterExpression> OldParameters;
        public Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> ModifeidLambda;
        private readonly int[] ParamsIndexesInAction;
        private readonly List<SingleTypeOfDomein> allTypes;

        public PreconditionLambdaModif(List<SingleTypeOfDomein> allTypes, int[] paramsIndexesInAction)
        {
            if (allTypes is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C34"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 114, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!allTypes.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C35"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 115, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (paramsIndexesInAction is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C36"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 116, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!paramsIndexesInAction.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C37"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 117, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            this.allTypes = allTypes;
            this.ParamsIndexesInAction = paramsIndexesInAction;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            OldParameters = node.Parameters;
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");
            int _parametersCount = _parameters.Count();

            switch (_parameters.Count())
            {
                case 0:
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 118, GloCla.ResMan.GetString("W8"));

                    List<ParameterExpression> parameters0 = new List<ParameterExpression>();
                    parameters0.Add(Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName + "1"));
                    parameters0.Add(Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName + "2"));
                    parameters0.Add(Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName + "3"));
                    _parameters = parameters0.AsReadOnly();

                    break;

                //nake 3-Parameters lambda from 1-Param lambda
                case 1:
                    List<ParameterExpression> parameters1 = _parameters.ToList<ParameterExpression>();
                    parameters1.Add(Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName + "2"));
                    parameters1.Add(Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName + "3"));
                    _parameters = parameters1.AsReadOnly();

                    break;

                //nake 3-Parameters lambda from 2-Param lambda
                case 2:
                    List<ParameterExpression> parameters2 = _parameters.ToList<ParameterExpression>();
                    parameters2.Add(Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName + "3"));
                    _parameters = parameters2.AsReadOnly();

                    break;

                //do nothing it's max
                case 3:                   
                    break;

                //It's bad
                default:
                    string ExceptionMess = String.Format(GloCla.ResMan.GetString("E33"));
                    GloCla.Tracer?.TraceEvent(TraceEventType.Error, 119, ExceptionMess);
                    throw new Exception(ExceptionMess);
            }

            Expression PrecoLambdaBody = Visit(node.Body);
            ModifeidLambda = Expression.Lambda<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>>(PrecoLambdaBody, _parameters);

            try
            {
                _ = ModifeidLambda.Compile();
            }
            catch (Exception e)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C38"), e.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 120, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            return ModifeidLambda;
        }

        private string NewParamName(string OldNodeName)
        {
            var param = OldParameters.First(p => p.Name == OldNodeName);
            int index = OldParameters.IndexOf(param);
            return GloCla.LamdbaParamPrefix + ParamsIndexesInAction[index];
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            string NewParamname = NewParamName(node.Name);
            return Expression.Parameter(typeof(ThumbnailObject), NewParamname);
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
            SingleTypeOfDomein ParameterModel = allTypes.Where(t => t.Type == node.Expression.Type).First();

            if(ParameterModel is null)
            {
                throw new Exception();
            }

            ///TODO in other version or never
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
            PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item");

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
                PropertyInfo PrecursorPropertyInfo = typeof(ThumbnailObject).GetProperty("Precursor");
                Expression PrecursorAccessExpression = Expression.MakeMemberAccess(newParam, PrecursorPropertyInfo);
                IndexAccessExpr = Expression.MakeIndex(PrecursorAccessExpression, TO_indekser, argument);
            }
           
            //Convert above expression from ValueType to particular type of frontal value
            return Expression.Convert(IndexAccessExpr, node.Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            string ExceptionMess = String.Format(GloCla.ResMan.GetString("C39"), node.ToString());
            GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 121, ExceptionMess);
            throw new Exception(ExceptionMess);
        }
    }
}