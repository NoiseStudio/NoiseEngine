using System.Text;

namespace NoiseEngine.InternalGenerator.Jobs {
    internal static class JobsGeneratorHelper {

        public const int ArgumentsCount = 8;

        public static void AppendTArguments(int tCount, StringBuilder builder) {
            if (tCount == 0)
                return;

            builder.Append('<');

            for (int i = 1; i <= tCount; i++) {
                builder.Append('T');
                builder.Append(i);

                if (i < tCount)
                    builder.Append(", ");
            }

            builder.Append('>');
        }

        public static void AppendEntityTuple(int tCount, StringBuilder builder) {
            if (tCount == 0) {
                builder.Append("Entity");
                return;
            }

            builder.Append("(Entity entity, ");

            for (int i = 1; i <= tCount; i++) {
                builder.Append('T');
                builder.Append(i);

                builder.Append(" component");
                builder.Append(i);

                if (i < tCount)
                    builder.Append(", ");
            }

            builder.Append(")");
        }

        public static void AppendEntityWhereConstraints(int tCount, StringBuilder builder, int indentation = 1) {
            if (tCount == 0) {
                builder.AppendLine(" {");
                return;
            }

            builder.AppendLine();

            for (int i = 1; i <= tCount; i++) {
                for (int j = 0; j < indentation; j++)
                    builder.Append(GeneratorConstants.Indentation);

                builder.Append("where T");
                builder.Append(i);
                builder.AppendLine(" : struct, IEntityComponent");
            }

            for (int j = 1; j < indentation; j++)
                builder.Append(GeneratorConstants.Indentation);

            builder.Append('{');
            builder.AppendLine();
        }

    }
}
