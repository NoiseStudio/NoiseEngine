﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoiseEngine.Generator.Jobs;

[Generator]
public class EntitySystemIncrementalGenerator : IIncrementalGenerator {

    private const string SystemFullNameT0 = "NoiseEngine.Jobs.EntitySystem";
    private const string SystemFullNameT1 = "NoiseEngine.Jobs.EntitySystem<TThreadStorage>";
    private const string AffectiveComponentFullName = "NoiseEngine.Jobs.IAffectiveComponent";
    private const string EntityFullName = "NoiseEngine.Jobs.Entity";
    private const string SystemCommandsFullName = "NoiseEngine.Jobs.SystemCommands";
    private const string InternalMethodObsoleteMessage =
        "This method is internal and is not part of the API. Do not use.";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValuesProvider<ClassDeclarationSyntax> systems = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (x, _) => x is ClassDeclarationSyntax,
                static (context, _) => {
                    if (
                        context.SemanticModel.GetDeclaredSymbol(context.Node) is not ITypeSymbol typeSymbol ||
                        typeSymbol.BaseType is null || (
                            typeSymbol.BaseType.ToDisplayString() != SystemFullNameT0 &&
                            !(
                                typeSymbol.BaseType.ToDisplayString().StartsWith(SystemFullNameT0 + "<") &&
                                typeSymbol.BaseType.ToDisplayString().EndsWith(">")
                            )
                        ) || typeSymbol.ToDisplayString() == SystemFullNameT1
                    ) {
                        return null!;
                    }

                    return (ClassDeclarationSyntax)context.Node;
                }
            ).Where(static x => x is not null);

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)>
            compilationAndSystems = context.CompilationProvider.Combine(systems.Collect());

        context.RegisterSourceOutput(compilationAndSystems, (ctx, source) => {
            object locker = new object();
            Parallel.ForEach(source.Item2, system => {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("// <auto-generated />");
                builder.AppendLine();

                Generate(source.Item1, builder, system);

                string result = builder.ToString();
                string systemName = $"{
                    system.ParentNodes().OfType<BaseNamespaceDeclarationSyntax>().First().Name.GetText()
                }.{system.Identifier.Text}.generated.cs";

                lock (locker)
                    ctx.AddSource(systemName, result);
            });
        });
    }

    private void Generate(Compilation compilation, StringBuilder builder, ClassDeclarationSyntax system) {
        if (GeneratorHelpers.AssertNotUsingInternalThings(builder, system))
            return;

        if (!system.Modifiers.Any(x => x.ValueText == "partial")) {
            builder.AppendLine("#error EntitySystem must be partial.");
            return;
        }

        GeneratorHelpers.GenerateUsings(builder, system);
        builder.AppendLine("#nullable enable");
        builder.AppendLine("#pragma warning disable 612, 618");
        GeneratorHelpers.GenerateNamespaceWithType(builder, system);
        builder.AppendLine();

        string? threadStorageType = system.ChildNodes().OfType<BaseListSyntax>().Single().ChildNodes()
            .OfType<SimpleBaseTypeSyntax>()
            .Select(x => x.Type.GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString())
            .SingleOrDefault(x => x.StartsWith(SystemFullNameT0 + "<") && x.EndsWith(">"));
        if (threadStorageType is not null) {
            int index = threadStorageType!.IndexOf('<') + 1;
            threadStorageType = threadStorageType.Substring(index, threadStorageType.Length - index - 1);
        }

        MethodDeclarationSyntax[] methods = system.ChildNodes().OfType<MethodDeclarationSyntax>()
            .Where(x => x.Identifier.Text == "OnUpdateEntity").ToArray();
        if (methods.Length > 1)
            builder.AppendLine("#warning Multiple `OnUpdateEntity` methods in one system, only first will be used.");

        MethodDeclarationSyntax? onUpdateEntity = methods.FirstOrDefault();
        if (
            onUpdateEntity is not null && onUpdateEntity.ChildNodes().First(x => x is PredefinedTypeSyntax)
                .GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString() != "void"
        ) {
            builder.AppendLine("#error `OnUpdateEntity` method must return `void`.");
        }

        (string parameterType, bool isRef, bool isIn, bool isOut, bool isAffective)[] parameters =
            GenerateInitializeMethod(compilation, builder, onUpdateEntity, threadStorageType);

        foreach ((string parameterType, bool isRef, bool isIn, bool isOut, _) in parameters) {
            if (isIn || isOut) {
                builder.Append("#error Parameter `").Append(parameterType)
                    .AppendLine("` has `in` or `out` keyword which is not allowed in `OnUpdateEntity`.");
            }

            if (isRef && (parameterType == EntityFullName || parameterType == SystemCommandsFullName)) {
                builder.Append("#error Parameter `").Append(parameterType)
                    .AppendLine("` has `ref` keyword which is not allowed in for not component parameter.");
            }
        }

        foreach (
            string parameter in parameters.Select(x => x.parameterType).GroupBy(x => x).Where(x => x.Count() > 1)
                .SelectMany(x => x).Distinct()
        ) {
            builder.Append("#warning Parameter `").Append(parameter)
                .AppendLine("` is used more than once in `OnUpdateEntity`.");
        }

        if (onUpdateEntity is not null)
            GenerateSystemExecutionMethod(builder, onUpdateEntity, threadStorageType, parameters);

        builder.AppendIndentation().AppendLine("}");
        builder.AppendLine("}");
    }

    private (string parameterType, bool isRef, bool isIn, bool isOut, bool isAffective)[] GenerateInitializeMethod(
        Compilation compilation, StringBuilder builder, MethodDeclarationSyntax? onUpdateEntity,
        string? threadStorageType
    ) {
        builder.AppendIndentation(2)
            .Append("[System.Obsolete(\"").Append(InternalMethodObsoleteMessage).AppendLine("\")]")
            .AppendIndentation(2).Append("protected override void ").Append(GeneratorConstants.InternalThings)
            .AppendLine("_Initialize() {");

        (string, bool, bool, bool, bool)[] parameters;
        bool componentWriteAccess = false;

        builder.AppendIndentation(3).Append(GeneratorConstants.InternalThings).Append("_Storage.UsedComponents = ");
        if (onUpdateEntity is null || onUpdateEntity.ParameterList.Parameters.Count == 0) {
            parameters = Array.Empty<(string, bool, bool, bool, bool)>();
            builder.AppendLine("System.Array.Empty<").Append(GeneratorConstants.InternalThings)
                .Append(".ComponentUsage>()");
        } else {
            parameters = new (string, bool, bool, bool, bool)[onUpdateEntity.ParameterList.Parameters.Count];
            int i = 0;

            bool empty = onUpdateEntity.ParameterList.Parameters.All(x => {
                string s = x.Type!.GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString();
                return s == EntityFullName || s == SystemCommandsFullName || s == threadStorageType;
            });

            if (empty) {
                builder.AppendLine("System.Array.Empty<").Append(GeneratorConstants.InternalThings)
                    .Append(".ComponentUsage>()");
            } else {
                builder.Append("new ").Append(GeneratorConstants.InternalThings).AppendLine(".ComponentUsage[] {");
            }

            foreach (ParameterSyntax parameter in onUpdateEntity.ParameterList.Parameters) {
                string name = parameter.Type!.GetSymbol<INamedTypeSymbol>(compilation).ToDisplayString();
                bool isRef = parameter.Modifiers.Any(x => x.IsKind(SyntaxKind.RefKeyword));
                bool isIn = parameter.Modifiers.Any(x => x.IsKind(SyntaxKind.InKeyword));
                bool isOut = parameter.Modifiers.Any(x => x.IsKind(SyntaxKind.OutKeyword));
                bool isAffective = parameter.Type!.GetSymbol<ITypeSymbol>(compilation).AllInterfaces.Any(
                    x => x.ToDisplayString() == AffectiveComponentFullName
                );

                if (name != EntityFullName && name != SystemCommandsFullName && name != threadStorageType) {
                    builder.AppendIndentation(4).Append("new ").Append(GeneratorConstants.InternalThings)
                        .Append(".ComponentUsage(typeof(").Append(name).Append("), ").Append(isRef ? "true" : "false")
                        .AppendLine("),");
                    componentWriteAccess |= isRef;
                }

                parameters[i++] = (name, isRef, isIn, isOut, isAffective);
            }

            if (!empty) {
                builder.Remove(builder.Length - 1 - Environment.NewLine.Length, 1)
                    .AppendIndentation(3).Append('}');
            }
        }
        builder.AppendLine(";");

        builder.AppendIndentation(3).Append(GeneratorConstants.InternalThings)
            .Append("_Storage.ComponentWriteAccess = ").Append(componentWriteAccess ? "true" : "false")
            .AppendLine(";");

        if (threadStorageType is not null) {
            builder.AppendIndentation(3).Append(GeneratorConstants.InternalThings)
                .Append("_Storage.ThreadStorages = new System.Collections.Concurrent.ConcurrentStack<")
                .Append(threadStorageType).AppendLine(">();");
        }

        builder.AppendIndentation(2).AppendLine("}").AppendLine();
        return parameters;
    }

    private void GenerateSystemExecutionMethod(
        StringBuilder builder, MethodDeclarationSyntax onUpdateEntity, string? threadStorageType,
        (string parameterType, bool isRef, bool isIn, bool isOut, bool isAffective)[] parameters
    ) {
        builder.AppendIndentation(2)
            .Append("[System.Obsolete(\"").Append(InternalMethodObsoleteMessage).AppendLine("\")]")
            .AppendIndentation(2).AppendLine(
                "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions." +
                "AggressiveOptimization)]"
            ).AppendIndentation(2).Append("protected override void ").Append(GeneratorConstants.InternalThings)
            .Append("_SystemExecution(").Append(GeneratorConstants.InternalThings).Append(".ExecutionData data, ")
            .Append(SystemCommandsFullName).AppendLine(" systemCommands) {");

        StringBuilder content = new StringBuilder(onUpdateEntity.ToFullString());
        content.IndexOf("void", out int index)
            .Replace("private", "", 0, index)
            .Replace("protected", "", 0, index)
            .Replace("internal", "", 0, index)
            .Replace("public", "", 0, index);

        builder.AppendLine("#if (!DEBUG)").AppendIndentation(3).AppendLine(
            "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions." +
            "AggressiveInlining | System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]"
        ).Append(content).AppendLine("#endif").AppendLine();

        if (threadStorageType is not null) {
            builder.AppendIndentation(3).Append(threadStorageType)
                .Append(" threadStorage = ").Append(GeneratorConstants.InternalThings)
                .Append("_Storage.PopThreadStorage<").Append(threadStorageType).AppendLine(">();");
        }

        builder.AppendIndentation(3).Append(EntityFullName).AppendLine("? entity;");
        if (parameters.Any(x => x.isAffective)) {
            builder.AppendIndentation(3).AppendLine("bool changeArchetype;");
            if (parameters.Count(x => x.isAffective) > 1)
                builder.AppendIndentation(3).AppendLine("bool changeArchetypeTemp;");
            builder.AppendIndentation(3).AppendLine("int archetypeHashCode;");
        }

        int i = 0;
        foreach ((string parameterType, bool isRef, _, _, _) in parameters) {
            if (parameterType is EntityFullName or SystemCommandsFullName || parameterType == threadStorageType)
                continue;

            builder.AppendIndentation(3).Append("nint offset").Append(i).Append(" = data.GetOffset<")
                .Append(parameterType).AppendLine(">();");

            if (isRef) {
                builder.AppendIndentation(3).Append(parameterType).Append(" oldParameter").Append(i).AppendLine(";");
                builder.AppendIndentation(3).Append("object? observers").Append(i)
                    .Append(" = data.GetChangedObservers(typeof(").Append(parameterType).AppendLine("));");
                builder.AppendIndentation(3).Append("NoiseEngineInternal_DoNotUse.ChangedList changed").Append(i)
                    .AppendLine(" = default;");
            }

            builder.AppendIndentation(3);
            if (isRef)
                builder.Append("ref ");
            builder.Append(parameterType).Append(" parameter").Append(i++);

            if (isRef) {
                builder.Append(" = ref ").Append(GeneratorConstants.InternalThings).Append(".NullRef<")
                    .Append(parameterType).Append(">()");
            }
            builder.AppendLine(";");
        }
        builder.AppendLine();

        builder.AppendIndentation(3)
            .AppendLine("for (nint i = data.StartIndex; i <= data.EndIndex; i += data.RecordSize) {")
            .AppendIndentation(4).AppendLine("entity = data.GetInternalComponent(i);")
            .AppendIndentation(4).AppendLine("if (entity is null)")
            .AppendIndentation(5).AppendLine("continue;").AppendLine();

        i = 0;
        foreach ((string parameterType, bool isRef, _, _, _) in parameters) {
            if (parameterType is EntityFullName or SystemCommandsFullName || parameterType == threadStorageType)
                continue;

            builder.AppendIndentation(4).Append("parameter").Append(i).Append(" = ");
            if (isRef)
                builder.Append("ref ");
            builder.Append("data.Get<").Append(parameterType).Append(">(i + offset").Append(i++).AppendLine(");");

            if (isRef) {
                builder.AppendIndentation(4).Append("oldParameter").Append(i - 1).Append(" = parameter").Append(i - 1)
                    .AppendLine(";");
            }
        }
        builder.AppendLine();

        builder.AppendIndentation(4).Append("OnUpdateEntity(");
        i = 0;
        foreach ((string parameterType, bool isRef, _, _, _) in parameters) {
            if (parameterType == EntityFullName) {
                builder.Append("entity");
            } else if (parameterType == SystemCommandsFullName) {
                builder.Append("systemCommands");
            } else if (parameterType == threadStorageType) {
                builder.Append("threadStorage");
            } else {
                if (isRef)
                    builder.Append("ref ");
                builder.Append("parameter").Append(i++);
            }
            builder.Append(", ");
        }

        if (parameters.Length != 0)
            builder.Remove(builder.Length - 2, 2);
        builder.AppendLine(");").AppendLine();

        if (parameters.Any(x => x.isAffective))
            builder.AppendIndentation(4).AppendLine("archetypeHashCode = 0;");

        i = 0;
        bool first = true;
        foreach ((string parameterType, bool isRef, _, _, bool isAffective) in parameters) {
            if (parameterType is EntityFullName or SystemCommandsFullName || parameterType == threadStorageType)
                continue;

            if (!isRef) {
                i++;
                continue;
            }

            if (isAffective) {
                builder.AppendIndentation(4).Append("changeArchetype");
                if (!first)
                    builder.Append("Temp");

                builder.Append(
                    " = NoiseEngineInternal_DoNotUse.CompareAffectiveComponent(ref archetypeHashCode, in oldParameter"
                ).Append(i).Append(", in parameter").Append(i).AppendLine(");");

                if (!first)
                    builder.AppendIndentation(4).AppendLine("changeArchetype |= changeArchetypeTemp;");
            }

            builder.AppendIndentation(4).Append("if (observers").Append(i).AppendLine(" is not null) {");
            builder.AppendIndentation(5).Append("if (");
            if (isAffective) {
                builder.Append("changeArchetype");
                if (!first)
                    builder.Append("Temp");
                builder.Append(" || ");
            }
            builder.Append("NoiseEngineInternal_DoNotUse.CompareComponent(in oldParameter").Append(i)
                .Append(", in parameter").Append(i).AppendLine(")) {");

            builder.AppendIndentation(6).Append("if (changed").Append(i).AppendLine(".Inner is null)");
            builder.AppendIndentation(7).Append("changed").Append(i)
                .Append(" = NoiseEngineInternal_DoNotUse.ChangedList.Rent<").Append(parameterType).AppendLine(">();");
            builder.AppendIndentation(6).Append("changed").Append(i).Append(".Add(i, oldParameter").Append(i)
                .AppendLine(");");

            builder.AppendIndentation(5).AppendLine("}");
            builder.AppendIndentation(4).AppendLine("}").AppendLine();

            if (isAffective && first)
                first = false;
            i++;
        }

        // Update archetype.
        if (!first) {
            builder.AppendIndentation(4).AppendLine("if (changeArchetype) {");
            builder.AppendIndentation(5).AppendLine(
                "int hashCode = NoiseEngineInternal_DoNotUse.ArchetypeHashCode(entity);"
            );

            foreach ((string parameterType, bool isRef, _, _, bool isAffective) in parameters) {
                if (!isRef || !isAffective)
                    continue;

                builder.AppendIndentation(5)
                    .Append("hashCode ^= NoiseEngineInternal_DoNotUse.ArchetypeComponentHashCode<")
                    .Append(parameterType).AppendLine(">(entity);");
            }
            builder.AppendLine();

            builder.AppendIndentation(5).AppendLine("hashCode ^= archetypeHashCode;");
            builder.AppendIndentation(5).AppendLine("data.ChangeArchetype.Add((i, hashCode));");

            builder.AppendIndentation(4).AppendLine("}");
        }

        builder.AppendIndentation(3).AppendLine("}").AppendLine();

        i = 0;
        foreach ((string parameterType, bool isRef, _, _, bool isAffective) in parameters) {
            if (parameterType is EntityFullName or SystemCommandsFullName || parameterType == threadStorageType)
                continue;

            if (!isRef) {
                i++;
                continue;
            }

            builder.AppendIndentation(3).Append("if (observers").Append(i).Append(" is not null && changed")
                .Append(i).AppendLine(".Inner is not null)");
            builder.AppendIndentation(4).Append("data.Changed.Add((observers").Append(i).Append(", changed").Append(i)
                .AppendLine(".Inner));");
        }

        if (threadStorageType is not null) {
            builder.AppendLine();
            builder.AppendIndentation(3).Append(GeneratorConstants.InternalThings)
                .AppendLine("_Storage.PushThreadStorage(threadStorage);");
        }

        builder.AppendIndentation(2).AppendLine("}").AppendLine();
    }

}
