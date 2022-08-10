using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

[AttributeUsage(System.AttributeTargets.Field)]
internal class OpCodeValidationAttribute : Attribute {

    public IReadOnlyList<Type> Tail { get; }

    public OpCodeValidationAttribute(params Type[] tail) {
        Tail = tail;
    }

    internal static void AssertTail<T>(T opCode, params Type[] expectedTail) where T : Enum {
        IReadOnlyList<Type> tail = opCode.GetCustomAttribute<OpCodeValidationAttribute>().Tail;

        if (expectedTail.Length < tail.Count)
            AssertTailThrow(opCode);

        for (int i = 0; i < tail.Count; i++) {
            if (tail[i] != expectedTail[i])
                AssertTailThrow(opCode);
        }

        if (expectedTail.Length == tail.Count)
            return;

        foreach (
            OpCodeValidationOptionalAttribute optional in
            opCode.GetCustomAttributes<OpCodeValidationOptionalAttribute>()
        ) {
            if (optional.Tail.Count != expectedTail.Length - tail.Count)
                continue;

            bool isBreaked = false;
            for (int i = 0; i < optional.Tail.Count; i++) {
                if (optional.Tail[i] != expectedTail[i + tail.Count]) {
                    isBreaked = true;
                    break;
                }
            }

            if (!isBreaked)
                return;
        }

        AssertTailThrow(opCode);
    }

    private static void AssertTailThrow<T>(T opCode) where T : Enum {
        throw new InvalidOperationException($"{opCode} does not support given arguments.");
    }

}
