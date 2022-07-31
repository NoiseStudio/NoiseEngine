using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace NoiseEngine.CodeGenerators.Jobs {
    [Generator]
    public class EntitySystemSourceGenerator : ISourceGenerator {

        public void Execute(GeneratorExecutionContext context) {
            for (int i = 0; i <= 8; i++)
                context.AddSource($"EntitySystemT{i}.generated.cs", SourceText.From(Generate(i), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) {
        }

        private string Generate(int tCount) {
            StringBuilder builder = new StringBuilder(@"// AUTO GENERATED - DO NOT EDIT

#nullable enable

namespace NoiseEngine.Jobs;

public abstract class EntitySystem");

            JobsHelper.AddTArguments(tCount, builder);

            builder.Append(" : EntitySystemBase");

            if (tCount > 0) {
                builder.AppendLine();

                for (int i = 1; i <= tCount; i++) {
                    builder.Append("    where T");
                    builder.Append(i);
                    builder.AppendLine(" : struct, IEntityComponent");
                }
            } else {
                builder.Append(' ');
            }

            builder.Append('{');
            builder.AppendLine();

            builder.Append(@"
    internal EntityQuery");

            JobsHelper.AddTArguments(tCount, builder);

            builder.AppendLine("? queryGeneric;");

            builder.Append(@"
    internal override void InternalExecute() {
        base.InternalExecute();

        foreach (");

            JobsHelper.AddIEnumeratorArguments(tCount, builder);

            builder.Append(@" element in queryGeneric!) {
            OnUpdateEntity(element");

            if (tCount > 0)
                builder.Append(".entity");

            for (int i = 1; i <= tCount; i++) {
                builder.Append(", element.component");
                builder.Append(i);
            }

            builder.Append(@");
        }

        ReleaseWork();
    }

    internal override void InternalUpdateEntity(Entity entity) {
        OnUpdateEntity(entity");

            for (int i = 1; i <= tCount; i++) {
                builder.Append(", queryGeneric!.components");
                builder.Append(i);
                builder.Append("![entity]");
            }

            builder.Append(@");
    }

    internal override void InternalInitialize(EntityWorld world, EntitySchedule? schedule) {
        base.InternalInitialize(world, schedule);

        queryGeneric = new EntityQuery");

            JobsHelper.AddTArguments(tCount, builder);

            builder.Append(@"(world, WritableComponents, Filter);
        query = queryGeneric;
    }

    /// <summary>
    /// This method is executed every cycle of this system on every <see cref=""Entity""/> assigned to this system.
    /// </summary>
    /// <param name=""entity"">Operated <see cref=""Entity""/>.</param>
");

            for (int i = 1; i <= tCount; i++) {
                builder.Append("    /// <param name=\"component");
                builder.Append(i);
                builder.AppendLine("\">Component of the operated <see cref=\"Entity\"/>.</param>");
            }

            builder.Append("    protected abstract void OnUpdateEntity(Entity entity");

            for (int i = 1; i <= tCount; i++) {
                builder.Append(", T");
                builder.Append(i);
                builder.Append(" component");
                builder.Append(i);
            }

            builder.AppendLine(");");
            builder.AppendLine();
            builder.Append('}');
            builder.AppendLine();

            return builder.ToString();
        }

    }
}
