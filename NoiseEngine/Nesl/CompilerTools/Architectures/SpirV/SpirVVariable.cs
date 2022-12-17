using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVVariable {

    public SpirVCompiler Compiler { get; }
    public NeslType NeslType { get; }
    public StorageClass StorageClass { get; }

    public SpirVId Id { get; }
    public uint Binding { get; }

    public SpirVVariable(
        SpirVCompiler compiler, NeslType neslType, StorageClass storageClass, SpirVGenerator generator
    ) {
        Compiler = compiler;
        NeslType = neslType;
        StorageClass = storageClass;

        SpirVType type = Compiler.GetSpirVType(neslType);

        switch (storageClass) {
            case StorageClass.Uniform:
                Id = CreateUniformStorage(generator, type, out uint binding);
                Binding = binding;
                break;
            case StorageClass.Function:
                Id = CreateFunctionStorage(generator, type);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public SpirVVariable(SpirVCompiler compiler, NeslField neslField) :
        this(compiler, neslField.FieldType, GetStorageClass(neslField), compiler.TypesAndVariables) {
    }

    private SpirVVariable(SpirVCompiler compiler, NeslType neslType, StorageClass storageClass, SpirVId id) {
        Compiler = compiler;
        NeslType = neslType;
        StorageClass = storageClass;
        Id = id;
    }

    public static SpirVVariable CreateFromParameter(SpirVCompiler compiler, NeslType neslType, SpirVId id) {
        return new SpirVVariable(compiler, neslType, StorageClass.Function, id);
    }

    private static StorageClass GetStorageClass(NeslField neslField) {
        if (neslField.Attributes.HasAnyAttribute("InAttribute"))
            return StorageClass.Input;
        if (neslField.Attributes.HasAnyAttribute("OutAttribute"))
            return StorageClass.Output;
        if (neslField.Attributes.HasAnyAttribute(nameof(UniformAttribute)))
            return StorageClass.Uniform;
        return StorageClass.Private;
    }

    public SpirVId GetAccess(SpirVGenerator generator) {
        return StorageClass switch {
            StorageClass.Uniform => GetAccessUniformStorage(generator),
            StorageClass.Function => Id,
            _ => throw new NotImplementedException()
        };
    }

    private SpirVId CreateVariableFromPointer(SpirVGenerator generator, SpirVType pointer) {
        SpirVId id = Compiler.GetNextId();
        generator.Emit(SpirVOpCode.OpVariable, pointer.Id, id, (uint)StorageClass);
        return id;
    }

    private SpirVId CreateUniformStorage(SpirVGenerator generator, SpirVType type, out uint binding) {
        SpirVType structure = Compiler.GetSpirVStruct(new SpirVType[] { type });
        SpirVType structurePointer = Compiler.BuiltInTypes.GetOpTypePointer(StorageClass, structure);

        SpirVId id;
        lock (generator)
            id = CreateVariableFromPointer(generator, structurePointer);

        lock (Compiler.Annotations) {
            Compiler.Annotations.Emit(
                SpirVOpCode.OpMemberDecorate, structure.Id, 0u.ToSpirVLiteral(), (uint)Decoration.Offset,
                0u.ToSpirVLiteral()
            );
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, structure.Id, (uint)Decoration.BufferBlock);
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, id, (uint)Decoration.DescriptorSet, 0u.ToSpirVLiteral());

            binding = Compiler.GetDescriptorSet(0u).NextBinding();
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, id, (uint)Decoration.Binding, binding.ToSpirVLiteral());
        }

        return id;
    }

    private SpirVId CreateFunctionStorage(SpirVGenerator generator, SpirVType type) {
        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(StorageClass, type);
        return CreateVariableFromPointer(generator, pointer);
    }

    private SpirVId GetAccessUniformStorage(SpirVGenerator generator) {
        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(
            StorageClass.Uniform, Compiler.GetSpirVType(NeslType)
        );

        SpirVId id = Compiler.GetNextId();
        generator.Emit(
            SpirVOpCode.OpAccessChain, pointer.Id, id, Id,
            new SpirVId[] { Compiler.GetConst(0) }
        );

        return id;
    }

}
