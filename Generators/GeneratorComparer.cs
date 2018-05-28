using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Generators
{
    class GeneratorComparer<T> : IEqualityComparer<T>
    {
        //TODO: Create constructor to choose generator method 
       // GeneratorComparer()
       // {

      //  }
        bool IEqualityComparer<T>.Equals(T t1,T t2)
        {
            var objProperties = GetPropertiesValues(t1);
            var thisProperties = GetPropertiesValues(t2);

            return objProperties
                .Zip(thisProperties, (x, y) => x.Equals(y))
                .All(x => x);
        }

        private List<Tuple<string, object>> GetPropertiesValues(T obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(x => Tuple.Create(x.Name, x.GetValue(obj)))
                .ToList();
        }

        public static bool EqualsByExpressions(T t1, T t2)
        {
            //Func<x,y,bool> Equals = (x,y) => GetPropertiesvalues(x).Zip(GetPropValues(y),(k,l)=>k.Equals(l)).All(x=>x) 
            ParameterExpression value1 = Expression.Parameter(typeof(T), "t1");
            ParameterExpression value2 = Expression.Parameter(typeof(T), "t2");
            Type type = typeof(GeneratorComparer<T>);
            MethodCallExpression GetProperties1 =
                Expression.Call(type.GetMethod("GetPropertiesValuesByExpressions",
                                    new[] { typeof(T) }),
            value1);
            MethodCallExpression GetProperties2 =
                Expression.Call(type.GetMethod("GetPropertiesValuesByExpressions",
                                    new[] {typeof(T)}),
                    value2);
            List<Tuple<string, T>> properties1 = Expression.Lambda<Func<T, List<Tuple<string, T>>>>(GetProperties1, value1).Compile()(t1);
            List<Tuple<string, T>> properties2 = Expression.Lambda<Func<T, List<Tuple<string, T>>>>(GetProperties2, value2).Compile()(t2);
            ParameterExpression par1 = Expression.Parameter(typeof(List<Tuple<string, T>>), "properties1");
            ParameterExpression par2 = Expression.Parameter(typeof(List<Tuple<string, T>>), "properties2");
            //TODO:Create lammda for zip
            return properties1.Zip(properties2, (x, y) => x.Equals(y)).All(x => x);




        }

        public static List<Tuple<string, T>> GetPropertiesValuesByExpressions(T obj)
        {
            //Func<obj, lisTuple<string,object>>> getPV =
            //   p => Tuple(p.Name,p.GetValue(obj))

            var type = typeof(T);
            var param = Expression.Parameter(type, "p");
            var propertyInfos = type.GetProperties(BindingFlags.Instance |
                                                   BindingFlags.Public);
            var expressions = new List<Expression>();
            foreach (var propertyInfo in propertyInfos)
            {
                Expression call = Expression.Call(
                    typeof(Tuple),
                    "Create",
                    new[]
                    {
                        typeof(string),
                        type
                    }, Expression.Parameter(typeof(string), "property name"), GetValueExpression(propertyInfo));
                expressions.Add(call);
            }
            var ex = Expression.Lambda<Func<T, List<Tuple<string, T>>>>(
                Expression.Block(expressions), param);
            return ex.Compile()(obj);
        }
        private static MethodCallExpression GetValueExpression(PropertyInfo propertyInfo)
        {
            return Expression.Call(typeof(PropertyInfo),
                "GetValue",
                new[]
                {
                    typeof(object)
                }, Expression.Parameter(typeof(object),"value"));
        }

        //public bool EqualsByReflectionEmit(T x, T y)
        //{

        //}
        public int GetHashCode(T x)
        {
            throw new NotImplementedException();
        }
    }
}
