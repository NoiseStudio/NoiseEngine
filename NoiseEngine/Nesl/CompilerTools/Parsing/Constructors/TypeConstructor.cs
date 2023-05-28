using NoiseEngine.Nesl.Emit;
using System;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal static class TypeConstructor {

    public static void Construct(Parser parser, NeslType type) {
        foreach (NeslMethod method in type.Interfaces.SelectMany(x => x.Methods)) {
            if (method.IsAbstract)
                AbstractMethod(parser, type, method);
            else
                throw new NotImplementedException(); // TODO: Implement interface defined method.
        }
    }

    private static void AbstractMethod(Parser parser, NeslType type, NeslMethod method) {
        NeslMethod? implemented = type.Methods.FirstOrDefault(
            x => x.Name == method.Name &&
            (x.Modifiers & ((NeslModifiers)uint.MaxValue ^ NeslModifiers.Abstract)) ==
            (method.Modifiers ^ NeslModifiers.Abstract) &&
            x.ReturnType == method.ReturnType &&
            x.ParameterTypes.SequenceEqual(method.ParameterTypes)
        );

        if (implemented is null) {
            parser.Throw(new CompilationError(
                parser.TypePointer, CompilationErrorType.AbstractMethodNotImplemented, method
            ));
            return;
        }

        if (implemented is not NeslMethodBuilder builder)
            throw new UnreachableException();

        foreach (NeslAttribute attribute in method.Attributes)
            builder.AddAttribute(attribute);
        foreach (NeslAttribute attribute in method.ReturnValueAttributes)
            builder.AddAttributeToReturnValue(attribute);
        for (int i = 0; i < method.ParameterAttributes.Count; i++) {
            foreach (NeslAttribute attribute in method.ParameterAttributes[i])
                builder.AddAttributeToParameter(i, attribute);
        }
    }

}
