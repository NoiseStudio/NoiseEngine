﻿using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Nesl.Operations.Branch;

public class ReturnValueTest : NeslTestEnvironment {

    public ReturnValueTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void ReturnValueUniform() {
        const float Value = 730.6435f;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>(true);

        // Get method.
        NeslMethodBuilder get = helper.DefineMethod(BuiltInTypes.Float32);
        get.AddAttribute(StaticAttribute.Create());
        IlGenerator il = get.IlGenerator;

        il.Emit(OpCode.ReturnValue, 0u);

        // Main method.
        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.Call, 1u, get, Array.Empty<uint>());
        il.Emit(OpCode.Load, 0u, 1u);
        il.Emit(OpCode.Return);

        // Assert.
        helper.ExecuteAndAssert(new float[] { Value }, Value);
    }

    [Fact]
    public void ReturnValueFunction() {
        const float Value = 0.123456789f;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>(true);

        // Get method.
        NeslMethodBuilder get = helper.DefineMethod(BuiltInTypes.Float32);
        get.SetModifiers(NeslModifiers.Static);
        get.AddAttribute(StaticAttribute.Create());
        IlGenerator il = get.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadFloat32, 1u, Value);
        il.Emit(OpCode.ReturnValue, 1u);

        // Main method.
        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.Call, 1u, get, Array.Empty<uint>());
        il.Emit(OpCode.Load, 0u, 1u);
        il.Emit(OpCode.Return);

        // Assert.
        helper.ExecuteAndAssert(null, Value);
    }

}
