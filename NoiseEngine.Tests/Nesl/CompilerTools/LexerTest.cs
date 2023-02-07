using NoiseEngine.Nesl.CompilerTools;
using System;

namespace NoiseEngine.Tests.Nesl.CompilerTools;

public class LexerTest {

    private const string Path = "Path";

    private const string Word1 = "The";
    private const string Word2 = "quick123";
    private const string Word3 = "brOWN";
    private const string Word4 = "2FOx";
    private const string Word5 = "1864";
    private const string Word6 = "JuMpS";

    private readonly Lexer lexer = new Lexer();

    [Fact]
    public void SinglelineComment() {
        Token[] tokens = lexer.Lex(Path, $"{Word5}\t\t   // {Word2}\r\n{Word3}");

        Assert.Equal(2, tokens.Length);
        Assert.Equal(new Token(Path, 1, 1, TokenType.Word, Word5.Length, Word5), tokens[0]);
        Assert.Equal(new Token(Path, 2, 1, TokenType.Word, Word3.Length, Word3), tokens[1]);
    }

    [Fact]
    public void Word() {
        Token[] tokens = lexer.Lex(Path, $"{Word1}   {Word2}\t\t{Word3}\r{Word4}\r\n{Word5}\n{Word6}");

        Assert.Equal(6, tokens.Length);
        Assert.Equal(new Token(Path, 1, 1, TokenType.Word, Word1.Length, Word1), tokens[0]);
        Assert.Equal(new Token(Path, 1, (uint)Word1.Length + 4, TokenType.Word, Word2.Length, Word2), tokens[1]);
        Assert.Equal(new Token(
            Path, 1, (uint)Word1.Length + 4 + (uint)Word2.Length + 2, TokenType.Word, Word3.Length, Word3
        ), tokens[2]);
        Assert.Equal(new Token(Path, 2, 1, TokenType.Word, Word4.Length, Word4), tokens[3]);
        Assert.Equal(new Token(Path, 3, 1, TokenType.Word, Word5.Length, Word5), tokens[4]);
        Assert.Equal(new Token(Path, 4, 1, TokenType.Word, Word6.Length, Word6), tokens[5]);
    }

