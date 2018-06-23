using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Generators
{
    class GenerationReflection : IGeneration
    {
        public Type GenerateClass(string name, Dictionary<string, Type> properties)
        {
            return null;
        }

        public T GetObject<T>(string nameOfClass)
        {
            
            var type = Type.GetType(nameOfClass, true);
            
            var instance = Activator.CreateInstance(type);

            foreach (var propertyInfo in type.GetProperties())
            {
                var value = RandomUtils.RandomUtilDictionary[propertyInfo.PropertyType]();
                propertyInfo.SetValue(instance, value, null);
            }

            return (T)instance;
        }
    }

    class GenerationEmit:IGeneration
    {

        private static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = (string)RandomUtils.RandomUtilDictionary[typeof(string)]();
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            var setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            var setIl = setPropMthdBldr.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
        public Type GenerateClass(string name, Dictionary<string, Type> properties)
        {
            var tb = GetTypeBuilder();
            var constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            foreach (var field in properties)
                CreateProperty(tb, field.Key, field.Value);

            var objectType = tb.CreateType();
            return objectType;
        }

        public T GetObject<T>(string nameOfClass)
        {
            throw new NotImplementedException();
            //var myType = GenerateClass(nameOfClass,);
            //return (T)Activator.CreateInstance(myType);
        }
    }


}
