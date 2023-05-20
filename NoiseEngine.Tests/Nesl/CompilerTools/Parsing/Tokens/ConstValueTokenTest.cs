using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Tokens;

public class ConstValueTokenTest {

    [Theory]
    [InlineData("5", 5)]
    [InlineData("-685645", -685645)]
    public void Int64(string text, long expected) {
        Assert.True(Parse(text, out ConstValueToken result, out _));
        Assert.True(result.ToInt64(out long a, out _));
        Assert.Equal(expected, a);
    }

    [Theory]
    [InlineData("5.8743", 5.8743)]
    [InlineData("-685645.4", -685645.4)]
    public void Float64(string text, double expected) {
        Assert.True(Parse(text, out ConstValueToken result, out _));
        Assert.True(result.ToFloat64(out double a, out _));
        Assert.Equal(expected, a);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void FloatBool(string text, bool expected) {
        Assert.True(Parse(text, out ConstValueToken result, out _));
        Assert.True(result.ToBool(out bool a, out _));
        Assert.Equal(expected, a);
    }

    private bool Parse(string text, out ConstValueToken result, out CompilationError error) {
        TokenBuffer buffer = new TokenBuffer(new Lexer().Lex("", text));
        return ConstValueToken.Parse(buffer, default, out result, out error);
    }

}
