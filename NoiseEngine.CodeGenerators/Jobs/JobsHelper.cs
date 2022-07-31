using System.Text;

namespace NoiseEngine.CodeGenerators.Jobs {
    internal static class JobsHelper {

        public static void AddIEnumeratorArguments(int tCount, StringBuilder builder) {
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

        public static void AddTArguments(int tCount, StringBuilder builder) {
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

    }
}
