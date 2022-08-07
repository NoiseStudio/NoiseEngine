using NoiseEngine.Nesl.Emit.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.Cil;

internal class CilCompiler {

    private readonly AssemblyBuilder assembly;
    private readonly ModuleBuilder module;

    private readonly ConcurrentDictionary<NeslType, Lazy<TypeBuilder>> types =
        new ConcurrentDictionary<NeslType, Lazy<TypeBuilder>>();
    private readonly ConcurrentDictionary<NeslField, Lazy<FieldBuilder>> fields =
        new ConcurrentDictionary<NeslField, Lazy<FieldBuilder>>();
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
        Parallel.ForEach(NeslAssembly.Types.SelectMany(x => x.Fields), x => GetCilField(x));
        Parallel.ForEach(NeslAssembly.Types, x => GetCilType(x).CreateType());

        return assembly;
    }

    private TypeBuilder GetCilType(NeslType neslType) {
        return types.GetOrAdd(neslType, _ => new Lazy<TypeBuilder>(
            () => module.DefineType(neslType.FullName, CilCompilerAttributes.GetAttributes(neslType))
        )).Value;
    }

    private Type GetCilTypeOutput(NeslType neslType) {
        if (neslType.Attributes.TryCastAnyAttribute(
            out PlatformDependentTypeRepresentationAttribute? platformDependentTypeRepresentation
        )) {
            if (platformDependentTypeRepresentation.CilTargetName is null)
                throw new InvalidOperationException();

            Type? type = typeof(Type).Assembly.GetType(platformDependentTypeRepresentation.CilTargetName);
            if (type is null)
                throw new InvalidOperationException();

            return type;
        }

        return GetCilType(neslType);
    }

    private FieldBuilder GetCilField(NeslField neslField) {
        return fields.GetOrAdd(neslField, _ => new Lazy<FieldBuilder>(() => {
            TypeBuilder type = GetCilType(neslField.ParentType);
            return type.DefineField(neslField.Name, GetCilTypeOutput(neslField.FieldType), FieldAttributes.Public);
        })).Value;
    }

    private MethodBuilder GetCilMethod(NeslMethod neslMethod) {
        return methods.GetOrAdd(neslMethod, _ => new Lazy<MethodBuilder>(() => {
            TypeBuilder type = GetCilType(neslMethod.Type);

            MethodBuilder method = type.DefineMethod(
                neslMethod.Name,
                CilCompilerAttributes.GetAttributes(neslMethod),
                neslMethod.ReturnType is not null ? GetCilTypeOutput(neslMethod.ReturnType!) : null,
                neslMethod.ParameterTypes.Count > 0 ?
                    neslMethod.ParameterTypes.Select(GetCilTypeOutput).ToArray() : null
            );

            CompileCode(neslMethod.GetInstructions(), neslMethod, method.GetILGenerator());

            return method;
        })).Value;
    }

    private void CompileCode(IEnumerable<Instruction> instructions, NeslMethod neslMethod, ILGenerator generator) {
        foreach (Instruction instruction in instructions) {
            switch (instruction.OpCode) {

                case Emit.OpCode.LoadArg:
                    generator.Emit(OpCodes.Ldarg, (int)instruction.ReadUInt8());
                    break;
                case Emit.OpCode.LoadField:
                    generator.Emit(OpCodes.Ldfld, GetCilField(neslMethod.Type.GetField(instruction.ReadUInt32())));
                    break;
                case Emit.OpCode.LoadUInt32:
                    generator.Emit(OpCodes.Ldc_I4, instruction.ReadUInt32());
                    break;
                case Emit.OpCode.LoadUInt64:
                    generator.Emit(OpCodes.Ldc_I8, instruction.ReadUInt64());
                    break;
                case Emit.OpCode.LoadFloat32:
                    generator.Emit(OpCodes.Ldc_R4, instruction.ReadFloat32());
                    break;

                case Emit.OpCode.SetElement:
                    generator.Emit(OpCodes.Stelem, GetCilTypeOutput(NeslAssembly.GetType(instruction.ReadUInt64())));
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
