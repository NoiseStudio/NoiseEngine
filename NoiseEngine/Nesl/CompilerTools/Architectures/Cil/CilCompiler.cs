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

    internal IEnumerable<NeslEntryPoint> EntryPoints { get; }

    internal CilJit Jit { get; }

    private CilCompiler(IEnumerable<NeslEntryPoint> entryPoints) {
        EntryPoints = entryPoints;

        Jit = new CilJit(this);

        string name = Guid.NewGuid().ToString();
        assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(name),
            AssemblyBuilderAccess.Run
        );

        module = assembly.DefineDynamicModule(name);
    }

    public static CilCompilationResult Compile(IEnumerable<NeslEntryPoint> entryPoints) {
        CilCompiler compiler = new CilCompiler(entryPoints);
        compiler.Compile();
        return new CilCompilationResult(compiler.assembly);
    }

    internal TypeBuilder GetCilType(NeslType neslType) {
        return types.GetOrAdd(neslType, _ => new Lazy<TypeBuilder>(
            () => module.DefineType(neslType.FullName, CilAttributes.GetAttributes(neslType))
        )).Value;
    }

    internal Type GetCilTypeOutput(NeslType neslType) {
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

    internal FieldBuilder GetCilField(NeslField neslField) {
        return fields.GetOrAdd(neslField, _ => new Lazy<FieldBuilder>(() => {
            TypeBuilder type = GetCilType(neslField.ParentType);
            return type.DefineField(neslField.Name, GetCilTypeOutput(neslField.FieldType), FieldAttributes.Public);
        })).Value;
    }

    internal MethodBuilder GetCilMethod(NeslMethod neslMethod) {
        return methods.GetOrAdd(neslMethod, _ => new Lazy<MethodBuilder>(() => {
            TypeBuilder type = GetCilType(neslMethod.Type);

            MethodBuilder method = type.DefineMethod(
                neslMethod.Name,
                CilAttributes.GetAttributes(neslMethod),
                neslMethod.ReturnType is not null ? GetCilTypeOutput(neslMethod.ReturnType!) : null,
                neslMethod.ParameterTypes.Count > 0 ?
                    neslMethod.ParameterTypes.Select(GetCilTypeOutput).ToArray() : null
            );

            Jit.CompileCode(neslMethod.GetInstructions(), neslMethod, method.GetILGenerator());

            return method;
        })).Value;
    }

    private void Compile() {
        Parallel.ForEach(EntryPoints, x => GetCilMethod(x.Method));

        Parallel.ForEach(types.Values, x => x.Value.CreateType());
    }

}
