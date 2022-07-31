using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace NoiseEngine.CodeGenerators.Jobs {
    [Generator]
    public class EntityQuerySourceGenerator : ISourceGenerator {

        public void Execute(GeneratorExecutionContext context) {
            for (int i = 0; i <= 8; i++)
                context.AddSource($"EntityQueryT{i}.generated.cs", SourceText.From(Generate(i), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) {
        }

        private string Generate(int tCount) {
            StringBuilder builder = new StringBuilder(@"// AUTO GENERATED - DO NOT EDIT

#nullable enable

using System.Collections;
using System.Collections.Concurrent;

namespace NoiseEngine.Jobs;

public class EntityQuery");

            EntityHelper.AppendTArguments(tCount, builder);

            builder.Append(" : EntityQueryBase, IEnumerable<");

            EntityHelper.AppendTuple(tCount, builder);

            builder.Append('>');

            EntityHelper.AppendWhereConstraints(tCount, builder);

            if (tCount > 0) {
                builder.AppendLine();

                for (int i = 1; i <= tCount; i++) {
                    builder.Append("    internal readonly ConcurrentDictionary<Entity, T");
                    builder.Append(i);
                    builder.Append("> components");
                    builder.Append(i);
                    builder.Append(';');
                    builder.AppendLine();
                }
            }

            builder.AppendLine(@"
    public EntityQuery(
        EntityWorld world, IReadOnlyList<Type>? writableComponents = null, IEntityFilter? filter = null
    ) : base(world, writableComponents, filter) {");

            for (int i = 1; i <= tCount; i++) {
                builder.Append("        components");
                builder.Append(i);
                builder.Append(" = world.ComponentsStorage.AddStorage<T");
                builder.Append(i);
                builder.AppendLine(">();");
            }

            builder.Append(@"    }

    /// <summary>
    /// Returns an enumerator that iterates through this <see cref=""EntityQuery""/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through this <see cref=""EntityQuery""/>.</returns>
    public IEnumerator<");

            EntityHelper.AppendTuple(tCount, builder);

            builder.AppendLine("> GetEnumerator() {");

            if (tCount > 0) {
                builder.Append(@"        foreach (Entity entity in Entities) {
            yield return (entity, ");

                for (int i = 1; i <= tCount; i++) {
                    builder.Append("components");
                    builder.Append(i);

                    builder.Append("[entity]");

                    if (i < tCount)
                        builder.Append(", ");
                }

                builder.AppendLine(@");
        }");
            } else {
                builder.AppendLine("        return Entities.GetEnumerator();");
            }

            builder.AppendLine("    }");
            builder.AppendLine();

            for (int i = 1; i <= tCount; i++) {
                builder.Append("    internal void SetComponent(Entity entity, T");
                builder.Append(i);
                builder.Append(@" component) {
        ComponentsStorage<Entity>.SetComponent(components");
                builder.Append(i);
                builder.AppendLine(@", entity, component);
    }");
                builder.AppendLine();
            }

            if (tCount > 0) {
                builder.Append(@"    internal override void RegisterGroup(EntityGroup group) {
        if (");

                for (int i = 1; i <= tCount; i++) {
                    builder.Append("group.HasComponent(typeof(T");
                    builder.Append(i);
                    builder.Append("))");

                    if (i < tCount)
                        builder.Append(" && ");
                }

                builder.AppendLine(@")
            base.RegisterGroup(group);
    }
");
            }

            builder.Append(@"    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
");

            return builder.ToString();
        }

    }
}
