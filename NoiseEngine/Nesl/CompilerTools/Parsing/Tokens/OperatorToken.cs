﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct OperatorToken(
    CodePointer Pointer, OperatorType Type, bool IsAssignment
) : IParserToken<OperatorToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out OperatorToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedOperator);
            return false;
        }

        OperatorType type;
        bool isAssignment = false;

        Token tokenRight;
        switch (token.Type) {
            case TokenType.Assigment:
                type = OperatorType.None;
                isAssignment = true;
                break;
            case TokenType.Addition:
                type = OperatorType.Addition;
                break;
            case TokenType.AdditionAssigment:
                type = OperatorType.Addition;
                isAssignment = true;
                break;
            case TokenType.Increment:
                type = OperatorType.Increment;
                break;
            case TokenType.Subtraction:
                type = OperatorType.Subtraction;
                break;
            case TokenType.SubtractionAssigment:
                type = OperatorType.Subtraction;
                isAssignment = true;
                break;
            case TokenType.Decrement:
                type = OperatorType.Decrement;
                break;
            case TokenType.Multiplication:
                type = OperatorType.Multiplication;
                break;
            case TokenType.MultiplicationAssigment:
                type = OperatorType.Multiplication;
                isAssignment = true;
                break;
            case TokenType.Exponentation:
                type = OperatorType.Exponentation;
                break;
            case TokenType.ExponentationAssigment:
                type = OperatorType.Exponentation;
                isAssignment = true;
                break;
            case TokenType.Division:
                type = OperatorType.Division;
                break;
            case TokenType.DivisionAssigment:
                type = OperatorType.Division;
                isAssignment = true;
                break;
            case TokenType.Remainder:
                type = OperatorType.Remainder;
                break;
            case TokenType.RemainderAssigment:
                type = OperatorType.Remainder;
                isAssignment = true;
                break;
            case TokenType.Equality:
                type = OperatorType.Equality;
                break;
            case TokenType.Inequality:
                type = OperatorType.Inequality;
                break;
            case TokenType.AngleBracketClosing:
                type = OperatorType.Greater;
                if (!buffer.TryReadNext(out tokenRight))
                    break;
                if (!IsNear(token, tokenRight)) {
                    buffer.Index--;
                    break;
                }

                if (tokenRight!.Type == TokenType.AngleBracketOpening) {
                    type = OperatorType.RightShift;
                    if (buffer.TryReadNext(TokenType.Assigment, out tokenRight) && IsNear(token, tokenRight))
                        isAssignment = true;
                    else if (tokenRight.Type != TokenType.None)
                        buffer.Index--;
                } else if (tokenRight.Type == TokenType.Assigment) {
                    type = OperatorType.GreaterOrEqual;
                } else {
                    buffer.Index--;
                }

                break;
            case TokenType.AngleBracketOpening:
                type = OperatorType.Less;
                if (!buffer.TryReadNext(out tokenRight))
                    break;
                if (!IsNear(token, tokenRight)) {
                    buffer.Index--;
                    break;
                }

                if (tokenRight!.Type == TokenType.AngleBracketOpening) {
                    type = OperatorType.LeftShift;
                    if (buffer.TryReadNext(TokenType.Assigment, out tokenRight) && IsNear(token, tokenRight))
                        isAssignment = true;
                    else if (tokenRight.Type != TokenType.None)
                        buffer.Index--;
                } else if (tokenRight.Type == TokenType.Assigment) {
                    type = OperatorType.LessOrEqual;
                } else {
                    buffer.Index--;
                }

                break;
            case TokenType.Negation:
                type = OperatorType.Negation;
                break;
            case TokenType.LogicalAnd:
                type = OperatorType.LogicalAnd;
                break;
            case TokenType.LogicalAndAssigment:
                type = OperatorType.LogicalAnd;
                isAssignment = true;
                break;
            case TokenType.LogicalOr:
                type = OperatorType.LogicalOr;
                break;
            case TokenType.LogicalOrAssigment:
                type = OperatorType.LogicalOr;
                isAssignment = true;
                break;
            case TokenType.LogicalXor:
                type = OperatorType.LogicalXor;
                break;
            case TokenType.LogicalXorAssigment:
                type = OperatorType.LogicalXor;
                isAssignment = true;
                break;
            case TokenType.ConditionalAnd:
                type = OperatorType.ConditionalAnd;
                break;
            case TokenType.ConditionalAndAssigment:
                type = OperatorType.ConditionalAnd;
                isAssignment = true;
                break;
            case TokenType.ConditionalOr:
                type = OperatorType.ConditionalOr;
                break;
            case TokenType.ConditionalOrAssigment:
                type = OperatorType.ConditionalOr;
                isAssignment = true;
                break;
            case TokenType.NullCoalescing:
                type = OperatorType.NullCoalescing;
                break;
            case TokenType.NullCoalescingAssigment:
                type = OperatorType.NullCoalescing;
                isAssignment = true;
                break;
            case TokenType.QuestionMark:
                type = OperatorType.Ternary;
                break;
            case TokenType.Colon:
                type = OperatorType.TernaryElse;
                break;
            case TokenType.Bitwise:
                type = OperatorType.Bitwise;
                break;
            case TokenType.BitwiseAssigment:
                type = OperatorType.Bitwise;
                isAssignment = true;
                break;
            default:
                result = default;
                error = new CompilationError(token, CompilationErrorType.ExpectedOperator);
                return false;
        }

        result = new OperatorToken(token.Pointer, type, isAssignment);
        error = default;
        return true;
    }

    private static bool IsNear(Token a, Token b) {
        return Math.Abs(a.Column - b.Column) == 1 && a.Line == b.Line;
    }

}
