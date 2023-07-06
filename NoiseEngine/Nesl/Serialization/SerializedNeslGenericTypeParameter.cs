using NoiseEngine.Nesl.CompilerTools.Generics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.Serialization;

internal sealed class SerializedNeslGenericTypeParameter : NeslGenericTypeParameter {

    private Dictionary<NeslType, IReadOnlyList<NeslMethod>>? constraintMethods;

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IEnumerable<NeslType> Interfaces { get; }
    public override IEnumerable<NeslMethod> Methods { get; }

    internal override IReadOnlyDictionary<NeslType, IReadOnlyList<NeslMethod>> ConstraintMethods =>
        constraintMethods ?? throw new NullReferenceException();

    public SerializedNeslGenericTypeParameter(
        NeslAssembly assembly, string name, IEnumerable<NeslAttribute> attributes, IEnumerable<NeslType> interfaces
    ) : base(assembly, name) {
        Attributes = attributes;
        Interfaces = interfaces;
        Methods = CreateMethodsFromInterfaces();
    }

    public void SetNestedConstraintInterfaces(IEnumerable<NeslType> nestedConstraintInterfaces) {
        Dictionary<NeslType, IReadOnlyList<NeslMethod>> constraintMethods =
            new Dictionary<NeslType, IReadOnlyList<NeslMethod>>();

        foreach (NeslType i in nestedConstraintInterfaces) {
            constraintMethods.Add(i, i.Methods.Select(x => new NeslGenericTypeParameterImplementedMethod(
                this, x
            )).ToArray());
        }

        this.constraintMethods = constraintMethods;
    }

}
