using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace NoiseEngine.CodeGenerators.Jobs {
    [Generator]
    public class EntitySourceGenerator : ISourceGenerator {

        public void Execute(GeneratorExecutionContext context) {
            StringBuilder builder = new StringBuilder(@"// AUTO GENERATED - DO NOT EDIT

#nullable enable

namespace NoiseEngine.Jobs;

public readonly partial struct Entity {
");

            for (int i = 1; i <= 8; i++) {
                for (int j = 1; j <= i; j++) {
                    Generate(i, j, builder, false);
                    Generate(i, j, builder, true);
                }
            }

            builder.AppendLine();
            builder.Append('}');
            builder.AppendLine();

            context.AddSource($"Entity.generated.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) {
        }

        private void Generate(int tCount, int selectedT, StringBuilder builder, bool isSystem) {
            string paramName = isSystem ? "system" : "query";
            string typeName = isSystem ? "EntitySystem" : "EntityQuery";

            builder.Append(@"
    /// <summary>
    /// Replaces T").Append(selectedT).Append(@" component assigned to this entity.
    /// </summary>
");

            for (int i = 1; i <= tCount; i++) {
                builder.Append("    /// <typeparam name=\"T");
                builder.Append(i);
                builder.AppendLine("\">Struct inheriting from <see cref=\"IEntityComponent\"/>.</typeparam>");
            }

            builder.Append("    /// <param name=\"");
            builder.Append(paramName);
            builder.Append("\"><see cref=\"");
            builder.Append(typeName);

            builder.Append(@"""/> which operating on this ").Append(selectedT).Append(@" component.</param>
    /// <param name=""component"">New component.</param>
    public void Set");

            EntityHelper.AppendTArguments(tCount, builder);

            builder.Append('(');
            builder.Append(typeName);

            EntityHelper.AppendTArguments(tCount, builder);
            builder.Append(' ');
            builder.Append(paramName);
            builder.Append(", T");
            builder.Append(selectedT);
            builder.Append(" component)");

            EntityHelper.AppendWhereConstraints(tCount, builder, 2);

            if (isSystem)
                builder.AppendLine("        Set(system.queryGeneric!, component);");
            else
                builder.AppendLine("        query.SetComponent(this, component);");

            builder.AppendLine("    }");
            builder.AppendLine();
        }

    }
}
