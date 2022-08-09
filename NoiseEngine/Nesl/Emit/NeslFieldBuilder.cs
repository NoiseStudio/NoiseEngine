using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class NeslFieldBuilder : NeslField {

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();

    public override IEnumerable<NeslAttribute> Attributes => attributes;

    internal NeslFieldBuilder(NeslType parentType, string name, NeslType fieldType)
        : base(parentType, name, fieldType) {
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslFieldBuilder"/>.
    /// </summary>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    public void AddAttribute(NeslAttribute attribute) {
        if (!attribute.Targets.HasFlag(AttributeTargets.Field)) {
            throw new InvalidOperationException(
                $"The `{attribute}` attribute cannot be assigned to a field.");
        }

        attributes.Add(attribute);
    }

}
