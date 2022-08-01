namespace NoiseEngine.Nesl.CompilerTools.Attributes;

public class PlatformDependentTypeRepresentationNeslAttribute : NeslAttribute {

    internal string? CilTargetName { get; }
    internal string? SpirVTargetName { get; }

    public PlatformDependentTypeRepresentationNeslAttribute(string? cilTargetName, string? sprirVTargetName) {
        CilTargetName = cilTargetName;
        SpirVTargetName = sprirVTargetName;
    }

}
