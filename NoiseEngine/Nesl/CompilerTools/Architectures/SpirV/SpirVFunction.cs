using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVFunction {

    public SpirVCompiler Compiler { get; }
    public NeslMethod NeslMethod { get; }

    public SpirVGenerator SpirVGenerator { get; }

    public SpirVId Id { get; }

    public SpirVFunction(SpirVCompiler compiler, NeslMethod neslMethod, StorageClass? objectStorageClass) {
        Compiler = compiler;
        NeslMethod = neslMethod;

        SpirVGenerator = new SpirVGenerator(Compiler);
        Id = Compiler.GetNextId();

        BeginFunction(objectStorageClass);
    }

    internal void Construct(SpirVGenerator generator) {
        generator.Writer.WriteBytes(SpirVGenerator.Writer.AsSpan());
        generator.Emit(SpirVOpCode.OpFunctionEnd);
    }

    private void BeginFunction(StorageClass? objectStorageClass) {
        SpirVType returnType;

        if (Compiler.TryGetEntryPoint(NeslMethod, out NeslEntryPoint entryPoint)) {
            returnType = entryPoint.ExecutionModel switch {
                ExecutionModel.Fragment => BeginFunctionFragment(),
                ExecutionModel.GLCompute => Compiler.GetSpirVType(NeslMethod.ReturnType),
                _ => throw new NotImplementedException()
            };
        } else {
            returnType = Compiler.GetSpirVType(NeslMethod.ReturnType);
        }

        // Create function type.
        int addedIndex = objectStorageClass.HasValue ? 1 : 0;
        SpirVType[] typeFunctionParameters = new SpirVType[NeslMethod.ParameterTypes.Count + addedIndex];

        if (objectStorageClass.HasValue) {
            SpirVType structure = Compiler.GetSpirVStruct(new SpirVType[] { Compiler.GetSpirVType(NeslMethod.Type) });
            typeFunctionParameters[0] = Compiler.BuiltInTypes.GetOpTypePointer(objectStorageClass.Value, structure);
        }

        for (int i = 0; i < NeslMethod.ParameterTypes.Count; i++) {
            typeFunctionParameters[i + addedIndex] = Compiler.BuiltInTypes.GetOpTypePointer(
                StorageClass.Function, Compiler.GetSpirVType(NeslMethod.ParameterTypes[i])
            );
        }

        SpirVType functionType = Compiler.BuiltInTypes.GetOpTypeFunction(returnType, typeFunctionParameters);

        // TODO: implement function control.
        SpirVGenerator.Emit(SpirVOpCode.OpFunction, returnType.Id, Id, 0, functionType.Id);

        // Parameter variables.
        SpirVVariable[] parameters = new SpirVVariable[NeslMethod.ParameterTypes.Count + addedIndex];

        if (objectStorageClass.HasValue) {
            SpirVId id = Compiler.GetNextId();
            SpirVGenerator.Emit(SpirVOpCode.OpFunctionParameter, typeFunctionParameters[0].Id, id);
            parameters[0] = SpirVVariable.CreateFromParameter(Compiler, NeslMethod.Type, id);
        }

        for (int i = 0; i < NeslMethod.ParameterTypes.Count; i++) {
            SpirVId id = Compiler.GetNextId();
            SpirVGenerator.Emit(SpirVOpCode.OpFunctionParameter, typeFunctionParameters[i + addedIndex].Id, id);
            parameters[i + addedIndex] = SpirVVariable.CreateFromParameter(Compiler, NeslMethod.ParameterTypes[i], id);
        }

        // Label and code.
        SpirVGenerator.Emit(SpirVOpCode.OpLabel, Compiler.GetNextId());

        if (!NeslMethod.Attributes.HasAnyAttribute(nameof(IntrinsicAttribute))) {
            new IlCompiler(Compiler, NeslMethod.GetInstructions(), NeslMethod, SpirVGenerator, parameters).Compile();
        } else {
            IntrinsicsManager.Process(Compiler, NeslMethod, SpirVGenerator, parameters);
        }
    }

    private SpirVType BeginFunctionFragment() {
        if (NeslMethod.ReturnType is not null) {
            SpirVVariable variable = new SpirVVariable(
                Compiler, NeslMethod.ReturnType, StorageClass.Output, Compiler.TypesAndVariables
            );
            Compiler.AddVariable(variable);

            lock (Compiler.Annotations) {
                Compiler.Annotations.Emit(
                    SpirVOpCode.OpDecorate, variable.Id, (uint)Decoration.Location, 0u.ToSpirVLiteral()
                );
            }
        }

        uint location = 0;
        foreach (NeslType parameterType in NeslMethod.ParameterTypes) {
            SpirVVariable variable = new SpirVVariable(
                Compiler, parameterType, StorageClass.Input, Compiler.TypesAndVariables
            );
            Compiler.AddVariable(variable);

            lock (Compiler.Annotations) {
                Compiler.Annotations.Emit(
                    SpirVOpCode.OpDecorate, variable.Id, (uint)Decoration.Location,
                    location++.ToSpirVLiteral()
                );
            }
        }

        return Compiler.BuiltInTypes.GetOpTypeVoid();
    }

}
