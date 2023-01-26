using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit.Attributes;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVVariable {

    public SpirVCompiler Compiler { get; }
    public NeslType NeslType { get; }
    public StorageClass StorageClass { get; }
    public NeslField? NeslField { get; }
    public object? AdditionalData { get; }

    public SpirVId Id { get; }
    public SpirVType PointerType { get; }
    public uint? Binding { get; }

    public SpirVVariable(
        SpirVCompiler compiler, NeslType neslType, StorageClass storageClass, SpirVGenerator generator,
        NeslField? neslField = null, object? additionalData = null
    ) {
        Compiler = compiler;
        NeslType = neslType;
        StorageClass = storageClass;
        NeslField = neslField;
        AdditionalData = additionalData;

        SpirVType type;
        if (
            neslType.FullNameWithAssembly == Buffers.ReadWriteBufferName &&
            storageClass != StorageClass.Uniform
        ) {
            type = Compiler.GetSpirVType(neslType, additionalData);
        } else {
            type = Compiler.GetSpirVType(neslType);
        }

        SpirVVariableCreationOutput output;
        switch (storageClass) {
            case StorageClass.Uniform:
                output = CreateUniformStorage(generator, type, out uint binding);
                Binding = binding;
                break;
            case StorageClass.Private:
            case StorageClass.Function:
            case StorageClass.Input:
            case StorageClass.Output:
                output = CreateFunctionStorage(generator, type);
                break;
            default:
                throw new NotImplementedException();
        }

        Id = output.Id;
        PointerType = output.PointerType;
    }

    public SpirVVariable(SpirVCompiler compiler, NeslField neslField) : this(
        compiler, neslField.FieldType, GetStorageClass(neslField), compiler.TypesAndVariables,
        neslField, GetAdditionalData(neslField)
    ) {
    }

    public SpirVVariable(
        SpirVCompiler compiler, NeslType neslType, StorageClass storageClass, SpirVId id, SpirVType pointerType,
        NeslField? neslField = null, object? additionalData = null
    ) {
        Compiler = compiler;
        NeslType = neslType;
        StorageClass = storageClass;
        Id = id;
        PointerType = pointerType;
        NeslField = neslField;
        AdditionalData = additionalData;
    }

    public static SpirVVariable CreateFromParameter(
        SpirVCompiler compiler, NeslType neslType, SpirVId id
    ) {
        SpirVType pointerType = compiler.BuiltInTypes.GetOpTypePointer(
            StorageClass.Function, compiler.GetSpirVType(neslType)
        );

        return new SpirVVariable(compiler, neslType, StorageClass.Function, id, pointerType);
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

    private static object? GetAdditionalData(NeslField neslField) {
        if (
            neslField.DefaultData is null ||
            neslField.FieldType.FullNameWithAssembly != Buffers.ReadWriteBufferName
        ) {
            return null;
        }

        ulong size = neslField.FieldType.GetField($"{NeslOperators.Phantom}T")!.FieldType.GetSize();
        return (uint)((ulong)neslField.DefaultData.Count * 8 / size);
    }

    public SpirVId GetAccess(SpirVGenerator generator) {
        return StorageClass switch {
            StorageClass.Input => Id,
            StorageClass.Uniform => GetAccessUniformStorage(generator),
            StorageClass.Output => Id,
            StorageClass.Private => Id,
            StorageClass.Function => Id,
            _ => throw new NotImplementedException()
        };
    }

    private SpirVId CreateVariableFromPointer(SpirVGenerator generator, SpirVType pointer, StorageClass storageClass) {
        SpirVId id = Compiler.GetNextId();
        generator.Emit(SpirVOpCode.OpVariable, pointer.Id, id, (uint)storageClass);
        return id;
    }

    private SpirVVariableCreationOutput CreateUniformStorage(
        SpirVGenerator generator, SpirVType type, out uint binding
    ) {
        SpirVType content = Compiler.GetSpirVStruct(new SpirVType[] { type });
        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(StorageClass, content);

        SpirVId id;
        lock (generator)
            id = CreateVariableFromPointer(generator, pointer, StorageClass);

        lock (Compiler.Annotations) {
            Compiler.Annotations.Emit(
                SpirVOpCode.OpMemberDecorate, content.Id, 0u.ToSpirVLiteral(), (uint)Decoration.Offset,
                0u.ToSpirVLiteral()
            );
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, content.Id, (uint)Decoration.BufferBlock);
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, id, (uint)Decoration.DescriptorSet, 0u.ToSpirVLiteral());

            binding = Compiler.GetDescriptorSet(0u).NextBinding();
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, id, (uint)Decoration.Binding, binding.ToSpirVLiteral());
        }

        return new SpirVVariableCreationOutput(id, pointer);
    }

    private SpirVVariableCreationOutput CreateFunctionStorage(SpirVGenerator generator, SpirVType type) {
        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(StorageClass, type);

        SpirVId id;
        lock (generator)
            id = CreateVariableFromPointer(generator, pointer, StorageClass);

        return new SpirVVariableCreationOutput(id, pointer);
    }

    private SpirVId GetAccessUniformStorage(SpirVGenerator generator) {
        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(
            StorageClass.Uniform, Compiler.GetSpirVType(NeslType)
        );

        SpirVId id = Compiler.GetNextId();
        generator.Emit(SpirVOpCode.OpAccessChain, pointer.Id, id, Id, stackalloc SpirVId[] { Compiler.GetConst(0) });

        return id;
    }

}
