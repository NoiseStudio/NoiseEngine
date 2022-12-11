using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Tests.Nesl.Emit.Attributes;

public class PlatformDependentTypeRepresentationAttributeTest {

    [Theory]
    [InlineData(null)]
    [InlineData("Quick")]
    public void Create(string? spirVTargetName) {
        PlatformDependentTypeRepresentationAttribute attribute =
            PlatformDependentTypeRepresentationAttribute.Create(spirVTargetName);
        attribute.AssertValid();

        Assert.Equal(spirVTargetName, attribute.SpirVTargetName);
    }

}
