using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace NoiseEngine.Common;

internal static class ManagedUtils {

    [RequiresDynamicCode("Method is not reachable with AOT compilation.")]
    public static int SizeOf(Type type) {
        DynamicMethod sizeOfMethod = new DynamicMethod(nameof(SizeOf) + "Impl", typeof(uint), null, true);

        ILGenerator il = sizeOfMethod.GetILGenerator();
        il.Emit(OpCodes.Sizeof, type);
        il.Emit(OpCodes.Ret);

        return (int)((Func<uint>)sizeOfMethod.CreateDelegate(typeof(Func<uint>)))();
    }

}
