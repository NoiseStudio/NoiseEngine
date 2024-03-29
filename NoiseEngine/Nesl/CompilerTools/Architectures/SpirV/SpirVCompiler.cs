﻿using NoiseEngine.Collections;
using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompiler {

    private readonly ConcurrentDictionary<object, Lazy<SpirVType>> types =
        new ConcurrentDictionary<object, Lazy<SpirVType>>();
    private readonly ConcurrentDictionary<NeslField, Lazy<SpirVVariable>> variables =
        new ConcurrentDictionary<NeslField, Lazy<SpirVVariable>>();
    private readonly ConcurrentDictionary<SpirVFunctionIdentifier, Lazy<SpirVFunction>> functions =
        new ConcurrentDictionary<SpirVFunctionIdentifier, Lazy<SpirVFunction>>();
    private readonly ConcurrentDictionary<object, Lazy<SpirVId>> consts =
        new ConcurrentDictionary<object, Lazy<SpirVId>>();
    private readonly ConcurrentDictionary<uint, Lazy<SpirVDescriptorSet>> descriptorSets =
        new ConcurrentDictionary<uint, Lazy<SpirVDescriptorSet>>();
    private readonly Dictionary<SpirVExtensions, SpirVId> extInstImportsDictionary =
        new Dictionary<SpirVExtensions, SpirVId>();

    private readonly ConcurrentBag<SpirVVariable> allVariables = new ConcurrentBag<SpirVVariable>();

    private uint nextId;

    internal IEnumerable<NeslEntryPoint> EntryPoints { get; }
    internal ShaderSettings Settings { get; }

    internal SpirVCompilationResultBuilder ResultBuilder { get; }
    internal SpirVBuiltInTypes BuiltInTypes { get; }
    internal SpirVBuiltInVariables BuiltInVariables { get; }
    internal SpirVConsts Consts { get; }
    internal EntryPointHelper EntryPointHelper { get; }
    internal PushConstantsHelper PushConstantsHelper { get; }

    internal SpirVGenerator Header { get; }
    internal SpirVGenerator Annotations { get; }
    internal SpirVGenerator TypesAndVariables { get; }

    private SpirVGenerator ExtInstImports { get; }

    private SpirVCompiler(IEnumerable<NeslEntryPoint> entryPoints, ShaderSettings settings) {
        EntryPoints = entryPoints;
        Settings = settings;

        ResultBuilder = new SpirVCompilationResultBuilder();
        BuiltInTypes = new SpirVBuiltInTypes(this);
        BuiltInVariables = new SpirVBuiltInVariables(this);
        Consts = new SpirVConsts(this);
        EntryPointHelper = new EntryPointHelper(this);
        PushConstantsHelper = new PushConstantsHelper(this);

        ExtInstImports = new SpirVGenerator(this);
        Header = new SpirVGenerator(this);
        Annotations = new SpirVGenerator(this);
        TypesAndVariables = new SpirVGenerator(this);
    }

    public static SpirVCompilationResult Compile(IEnumerable<NeslEntryPoint> entryPoints, ShaderSettings settings) {
        SpirVCompiler compiler = new SpirVCompiler(entryPoints, settings);
        compiler.CompileWorker();
        return compiler.ResultBuilder.Build();
    }

    public SpirVId GetOrAddExtension(SpirVExtensions extension) {
        if (extInstImportsDictionary.TryGetValue(extension, out SpirVId id))
            return id;

        lock (extInstImportsDictionary) {
            if (extInstImportsDictionary.TryGetValue(extension, out id))
                return id;

            id = GetNextId();
            extInstImportsDictionary.Add(extension, id);

            ExtInstImports.Emit(SpirVOpCode.OpExtInstImport, id, (extension switch {
                SpirVExtensions.Glsl => "GLSL.std.450",
                _ => throw new NotImplementedException()
            }).ToSpirVLiteral());
        }

        return id;
    }

    internal SpirVId GetNextId() {
        return new SpirVId(Interlocked.Increment(ref nextId));
    }

    internal SpirVId GetConst(uint data) {
        return consts.GetOrAdd(data, _ => new Lazy<SpirVId>(() => {
            SpirVId id = GetNextId();
            lock (TypesAndVariables) {
                TypesAndVariables.Emit(
                    SpirVOpCode.OpConstant, BuiltInTypes.GetOpTypeInt(32, false).Id, id, data.ToSpirVLiteral()
                );
            }
            return id;
        })).Value;
    }

    internal SpirVId GetConst(int data) {
        return consts.GetOrAdd(data, _ => new Lazy<SpirVId>(() => {
            SpirVId id = GetNextId();
            lock (TypesAndVariables) {
                TypesAndVariables.Emit(
                    SpirVOpCode.OpConstant, BuiltInTypes.GetOpTypeInt(32, true).Id, id, data.ToSpirVLiteral()
                );
            }
            return id;
        })).Value;
    }

    internal SpirVId GetConst(float data) {
        return consts.GetOrAdd(data, _ => new Lazy<SpirVId>(() => {
            SpirVId id = GetNextId();
            lock (TypesAndVariables) {
                TypesAndVariables.Emit(
                    SpirVOpCode.OpConstant, BuiltInTypes.GetOpTypeFloat(32).Id, id, data.ToSpirVLiteral()
                );
            }
            return id;
        })).Value;
    }

    internal SpirVDescriptorSet GetDescriptorSet(uint index) {
        return descriptorSets.GetOrAdd(index, _ => new Lazy<SpirVDescriptorSet>(() => new SpirVDescriptorSet())).Value;
    }

    internal void AddVariable(SpirVVariable variable) {
        allVariables.Add(variable);
    }

    internal SpirVVariable GetSpirVVariable(NeslField neslField) {
        return variables.GetOrAdd(neslField, _ => new Lazy<SpirVVariable>(() => {
            SpirVVariable variable = new SpirVVariable(this, neslField);
            AddVariable(variable);
            return variable;
        })).Value;
    }

    internal SpirVType GetSpirVType(NeslType? neslType, object? additionalData = null) {
        if (neslType is null)
            return BuiltInTypes.GetOpTypeVoid();

        return types.GetOrAdd((neslType, additionalData), _ => new Lazy<SpirVType>(() => {
            if (neslType.Attributes.TryCastAnyAttribute(
                out PlatformDependentTypeRepresentationAttribute? platformDependentTypeRepresentation
            )) {
                string? name = platformDependentTypeRepresentation.SpirVTargetName;
                if (name is null)
                    throw new InvalidOperationException();

                if (!BuiltInTypes.TryGetTypeByName(neslType, name, additionalData, out SpirVType ? type))
                    throw new InvalidOperationException("Invalid built-in type name.");
                return type;
            }

            return new SpirVType(this, neslType);
        })).Value;
    }

    internal SpirVType GetSpirVStruct(IReadOnlyList<SpirVType> fields) {
        return types.GetOrAdd(new EquatableReadOnlyList<SpirVType>(fields), _ => new Lazy<SpirVType>(() => {
            SpirVId id = GetNextId();

            Span<SpirVId> fieldIds = stackalloc SpirVId[fields.Count];
            for (int i = 0; i < fields.Count; i++)
                fieldIds[i] = fields[i].Id;

            lock (TypesAndVariables)
                TypesAndVariables.Emit(SpirVOpCode.OpTypeStruct, id, fieldIds);

            return new SpirVType(this, id);
        })).Value;
    }

    internal SpirVFunction GetSpirVFunction(SpirVFunctionIdentifier identifier) {
        return functions.GetOrAdd(identifier,
            _ => new Lazy<SpirVFunction>(() => new SpirVFunction(this, identifier))
        ).Value;
    }

    internal bool TryGetEntryPoint(NeslMethod neslMethod, [NotNullWhen(true)] out NeslEntryPoint entryPoint) {
        entryPoint = EntryPoints.FirstOrDefault(x => x.Method == neslMethod);
        return entryPoint != default;
    }

    private void CompileWorker() {
        // Generate entry points.
        //Parallel.ForEach(EntryPoints, x => GetSpirVFunction(new SpirVFunctionIdentifier(
        //    x.Method, Array.Empty<SpirVVariable>()
        //)));

        // Emit OpEntryPoint.
        foreach (NeslEntryPoint entryPoint in EntryPoints) {
            SpirVVariable[]? parameters = null;
            switch (entryPoint.ExecutionModel) {
                case ExecutionModel.Vertex:
                    (parameters, VertexInputDescription description) =
                        EntryPointHelper.CreateVertexInputs(entryPoint.Method);
                    ResultBuilder.VertexInputDesciptions.Add(entryPoint.Method, description);
                    break;
                case ExecutionModel.Fragment:
                    parameters = EntryPointHelper.CreateFragmentInputs(entryPoint.Method);
                    break;
            };

            SpirVFunction function = GetSpirVFunction(new SpirVFunctionIdentifier(
                entryPoint.Method, parameters ?? Array.Empty<SpirVVariable>()
            ));

            FastList<SpirVId> ids = new FastList<SpirVId>();

            if (function.OutputVariable is not null)
                ids.Add(function.OutputVariable.Id);

            if (parameters is not null) {
                foreach (SpirVVariable parameter in parameters) {
                    if (parameter.AdditionalData is not SpirVVariable[] innerVariables) {
                        ids.Add(parameter.Id);
                        continue;
                    }

                    foreach (SpirVVariable innerVariable in innerVariables)
                        ids.Add(innerVariable.Id);
                }
            }

            foreach (SpirVVariable variable in function.UsedIOVariables) {
                if (variable != function.OutputVariable)
                    ids.Add(variable.Id);
            }

            Header.Emit(
                SpirVOpCode.OpEntryPoint, (uint)entryPoint.ExecutionModel,
                function.Id, entryPoint.Method.Guid.ToString().ToSpirVLiteral(),
                ids.AsSpan()
            );

            // TODO: add support for another execution modes.
            switch (entryPoint.ExecutionModel) {
                case ExecutionModel.Vertex:
                    break;
                case ExecutionModel.Fragment:
                    Header.Emit(SpirVOpCode.OpExecutionMode, function.Id, (uint)ExecutionMode.OriginUpperLeft);
                    break;
                case ExecutionModel.GLCompute:
                    entryPoint.Method.Attributes.TryCastAnyAttribute(out KernelAttribute? k);
                    Vector3<uint> size = k!.LocalSize;

                    SpirVLiteral literal = size.X.ToSpirVLiteral() + size.Y.ToSpirVLiteral() + size.Z.ToSpirVLiteral();
                    Header.Emit(SpirVOpCode.OpExecutionMode, function.Id, (uint)ExecutionMode.LocalSize, literal);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        // Construct result.
        SpirVGenerator generator = new SpirVGenerator(this);
        FirstWords(generator);

        generator.Emit(SpirVOpCode.OpCapability, (uint)Capability.Shader);
        generator.Writer.WriteBytes(ExtInstImports.Writer.AsSpan());
        generator.Emit(SpirVOpCode.OpMemoryModel, (uint)AddressingModel.Logical, (uint)MemoryModel.Glsl450);

        generator.Writer.WriteBytes(Header.Writer.AsSpan());
        generator.Writer.WriteBytes(Annotations.Writer.AsSpan());
        generator.Writer.WriteBytes(TypesAndVariables.Writer.AsSpan());

        foreach (Lazy<SpirVFunction> lazy in functions.Values)
            lazy.Value.Construct(generator);

        // Set bound.
        BinaryPrimitives.WriteUInt32LittleEndian(generator.Writer.AsSpan(12), GetNextId().RawId);

        ResultBuilder.Code = generator.Writer.ToArray();

        // Set bindings.
        foreach ((NeslField field, Lazy<SpirVVariable> variable) in variables) {
            if (variable.Value.Binding.HasValue)
                ResultBuilder.Bindings.Add(field, variable.Value.Binding.Value);
        }
    }

    /// <summary>
    /// https://registry.khronos.org/SPIR-V/specs/1.2/SPIRV.html#_a_id_physicallayout_a_physical_layout_of_a_spir_v_module_and_instruction
    /// </summary>
    private void FirstWords(SpirVGenerator generator) {
        generator.Writer.WriteUInt32(0x07230203); // Magic number.
        generator.Writer.WriteUInt32(0x00010300); // Version number.
        generator.Writer.WriteUInt32(0x1ec5712d); // Generator's magic number.
        generator.Writer.WriteUInt32(0x0); // Bound space.
        generator.Writer.WriteUInt32(0x0);
    }

}
