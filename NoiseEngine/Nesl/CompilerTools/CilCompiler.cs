using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NoiseEngine.Nesl.CompilerTools;

internal class CilCompiler {

    private readonly AssemblyBuilder assembly;
    private readonly ModuleBuilder module;

    private readonly Dictionary<NeslType, TypeBuilder> types = new Dictionary<NeslType, TypeBuilder>();
    private readonly Dictionary<NeslMethod, MethodBuilder> methods = new Dictionary<NeslMethod, MethodBuilder>();

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
        foreach (NeslMethod neslMethod in NeslAssembly.Types.SelectMany(x => x.Methods))
            GetCilMethod(neslMethod);

        return assembly;
    }

    private TypeBuilder GetCilType(NeslType neslType) {
        if (!types.TryGetValue(neslType, out TypeBuilder? type)) {
            type = module.DefineType(neslType.FullName, neslType.Attributes.GetCilAttributes());
            types.Add(neslType, type);
        }

        return type;
    }

    private MethodBuilder GetCilMethod(NeslMethod neslMethod) {
        if (!methods.TryGetValue(neslMethod, out MethodBuilder? method)) {
            TypeBuilder type = GetCilType(neslMethod.Type);

            method = type.DefineMethod(
                neslMethod.Name,
                neslMethod.Attributes.GetCilAttributes(),
                neslMethod.ReturnType is null ? GetCilType(neslMethod.ReturnType!) : null,
                neslMethod.ParameterTypes.Count > 0 ? neslMethod.ParameterTypes.Select(GetCilType).ToArray() : null
            );
        }

        return method;
    }

}
