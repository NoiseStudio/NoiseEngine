namespace NoiseEngine.Tests;

internal class FactRequireAttribute : FactAttribute {

    public TestRequirements TestRequirements { get; }

    public FactRequireAttribute(TestRequirements testRequirements) {
        TestRequirements = testRequirements;
        Skip = TestRequirements.ToSkipReason();
    }

}
