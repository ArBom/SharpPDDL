using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SharpPDDL
{
    class MemberofLambdaListerPDDL : ExpressionVisitor
    {
        internal ReadOnlyCollection<ParameterExpression> _parameters;
        internal List<string>[] used;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");

            switch (_parameters.Count())
            {
                case 1:
                    {
                        used = new List<string>[1];
                        used[0] = new List<string>();
                        break;
                    }

                case 2:
                    {
                        used = new List<string>[2];
                        used[0] = new List<string>();
                        used[1] = new List<string>();
                        break;
                    }

                case 3:
                    {
                        used = new List<string>[3];
                        used[0] = new List<string>();
                        used[1] = new List<string>();
                        used[2] = new List<string>();
                        break;
                    }

                case 4:
                    {
                        used = new List<string>[4];
                        used[0] = new List<string>();
                        used[1] = new List<string>();
                        used[2] = new List<string>();
                        used[3] = new List<string>();
                        break;
                    }

                default:
                    {
                        throw new Exception();
                    }
            }

            Visit(node.Body);
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example string("Parameter") 
            string memberExpressionName = node.Expression.ToString();

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            //Find parameter with memberExpressionName name...
            ParameterExpression t = _parameters.First(p => p.Name == memberExpressionName);

            if (t is null)
            {
                //there is no argument -> something went wrong
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 17, GloCla.ResMan.GetString("C1"), memberExpressionName, node.ToString());
                throw new Exception(GloCla.ResMan.GetString("C1"));
            }

            //...take index of it from all _parameters list...
            int index = _parameters.IndexOf(t);

            //...check is it already added...
            if (!used[index].Contains(MemberName))
                //...if not add it.
                used[index].Add(MemberName);

            return node;
        }
    }
}
