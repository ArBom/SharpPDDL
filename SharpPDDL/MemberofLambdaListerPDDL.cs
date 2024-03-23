using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
                //there is no argument -> something went wrong
                throw new Exception();

            //...take index of it from all _parameters list...
            int index = _parameters.IndexOf(t);

            //...check is it already added...
            if (!used[index].Contains(MemberName))
                //...if not add it.
                used[index].Add(MemberName);

            return node;

            //adding expression in use to the list using value and take new parameter
            //check is it use in 0th parameter...
            if (memberExpressionName == _parameters[0].Name)
            {
                //...if so check is it already added...
                if (!used[0].Contains(MemberName))
                    //...if not add it.
                    used[0].Add(MemberName);
            }
            //check is it use in 1th parameter
            else if (memberExpressionName == _parameters[1].Name)                                                           //TODO tutaj może się odwoływać poza tablice!!!!
            {
                //...if so check is it already added...
                if (!used[1].Contains(MemberName))
                    //...if not add it.
                    used[1].Add(MemberName);
            }
            else
                return node;


        }
    }
}
