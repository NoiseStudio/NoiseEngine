using System.Numerics;

namespace NoiseEngine.Mathematics.Helpers;

internal static class NumberHelper<T> where T : INumber<T> {

    public static T Value2 { get; } = T.One + T.One;
    public static T Value3 { get; } = Value2 + T.One;
    public static T Value5 { get; } = Value2 + Value3;
    public static T Value180 { get; } = Value2 * Value2 * Value3 * Value3 * Value5;

    public static T Half { get; } = T.One / Value2;

}
