using System;
using System.Collections.Generic;
using System.Text;

namespace NoiseEngine.Nesl.CompilerTools;

internal class Lexer {

    private readonly List<Token> tokens = new List<Token>();
    private readonly StringBuilder value = new StringBuilder();
    private readonly Stack<int> roundBracket = new Stack<int>();
    private readonly Stack<int> squareBracket = new Stack<int>();
    private readonly Stack<int> curlyBracket = new Stack<int>();
    private readonly Stack<int> angleBracket = new Stack<int>();

    private string path = null!;
    private uint line;
    private uint column;
    private uint startColumn;

    private bool isComment;
    private bool isMultiline;

    private char? lastChar;
    private char? secondLastChar;
    private bool clearLastChar;

    private uint operatorLine;
    private uint operatorColumn;

    public Token[] Lex(string path, string content) {
        this.path = path;

        line = 1;
        column = 0;

        isComment = false;
        isMultiline = false;

        lastChar = null;
        secondLastChar = null;
        clearLastChar = false;

        for (int i = 0; i < content.Length; i++) {
            char c = content[i];
            column++;
            AnalyzeChar(c);
        }
        AnalyzeChar('\n');

        this.path = null!;
        roundBracket.Clear();
        squareBracket.Clear();
        curlyBracket.Clear();
        angleBracket.Clear();

        Token[] result = tokens.ToArray();
        tokens.Clear();
        return result;
    }

