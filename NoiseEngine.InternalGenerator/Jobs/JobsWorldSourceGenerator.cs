using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace NoiseEngine.InternalGenerator.Jobs {
    [Generator]
    public class JobsWorldSourceGenerator : ISourceGenerator {

        public void Execute(GeneratorExecutionContext context) {
            StringBuilder builder = new StringBuilder(@"// AUTO GENERATED - DO NOT EDIT

#nullable enable

namespace NoiseEngine.Jobs;

public partial class JobsWorld {
");

            builder.AppendLine();
            for (int i = 0; i <= JobsGeneratorHelper.ArgumentsCount; i++)
                GenerateDelegate(i, builder);

            builder.AppendLine();
            for (int i = 0; i <= JobsGeneratorHelper.ArgumentsCount; i++)
                Generate(i, builder);

            builder.AppendLine();
            builder.Append('}');
            builder.AppendLine();

            context.AddSource($"JobsWorld.generated.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) {
        }

        private void GenerateDelegate(int tCount, StringBuilder builder) {
            builder.Append("    public delegate void JobT");
            builder.Append(tCount);
            JobsGeneratorHelper.AppendTArguments(tCount, builder);
            builder.Append('(');

            for (int i = 1; i <= tCount; i++) {
                builder.Append('T');
                builder.Append(i);
                builder.Append(" argument");
                builder.Append(i);

                if (i < tCount)
                    builder.Append(", ");
            }

            builder.AppendLine(");");
        }

        private void Generate(int tCount, StringBuilder builder) {
            builder.Append(@"    /// <summary>
    /// Creates new <see cref=""Job""/> in this <see cref=""JobsWorld""/>.
    /// </summary>
");

            for (int i = 1; i <= tCount; i++) {
                builder.Append("    /// <typeparam name=\"T");
                builder.Append(i);
                builder.Append("\">Type of argument ");
                builder.Append(i);
                builder.AppendLine(".</typeparam>");
            }

            builder.Append(@"    /// <param name=""toExecute"">The method that will be executed.</param>
    /// <param name=""relativeExecutionTime"">
    /// Relative waiting time in milliseconds to <see cref=""Job""/> execution.
    /// </param>
");

            for (int i = 1; i <= tCount; i++) {
                builder.Append("    /// <param name=\"argument");
                builder.Append(i);
                builder.Append("\">Argument ");
                builder.Append(i);
                builder.AppendLine(".</param>");
            }

            builder.Append(@"    /// <returns>New <see cref=""Job""/> with given arguments.</returns>
    public Job EnqueueJob");

            JobsGeneratorHelper.AppendTArguments(tCount, builder);

            builder.Append("(JobT");
            builder.Append(tCount);
            JobsGeneratorHelper.AppendTArguments(tCount, builder);

            builder.Append(" toExecute, uint relativeExecutionTime");

            for (int i = 1; i <= tCount; i++) {
                builder.Append(", T");
                builder.Append(i);
                builder.Append(" argument");
                builder.Append(i);
            }

            builder.AppendLine(@") {
        Job job = EnqueueJobWorker(toExecute, relativeExecutionTime);
");

            if (tCount > 0) {
                for (int i = 1; i <= tCount; i++) {
                    builder.Append("        ComponentsStorage.AddComponent(job, argument");
                    builder.Append(i);
                    builder.AppendLine(");");
                }

                builder.AppendLine();
            }

            builder.AppendLine(@"        AddNewJobToQueue(job);
        return job;
    }
");
        }

    }
}
