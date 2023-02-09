using System;

namespace NoiseEngine.Nesl.CompilerTools;

internal class TokenBuffer {

    private readonly ArraySegment<Token> tokens;

    public int Index { get; set; }
    public bool HasNextTokens => tokens.Count > Index;

    public TokenBuffer(Token[] tokens) {
        this.tokens = tokens;
    }

    public bool TryReadNext(TokenType type, out Token token) {
        if (Index >= tokens.Count) {
            Token t = tokens[tokens.Count - 1];
            token = new Token(t.Path, t.Line, t.Column + (uint)t.Length, TokenType.None, 0, null);
            return false;
        }

        token = tokens[Index++];
        return token.Type == type;
    }

}
