using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static Generators.RandomUtils;

namespace Generators
{
    public class GenerationReflection : IGeneration
    {

        public T GetObject<T>(Type nameOfClass)
        {

            var instance = Activator.CreateInstance(nameOfClass);

            foreach (var propertyInfo in nameOfClass.GetProperties())
            {
                var value = GetRandomObject(propertyInfo.PropertyType);
                propertyInfo.SetValue(instance, value, null);
            }

            return (T)instance;
        }
    }

    public class GenerationEmit : IGeneration
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
            nameOfClass.ToString(),
             TypeAttributes.Public);

            var properties = nameOfClass.GetProperties();
            var propertyBuilders = properties.Select(propertyInfo =>
                tb.DefineProperty(propertyInfo.Name,propertyInfo.Attributes,propertyInfo.PropertyType,null)).ToList();
           
            foreach (var t1 in propertyBuilders)
            {
//  var ctorIL = ctors[0].GetILGenerator();
                var getSetAttr = MethodAttributes.Public |
                                 MethodAttributes.SpecialName | MethodAttributes.HideBySig;
                var mbSetAccessor = tb.DefineMethod(
                    "set_value",
                    getSetAttr,
                    null,
                    new[] { t1.PropertyType });
         
                var setIl = mbSetAccessor.GetILGenerator();
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Ldarg_1);
                setIl.Emit(OpCodes.Stfld, t1.PropertyType);
                setIl.Emit(OpCodes.Ret);
              
                t1.SetSetMethod(mbSetAccessor);
            }
         
            var t = tb.CreateType();

            ab.Save(aName.Name + ".dll");

            var obj = Activator.CreateInstance(t);
          //  var obj = CreateObjectFactory(t);
            //PropertyInfo piInstance = obj.GetType().GetProperty("Sname");
            //piInstance.SetValue(obj,
            //    Convert.ChangeType(GetRandomObject(piInstance.PropertyType), piInstance.PropertyType));
            //var ty = Convert.ChangeType(GetRandomObject(properties[0].PropertyType), properties[0].PropertyType);
            var newProperties = obj
                .GetType()
                .GetProperties();
            foreach (var property in newProperties)
            {
                property.SetValue(obj, GetRandomObject(property.PropertyType));
            }

            return (T)obj;

        }
        //http://mironabramson.com/blog/post/2008/08/Fast-version-of-the-ActivatorCreateInstance-method-using-IL.aspx
        private static readonly Hashtable creatorCache = Hashtable.Synchronized(new Hashtable());
        private readonly static Type coType = typeof(CreateObject);
        public delegate object CreateObject();

        /// <summary>
        /// Create an object that will used as a 'factory' to the specified type T 
        /// <returns></returns>
        public static CreateObject CreateObjectFactory(Type val ) 
        {
            var t = val;
            var c = creatorCache[t] as CreateObject;
            if (c == null)
            {
                lock (creatorCache.SyncRoot)
                {
                    c = creatorCache[t] as CreateObject;
                    if (c != null)
                    {
                        return c;
                    }
                    DynamicMethod dynMethod = new DynamicMethod("DM$OBJ_FACTORY_" + t.Name, typeof(object), null, t);
                    ILGenerator ilGen = dynMethod.GetILGenerator();

                    ilGen.Emit(OpCodes.Newobj, t.GetConstructor(Type.EmptyTypes));
                    ilGen.Emit(OpCodes.Ret);
                    c = (CreateObject)dynMethod.CreateDelegate(coType);
                    creatorCache.Add(t, c);
                }
            }
            return c;
        }
    }

    public class GenerationExpressions : IGeneration
    {
        private readonly Dictionary<Type, Func<object>> typeMapping
            = new Dictionary<Type, Func<object>>();

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
            var properties = classOfObject
                .GetProperties()
                .ToArray();
            var memberBindings = properties
                .Select(t => 
                    Expression.Bind(t, Expression.Constant(Convert.ChangeType(GetRandomObject(t.PropertyType), t.PropertyType))))
                .Cast<MemberBinding>()
                .ToList();

            var memberInitExpression = Expression.MemberInit(newObject, memberBindings);
            var lambda = Expression.Lambda<Func<object>>(memberInitExpression);
            return lambda;
        }
    }


}
