using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Generators
{
    internal class GenerationReflection : IGeneration
    {

        public T GetObject<T>(Type nameOfClass)
        {

            var instance = Activator.CreateInstance(nameOfClass);

            foreach (var propertyInfo in nameOfClass.GetProperties())
            {
                var value = RandomUtils.GetRandomObject(propertyInfo.PropertyType);
                propertyInfo.SetValue(instance, value, null);
            }

            return (T)instance;
        }
    }

    internal class GenerationEmit : IGeneration
    {


        public T GetObject<T>(Type nameOfClass)
        {
            var aName = new AssemblyName("DynamicAssemblyExample");
            var ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave);

            var mb =
                ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            var tb = mb.DefineType(
            "yDynamicType",
             TypeAttributes.Public);

            var properties = nameOfClass.GetProperties();
            var propertyBuilders = properties.Select(propertyInfo =>
                tb.DefineProperty(propertyInfo.Name, propertyInfo.Attributes,
                    propertyInfo.PropertyType, propertyInfo.GetRequiredCustomModifiers())).ToList();
           
            foreach (var t1 in propertyBuilders)
            {
//  var ctorIL = ctors[0].GetILGenerator();
                var getSetAttr = MethodAttributes.Public |
                                 MethodAttributes.SpecialName | MethodAttributes.HideBySig;
                var mbNumberSetAccessor = tb.DefineMethod(
                    "set",
                    getSetAttr,
                    null,
                    new[] { t1.PropertyType });

                var setIl = mbNumberSetAccessor.GetILGenerator();
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Ldarg_1);
                setIl.Emit(OpCodes.Stfld, t1.PropertyType);
                setIl.Emit(OpCodes.Ret);
              
                t1.SetSetMethod(mbNumberSetAccessor);
            }
         
            var t = tb.CreateType();

            ab.Save(aName.Name + ".dll");

            var obj = Activator.CreateInstance(t);
            foreach (var property in properties)
            {
                property.SetValue(obj, RandomUtils.GetRandomObject(property.PropertyType));
            }

            return (T)obj;

        }
    }

    class GenerationExpressions : IGeneration
    {
        private readonly Dictionary<Type, Func<object>> typeMapping
            = new Dictionary<Type, Func<object>>();

        public T GetObject<T>(string nameOfClass)
        {
            return GetObject<T>(Type.GetType(nameOfClass));
        }

        public T GetObject<T>(Type classOfObject)
        {
            if (!typeMapping.ContainsKey(classOfObject))
            {
                typeMapping[classOfObject] = GetInitObjectLambda(classOfObject).Compile();
            }

            return (T)typeMapping[classOfObject]();
        }

        private Expression<Func<object>> GetInitObjectLambda(Type classOfObject)
        {
            var newObject =
                Expression.New(classOfObject);

            var memberInfos = classOfObject.GetMembers();
            var memberBindings = memberInfos.Select(memberInfo =>
                Expression.Bind(
                    memberInfo,
                    Expression.Constant(
                        RandomUtils.GetRandomObject(
                            memberInfo.GetType())))).Cast<MemberBinding>().ToList();

            var memberInitExpression = Expression.MemberInit(newObject, memberBindings);
            var lambda = Expression.Lambda<Func<object>>(memberInitExpression);
            return lambda;
        }
    }


}
