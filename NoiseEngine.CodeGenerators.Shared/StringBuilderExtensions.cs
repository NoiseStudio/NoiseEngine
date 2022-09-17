using System.Text;

namespace NoiseEngine.CodeGenerators;

internal static class StringBuilderExtensions {

    public static StringBuilder AppendIndentation(this StringBuilder stringBuilder, int indent = 1) {
        for (int i = 0; i < indent; i++)
            stringBuilder.Append(GeneratorConstants.Indentation);
        return stringBuilder;
    }

}
