using NoiseEngine.Nesl.Runtime;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class NeslMethodBuilder : NeslMethod {

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();

    public IlGenerator IlGenerator { get; }

    public override IEnumerable<NeslAttribute> Attributes => attributes;

    protected override IlContainer IlContainer => IlGenerator;

    internal NeslMethodBuilder(NeslTypeBuilder type, string name) : base(type, name) {
        IlGenerator = new IlGenerator((NeslAssemblyBuilder)type.Assembly);
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslAssemblyBuilder"/>.
    /// </summary>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    public void AddAttribute(NeslAttribute attribute) {
        attributes.Add(attribute);
    }

}
