using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal static class MethodConstructor {

    public static void SetMethodGenericConstrains(
        Parser parser, NeslMethodBuilder method, IReadOnlyList<ConstraintToken> constraints
    ) {
        foreach (ConstraintToken constraint in constraints) {
            if (constraint.GenericParameter.GenericTokens.Count != 0) {
                parser.Throw(new CompilationError(
                    constraint.GenericParameter.GenericTokens[0].Pointer,
                    CompilationErrorType.ConstraintGenericParameterNotAllowed,
                    constraint.GenericParameter.GenericTokens[0]
                ));
                continue;
            }

            List<NeslType> c = new List<NeslType>();
            foreach (TypeIdentifierToken typeIdentifier in constraint.Constraints) {
                if (!parser.TryGetType(typeIdentifier, out NeslType? type))
                    continue;
                if (!type.IsInterface) {
                    parser.Throw(new CompilationError(
                        typeIdentifier.Pointer, CompilationErrorType.ConstraintTypeMustBeAInterface, typeIdentifier
                    ));
                }

                c.Add(type);
            }

            NeslGenericTypeParameter? parameter =
                method.GenericTypeParameters.FirstOrDefault(x => x.Name == constraint.GenericParameter.Identifier);
            if (parameter is not null) {
                if (parameter is not NeslGenericTypeParameterBuilder builder)
                    throw new UnreachableException();

                foreach (NeslType type in c)
                    builder.AddConstraint(type);
                continue;
            }

            parameter =
                method.Type.GenericTypeParameters.FirstOrDefault(x => x.Name == constraint.GenericParameter.Identifier);
            if (parameter is not null) {
                method.SetTypeGenericConstraints(parameter, c);
                continue;
            }

            parser.Throw(new CompilationError(
                constraint.GenericParameter.Pointer, CompilationErrorType.GenericParameterNotFound,
                constraint.GenericParameter
            ));
        }
    }

}
