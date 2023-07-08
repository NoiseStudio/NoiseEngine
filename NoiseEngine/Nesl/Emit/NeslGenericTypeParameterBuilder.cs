using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Nesl.CompilerTools.Generics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Nesl.Emit;

public class NeslGenericTypeParameterBuilder : NeslGenericTypeParameter {

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();
    private readonly ConcurrentHashSet<NeslType> constraints = new ConcurrentHashSet<NeslType>();

    private NeslMethod[]? methods;
    private ConcurrentDictionary<NeslType, IReadOnlyList<NeslMethod>>? constraintMethods;

    public override IEnumerable<NeslAttribute> Attributes => attributes;
    public override IEnumerable<NeslType> Interfaces => constraints;

    public override IEnumerable<NeslMethod> Methods {
        get {
            if (methods is not null)
                return methods;

            lock (constraints) {
                methods ??= CreateMethodsFromInterfaces();
                return methods;
            }
        }
    }

    internal override IReadOnlyDictionary<NeslType, IReadOnlyList<NeslMethod>> ConstraintMethods =>
        constraintMethods is null ? Enumerable.Empty<KeyValuePair<NeslType, IReadOnlyList<NeslMethod>>>()
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : constraintMethods;

    internal NeslGenericTypeParameterBuilder(NeslAssembly assembly, string name) : base(assembly, name) {
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslGenericTypeParameterBuilder"/>.
    /// </summary>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Given <paramref name="attribute"/> cannot be assigned to this target.
    /// </exception>
    public void AddAttribute(NeslAttribute attribute) {
        if (!attribute.Targets.HasFlag(AttributeTargets.GenericTypeParameter)) {
            throw new InvalidOperationException(
                $"The `{attribute}` attribute cannot be assigned to a generic type parameter.");
        }

        attributes.Add(attribute);
    }

    /// <summary>
    /// Implements given <paramref name="constraint"/> in this <see cref="NeslGenericTypeParameterBuilder"/>.
    /// </summary>
    /// <param name="constraint">Constraint which this <see cref="NeslGenericTypeParameterBuilder"/> implements.</param>
    public void AddConstraint(NeslType constraint) {
        if (!constraint.IsInterface)
            throw new ArgumentException("Given type is not an interface.", nameof(constraint));
        constraints.Add(constraint);
    }

    internal IReadOnlyList<NeslMethod> GetOrAddNestedConstraint(NeslType i) {
        if (!i.IsInterface)
            throw new ArgumentException("Given type is not an interface.", nameof(i));

        if (constraintMethods is null) {
            Interlocked.CompareExchange(
                ref constraintMethods, new ConcurrentDictionary<NeslType, IReadOnlyList<NeslMethod>>(), null
            );
        }

        return constraintMethods.GetOrAdd(i, _ => i.Methods.Select(x => new NeslGenericTypeParameterImplementedMethod(
            this, x
        )).ToArray());
    }

}
