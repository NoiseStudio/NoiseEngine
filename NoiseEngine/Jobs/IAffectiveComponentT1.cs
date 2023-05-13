namespace NoiseEngine.Jobs;

public interface IAffectiveComponent<TSelf> : IAffectiveComponent {

    /// <summary>
    /// Checks if this component is affectively equal to other component.
    /// </summary>
    /// <param name="other">Other TSelf component.</param>
    /// <returns>
    /// <see langword="true"/> when components is affectively equals; otherwise <see langword="false"/>.
    /// </returns>
    public bool AffectiveEquals(TSelf other);

}
