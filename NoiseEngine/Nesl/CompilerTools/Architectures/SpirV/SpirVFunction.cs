using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVFunction {

    public SpirVCompiler Compiler { get; }
    public NeslMethod NeslMethod { get; }

    public SpirVGenerator SpirVGenerator { get; }

    public SpirVId Id { get; }

    public SpirVFunction(SpirVCompiler compiler, NeslMethod neslMethod) {
        Compiler = compiler;
        NeslMethod = neslMethod;

        SpirVGenerator = new SpirVGenerator(Compiler);
        Id = Compiler.GetNextId();

        BeginFunction();
    }

    internal void Construct(SpirVGenerator generator) {
        generator.Writer.WriteBytes(SpirVGenerator.Writer.AsSpan());
        generator.Emit(SpirVOpCode.OpFunctionEnd);
    }

    private void BeginFunction() {
        if (Compiler.TryGetEntryPoint(NeslMethod, out NeslEntryPoint entryPoint)) {
            if (entryPoint.ExecutionModel == ExecutionModel.Fragment) {
                if (NeslMethod.ReturnType is not null)
                    new SpirVVariable(Compiler, NeslMethod.ReturnType, StorageClass.Output);

                foreach (NeslType parameterType in NeslMethod.ParameterTypes)
                    new SpirVVariable(Compiler, parameterType, StorageClass.Input);
            } else {
                throw new NotImplementedException();
            }
        }

        SpirVType returnType = Compiler.GetSpirVType(NeslMethod.ReturnType);
        SpirVType functionType = Compiler.BuiltInTypes.GetOpTypeFunction(returnType);

        // TODO: implement function control.
        SpirVGenerator.Emit(SpirVOpCode.OpFunction, returnType.Id, Id, 0, functionType.Id);

        SpirVGenerator.Emit(SpirVOpCode.OpLabel, Compiler.GetNextId());

        Compiler.Jit.CompileCode(NeslMethod.GetInstructions(), NeslMethod, SpirVGenerator);
    }

}
