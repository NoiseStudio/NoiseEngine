using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.CompilerTools.Generics;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Nesl;

public abstract class NeslMethod : INeslGenericTypeParameterOwner {

    private ConcurrentDictionary<NeslType[], Lazy<NeslMethod>>? genericMakedMethods;

    public abstract IEnumerable<NeslAttribute> Attributes { get; }
    public abstract IEnumerable<NeslAttribute> ReturnValueAttributes { get; }
    public abstract IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes { get; }
    public abstract IEnumerable<NeslGenericTypeParameter> GenericTypeParameters { get; }

    protected abstract IlContainer IlContainer { get; }

    public NeslType? ReturnType { get; private protected set; }
    public IReadOnlyList<NeslType> ParameterTypes { get; private protected set; }
    public NeslType Type { get; }
    public string Name { get; }
    public Guid Guid { get; }

    public NeslAssembly Assembly => Type.Assembly;
    public string FullName => $"{Type.FullName}::{Name}";

    public bool IsGeneric => GenericTypeParameters.Any();
    public bool IsStatic => Attributes.HasAnyAttribute(nameof(StaticAttribute));

    private ConcurrentDictionary<NeslType[], Lazy<NeslMethod>> GenericMakedMethods {
        get {
            if (genericMakedMethods is null) {
                Interlocked.CompareExchange(
                    ref genericMakedMethods,
                    new ConcurrentDictionary<NeslType[], Lazy<NeslMethod>>(
                        new ReadOnlyListEqualityComparer<NeslType>()
                    ),
                    null
                );
            }

            return genericMakedMethods;
        }
    }

    protected NeslMethod(NeslType type, string name, NeslType? returnType, NeslType[] parameterTypes) {
        Type = type;
        Name = name;
        ReturnType = returnType;
        ParameterTypes = parameterTypes;

        Guid = Guid.NewGuid();
    }

    /// <summary>
    /// Creates new <see cref="NeslMethod"/> with data from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"><see cref="SerializationReader"/>.</param>
    /// <returns>New <see cref="NeslMethod"/> with data from <paramref name="reader"/>.</returns>
    internal static NeslMethod Deserialize(SerializationReader reader) {
        NeslAssembly assembly = reader.GetFromStorage<NeslAssembly>();
        return new SerializedNeslMethod(
            assembly.GetType(reader.ReadUInt64()),
            reader.ReadString(),
            reader.ReadBool() ? assembly.GetType(reader.ReadUInt64()) : null,
            reader.ReadEnumerableUInt64().Select(assembly.GetType).ToArray(),
            reader.ReadEnumerable<NeslAttribute>().ToArray(),
            reader.ReadEnumerable<NeslAttribute>().ToArray(),
            DeserializeParameterAttributes(reader).ToArray(),
            reader.ReadEnumerableUInt64().Select(assembly.GetType).Cast<NeslGenericTypeParameter>().ToArray(),
            reader.ReadObject<IlContainer>()
        );
    }

    private static IEnumerable<IEnumerable<NeslAttribute>> DeserializeParameterAttributes(SerializationReader reader) {
        int length = reader.ReadInt32();
        for (int i = 0; i < length; i++)
            yield return reader.ReadEnumerable<NeslAttribute>().ToArray();
    }

    internal void Serialize(SerializationUsed used, SerializationWriter writer) {
        writer.WriteUInt64(Assembly.GetLocalTypeId(Type));
        writer.WriteString(Name);

        if (ReturnType is null) {
            writer.WriteBool(false);
        } else {
            writer.WriteBool(true);
            used.Add(Type, ReturnType);
            writer.WriteUInt64(Assembly.GetLocalTypeId(ReturnType));
        }

        used.Add(Type, ParameterTypes);
        writer.WriteEnumerable(ParameterTypes.Select(Assembly.GetLocalTypeId));

        writer.WriteEnumerable(Attributes);
        writer.WriteEnumerable(ReturnValueAttributes);

        writer.WriteInt32(ParameterAttributes.Count);
        foreach (IEnumerable<NeslAttribute> parameterAttribute in ParameterAttributes)
            writer.WriteEnumerable(parameterAttribute);

        used.Add(Type, GenericTypeParameters);
        writer.WriteEnumerable(GenericTypeParameters.Select(Assembly.GetLocalTypeId));

        writer.WriteObject(IlContainer);
    }

