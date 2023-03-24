using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal record ValueToken(
    OperatorType LeftOperator, IValueContent Value, OperatorType? RightOperator, ValueToken? NextValue
) : IParserToken<ValueToken>, IValueContent {

    public OperatorType Operator { get; private set; }

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ValueToken? result,
        out CompilationError error
    ) {
        OperatorToken? leftOperator = null;
        if (
            OperatorToken.Parse(buffer, errorMode, out OperatorToken tempOperator, out _) &&
            !tempOperator.IsAssigment && (
                tempOperator.Type == OperatorType.Increment ||
                tempOperator.Type == OperatorType.Decrement ||
                tempOperator.Type == OperatorType.Subtraction ||
                tempOperator.Type == OperatorType.Negation ||
                tempOperator.Type == OperatorType.Bitwise
            )
        ) {
            leftOperator = tempOperator;
        } else {
            buffer.Index--;
        }

        if (!buffer.TryReadNext(out Token token)) {
            result = null;
            error = new CompilationError(token, CompilationErrorType.ExpectedValue);
            return false;
        }

        IValueContent value;
        if (token.Type == TokenType.Word) {
            bool isNew = token.Value == "new";
            if (!isNew)
                buffer.Index--;

            if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken typeIdentifier, out error)) {
                result = default;
                return false;
            }

            RoundBracketsToken? roundBrackets = null;
            int index = buffer.Index;
            if (RoundBracketsToken.Parse(buffer, errorMode, out RoundBracketsToken tempRoundBrackets, out _))
                roundBrackets = tempRoundBrackets;
            else
                buffer.Index = index;

            CurlyBracketsToken? curlyBrackets = null;
            if (isNew) {
                index = buffer.Index;
                if (CurlyBracketsToken.Parse(buffer, errorMode, out CurlyBracketsToken tempCurlyBrackets, out _))
                    curlyBrackets = tempCurlyBrackets;
                else
                    buffer.Index = index;
            }

            ValueToken? indexer = null;
            index = buffer.Index;
            if (buffer.TryReadNext(TokenType.SquareBracketOpening, out token)) {
                if (!Parse(buffer, errorMode, out indexer, out error)) {
                    result = default;
                    return false;
                }

                if (token.Length <= 1) {
                    result = null;
                    error = new CompilationError(token, CompilationErrorType.ExpectedClosingSquareBracket);
                    return false;
                }
            } else {
                buffer.Index = index;
            }
        } else if (token.Type == TokenType.RoundBracketOpening) {
            if (Parse(buffer, errorMode, out ValueToken? innerValue, out error)) {
                value = innerValue;
            } else {
                result = default;
                return false;
            }

            if (token.Length <= 1) {
                result = null;
                error = new CompilationError(token, CompilationErrorType.ExpectedClosingRoundBracket);
                return false;
            }
        } else {
            result = null;
            error = new CompilationError(token, CompilationErrorType.ExpectedValue);
            return false;
        }

        OperatorToken? rightOperator = null;
        if (
            OperatorToken.Parse(buffer, errorMode, out tempOperator, out _) &&
            !tempOperator.IsAssigment && (
                tempOperator.Type == OperatorType.Increment ||
                tempOperator.Type == OperatorType.Decrement
            )
        ) {
            rightOperator = tempOperator;
        } else {
            buffer.Index--;
        }

        ValueToken? nextValue = null;
        if (OperatorToken.Parse(buffer, errorMode, out tempOperator, out _) && !tempOperator.IsAssigment) {
            if (Parse(buffer, errorMode, out nextValue, out error)) {
                nextValue.Operator = tempOperator.Type;
            } else {
                result = null;
                return false;
            }
        } else {
            buffer.Index--;
        }

        result = new ValueToken(
            leftOperator?.Type ?? OperatorType.None, value, rightOperator?.Type ?? OperatorType.None, nextValue
        );
        error = default;
        return true;
    }

}
