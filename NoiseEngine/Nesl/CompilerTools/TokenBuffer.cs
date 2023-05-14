using System;

namespace NoiseEngine.Nesl.CompilerTools;

internal class TokenBuffer {


    public ArraySegment<Token> Tokens { get; }
    public int Index { get; set; }
    public bool HasNextTokens => Tokens.Count > Index;

    public TokenBuffer(ArraySegment<Token> tokens) {
        Tokens = tokens;
    }

    public bool TryReadNext(out Token token) {
        if (Index >= Tokens.Count) {
            Token t = Tokens[Tokens.Count - 1];
            token = new Token(t.Path, t.Line, t.Column + (uint)t.Length, TokenType.None, 0, null);
            return false;
        }

        token = Tokens[Index++];
        return true;
    }

    public bool TryReadNext(TokenType type, out Token token) {
        if (TryReadNext(out token))
            return token.Type == type;
        return false;
    }

    public bool TryReadNext(TokenType type) {
        return TryReadNext(type, out _);
    }

}
