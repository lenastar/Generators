using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Exortech.NetReflector;

namespace Generators
{
    class GeneratorComparer<T> : IEqualityComparer<T>
    {
        //TODO: Create constructor to choose generator method 
        // GeneratorComparer()
        // {

        //  }
        bool IEqualityComparer<T>.Equals(T t1, T t2)
        {
            var objProperties = GetPropertiesValues(t1);
            var thisProperties = GetPropertiesValues(t2);

            return objProperties
                .Zip(thisProperties, (x, y) => x.Equals(y))
                .All(x => x);
        }

        private static IEnumerable<Tuple<string, object>> GetPropertiesValues(T obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(x => Tuple.Create(x.Name, x.GetValue(obj)))
                .ToList();
        }

        public static bool EqualsByExpressions(T t1, T t2)
        {
            //Func<x,y,bool> Equals = (x,y) => GetPropertiesvalues(x).Zip(GetPropValues(y),(k,l)=>k.Equals(l)).All(x=>x) 

            var value1 = Expression.Parameter(typeof(T), "t1");
            var value2 = Expression.Parameter(typeof(T), "t2");

            var type = typeof(GeneratorComparer<T>);

            var res = GetPropertiesValuesByExpressions(t1);
            Console.ReadKey();

            var GetProperties1 =
                Expression.Call(type.GetMethod("GetPropertiesValuesByExpressions",
                                    new[] { typeof(T) }),
            value1);
            var GetProperties2 =
                Expression.Call(type.GetMethod("GetPropertiesValuesByExpressions",
                                    new[] { typeof(T) }),
                    value2);

            var properties1 = Expression.Lambda<Func<T, List<Tuple<string, T>>>>(GetProperties1, value1).Compile()(t1);
            var properties2 = Expression.Lambda<Func<T, List<Tuple<string, T>>>>(GetProperties2, value2).Compile()(t2);

            var par1 = Expression.Parameter(typeof(List<Tuple<string, T>>), "properties1");
            var par2 = Expression.Parameter(typeof(List<Tuple<string, T>>), "properties2");
            //TODO:Create lammda for zip
            return properties1.Zip(properties2, (x, y) => x.Equals(y)).All(x => x);
        }

        private static IEnumerable<Tuple<string, object>> GetPropertiesValuesByExpressions(T obj)
        {
            //Func<obj, lisTuple<string,object>>> getPV =
            //   p => Tuple(p.Name,p.GetValue(obj))

            var param = Expression.Parameter(typeof(T), "p");
            var propertyInfos = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            //var expressions = propertyInfos.Select(GetTupleExpression).Cast<Expression>().ToList();
            Test();
            var ex = Expression.Lambda<Func<T, List<Tuple<string, object>>>>(
                Expression.Block(), param);
            return ex.Compile()(obj);
        }

        private static MethodCallExpression GetValueExpression(Expression instance)
        {
            return Expression.Call(
                instance,
                typeof(PropertyInfo).GetMethod("GetValue", new[] { typeof(object) }),
                Expression.Parameter(typeof(object), "Instance")
            );
        }

        private static void Test()
        {
            var address = new Address("asdasd", "qwer");
            var pr = address.GetType().GetProperties()[0];

            var getTuple = GetTupleExpression();
            var getValue = GetValueExpression(Expression.Constant(pr));

            var flambda = Expression.Lambda<Func<object, object>>(
                getValue,
                getValue.Arguments.Cast<ParameterExpression>()
            );
            var ffunc = flambda.Compile();
            var fres = ffunc(address);

            var slambda = Expression.Lambda<Func<string, object, Tuple<string, object>>>(
                              getTuple,
                              getTuple.Arguments.Cast<ParameterExpression>()
                          );

            var sfunc = slambda.Compile();
            var sres = sfunc("name", address);

            var block = Expression.Block(
                flambda.Parameters.Concat(slambda.Parameters),
                flambda,
                slambda
            );
            var tlambda = Expression.Lambda<Func<string, object, object>>(
                block,
                block.Variables
            );
            var tfunc = tlambda.Compile();
            var tres = tfunc("asd", address);

            var a = 1;

        }

        private static MethodCallExpression GetTupleExpression()
        {
            var methodInfo = typeof(Tuple)
                .GetMethods()
                .First(info => info.GetGenericArguments().Length == 2 && info.Name == "Create")
                .MakeGenericMethod(typeof(string), typeof(object));

            return Expression.Call(
                methodInfo,
                new Expression[]
                {
                    Expression.Parameter(typeof(string), "string parameter"),
                    Expression.Parameter(typeof(object), "object parameter")
                }
            );
        }

        public delegate bool EqualsByReflectionEmit(T x, T y);

        public static EqualsByReflectionEmit GenerateEqualsByReflectionEmit()
        {
            var dynMethod = new DynamicMethod("callme", typeof(bool), new Type[] { typeof(T), typeof(T) });
            var prm1 = dynMethod.DefineParameter(1, ParameterAttributes.In, "x");
            var prm2 = dynMethod.DefineParameter(2, ParameterAttributes.In, "y");
            GenerateEqualsMethodBody(dynMethod.GetILGenerator(), new[] { prm1, prm2 });

            return (EqualsByReflectionEmit)dynMethod.CreateDelegate(typeof(EqualsByReflectionEmit));

        }

        private static void GenerateEqualsMethodBody(ILGenerator gen, ParameterBuilder[] prm)
        {
            gen.Emit(OpCodes.Nop);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, typeof(GeneratorComparer<T>).GetMethod("GetPropertiesValues"));
            gen.Emit(OpCodes.Stloc_0);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Call, typeof(GeneratorComparer<T>).GetMethod("GetPropertiesValues"));
            gen.Emit(OpCodes.Stloc_1);

            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldsfld);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brtrue_S);
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Ldsfld);
            gen.Emit(OpCodes.Ldftn);
            gen.Emit(OpCodes.Newobj, typeof(Func<Tuple<string, object>, bool>).GetConstructor(new Type[0]));
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brtrue_S);
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Ldsfld);
            gen.Emit(OpCodes.Ldftn);
            gen.Emit(OpCodes.Newobj, typeof(Func<bool, bool>).GetConstructor(new Type[0]));
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Stsfld);
            gen.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("All"));
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Br_S);

            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ret);


        }

        public int GetHashCode(T x)
        {
            throw new NotImplementedException();
        }
    }
}
