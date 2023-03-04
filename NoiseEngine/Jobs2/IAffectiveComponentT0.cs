using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public interface IAffectiveComponent : IComponent {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetAffectiveHashCode<T>(T component) where T : IComponent {
        if (component is IAffectiveComponent<T> affectiveComponent)
            return affectiveComponent.GetAffectiveHashCode();
        return 0;
    }

    /// <summary>
    /// Computes affective hash code of this component.
    /// </summary>
    /// <remarks>
    /// <see cref="IAffectiveSystem"/> uses this method to determine components. If two components have the same
    /// affective hash code, they are considered into the same <see cref="IAffectiveSystem"/>.
    /// </remarks>
    /// <returns>Affective hash code of this component.</returns>
    public int GetAffectiveHashCode();

}
