using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ConstValueToken(
    CodePointer Pointer, ConstValueType Type, object Value
) : IParserToken<ConstValueToken>, IValueContent {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ConstValueToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedConstValue);
            return false;
        }

        // Check negative.
        bool negative = false;
        Token? previousToken = null;
        if (token.Type == TokenType.Subtraction) {
            negative = true;
            previousToken = token;
            if (!buffer.TryReadNext(out token)) {
                result = default;
                error = new CompilationError(token, CompilationErrorType.ExpectedConstValue);
                return false;
            }
        }

        switch (token.Type) {
            case TokenType.Word:
                return GetFromWord(buffer, previousToken, token, negative, out result, out error);
            case TokenType.StringContent:
                return GetFromStringContent(buffer, previousToken, token, negative, out result, out error);
            default:
                result = default;
                error = new CompilationError(token, CompilationErrorType.ExpectedConstValue);
                return false;
        }
    }

    private static bool GetFromWord(
        TokenBuffer buffer, Token? previousToken, Token token, bool negative,
        [NotNullWhen(true)] out ConstValueToken result, out CompilationError error
    ) {
        // Bool.
        if (token.Value == "true" || token.Value == "false") {
            if (negative) {
                result = default;
                error = new CompilationError(
                    previousToken!.Value, CompilationErrorType.SubtractionOperatorNotMatchExpression
                );
                return false;
            }

            result = new ConstValueToken(token.Pointer, ConstValueType.Bool, token.Value == "true");
            error = default;
            return true;
        }

        // Integers.
        int index = buffer.Index;
        if (!buffer.TryReadNext(TokenType.Dot, out _)) {
            buffer.Index = index;
            if (!ulong.TryParse(token.Value, NumberStyles.Any, null, out ulong value)) {
                result = default;
                error = new CompilationError(token, CompilationErrorType.ExpectedNumber);
                return false;
            }

            if (negative)
                result = new ConstValueToken(token.Pointer, ConstValueType.Integer, -(long)value);
            else
                result = new ConstValueToken(token.Pointer, ConstValueType.UnsignedInteger, value);
            error = default;
            return true;
        }

        // Float.
        if (!buffer.TryReadNext(TokenType.Word, out Token floatToken)) {
            result = default;
            error = new CompilationError(floatToken, CompilationErrorType.ExpectedNumber);
            return false;
        }
        if (!double.TryParse(
            $"{token.Value}.{floatToken.Value}", NumberStyles.Any, CultureInfo.InvariantCulture, out double floatValue
        )) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedNumber);
            return false;
        }

        if (negative)
            result = new ConstValueToken(token.Pointer, ConstValueType.Float, -floatValue);
        else
            result = new ConstValueToken(token.Pointer, ConstValueType.Float, floatValue);
        error = default;
        return true;
    }

    private static bool GetFromStringContent(
        TokenBuffer buffer, Token? previousToken, Token token, bool negative,
        [NotNullWhen(true)] out ConstValueToken result, out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.StringEnd, out Token endToken) || endToken.Path != token.Path) {
            result = default;
            error = new CompilationError(endToken, CompilationErrorType.ExpectedQuotationMark);
            return false;
        }

        if (negative) {
            result = default;
            error = new CompilationError(
                previousToken!.Value, CompilationErrorType.SubtractionOperatorNotMatchExpression
            );
            return false;
        }

        result = new ConstValueToken(token.Pointer, ConstValueType.String, token.Value!);
        error = default;
        return true;
    }

    public bool ToInt64(out long value, out CompilationError error) {
        switch (Type) {
            case ConstValueType.Integer:
                value = (long)Value;
                error = default;
                return true;
            case ConstValueType.UnsignedInteger:
                ulong a = (ulong)Value;
                if (a > long.MaxValue)
                    break; // TODO: Add out of range error.

                value = (long)a;
                error = default;
                return true;
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedInt64, Value.ToString() ?? "");
        return false;
    }

    public bool ToInt32(out int value, out CompilationError error) {
        switch (Type) {
            case ConstValueType.Integer:
                long b = (long)Value;
                if (b < int.MinValue || b > int.MaxValue)
                    break; // TODO: Add out of range error.

                value = (int)b;
                error = default;
                return true;
            case ConstValueType.UnsignedInteger:
                ulong a = (ulong)Value;
                if (a > int.MaxValue)
                    break; // TODO: Add out of range error.

                value = (int)a;
                error = default;
                return true;
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedInt32, Value.ToString() ?? "");
        return false;
    }

    public bool ToInt16(out short value, out CompilationError error) {
        switch (Type) {
            case ConstValueType.Integer:
                long b = (long)Value;
                if (b < short.MinValue || b > short.MaxValue)
                    break; // TODO: Add out of range error.

                value = (short)b;
                error = default;
                return true;
            case ConstValueType.UnsignedInteger:
                ulong a = (ulong)Value;
                if (a > (ulong)short.MaxValue)
                    break; // TODO: Add out of range error.

                value = (short)a;
                error = default;
                return true;
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedInt16, Value.ToString() ?? "");
        return false;
    }

    public bool ToInt8(out sbyte value, out CompilationError error) {
        switch (Type) {
            case ConstValueType.Integer:
                long b = (long)Value;
                if (b < sbyte.MinValue || b > sbyte.MaxValue)
                    break; // TODO: Add out of range error.

                value = (sbyte)b;
                error = default;
                return true;
            case ConstValueType.UnsignedInteger:
                ulong a = (ulong)Value;
                if (a > (ulong)sbyte.MaxValue)
                    break; // TODO: Add out of range error.

                value = (sbyte)a;
                error = default;
                return true;
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedInt8, Value.ToString() ?? "");
        return false;
    }

    public bool ToUInt64(out ulong value, out CompilationError error) {
        if (Type == ConstValueType.UnsignedInteger) {
            value = (ulong)Value;
            error = default;
            return true;
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedUInt64, Value.ToString() ?? "");
        return false;
    }

    public bool ToUInt32(out uint value, out CompilationError error) {
        if (Type == ConstValueType.UnsignedInteger) {
            ulong a = (ulong)Value;
            if (a <= uint.MaxValue) {
                value = (uint)a;
                error = default;
                return true;
            }
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedUInt32, Value.ToString() ?? "");
        return false;
    }

    public bool ToUInt16(out ushort value, out CompilationError error) {
        if (Type == ConstValueType.UnsignedInteger) {
            ulong a = (ulong)Value;
            if (a <= ushort.MaxValue) {
                value = (ushort)a;
                error = default;
                return true;
            }
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedUInt16, Value.ToString() ?? "");
        return false;
    }

    public bool ToUInt8(out byte value, out CompilationError error) {
        if (Type == ConstValueType.UnsignedInteger) {
            ulong a = (ulong)Value;
            if (a <= byte.MaxValue) {
                value = (byte)a;
                error = default;
                return true;
            }
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedUInt8, Value.ToString() ?? "");
        return false;
    }

    public bool ToFloat64(out double value, out CompilationError error) {
        if (Type == ConstValueType.Float) {
            value = (double)Value;
            error = default;
            return true;
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedFloat64, Value.ToString() ?? "");
        return false;
    }

    public bool ToFloat32(out float value, out CompilationError error) {
        if (Type == ConstValueType.Float) {
            double a = (double)Value;
            if (a >= float.MinValue || a <= float.MaxValue) {
                value = (float)a;
                error = default;
                return true;
            }
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedFloat32, Value.ToString() ?? "");
        return false;
    }

    public bool ToFloat16(out Half value, out CompilationError error) {
        if (Type == ConstValueType.Float) {
            double a = (double)Value;
            if (a >= (double)Half.MinValue || a <= (double)Half.MaxValue) {
                value = (Half)a;
                error = default;
                return true;
            }
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedFloat16, Value.ToString() ?? "");
        return false;
    }

    public bool ToBool(out bool value, out CompilationError error) {
        if (Type == ConstValueType.Bool) {
            value = (bool)Value;
            error = default;
            return true;
        }

        value = default;
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedBool, Value.ToString() ?? "");
        return false;
    }

    public bool ToString(out string value, out CompilationError error) {
        if (Type == ConstValueType.String) {
            value = (string)Value;
            error = default;
            return true;
        }

        value = "";
        error = new CompilationError(Pointer, CompilationErrorType.ExpectedString, Value.ToString() ?? "");
        return false;
    }

}