    /// <summary>
    /// Constructs <see cref="NeslMethod"/> with given <paramref name="typeArguments"/>
    /// from this generic <see cref="NeslMethod"/>.
    /// </summary>
    /// <param name="typeArguments"><see cref="NeslType"/>s which replaces generic type parameters.</param>
    /// <returns>Final <see cref="NeslType"/> with given <paramref name="typeArguments"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Type"/> is generic.</exception>
    /// <exception cref="InvalidOperationException">This <see cref="NeslMethod"/> is not generic.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The number of given <paramref name="typeArguments"/> does not match
    /// the defined number of generic type parameters.
    /// </exception>
    public virtual NeslMethod MakeGeneric(params NeslType[] typeArguments) {
        return MakeGenericWorker(typeArguments, null);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return FullName;
    }

    internal IEnumerable<Instruction> GetInstructions() {
        return IlContainer.GetInstructions();
    }

    internal IlContainer GetIlContainer() {
        return IlContainer;
    }

    internal NeslMethod MakeGenericWorker(
        NeslType[] typeArguments,
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType>? genericReflectedTypeTargetTypes
    ) {
        if (Type.IsGeneric) {
            throw new InvalidOperationException(
                "Unable to construct a generic method on an unconstructed generic type.");
        }

        if (!IsGeneric)
            throw new InvalidOperationException($"Method {Name} is not generic.");

        if (GenericTypeParameters.Count() != typeArguments.Length) {
            throw new ArgumentOutOfRangeException(
                nameof(typeArguments),
                $"The number of given {nameof(typeArguments)} does not match the " +
                "defined number of generic type parameters."
            );
        }

        return GenericMakedMethods.GetOrAdd(typeArguments, _ => new Lazy<NeslMethod>(() => {
            Dictionary<NeslGenericTypeParameter, NeslType> targetTypes =
                new Dictionary<NeslGenericTypeParameter, NeslType>();

            bool hasGenericTypeArguments = false;

            int i = 0;
            foreach (NeslGenericTypeParameter genericTypeParameter in GenericTypeParameters) {
                NeslType typeArgument = typeArguments[i++];

                genericTypeParameter.AssertConstraints(typeArgument);
                targetTypes.Add(genericTypeParameter, typeArgument);

                hasGenericTypeArguments |= typeArgument is NeslGenericTypeParameter;
            }

            // Create not fully generic constructed type.
            if (hasGenericTypeArguments)
                return new NotFullyConstructedGenericNeslMethod(this, typeArguments.ToImmutableArray());

            if (genericReflectedTypeTargetTypes is not null) {
                foreach ((NeslGenericTypeParameter key, NeslType value) in genericReflectedTypeTargetTypes) {
                    if (targetTypes.Keys.All(x => x.Name != key.Name))
                        targetTypes.Add(key, value);
                }
            }

            // Return and parameter types.
            NeslType? methodReturnType = ReturnType;
            if (methodReturnType is not null)
                methodReturnType = GenericHelper.GetFinalType(Type, Type, methodReturnType, targetTypes);

            NeslType[] methodParameterTypes = new NeslType[ParameterTypes.Count];

            i = 0;
            foreach (NeslType parameterType in ParameterTypes)
                methodParameterTypes[i++] = GenericHelper.GetFinalType(Type, Type, parameterType, targetTypes);

            // Construct new method.
            return new SerializedNeslMethod(
                Type,
                Name,
                methodReturnType,
                methodParameterTypes,
                GenericHelper.RemoveGenericsFromAttributes(Attributes, targetTypes),
                GenericHelper.RemoveGenericsFromAttributes(ReturnValueAttributes, targetTypes),
                ParameterAttributes.Select(x => GenericHelper.RemoveGenericsFromAttributes(x, targetTypes)),
                Array.Empty<NeslGenericTypeParameter>(),
                GenericIlGenerator.RemoveGenerics(Type, Type, this, targetTypes)
            );
        })).Value;
    }

}
