using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools.Generics;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Nesl;

public abstract class NeslType : INeslGenericTypeParameterOwner {

    private const char Delimiter = '.';

    private ConcurrentDictionary<NeslType[], Lazy<NeslType>>? genericMakedTypes;

    public abstract IEnumerable<NeslAttribute> Attributes { get; }
    public abstract IEnumerable<NeslGenericTypeParameter> GenericTypeParameters { get; }
    public abstract IReadOnlyList<NeslField> Fields { get; }
    public abstract IEnumerable<NeslMethod> Methods { get; }
    public abstract NeslTypeKind Kind { get; }
    public abstract IEnumerable<NeslType> Interfaces { get; }

    public virtual string Name => FullName.Substring(FullName.LastIndexOf(Delimiter) + 1);
    public virtual string Namespace {
        get {
            int index = FullName.LastIndexOf(Delimiter);
            if (index == -1)
                return string.Empty;
            return FullName.Substring(0, index);
        }
    }

    public NeslAssembly Assembly { get; }
    public string FullName { get; }
    public string FullNameWithAssembly => $"{Assembly.Name}::{FullName}";

    public bool IsGeneric => GenericTypeParameters.Any();
    public bool IsGenericMaked => GenericMakedFrom is not null;
    public bool IsClass => Kind == NeslTypeKind.Class;
    public bool IsValueType => Kind == NeslTypeKind.Struct;
    public bool IsInterface => Kind == NeslTypeKind.Interface;

    public virtual IEnumerable<NeslType> GenericMakedTypeParameters => Enumerable.Empty<NeslType>();
    public virtual NeslType? GenericMakedFrom { get; }

    private ConcurrentDictionary<NeslType[], Lazy<NeslType>> GenericMakedTypes {
        get {
            if (genericMakedTypes is null) {
                Interlocked.CompareExchange(
                    ref genericMakedTypes,
                    new ConcurrentDictionary<NeslType[], Lazy<NeslType>>(new ReadOnlyListEqualityComparer<NeslType>()),
                    null
                );
            }

            return genericMakedTypes;
        }
    }

    protected NeslType(NeslAssembly assembly, string fullName, NeslType? genericMakedFrom = null) {
        Assembly = assembly;
        FullName = fullName;
        GenericMakedFrom = genericMakedFrom;
    }

    /// <summary>
    /// Constructs <see cref="NeslType"/> with given <paramref name="typeArguments"/>
    /// from this generic <see cref="NeslType"/>.
    /// </summary>
    /// <param name="typeArguments"><see cref="NeslType"/>s which replaces generic type parameters.</param>
    /// <returns>Final <see cref="NeslType"/> with given <paramref name="typeArguments"/>.</returns>
    /// <exception cref="InvalidOperationException">This <see cref="NeslType"/> is not generic.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The number of given <paramref name="typeArguments"/> does not match
    /// the defined number of generic type parameters.
    /// </exception>
    public virtual NeslType MakeGeneric(params NeslType[] typeArguments) {
        if (!IsGeneric)
            throw new InvalidOperationException($"Type {Name} is not generic.");

        if (GenericTypeParameters.Count() != typeArguments.Length) {
            throw new ArgumentOutOfRangeException(
                nameof(typeArguments),
                $"The number of given {nameof(typeArguments)} does not match the " +
                "defined number of generic type parameters."
            );
        }

        return GenericMakedTypes.GetOrAdd(typeArguments, _ => new Lazy<NeslType>(() => {
            Dictionary<NeslGenericTypeParameter, NeslType> targetTypes =
                UnsafeTargetTypesFromMakeGeneric(GenericTypeParameters, typeArguments, out bool isFullyConstructed);

            // Create not fully generic maked type.
            if (!isFullyConstructed)
                return new NotFullyConstructedGenericNeslType(this, targetTypes, typeArguments.ToImmutableArray());

            // Create fully generic maked type.
            SerializedNeslType type = UnsafeCreateTypeFromMakeGeneric(typeArguments);
            type.UnsafeInitializeTypeFromMakeGeneric(targetTypes);
            return type;
        })).Value;
    }

