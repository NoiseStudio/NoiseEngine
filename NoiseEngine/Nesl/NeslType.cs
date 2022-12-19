using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools.Generics;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public virtual string Name => FullName.Substring(FullName.LastIndexOf(Delimiter) + 1);
    public virtual string Namespace => FullName.Substring(0, FullName.LastIndexOf(Delimiter));

    public NeslAssembly Assembly { get; }
    public string FullName { get; }
    public string FullNameWithAssembly => $"{Assembly.Name}::{FullName}";

    public bool IsGeneric => GenericTypeParameters.Any();
    public bool IsClass => !IsValueType;
    public bool IsValueType => Attributes.HasAnyAttribute(nameof(ValueTypeAttribute));

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

    protected NeslType(NeslAssembly assembly, string fullName) {
        Assembly = assembly;
        FullName = fullName;
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
                new Dictionary<NeslGenericTypeParameter, NeslType>();

            bool hasGenericTypeArguments = false;

            int i = 0;
            foreach (NeslGenericTypeParameter genericTypeParameter in GenericTypeParameters) {
                NeslType typeArgument = typeArguments[i++];

                genericTypeParameter.AssertConstraints(typeArgument);
                targetTypes.Add(genericTypeParameter, typeArgument);

                hasGenericTypeArguments |= typeArgument is NeslGenericTypeParameter;
            }

            // Create not fully generic maked type.
            if (hasGenericTypeArguments)
                return new NotFullyConstructedGenericNeslType(this, typeArguments.ToImmutableArray());

            // Create fully generic maked type.
            SerializedNeslType type = new SerializedNeslType(
                Assembly, FullName, GenericHelper.RemoveGenericsFromAttributes(Attributes, targetTypes)
            );

            // Create fields.
            Dictionary<uint, NeslField> idToField = new Dictionary<uint, NeslField>();

            List<NeslField> fields = new List<NeslField>();
            foreach (NeslField field in Fields) {
                fields.Add(new SerializedNeslField(
                    type, field.Name, GenericHelper.GetFinalType(field.FieldType, targetTypes),
                    GenericHelper.RemoveGenericsFromAttributes(field.Attributes, targetTypes)
                ));

                uint id = (uint)idToField.Count;
                idToField.Add(id, field);
            }

            type.SetFields(fields.ToArray(), idToField.ToImmutableDictionary());

            // Create methods.
            List<NeslMethod> methods = new List<NeslMethod>();

            foreach (NeslMethod method in Methods) {
                if (method.IsGeneric) {
                    methods.Add(new GenericNeslMethodInConstructedGenericNeslType(type, method, targetTypes));
                    continue;
                }

                // Return and parameter types.
                NeslType? methodReturnType = method.ReturnType;
                if (methodReturnType is not null)
                    methodReturnType = GenericHelper.GetFinalType(methodReturnType, targetTypes);

                NeslType[] methodParameterTypes = new NeslType[method.ParameterTypes.Count];

                i = 0;
                foreach (NeslType parameterType in method.ParameterTypes)
                    methodParameterTypes[i++] = GenericHelper.GetFinalType(parameterType, targetTypes);

                // Construct new method.
                methods.Add(new SerializedNeslMethod(
                    type,
                    method.Name,
                    methodReturnType,
                    methodParameterTypes,
                    GenericHelper.RemoveGenericsFromAttributes(method.Attributes, targetTypes),
                    GenericHelper.RemoveGenericsFromAttributes(method.ReturnValueAttributes, targetTypes),
                    method.ParameterAttributes.Select(x => GenericHelper.RemoveGenericsFromAttributes(x, targetTypes)),
                    method.GenericTypeParameters.ToArray(),
                    GenericIlGenerator.RemoveGenerics(method, targetTypes)
                ));
            }

            type.SetMethods(methods.ToArray());

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

    internal abstract NeslField GetField(uint localFieldId);

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

}
