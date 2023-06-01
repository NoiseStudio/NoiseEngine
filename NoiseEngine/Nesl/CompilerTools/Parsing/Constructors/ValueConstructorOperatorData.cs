namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal record ValueConstructorOperatorData(string InterfaceName, string MethodName) {

    public string InterfaceFullNameWithAssembly => $"System::System.Operators.{InterfaceName}";

}
