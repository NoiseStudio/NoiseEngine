namespace NoiseEngine.Tests;

internal class TheoryRequireAttribute : TheoryAttribute {

    public TestRequirements TestRequirements { get; }

    public TheoryRequireAttribute(TestRequirements testRequirements) {
        TestRequirements = testRequirements;
        Skip = TestRequirements.ToSkipReason();
    }

}