    /// <summary>
    /// Finds <see cref="NeslField"/> with given <paramref name="name"/> in this <see cref="NeslType"/>.
    /// </summary>
    /// <param name="name">Name of the searched <see cref="NeslField"/>.</param>
    /// <returns><see cref="NeslField"/> when type was found, <see langword="null"/> when not.</returns>
    public NeslField? GetField(string name) {
        return Fields.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Finds <see cref="NeslMethod"/> with given <paramref name="name"/> in this <see cref="NeslType"/>.
    /// </summary>
    /// <param name="name">Name of the searched <see cref="NeslMethod"/>.</param>
    /// <returns><see cref="NeslMethod"/> when type was found, <see langword="null"/> when not.</returns>
    public NeslMethod? GetMethod(string name) {
        return Methods.Where(x => x.Name == name).MinBy(x => x.Guid);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return FullName;
    }

    internal virtual void PrepareHeader(SerializationUsed used, NeslAssembly serializedAssembly) {
        used.Add(this, GenericTypeParameters);

        if (!IsGenericMaked)
            return;

        used.Add(this, GenericMakedFrom!);
    }

    internal virtual bool SerializeHeader(NeslAssembly serializedAssembly, SerializationWriter writer) {
        if (serializedAssembly != Assembly) {
            writer.WriteBool(false);
            writer.WriteString(Assembly.Name);
            writer.WriteString(FullName);

            writer.WriteBool(IsGenericMaked);
            if (IsGenericMaked)
                writer.WriteEnumerable(GenericMakedTypeParameters.Select(serializedAssembly.GetLocalTypeId));

            return false;
        }

        writer.WriteBool(true);
        if (IsGenericMaked) {
            writer.WriteUInt8((byte)NeslTypeUsageKind.GenericMaked);
            writer.WriteUInt64(Assembly.GetLocalTypeId(GenericMakedFrom!));
            return true;
        }

        writer.WriteUInt8((byte)NeslTypeUsageKind.Normal);
        writer.WriteUInt8((byte)Kind);
        writer.WriteString(FullName);
        writer.WriteEnumerable(Attributes);
        return false;
    }

    internal void SerializeBody(SerializationUsed used, SerializationWriter writer) {
        used.Add(this, GenericTypeParameters);
        writer.WriteEnumerable(GenericTypeParameters.Select(Assembly.GetLocalTypeId));;

        writer.WriteInt32(Fields.Count);
        foreach (NeslField field in Fields)
            field.Serialize(used, writer);

        used.Add(this, Interfaces);
        writer.WriteEnumerable(Interfaces.Select(Assembly.GetLocalTypeId));
    }

    internal void SerializeMethods(SerializationWriter writer) {
        writer.WriteInt32(Methods.Count());
        foreach (NeslMethod method in Methods)
            method.Serialize(writer);
    }

    internal ulong GetSize() {
        if (Attributes.TryCastAnyAttribute(out SizeAttribute? attribute))
            return attribute.Size;

        ulong size = 0;
        foreach (NeslField field in Fields) {
            if (!field.IsStatic)
                size += field.FieldType.GetSize();
        }

        return size;
    }

    internal void UnsafeAddToGenericMaked(SerializedNeslType type) {
        if (!GenericMakedTypes.TryAdd(type.GenericMakedTypeParameters.ToArray(), new Lazy<NeslType>(type)))
            throw new UnreachableException();
    }

    internal SerializedNeslType UnsafeCreateTypeFromMakeGeneric(NeslType[] typeArguments) {
        return new SerializedNeslType(
            Assembly, Kind, FullName, Array.Empty<NeslAttribute>(), this, typeArguments
        );
    }

    internal Dictionary<NeslGenericTypeParameter, NeslType> UnsafeTargetTypesFromMakeGeneric(
        IEnumerable<NeslGenericTypeParameter> genericTypeParameters, NeslType[] typeArguments,
        out bool isFullyConstructed
    ) {
        isFullyConstructed = true;
        Dictionary<NeslGenericTypeParameter, NeslType> targetTypes =
            new Dictionary<NeslGenericTypeParameter, NeslType>();

        int i = 0;
        foreach (NeslGenericTypeParameter genericTypeParameter in genericTypeParameters) {
            NeslType typeArgument = typeArguments[i++];

            genericTypeParameter.AssertConstraints(typeArgument);
            targetTypes.Add(genericTypeParameter, typeArgument);

            if (isFullyConstructed && typeArgument is NeslGenericTypeParameter)
                isFullyConstructed = false;
        }

        return targetTypes;
    }

}
