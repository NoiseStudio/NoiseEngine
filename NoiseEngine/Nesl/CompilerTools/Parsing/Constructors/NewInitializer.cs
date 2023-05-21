using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal static class NewInitializer {

    public static void Initialize(Parser parser, NeslType type, uint constructedId, TokenBuffer buffer) {
        while (true) {
            if (!NameToken.Parse(buffer, parser.ErrorMode, out NameToken name, out CompilationError error)) {
                parser.Throw(error);
                return;
            }

            if (!buffer.TryReadNext(TokenType.Assigment, out Token token)) {
                parser.Throw(new CompilationError(token, CompilationErrorType.ExpectedAssigmentOperator));
                return;
            }

            if (!ValueToken.Parse(buffer, parser.ErrorMode, out ValueToken? value, out error)) {
                parser.Throw(error);
                return;
            }

            uint valueId = ValueConstructor.Construct(value, parser).Id;
            NeslField? field = type.GetField(name.Name);
            if (field is null) {
                parser.Throw(new CompilationError(name.Pointer, CompilationErrorType.FieldNotFound, name.Name));
            } else {
                IlGenerator il = parser.CurrentMethod.IlGenerator;
                il.Emit(OpCode.SetField, constructedId, field.Id, valueId);
            }

            if (!buffer.HasNextTokens)
                break;

            if (!buffer.TryReadNext(TokenType.Comma, out token)) {
                parser.Throw(new CompilationError(token, CompilationErrorType.ExpectedComma));
                return;
            }
        }
    }

}
