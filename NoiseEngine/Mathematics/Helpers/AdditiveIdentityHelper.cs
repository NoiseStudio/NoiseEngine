using System.Numerics;

namespace NoiseEngine.Mathematics.Helpers;

internal static class AdditiveIdentityHelper<TSelf, TResult> where TSelf : IAdditiveIdentity<TSelf, TResult>
{

    public static TResult AdditiveIdentity => TSelf.AdditiveIdentity;

}
