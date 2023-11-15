using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal class ThumbnailObLambdaModif : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> _parameters;
        public List<string>[] used;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            used = new List<string>[2];
            used[0] = new List<string>();
            used[1] = new List<string>();

            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");

            if (_parameters.Count() == 0)
            {
                //its no sens; its always true or always false
                throw new Exception();
            }

            if (_parameters.Count() == 1)
            {
                string NameOfNewOne = _parameters.First().Name == "null" ? "null2" : "null";
                List<ParameterExpression> parameters = _parameters.ToList<ParameterExpression>();
                parameters.Add(Expression.Parameter(typeof(ThumbnailObject), NameOfNewOne));
                _parameters = parameters.AsReadOnly();
            }

            var ret = Expression.Lambda(Visit(node.Body), _parameters);
            Func<ThumbnailObject, ThumbnailObject, bool> zwrotka = ret.Compile() as Func<ThumbnailObject, ThumbnailObject, bool>;

            return ret;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var a = Expression.Parameter(typeof(ThumbnailObject), node.Name);
            return a;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            return node.Update(left, VisitAndConvert(node.Conversion, "VisitBinary"), right);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example "Parameter" 
            ParameterExpression parameterExpression = Expression.Parameter(typeof(ThumbnailObject), node.Expression.ToString());

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            //adding expression in use to the list using value
            //check is it use in 0th parameter...
            if (parameterExpression.Name == _parameters[0].Name)
            {
                //...if so check is it already added...
                if (!used[0].Contains(MemberName))
                    //...if not add it.
                    used[0].Add(MemberName);
            }
            //check is it use in 1th parameter
            else if (parameterExpression.Name == _parameters[1].Name)
            {
                //...if so check is it already added...
                if (!used[1].Contains(MemberName))
                    //...if not add it.
                    used[1].Add(MemberName);
            }
            else
                //there is no more arguments -> something went wrong
                throw new Exception();

            //MethodInfo GetValue_from_ThumbnailObject_MethodInfo = typeof(ThumbnailObject).GetMethod("getValue"); //<- to jest nullem po wykonaniu
            //var b = BindingFlags.NonPublic | BindingFlags.Instance;




            ParameterModifier parameterModifier = new ParameterModifier(0);
            

            ConstantExpression dictKeyConstant = Expression.Constant(MemberName);




            //var pok = Expression.MakeIndex(parameterExpression, typeof(ThumbnailObject).GetProperty(""), new[] { dictKeyConstant });

            //var c = new Type[] { str };

            //MethodInfo GetValue_from_ThumbnailObject_MethodInfo = typeof(ThumbnailObject).GetMethod("getValue", c);
            //Expression constantExpression = Expression.Constant(MemberName);
            //MethodCallExpression.Property()
            //MethodCallExpression a = MethodCallExpression.Call(GetValue_from_ThumbnailObject_MethodInfo, constantExpression);

            var dicti = MemberExpression.PropertyOrField(parameterExpression, "Dict");
            Type dictionaryType = typeof(ThumbnailObject).GetField("Dict").FieldType;

            PropertyInfo indexerProp = dictionaryType.GetProperty("Item");
            
            IndexExpression dictAccess = Expression.MakeIndex(dicti, indexerProp, new[] { dictKeyConstant });
            Expression dictAccess2 = Expression.Convert(dictAccess, node.Type);



            return dictAccess2;
            //typeof(list<string>).getproperty("item")
            //var dictKeyConstant = Expression.Constant(MemberName);
           
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new Exception("You cannot to use object method call to create model of object. Try to write this method (" + node.ToString() + ")as new lambda which uses only ValueType member(s) of object.");
        }
    }
}