    [Fact]
    public void DotCommaColonSemicolonQuestionMark() {
        Token[] tokens = lexer.Lex(Path, $"{Word1}.{Word2},{Word3}:{Word4};{Word5}?");

        Assert.Equal(10, tokens.Length);

        uint column = 1;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word1.Length, Word1), tokens[0]);

        column += (uint)Word1.Length;
        Assert.Equal(new Token(Path, 1, column, TokenType.Dot, 1, null), tokens[1]);
        Assert.Equal(new Token(Path, 1, ++column, TokenType.Word, Word2.Length, Word2), tokens[2]);

        column += (uint)Word2.Length;
        Assert.Equal(new Token(Path, 1, column, TokenType.Comma, 1, null), tokens[3]);
        Assert.Equal(new Token(Path, 1, ++column, TokenType.Word, Word3.Length, Word3), tokens[4]);

        column += (uint)Word3.Length;
        Assert.Equal(new Token(Path, 1, column, TokenType.Colon, 1, null), tokens[5]);
        Assert.Equal(new Token(Path, 1, ++column, TokenType.Word, Word4.Length, Word4), tokens[6]);

        column += (uint)Word4.Length;
        Assert.Equal(new Token(Path, 1, column, TokenType.Semicolon, 1, null), tokens[7]);
        Assert.Equal(new Token(Path, 1, ++column, TokenType.Word, Word5.Length, Word5), tokens[8]);

        column += (uint)Word5.Length;
        Assert.Equal(new Token(Path, 1, column, TokenType.QuestionMark, 1, null), tokens[9]);
    }

    [Fact]
    public void RoundBracket() {
        BracketHelper(TokenType.RoundBracketOpening, '(', TokenType.RoundBracketClosing, ')');
    }

    [Fact]
    public void SquareBracket() {
        BracketHelper(TokenType.SquareBracketOpening, '[', TokenType.SquareBracketClosing, ']');
    }

    [Fact]
    public void CurlyBracket() {
        BracketHelper(TokenType.CurlyBracketOpening, '{', TokenType.CurlyBracketClosing, '}');
    }

    [Fact]
    public void AngleBracket() {
        BracketHelper(TokenType.AngleBracketOpening, '<', TokenType.AngleBracketClosing, '>');
    }

    [Fact]
    public void AdditionIncrement() {
        DoubledOperatorHelper('+', TokenType.Addition, TokenType.Increment);
    }

    [Fact]
    public void AdditionAssigment() {
        CompoundAssigmentHelper("+", TokenType.Addition, TokenType.AdditionAssigment);
    }

    [Fact]
    public void SubtractionDecrement() {
        DoubledOperatorHelper('-', TokenType.Subtraction, TokenType.Decrement);
    }

    [Fact]
    public void SubtractionAssigment() {
        CompoundAssigmentHelper("-", TokenType.Subtraction, TokenType.SubtractionAssigment);
    }

    [Fact]
    public void MultiplicationExponentation() {
        DoubledOperatorHelper('*', TokenType.Multiplication, TokenType.Exponentation);
    }

    [Fact]
    public void MultiplicationAssigment() {
        CompoundAssigmentHelper("*", TokenType.Multiplication, TokenType.MultiplicationAssigment);
    }

    [Fact]
    public void ExponentationAssigment() {
        CompoundAssigmentHelper("**", TokenType.Exponentation, TokenType.ExponentationAssigment);
    }

    [Fact]
    public void DivisionAssigment() {
        CompoundAssigmentHelper("/", TokenType.Division, TokenType.DivisionAssigment);
    }

    [Fact]
    public void RemainderAssigment() {
        CompoundAssigmentHelper("%", TokenType.Remainder, TokenType.RemainderAssigment);
    }

    [Fact]
    public void AssigmentEquality() {
        CompoundAssigmentHelper("=", TokenType.Assigment, TokenType.Equality);
    }

    [Fact]
    public void NegationInequality() {
        CompoundAssigmentHelper("!", TokenType.Negation, TokenType.Inequality);
    }

    [Fact]
    public void LogicalAndAssigment() {
        CompoundAssigmentHelper("&", TokenType.LogicalAnd, TokenType.LogicalAndAssigment);
    }

    [Fact]
    public void LogicalOrAssigment() {
        CompoundAssigmentHelper("|", TokenType.LogicalOr, TokenType.LogicalOrAssigment);
    }

    [Fact]
    public void LogicalXorAssigment() {
        CompoundAssigmentHelper("^", TokenType.LogicalXor, TokenType.LogicalXorAssigment);
    }

    [Fact]
    public void ConditionalAndAssigment() {
        CompoundAssigmentHelper("&&", TokenType.ConditionalAnd, TokenType.ConditionalAndAssigment);
    }

    [Fact]
    public void ConditionalOrAssigment() {
        CompoundAssigmentHelper("||", TokenType.ConditionalOr, TokenType.ConditionalOrAssigment);
    }

    [Fact]
    public void NullCoalescingAssigment() {
        CompoundAssigmentHelper("??", TokenType.NullCoalescing, TokenType.NullCoalescingAssigment);
    }

    private void BracketHelper(TokenType opening, char openingChar, TokenType closing, char closingChar) {
        Token[] tokens = lexer.Lex(
            Path, $"{Word1}{openingChar}{Word2},{Word3}{openingChar}{Word4}{closingChar}{closingChar}"
        );

        Assert.Equal(9, tokens.Length);

        uint column = 1;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word1.Length, Word1), tokens[0]);

        column += (uint)Word1.Length;
        Assert.Equal(new Token(Path, 1, column++, opening, 8, null), tokens[1]);
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word2.Length, Word2), tokens[2]);

        column += (uint)Word2.Length;
        Assert.Equal(new Token(Path, 1, column++, TokenType.Comma, 1, null), tokens[3]);
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word3.Length, Word3), tokens[4]);

        column += (uint)Word3.Length;
        Assert.Equal(new Token(Path, 1, column++, opening, 3, null), tokens[5]);
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word4.Length, Word4), tokens[6]);

        column += (uint)Word4.Length;
        Assert.Equal(new Token(Path, 1, column++, closing, -3, null), tokens[7]);
        Assert.Equal(new Token(Path, 1, column++, closing, -8, null), tokens[8]);
    }

    private void DoubledOperatorHelper(char c, TokenType single, TokenType doubled) {
        Token[] tokens = lexer.Lex(Path, $"{Word1}{c}{c} {c} {Word2}{c} {c} {c}{c}{Word3}{c}");

        Assert.Equal(9, tokens.Length);

        uint column = 1;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word1.Length, Word1), tokens[0]);

        column += (uint)Word1.Length;
        Assert.Equal(new Token(Path, 1, column, doubled, 2, null), tokens[1]);
        column += 3;
        Assert.Equal(new Token(Path, 1, column, single, 1, null), tokens[2]);
        column += 2;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word2.Length, Word2), tokens[3]);

        column += (uint)Word2.Length;
        Assert.Equal(new Token(Path, 1, column, single, 1, null), tokens[4]);
        column += 2;
        Assert.Equal(new Token(Path, 1, column, single, 1, null), tokens[5]);
        column += 2;
        Assert.Equal(new Token(Path, 1, column, doubled, 2, null), tokens[6]);
        column += 2;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word3.Length, Word3), tokens[7]);

        column += (uint)Word3.Length;
        Assert.Equal(new Token(Path, 1, column, single, 1, null), tokens[8]);
    }

    private void CompoundAssigmentHelper(string c, TokenType compound, TokenType assigment) {
        Token[] tokens = lexer.Lex(Path, $"{Word1}{c}= {c} {Word2}{c} {c} {c}={c}={Word3}{c}=\n{c}");

        Assert.Equal(11, tokens.Length);

        uint column = 1;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word1.Length, Word1), tokens[0]);

        column += (uint)Word1.Length;
        Assert.Equal(new Token(Path, 1, column, assigment, 1 + c.Length, null), tokens[1]);
        column += 2 + (uint)c.Length;
        Assert.Equal(new Token(Path, 1, column, compound, c.Length, null), tokens[2]);
        column += 1 + (uint)c.Length;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word2.Length, Word2), tokens[3]);

        column += (uint)Word2.Length;
        Assert.Equal(new Token(Path, 1, column, compound, c.Length, null), tokens[4]);
        column += 1 + (uint)c.Length;
        Assert.Equal(new Token(Path, 1, column, compound, c.Length, null), tokens[5]);
        column += 1 + (uint)c.Length;
        Assert.Equal(new Token(Path, 1, column, assigment, 1 + c.Length, null), tokens[6]);
        column += 1 + (uint)c.Length;
        Assert.Equal(new Token(Path, 1, column, assigment, 1 + c.Length, null), tokens[7]);
        column += 1 + (uint)c.Length;
        Assert.Equal(new Token(Path, 1, column, TokenType.Word, Word3.Length, Word3), tokens[8]);

        column += (uint)Word3.Length;
        Assert.Equal(new Token(Path, 1, column, assigment, 1 + c.Length, null), tokens[9]);

        Assert.Equal(new Token(Path, 2, 1, compound, c.Length, null), tokens[10]);
    }

}
