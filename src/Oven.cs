using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace Oven
{
    public class Oven
    {
        private Oven()
        {
        }

        private static TypeBuilder CreateType(Type intf)
        {
            var implName = intf.Name + "Impl";

            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName(implName),
                    AssemblyBuilderAccess.Run);
            var moduleBuilder =
                assemblyBuilder.DefineDynamicModule("Module");
            var typeBuilder = moduleBuilder.DefineType(
                implName,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null,
                new Type[] { intf });

            return typeBuilder;
        }
        private static MethodBuilder CreateMethod(
            Type intf, Type impl,
            TypeBuilder typeBuilder, MethodInfo method)
        {
            var paramTypes =
                    method.GetParameters().Select(m => m.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Public |
                MethodAttributes.Virtual |
                MethodAttributes.NewSlot |
                MethodAttributes.HideBySig |
                MethodAttributes.Final,
                method.ReturnType,
                paramTypes);
            var ilGen = methodBuilder.GetILGenerator();

            /* args... -> object[] */
            int argc = 0;
            var args = ilGen.DeclareLocal(typeof(object[]));
            var typeInfo = ilGen.DeclareLocal(typeof(Type));
            var methodInfo = ilGen.DeclareLocal(typeof(MethodInfo));

            ilGen.Emit(OpCodes.Ldc_I4, paramTypes.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(object));
            ilGen.Emit(OpCodes.Stloc, args);

            foreach (var param in method.GetParameters())
            {
                ilGen.Emit(OpCodes.Ldloc, args);
                ilGen.Emit(OpCodes.Ldc_I4, argc);
                ilGen.Emit(OpCodes.Ldarg, argc + 1);
                if (paramTypes[argc].IsValueType)
                    ilGen.Emit(OpCodes.Box, paramTypes[argc]);
                ilGen.Emit(OpCodes.Stelem_Ref);

                argc++;
            }

            /* ld_this */
            ilGen.Emit(OpCodes.Ldarg_0);

            var getTypeFromHandle =
                typeof(Type).GetMethod("GetTypeFromHandle");
            var getMethodFromHandle =
                typeof(MethodBase).GetMethod(
                    "GetMethodFromHandle",
                    new[] { typeof(RuntimeMethodHandle) });
            ilGen.Emit(OpCodes.Ldtoken, intf);
            ilGen.Emit(OpCodes.Call, getTypeFromHandle);
            ilGen.Emit(OpCodes.Castclass, typeof(Type));
            ilGen.Emit(OpCodes.Ldtoken, method);
            ilGen.Emit(OpCodes.Call, getMethodFromHandle);
            ilGen.Emit(OpCodes.Castclass, typeof(MethodInfo));

            ilGen.Emit(OpCodes.Ldloc, args);

            /* performs proxy call */
            ilGen.Emit(
                OpCodes.Call,
                impl.GetMethod(
                    "OnMethod",
                    BindingFlags.Instance | BindingFlags.Public));
            /* return value of `RPCCall` will be automatically passed to caller,
               but it needs to be unboxed to original type before returning. */
            ilGen.Emit(OpCodes.Unbox, method.ReturnType);
            ilGen.Emit(OpCodes.Ldobj, method.ReturnType);
            ilGen.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(
                methodBuilder,
                intf.GetMethod(method.Name));

            return methodBuilder;
        }

        public static TBakeInterface Bake<TBakeInterface, TBakeImpl>()
            where TBakeImpl : IFilling
        {
            var typeBuilder = CreateType(typeof(TBakeInterface));

            /* black magic */
            foreach (var method in typeof(TBakeInterface).GetMethods())
            {
                CreateMethod(
                    typeof(TBakeInterface), typeof(TBakeImpl),
                    typeBuilder, method);
            }

            Type type = typeBuilder.CreateType();
            object obj = Activator.CreateInstance(type);

            return (TBakeInterface)obj;
        }
    }
}
