using NoiseEngine.Nesl.CompilerTools.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace NoiseEngine.Nesl.CompilerTools;

internal class CilCompiler {

    private readonly AssemblyBuilder assembly;
    private readonly ModuleBuilder module;

    private readonly ConcurrentDictionary<NeslType, Lazy<TypeBuilder>> types =
        new ConcurrentDictionary<NeslType, Lazy<TypeBuilder>>();
    private readonly ConcurrentDictionary<NeslMethod, Lazy<MethodBuilder>> methods =
        new ConcurrentDictionary<NeslMethod, Lazy<MethodBuilder>>();

    public NeslAssembly NeslAssembly { get; }

    public CilCompiler(NeslAssembly neslAssembly) {
        NeslAssembly = neslAssembly;

        assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(neslAssembly.Name),
            AssemblyBuilderAccess.Run
        );

        module = assembly.DefineDynamicModule(neslAssembly.Name);
    }

    public Assembly Compile() {
        Parallel.ForEach(NeslAssembly.Types.SelectMany(x => x.Methods), x => GetCilMethod(x));
        Parallel.ForEach(NeslAssembly.Types, x => GetCilType(x).CreateType());

        return assembly;
    }

    private TypeBuilder GetCilType(NeslType neslType) {
        return types.GetOrAdd(neslType, _ => new Lazy<TypeBuilder>(
            () => module.DefineType(neslType.FullName, neslType.Attributes.GetCilAttributes())
        )).Value;
    }

    private Type GetCilTypeOutput(NeslType neslType) {
        PlatformDependentTypeRepresentationNeslAttribute? platformDependentTypeRepresentation =
            (PlatformDependentTypeRepresentationNeslAttribute?)neslType.CustomAttributes.FirstOrDefault(
                x => x.GetType() == typeof(PlatformDependentTypeRepresentationNeslAttribute)
            );

        if (platformDependentTypeRepresentation is not null) {
            if (platformDependentTypeRepresentation.CilTargetName is null)
                throw new InvalidOperationException();

            Type? type = typeof(Type).Assembly.GetType(platformDependentTypeRepresentation.CilTargetName);
            if (type is null)
                throw new InvalidOperationException();

            return type;
        }

        return GetCilType(neslType);
    }

    private MethodBuilder GetCilMethod(NeslMethod neslMethod) {
        return methods.GetOrAdd(neslMethod, _ => new Lazy<MethodBuilder>(() => {
            TypeBuilder type = GetCilType(neslMethod.Type);

            MethodBuilder method = type.DefineMethod(
                neslMethod.Name,
                neslMethod.Attributes.GetCilAttributes(),
                neslMethod.ReturnType is not null ? GetCilTypeOutput(neslMethod.ReturnType!) : null,
                neslMethod.ParameterTypes.Count > 0 ?
                    neslMethod.ParameterTypes.Select(GetCilTypeOutput).ToArray() : null
            );

            CompileCode(neslMethod.GetInstructions(), method.GetILGenerator());

            return method;
        })).Value;
    }

    private void CompileCode(IEnumerable<Instruction> instructions, ILGenerator generator) {
        foreach (Instruction instruction in instructions) {
            switch (instruction.OpCode) {

                case Emit.OpCode.LoadFloat32:
                    generator.Emit(OpCodes.Ldc_R4, instruction.ReadFloat32());
                    break;

                case Emit.OpCode.Add:
                    generator.Emit(OpCodes.Add);
                    break;
                case Emit.OpCode.Sub:
                    generator.Emit(OpCodes.Sub);
                    break;
                case Emit.OpCode.Mul:
                    generator.Emit(OpCodes.Mul);
                    break;
                case Emit.OpCode.Div:
                    generator.Emit(OpCodes.Div);
                    break;

                case Emit.OpCode.Call:
                    generator.Emit(OpCodes.Call, GetCilMethod(NeslAssembly.GetMethod(instruction.ReadUInt64())));
                    break;
                case Emit.OpCode.Return:
                    generator.Emit(OpCodes.Ret);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

}
