using System.Numerics;

namespace NoiseEngine.Mathematics.Helpers;

internal static class MultiplicativeIdentityHelper<TSelf, TResult>
    where TSelf : IMultiplicativeIdentity<TSelf, TResult>
{

    public static TResult MultiplicativeIdentity => TSelf.MultiplicativeIdentity;

}
