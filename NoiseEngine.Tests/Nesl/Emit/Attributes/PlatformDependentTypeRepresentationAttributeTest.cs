using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class PlatformDependentTypeRepresentationAttributeTest {

    [Theory]
    [InlineData(null, null)]
    [InlineData("Quick", "Brown")]
    public void Create(string? cilTargetName, string? spirVTargetName) {
        PlatformDependentTypeRepresentationAttribute attribute =
            PlatformDependentTypeRepresentationAttribute.Create(cilTargetName, spirVTargetName);
        attribute.AssertValid();

        Assert.Equal(cilTargetName, attribute.CilTargetName);
        Assert.Equal(spirVTargetName, attribute.SpirVTargetName);
    }

}
