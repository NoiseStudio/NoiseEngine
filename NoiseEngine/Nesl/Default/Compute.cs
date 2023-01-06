using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Emit.Attributes.Internal;

namespace NoiseEngine.Nesl.Default;

internal static class Compute {

    /*public static NeslMethod WorkgroupCount3 { get; }
    public static NeslMethod WorkgroupSize3 { get; }
    public static NeslMethod GlobalInvocation { get; }*/
    public static NeslMethod GlobalInvocation3 { get; }

    static Compute() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Compute");

        /*WorkgroupCount3 = DefineProperty(
            type, nameof(WorkgroupCount3), Vectors.GetVector3(BuiltInTypes.UInt32)
        );
        WorkgroupSize3 = DefineProperty(
            type, nameof(WorkgroupSize3), Vectors.GetVector3(BuiltInTypes.UInt32)
        );*/
        GlobalInvocation3 = DefineProperty(
            type, nameof(GlobalInvocation3), Vectors.GetVector3(BuiltInTypes.UInt32)
        );

        //GlobalInvocation = CreateGlobalInvocation(type);
    }

    private static NeslMethod DefineProperty(NeslTypeBuilder type, string name, NeslType returnType) {
        NeslMethodBuilder property = type.DefineMethod(NeslOperators.PropertyGet + name, returnType);

        property.AddAttribute(StaticAttribute.Create());
        property.AddAttribute(IntrinsicAttribute.Create());

        return property;
    }

    /*private static NeslMethod CreateGlobalInvocation(NeslTypeBuilder type) {
        NeslMethodBuilder property = type.DefineMethod(
            NeslOperators.PropertyGet + nameof(GlobalInvocation), BuiltInTypes.UInt32
        );
        property.AddAttribute(StaticAttribute.Create());
        IlGenerator il = property.IlGenerator;

        il.Emit(OpCode.DefVariable, Vectors.GetVector3(BuiltInTypes.UInt32));
        il.Emit(OpCode.DefVariable, Vectors.GetVector3(BuiltInTypes.UInt32));

        il.Emit(OpCode.Call, 0u, WorkgroupCount3, stackalloc uint[0]);
        il.Emit(OpCode.Call, 1u, WorkgroupSize3, stackalloc uint[0]);
        il.Emit(OpCode.Multiple, 1u, 0u, 1u);

        il.Emit(OpCode.Call, 0u, GlobalInvocation3, stackalloc uint[0]);

        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);

        // Y
        il.Emit(OpCode.LoadField, 2u, 0u, 1u);
        il.Emit(OpCode.LoadField, 3u, 1u, 0u);
        il.Emit(OpCode.Multiple, 2u, 2u, 3u);
        il.Emit(OpCode.SetField, 0u, 1u, 2u);

        // X
        il.Emit(OpCode.LoadField, 2u, 1u, 1u);
        il.Emit(OpCode.Multiple, 3u, 2u, 3u);
        il.Emit(OpCode.LoadField, 2u, 0u, 0u);
        il.Emit(OpCode.Multiple, 2u, 2u, 3u);

        // Sum
        il.Emit(OpCode.LoadField, 3u, 0u, 1u);
        il.Emit(OpCode.Add, 2u, 2u, 3u);
        il.Emit(OpCode.LoadField, 3u, 0u, 2u);
        il.Emit(OpCode.Add, 2u, 2u, 3u);

        il.Emit(OpCode.ReturnValue, 2u);

        return property;
    }*/

}
