using System.Text;

namespace NoiseEngine.Generator;

internal static class StringBuilderExtensions {

    public static StringBuilder AppendIndentation(this StringBuilder stringBuilder, int indent = 1) {
        for (int i = 0; i < indent; i++)
            stringBuilder.Append(GeneratorConstants.Indentation);
        return stringBuilder;
    }

    public static StringBuilder IndexOf(this StringBuilder stringBuilder, string value, out int index) {
        return stringBuilder.IndexOf(value, 0, out index);
    }

    public static StringBuilder IndexOf(this StringBuilder stringBuilder, string value, int startIndex, out int index) {
        if (value.Length == 0 || value.Length > stringBuilder.Length) {
            index = -1;
            return stringBuilder;
        }

        for (int i = startIndex; i < stringBuilder.Length; i++) {
            if (stringBuilder[i] == value[0]) {
                bool found = true;
                for (int j = 1; j < value.Length; j++) {
                    int k = i + j;

                    if (k >= stringBuilder.Length) {
                        found = false;
                        break;
                    }

                    if (stringBuilder[i + j] != value[j]) {
                        found = false;
                        break;
                    }
                }

                if (found) {
                    index = i;
                    return stringBuilder;
                }
            }
        }

        index = -1;
        return stringBuilder;
    }

}
