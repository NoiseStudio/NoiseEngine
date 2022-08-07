using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NoiseEngine.Nesl;

public static class NeslAttributeExtensions {

    /// <summary>
    /// Tries casts any <see cref="NeslAttribute"/> from <paramref name="attributes"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Result type of attribute.</typeparam>
    /// <param name="attributes">Attributes to cast.</param>
    /// <param name="attribute">Casted attribute.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise <see langword="false"/>.</returns>
    public static bool TryCastAnyAttribute<T>(
        this IEnumerable<NeslAttribute> attributes, [NotNullWhen(true)] out T? attribute
    ) where T : NeslAttribute, new() {
        foreach (NeslAttribute a in attributes) {
            if (a.TryCast(out attribute))
                return true;
        }

        attribute = null;
        return false;
    }

    /// <summary>
    /// Checks if <paramref name="attributes"/> has <see cref="NeslAttribute"/> with given <paramref name="fullName"/>.
    /// </summary>
    /// <param name="attributes">Attributes to check.</param>
    /// <param name="fullName">Full name of searched <see cref="NeslAttribute"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="attributes"/> has <see cref="NeslAttribute"/>
    /// with given <paramref name="fullName"/>; otherwise false.
    /// </returns>
    public static bool HasAnyAttribute(this IEnumerable<NeslAttribute> attributes, string fullName) {
        return attributes.Any(x => x.FullName == fullName);
    }

}
