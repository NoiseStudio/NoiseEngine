﻿using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVFunction {

    public SpirVCompiler Compiler { get; }
    public NeslMethod NeslMethod { get; }

    public SpirVGenerator SpirVGenerator { get; }
    public SpirVGenerator CodeSpirVGenerator { get; }

    public SpirVId Id { get; }
    public SpirVVariable? OutputVariable { get; private set; }
    public HashSet<SpirVVariable> UsedIOVariables { get; } = new HashSet<SpirVVariable>();

    public SpirVFunction(SpirVCompiler compiler, SpirVFunctionIdentifier identifier) {
        Compiler = compiler;
        NeslMethod = identifier.NeslMethod;

        SpirVGenerator = new SpirVGenerator(Compiler);
        CodeSpirVGenerator = new SpirVGenerator(Compiler);
        Id = Compiler.GetNextId();

        BeginFunction(identifier);
    }

    internal void Construct(SpirVGenerator generator) {
        generator.Writer.WriteBytes(SpirVGenerator.Writer.AsSpan());
        generator.Writer.WriteBytes(CodeSpirVGenerator.Writer.AsSpan());
        generator.Emit(SpirVOpCode.OpFunctionEnd);
    }

    private void BeginFunction(SpirVFunctionIdentifier identifier) {
        SpirVType returnType;

        bool isEntryPoint = Compiler.TryGetEntryPoint(NeslMethod, out NeslEntryPoint entryPoint);
        if (isEntryPoint) {
            returnType = entryPoint.ExecutionModel switch {
                ExecutionModel.Vertex => BeginFunctionFragment(),
                ExecutionModel.Fragment => BeginFunctionFragment(),
                ExecutionModel.GLCompute => Compiler.GetSpirVType(NeslMethod.ReturnType),
                _ => throw new NotImplementedException()
            };
        } else {
            returnType = Compiler.GetSpirVType(NeslMethod.ReturnType);
        }

        // Create function type.
        SpirVVariable[] parameters = new SpirVVariable[identifier.Parameters.Count];
        SpirVVariable[] dynamicParameters = new SpirVVariable[identifier.DynamicParameters.Count];
        SpirVType[] typeFunctionParameterPointers = new SpirVType[identifier.DynamicParameters.Count];
        bool isStatic = identifier.IsStatic;

        int j = 0;
        for (int i = 0; i < parameters.Length; i++) {
            SpirVVariable? parameter = identifier.Parameters[i];

            if (parameter is null) {
                NeslType neslType;
                if (isStatic)
                    neslType = NeslMethod.ParameterTypes[i];
                else if (i == 0)
                    neslType = NeslMethod.Type;
                else
                    neslType = NeslMethod.ParameterTypes[i - 1];

                SpirVId id = Compiler.GetNextId();
                parameter = SpirVVariable.CreateFromParameter(Compiler, neslType, id, identifier.DynamicParameters[j]);

                typeFunctionParameterPointers[j] = parameter.PointerType;
                dynamicParameters[j] = parameter;
                j++;
            }

            parameters[i] = parameter;
        }

        SpirVType functionType = Compiler.BuiltInTypes.GetOpTypeFunction(returnType, typeFunctionParameterPointers);

        // TODO: implement function control.
        SpirVGenerator.Emit(SpirVOpCode.OpFunction, returnType.Id, Id, 0, functionType.Id);

        // Emit dynamic parameters.
        foreach (SpirVVariable parameter in dynamicParameters)
            SpirVGenerator.Emit(SpirVOpCode.OpFunctionParameter, parameter.PointerType.Id, parameter.Id);

        // Label and code.
        SpirVGenerator.Emit(SpirVOpCode.OpLabel, Compiler.GetNextId());

        if (!NeslMethod.Attributes.HasAnyAttribute(nameof(IntrinsicAttribute))) {
            IlCompiler ilCompiler = new IlCompiler(
                Compiler, NeslMethod.GetInstructions(), NeslMethod, CodeSpirVGenerator, parameters,
                this, isEntryPoint ? entryPoint.ExecutionModel : null
            );
            ilCompiler.Compile();

            if (isEntryPoint) {
                foreach (SpirVVariable variable in ilCompiler.UsedVariables) {
                    if (variable.NeslField?.DefaultData is not null)
                        DefaultDataHelper.Store(Compiler, variable, variable.NeslField, SpirVGenerator);
                }
            }
        } else {
            IntrinsicsManager.Process(CodeSpirVGenerator, this, parameters);
        }
    }

    private SpirVType BeginFunctionFragment() {
        if (NeslMethod.ReturnType is not null) {
            OutputVariable = new SpirVVariable(
                Compiler, NeslMethod.ReturnType, StorageClass.Output, Compiler.TypesAndVariables
            );
            Compiler.AddVariable(OutputVariable);
            UsedIOVariables.Add(OutputVariable);

            lock (Compiler.Annotations) {
                Compiler.Annotations.Emit(
                    SpirVOpCode.OpDecorate, OutputVariable.Id, (uint)Decoration.Location, 0u.ToSpirVLiteral()
                );
            }
        }

        return Compiler.BuiltInTypes.GetOpTypeVoid();
    }

}
