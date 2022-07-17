using System.Numerics;

namespace NoiseEngine.Mathematics;

internal static class NumberHelper<T> where T : INumber<T> {

    public static T Two { get; } = T.One + T.One;
    public static T Three { get; } = Two + T.One;
    public static T Five { get; } = Two + Three;

    public static T Half { get; } = T.One / Two;

    public static T Value180 { get; } = Two * Two * Three * Three * Five;

}
