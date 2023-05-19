using NoiseEngine.Nesl;
using System.Diagnostics;
using System.Globalization;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

string builtInResourcesPath = Path.Combine(
    Directory.GetParent(Environment.CurrentDirectory)!.FullName, "NoiseEngine", "BuiltInResources"
);
string outputPath = Path.Combine(
    Directory.GetParent(Environment.CurrentDirectory)!.FullName, "NoiseEngine", args[0]
);

Console.Write("Compiling built-in shaders...");
Stopwatch stopwatch = Stopwatch.StartNew();
string shadersPath = Path.Combine(builtInResourcesPath, "Shaders");
bool result = NeslCompiler.TryCompileWithoutDefault(
    "System", shadersPath, Directory.EnumerateFiles(shadersPath, "*.nesl", SearchOption.AllDirectories)
        .Select(x => new NeslFile(x, File.ReadAllText(x))), Enumerable.Empty<NeslAssembly>(),
    out NeslAssembly? assembly, out IEnumerable<CompilationError> errors
);
stopwatch.Stop();
Console.WriteLine($" {(result ? "Done" : "Failed")} in {stopwatch.Elapsed.TotalSeconds:0.##}s!");

int i = 0;
foreach (CompilationError error in errors) {
    if (i == 0)
        Console.WriteLine("Shaders compilation output:");

    Console.ForegroundColor = error.Severity switch {
        CompilationErrorSeverity.Error => ConsoleColor.Red,
        CompilationErrorSeverity.Warning => ConsoleColor.Yellow,
        _ => throw new NotImplementedException(),
    };

    Console.WriteLine(error);
    Console.ResetColor();
    i++;
}

if (i > 0)
    Console.WriteLine();
if (!result)
    Environment.Exit(-1);

Console.Write("Serializing compiled shaders...");
File.WriteAllBytes(Path.Combine(outputPath, "System.nesil"), assembly!.GetRawBytes());
Console.WriteLine(" Done!");
