using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Utils;

internal static class GetterSetterUtils {

    public static GetterSetterUtilsResult? FromCurlyBrackets(
        Parser parser, CurlyBracketsToken curlyBrackets, AttributesToken attributes
    ) {
        TokenBuffer buffer = curlyBrackets.Buffer;
        bool successful = buffer.TryReadNext(TokenType.Word, out Token word);

        if (!successful || word.Value! != "get") {
            parser.Throw(new CompilationError(word, CompilationErrorType.ExpectedGetter));
            successful = false;
        }
        if (successful && !SemicolonToken.Parse(buffer, parser.ErrorMode, out _, out CompilationError semicolonError)) {
            parser.Throw(semicolonError);
            buffer.Index--;
        }

        bool hasSetter = false;
        bool hasInitializer = false;
        if (buffer.TryReadNext(TokenType.Word, out word)) {
            hasSetter = word.Value! == "set";
            hasInitializer = word.Value! == "init";

            if (!hasSetter && !hasInitializer) {
                parser.Throw(new CompilationError(word, CompilationErrorType.ExpectedInitializerOrSetter));
            } else if (!SemicolonToken.Parse(buffer, parser.ErrorMode, out _, out semicolonError)) {
                parser.Throw(semicolonError);
                buffer.Index--;
            }
        }

        if (buffer.TryReadNext(out word))
            parser.Throw(new CompilationError(word, CompilationErrorType.UnexpectedExpression));

        if (!successful)
            return null;

        IReadOnlyList<NeslAttribute> a = attributes.Compile(parser, AttributeTargets.Method);
        return new GetterSetterUtilsResult(
            hasSetter, hasInitializer, null, a, null, a
        );
    }

}
