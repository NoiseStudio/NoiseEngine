using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NoiseEngine.Jobs2;

internal static class ArchetypeColumnCreator {

    private static readonly ModuleBuilder moduleBuilder;
    private static readonly Type wrapper;
    private static nuint index;

    static ArchetypeColumnCreator() {
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(typeof(ArchetypeColumnCreator).FullName ?? nameof(ArchetypeColumnCreator)),
            AssemblyBuilderAccess.RunAndCollect
        );

        moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");

        TypeBuilder typeBuilder = moduleBuilder.DefineType("Wrapper", TypeAttributes.SequentialLayout);
        GenericTypeParameterBuilder generic = typeBuilder.DefineGenericParameters("T")[0];
        typeBuilder.DefineField("v", generic, FieldAttributes.Private);
        wrapper = typeBuilder.CreateType();
    }

    public static Type CreateColumnType(IEnumerable<Type> componentTypes) {
        lock (moduleBuilder) {
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "ArchetypeColumn" + index++, TypeAttributes.SequentialLayout
            );

            int i = 0;
            foreach (Type componentType in componentTypes)
                typeBuilder.DefineField("f" + i++, wrapper.MakeGenericType(componentType), FieldAttributes.Private);

            return typeBuilder.CreateType();
        }
    }

}
