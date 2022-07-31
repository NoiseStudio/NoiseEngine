using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace NoiseEngine.CodeGenerators.Jobs {
    [Generator]
    public class EntityWorldSourceGenerator : ISourceGenerator {

        public void Execute(GeneratorExecutionContext context) {
            StringBuilder builder = new StringBuilder(@"// AUTO GENERATED - DO NOT EDIT

#nullable enable

namespace NoiseEngine.Jobs;

public partial class EntityWorld {
");

            for (int i = 0; i <= 8; i++)
                Generate(i, builder);

            builder.AppendLine();
            builder.Append('}');
            builder.AppendLine();

            context.AddSource($"EntityWorld.generated.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) {
        }

        private void Generate(int tCount, StringBuilder builder) {
            builder.Append(@"
    /// <summary>
    /// Creates new entity in this entity world.
    /// </summary>
");

            for (int i = 1; i <= tCount; i++) {
                builder.Append("    /// <typeparam name=\"T");
                builder.Append(i);
                builder.AppendLine("\">Struct inheriting from <see cref=\"IEntityComponent\"/>.</typeparam>");
            }

            for (int i = 1; i <= tCount; i++) {
                builder.Append("    /// <param name=\"component");
                builder.Append(i);
                builder.AppendLine("\">Component being added.</param>");
            }

            builder.Append(@"    /// <returns><see cref=""Entity""/>.</returns>
    public Entity NewEntity");

            EntityHelper.AppendTArguments(tCount, builder);

            builder.Append('(');

            for (int i = 1; i <= tCount; i++) {
                builder.Append('T');
                builder.Append(i);
                builder.Append(" component");
                builder.Append(i);

                if (i < tCount)
                    builder.Append(", ");
            }

            builder.Append(')');

            EntityHelper.AppendWhereConstraints(tCount, builder, 2);

            builder.AppendLine("        Entity entity = NewEntityWorker();");

            if (tCount > 0) {
                builder.AppendLine();

                for (int i = 1; i <= tCount; i++) {
                    builder.Append("        ComponentsStorage.AddComponent(entity, component");
                    builder.Append(i);
                    builder.AppendLine(");");
                }
            }

            builder.Append(@"
        AddNewEntityToGroup(entity, new List<Type>()");

            if (tCount > 0) {
                builder.Append(@" {
            ");

                for (int i = 1; i <= tCount; i++) {
                    builder.Append("typeof(T");
                    builder.Append(i);
                    builder.Append(')');

                    if (i < tCount)
                        builder.Append(", ");
                }

                builder.Append(@"
        }");
            }

            builder.Append(@");

        return entity;
    }
");
        }

    }
}
