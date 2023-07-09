using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Rendering.Vulkan;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class EntryPointHelper {

    public SpirVCompiler Compiler { get; }

    public EntryPointHelper(SpirVCompiler compiler) {
        Compiler = compiler;
    }

    public SpirVVariable[] CreateFragmentInputs(NeslMethod method) {
        SpirVVariable[] parameters = new SpirVVariable[method.ParameterTypes.Count];

        uint location = 0;
        for (int i = 0; i < method.ParameterTypes.Count; i++) {
            SpirVVariable variable = new SpirVVariable(
                Compiler, method.ParameterTypes[i], StorageClass.Input, Compiler.TypesAndVariables
            );
            Compiler.AddVariable(variable);
            parameters[i] = variable;

            lock (Compiler.Annotations) {
                Compiler.Annotations.Emit(
                    SpirVOpCode.OpDecorate, variable.Id, (uint)Decoration.Location,
                    location++.ToSpirVLiteral()
                );
            }
        }

        return parameters;
    }

    public (SpirVVariable[], VertexInputDescription) CreateVertexInputs(NeslMethod method) {
        SpirVVariable[] parameters = new SpirVVariable[method.ParameterTypes.Count];
        VertexInputBindingDescription[] bindings = new VertexInputBindingDescription[method.ParameterTypes.Count];

        FastList<SpirVVariable> innerVariables = new FastList<SpirVVariable>();
        FastList<VertexInputAttributeDescription> inputAttributes = new FastList<VertexInputAttributeDescription>();

        uint location = 0;
        uint offset = 0;
        for (int i = 0; i < method.ParameterTypes.Count; i++) {
            uint lastOffset = offset;

            innerVariables.Clear();
            GetInnerVariables(ref location, ref offset, method.ParameterTypes[i], inputAttributes, innerVariables);
            bindings[i] = new VertexInputBindingDescription(0, offset - lastOffset, VertexInputRate.Vertex);

            SpirVVariable variable;
            if (innerVariables.Count == 0) {
                variable = new SpirVVariable(
                    Compiler, method.ParameterTypes[i], StorageClass.Input, Compiler.TypesAndVariables
                );
                Compiler.AddVariable(variable);
            } else if (innerVariables.Count == 1) {
                variable = innerVariables[0];
            } else {
                variable = new SpirVVariable(
                    Compiler, method.ParameterTypes[i], StorageClass.Input, new SpirVId(), null!, null,
                    innerVariables.ToArray()
                );
            }

            parameters[i] = variable;
        }

        return (parameters, new VertexInputDescription(bindings, inputAttributes.ToArray()));
    }

    private void GetInnerVariables(
        ref uint location, ref uint offset, NeslType type, FastList<VertexInputAttributeDescription> inputAttributes,
        FastList<SpirVVariable> innerVariables
    ) {
        switch (type.FullNameWithAssembly) {
            case Vectors.Vector2Name:
                GetInnerVariablesVector(
                    ref location, ref offset, type, inputAttributes, innerVariables, 2,
                    VulkanFormat.R32G32_SFloat
                );
                break;
            case Vectors.Vector3Name:
                GetInnerVariablesVector(
                    ref location, ref offset, type, inputAttributes, innerVariables, 3,
                    VulkanFormat.R32G32B32_SFloat
                );
                break;
            case Vectors.Vector4Name:
                GetInnerVariablesVector(
                    ref location, ref offset, type, inputAttributes, innerVariables, 4,
                    VulkanFormat.R32G32B32A32_SFloat
                );
                break;
        }

        foreach (NeslField field in type.Fields) {
            if (field.IsStatic)
                continue;

            GetInnerVariables(ref location, ref offset, field.FieldType, inputAttributes, innerVariables);
        }
    }

    private bool GetInnerVariablesVector(
        ref uint location, ref uint offset, NeslType type, FastList<VertexInputAttributeDescription> inputAttributes,
        FastList<SpirVVariable> innerVariables, uint size, VulkanFormat floatFormat
    ) {
        Debug.Assert(!type.IsGeneric, "Type cannot be generic.");

        NeslType tType = type.GenericMakedTypeParameters.Single();
        switch (tType.FullNameWithAssembly) {
            case BuiltInTypes.Float32Name:
                SpirVVariable variable = new SpirVVariable(
                    Compiler, type, StorageClass.Input, Compiler.TypesAndVariables
                );
                Compiler.AddVariable(variable);
                innerVariables.Add(variable);

                lock (Compiler.Annotations) {
                    Compiler.Annotations.Emit(
                        SpirVOpCode.OpDecorate, variable.Id, (uint)Decoration.Location,
                        location.ToSpirVLiteral()
                    );
                }

                inputAttributes.Add(new VertexInputAttributeDescription(
                    location++, 0, floatFormat, offset
                ));
                offset += size * sizeof(float);
                return true;
        }
        return false;
    }

}