    private void AnalyzeChar(char c) {
        if (isComment && AnalyzeComment(c))
            return;

        switch (secondLastChar) {
            case '*':
                if (c != '=') {
                    AppendOperator(TokenType.Exponentation, 2);
                    secondLastChar = null;
                }
                break;
            case '&':
                if (c != '=') {
                    AppendOperator(TokenType.ConditionalAnd, 2);
                    secondLastChar = null;
                }
                break;
            case '|':
                if (c != '=') {
                    AppendOperator(TokenType.ConditionalOr, 2);
                    secondLastChar = null;
                }
                break;
            case '?':
                if (c != '=') {
                    AppendOperator(TokenType.NullCoalescing, 2);
                    secondLastChar = null;
                }
                break;
        }

        switch (c) {
            case '+':
                if (lastChar == '+') {
                    AppendOperator(TokenType.Increment, 2);
                    clearLastChar = true;
                } else {
                    AppendWordAndStoreOperator();
                }
                break;
            case '-':
                if (lastChar == '-') {
                    AppendOperator(TokenType.Decrement, 2);
                    clearLastChar = true;
                } else {
                    AppendWordAndStoreOperator();
                }
                break;
            case '*':
                if (lastChar == '*') {
                    secondLastChar = '*';
                    clearLastChar = true;
                } else {
                    AppendWordAndStoreOperator();
                }
                break;
            case '%':
                AppendWordAndStoreOperator();
                break;
            case '=':
                bool isDefaultA = false;
                switch (lastChar) {
                    case '+':
                        AppendOperator(TokenType.AdditionAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '-':
                        AppendOperator(TokenType.SubtractionAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '*':
                        AppendOperator(TokenType.MultiplicationAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '/':
                        AppendOperator(TokenType.DivisionAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '%':
                        AppendOperator(TokenType.RemainderAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '=':
                        AppendOperator(TokenType.Equality, 2);
                        clearLastChar = true;
                        break;
                    case '!':
                        AppendOperator(TokenType.Inequality, 2);
                        clearLastChar = true;
                        break;
                    case '&':
                        AppendOperator(TokenType.LogicalAndAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '|':
                        AppendOperator(TokenType.LogicalOrAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '^':
                        AppendOperator(TokenType.LogicalXorAssigment, 2);
                        clearLastChar = true;
                        break;
                    case '?':
                        AppendOperator(TokenType.QuestionMark, 1);
                        AppendSingle(TokenType.Assigment);
                        clearLastChar = true;
                        break;
                    default:
                        isDefaultA = true;
                        break;
                }

                bool isDefaultB = false;
                switch (secondLastChar) {
                    case '*':
                        AppendOperator(TokenType.ExponentationAssigment, 3);
                        clearLastChar = true;
                        secondLastChar = null;
                        break;
                    case '&':
                        AppendOperator(TokenType.ConditionalAndAssigment, 3);
                        clearLastChar = true;
                        secondLastChar = null;
                        break;
                    case '|':
                        AppendOperator(TokenType.ConditionalOrAssigment, 3);
                        clearLastChar = true;
                        secondLastChar = null;
                        break;
                    case '?':
                        AppendOperator(TokenType.NullCoalescingAssigment, 3);
                        clearLastChar = true;
                        secondLastChar = null;
                        break;
                    default:
                        isDefaultB = true;
                        break;
                }

                if (isDefaultA && isDefaultB)
                    AppendWordAndStoreOperator();
                break;
            case '!':
                AppendWordAndStoreOperator();
                break;
            case '&':
                if (lastChar == '&') {
                    secondLastChar = '&';
                    clearLastChar = true;
                } else {
                    AppendWordAndStoreOperator();
                }
                break;
            case '|':
                if (lastChar == '|') {
                    secondLastChar = '|';
                    clearLastChar = true;
                } else {
                    AppendWordAndStoreOperator();
                }
                break;
            case '^':
                AppendWordAndStoreOperator();
                break;

            case '.':
                AppendSingle(TokenType.Dot);
                break;
            case ',':
                AppendSingle(TokenType.Comma);
                break;
            case ':':
                AppendSingle(TokenType.Colon);
                break;
            case ';':
                AppendSingle(TokenType.Semicolon);
                break;
            case '?':
                if (lastChar == '?') {
                    secondLastChar = '?';
                    clearLastChar = true;
                } else {
                    AppendWordAndStoreOperator();
                }
                break;

            case '(':
                AppendBracketOpening(roundBracket, TokenType.RoundBracketOpening);
                break;
            case ')':
                AppendBracketClosing(roundBracket, TokenType.RoundBracketClosing);
                break;
            case '[':
                AppendBracketOpening(squareBracket, TokenType.SquareBracketOpening);
                break;
            case ']':
                AppendBracketClosing(squareBracket, TokenType.SquareBracketClosing);
                break;
            case '{':
                AppendBracketOpening(curlyBracket, TokenType.CurlyBracketOpening);
                break;
            case '}':
                AppendBracketClosing(curlyBracket, TokenType.CurlyBracketClosing);
                break;
            case '<':
                AppendBracketOpening(angleBracket, TokenType.AngleBracketOpening);
                break;
            case '>':
                AppendBracketClosing(angleBracket, TokenType.AngleBracketClosing);
                break;

            case '/':
                if (lastChar != '/') {
                    AppendWordAndStoreOperator();
                } else {
                    isComment = true;
                    isMultiline = false;
                    clearLastChar = true;
                }
                break;
            case '\n':
                if (lastChar == '\r')
                    column--;
                else
                    AppendLine();
                break;
            case '\r':
                AppendLine();
                break;
            case '\t':
            case ' ':
                AppendWord();
                break;
            default:
                if (value.Length == 0)
                    startColumn = column;
                value.Append(c);
                break;
        }

        if (clearLastChar) {
            lastChar = null;
            clearLastChar = false;
        } else {
            switch (lastChar) {
                case '+':
                    AppendOperator(TokenType.Addition, 1);
                    break;
                case '-':
                    AppendOperator(TokenType.Subtraction, 1);
                    break;
                case '*':
                    AppendOperator(TokenType.Multiplication, 1);
                    break;
                case '/':
                    AppendOperator(TokenType.Division, 1);
                    break;
                case '%':
                    AppendOperator(TokenType.Remainder, 1);
                    break;
                case '=':
                    AppendOperator(TokenType.Assigment, 1);
                    break;
                case '!':
                    AppendOperator(TokenType.Negation, 1);
                    break;
                case '&':
                    AppendOperator(TokenType.LogicalAnd, 1);
                    break;
                case '|':
                    AppendOperator(TokenType.LogicalOr, 1);
                    break;
                case '^':
                    AppendOperator(TokenType.LogicalXor, 1);
                    break;
                case '?':
                    AppendOperator(TokenType.QuestionMark, 1);
                    break;
            }

            lastChar = c;
        }
    }

    private bool AnalyzeComment(char c) {
        if (!isMultiline) {
            if (c == '\r' || c == '\n') {
                isComment = false;
                return false;
            }
            return true;
        }

        throw new NotImplementedException();
    }

    private void AppendSingle(TokenType type) {
        AppendWord();
        tokens.Add(new Token(path, line, column, type, 1, null));
    }

    private void AppendOperator(TokenType type, int length) {
        AppendWord();
        tokens.Add(new Token(path, operatorLine, operatorColumn, type, length, null));
    }

    private void AppendBracketOpening(Stack<int> bracketStack, TokenType bracketOpening) {
        AppendSingle(bracketOpening);
        bracketStack.Push(tokens.Count - 1);
    }

    private void AppendBracketClosing(Stack<int> bracketStack, TokenType bracketClosing) {
        AppendWord();

        int length = -1;
        if (bracketStack.TryPop(out int result)) {
            length = tokens.Count + 1 - result;
            tokens[result] = tokens[result] with { Length = length };
        }

        tokens.Add(new Token(path, line, column, bracketClosing, -length, null));
    }

    private void AppendWord() {
        if (value.Length == 0)
            return;

        tokens.Add(new Token(path, line, startColumn, TokenType.Word, value.Length, value.ToString()));
        value.Clear();
    }

    private void AppendLine() {
        AppendWord();
        line++;
        column = 0;
    }

    private void AppendWordAndStoreOperator() {
        AppendWord();
        operatorLine = line;
        operatorColumn = column;
    }

}
