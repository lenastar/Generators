using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Generators.Expressions
{
    //TODO: Сделать GetConstructor у ЛЮБОГО КЛАССА, чтобы передавать туда аргументы на языке Expressions 
    public class ValueType<T>
    {
        private static Func<object, List<Tuple<string, object>>> getPropertiesValues;

        static ValueType()
        {
            getPropertiesValues = GetPropertiesValues;
        }

        public override string ToString()
        {

            var properties = getPropertiesValues(this)
                .OrderBy(x => x.Item1)
                .Select(tuple => $"{tuple.Item1}: {tuple.Item2}");

            return typeof(T).Name + '(' + string.Join("; ", properties) + ')';
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return getPropertiesValues(this)
                    .Aggregate(0, (x, y) => x.GetHashCode() * 5 ^ y.GetHashCode() * 3);
            }
        }

        public override bool Equals(object obj)
        {
            var objProperties = getPropertiesValues(obj);
            var thisProperties = getPropertiesValues(this);

            return objProperties
                .Zip(thisProperties, (x, y) => x.Equals(y))
                .All(x => x);
        }

        public bool Equals(T value)
        {
            return value != null && value.Equals(this);
        }

        private static List<Tuple<string, object>> GetPropertiesValues(object obj)
        {
            //Func<obj, lisTuple<string,object>>> getPV =
            //   p => Tuple(p.Name,p.GetValue(obj))

            var type = typeof(T);
            var info = Expression.Parameter(typeof(string), "info");
            var context = Expression.Parameter(typeof(string), "context");
        //    var newExpression = Expression.New(typeof(T).GetConstructor(new[] {typeof(string), typeof(string)}));
            var param = Expression.Parameter(type, "p");
         //   var list = new List<MemberBinding>();
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
                        typeof(object)
                    },
                    new Expression[]
                    {
                        Expression.Parameter(typeof(string), "property name"),
                        GetValueExpression(propertyInfo)
                    }
                );
                    expressions.Add(call);
            }
            var ex = Expression.Lambda<Func<object, List<Tuple<string, object>>>>(
                Expression.Block(expressions),
                new[] { param });
            var resFunc = ex.Compile();

            return resFunc(obj);
            //  return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            //      .Select(x => Tuple.Create(x.Name, x.GetValue(obj)))
            //      .ToList();
        }
        
        private static MethodCallExpression GetValueExpression(PropertyInfo propertyInfo)
        {
            //TODO: System.REflection.PropertyInfo не имеет метода getValue???
            return Expression.Call(typeof(PropertyInfo),
                "GetValue",
                new[]
                {
                    typeof(object)
                },
                new Expression[]
                {
                    Expression.Parameter(typeof(object),"value")
                });
        }
    }
}