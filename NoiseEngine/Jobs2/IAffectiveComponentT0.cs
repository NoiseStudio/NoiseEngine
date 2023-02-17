namespace NoiseEngine.Jobs2;

public interface IAffectiveComponent : IComponent {

    /// <summary>
    /// Computes affective hash code of this component.
    /// </summary>
    /// <returns>Affective hash code of this component.</returns>
    public int GetAffectiveHashCode();

}
